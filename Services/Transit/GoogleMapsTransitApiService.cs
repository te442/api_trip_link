using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using API_trip_link.Models;
using API_trip_link.Services.Optimizer;
using Microsoft.AspNetCore.WebUtilities;

namespace API_trip_link.Services.Transit
{
    public class GoogleMapsTransitApiService : ITransitApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly MockTransitApiService _fallback = new();
        private readonly ConcurrentDictionary<string, TransitQueryResult> _cache = new();

        public GoogleMapsTransitApiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config     = config;
        }

        public async Task<TransitQueryResult> GetTransitTimeAsync(
            TransitLocation from,
            TransitLocation to,
            double baseTransitHours,
            double walkingHours,
            DateTime departureTime)
        {
            var apiKey = _config["GoogleMaps:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                LogScoreTableCall("MockFallback", from, to, departureTime, "no API key");
                return await _fallback.GetTransitTimeAsync(from, to, baseTransitHours, walkingHours, departureTime);
            }

            var cacheKey = BuildCacheKey(from, to, departureTime);
            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                LogScoreTableCall("CacheHit", from, to, departureTime,
                    $"bus={cached.BusTransitHours:F2}h car={cached.CarTransitHours:F2}h");
                return cached;
            }

            try
            {
                var origin      = FormatLocation(from);
                var destination = FormatLocation(to);
                var departure   = new DateTimeOffset(departureTime).ToUnixTimeSeconds();

                LogScoreTableCall("Request", from, to, departureTime, $"origin={origin} dest={destination}");

                var transitTask = FetchDirectionsAsync(origin, destination, "transit", departure, apiKey);
                var drivingTask = FetchDirectionsAsync(origin, destination, "driving", departure, apiKey);
                await Task.WhenAll(transitTask, drivingTask);
                var transitLeg = await transitTask;
                var drivingLeg = await drivingTask;

                if (transitLeg == null && drivingLeg == null)
                {
                    LogScoreTableCall("MockFallback", from, to, departureTime, "GMaps returned no routes");
                    return await _fallback.GetTransitTimeAsync(from, to, baseTransitHours, walkingHours, departureTime);
                }
                //הכנת אובייקט תוצאה כולל פרטים לאלגוריתם הראשי
                var result = new TransitQueryResult
                {
                    BusTransitHours = transitLeg?.DurationHours ?? baseTransitHours,//זמן הנסיעה באוטובוס
                    CarTransitHours = drivingLeg?.DurationHours ?? baseTransitHours * 0.75,// מוכפל ב 0.75 כדי לקבל את הזמן הנסיעה ברכב
                    HasDirectBus    = transitLeg?.HasDirectBus ?? false,//האם יש קו ישיר
                    WalkingHours    = walkingHours,//זמן הטיול בפיתוח
                    TransitSteps    = transitLeg?.Steps ?? new()
                };
                //הכנת תוצאה  לאלגוריתם הראשי
                LogScoreTableCall("Response", from, to, departureTime,
                    $"bus={result.BusTransitHours:F2}h car={result.CarTransitHours:F2}h direct={result.HasDirectBus}");

                _cache[cacheKey] = result;
                return result;
            }
            catch (Exception ex)
            {
                LogScoreTableCall("Error", from, to, departureTime, ex.Message);
                return await _fallback.GetTransitTimeAsync(from, to, baseTransitHours, walkingHours, departureTime);
            }
        }
//פעולה המוצאת את הזמן המירוח באמצעות שירותי Google Maps
        private async Task<ParsedLeg?> FetchDirectionsAsync(
            string origin, string destination, string mode, long departureUnix, string apiKey)
        {
            //הגדרת הקישור לשירותי Google Maps
            var baseUrl  = _config["GoogleMaps:BaseUrl"] ?? "https://maps.googleapis.com/maps/api";
            if (!baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("GoogleMaps:BaseUrl must use HTTPS.");
            var region   = _config["GoogleMaps:Region"] ?? "il";
            var language = _config["GoogleMaps:Language"] ?? "he";

            // הגדרת פרמטרי הבקשה לשירותי Google Maps
            var queryParams = new Dictionary<string, string?>
            {
                ["origin"]         = origin,//מקום המוצא מתקבל מהמשתמש בטקסט 
                ["destination"]    = destination,
                ["mode"]           = mode,//סוג התחבורה
                ["departure_time"] = departureUnix.ToString(CultureInfo.InvariantCulture),
                ["region"]         = region,//איזור בארץ
                ["language"]       = language,
                ["key"]            = apiKey//מפתח השירות מקבל מהקובץ הגדרות
            };
            //הגדרת הקישור לשירותי Google Maps עם פרמטרים של הבקשה
            var url = QueryHelpers.AddQueryString($"{baseUrl}/directions/json", queryParams);

            LogGMapsHttp(mode, origin, destination, departureUnix, url);

            using var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                LogGMapsHttpResult(mode, origin, destination, $"HTTP {(int)response.StatusCode}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<GoogleDirectionsResponse>(json);
            if (data == null || data.Status != "OK" || data.Routes.Count == 0)
            {
                LogGMapsHttpResult(mode, origin, destination, $"status={data?.Status ?? "null"}");
                return null;
            }

            var leg = data.Routes[0].Legs.FirstOrDefault();
            if (leg == null) return null;

            var steps = new List<GoogleTransitStep>();
            bool hasDirectBus = false;

            foreach (var step in leg.Steps.Where(s => s.TravelMode == "TRANSIT"))
            {
                var td = step.TransitDetails;
                if (td == null) continue;

                if (td.NumStops == 0) hasDirectBus = true;

                steps.Add(new GoogleTransitStep
                {
                    LineName      = td.Line?.ShortName ?? td.Line?.Name ?? "",
                    VehicleType   = td.Line?.Vehicle?.Type ?? "",
                    FromStation   = td.DepartureStop?.Name ?? "",
                    ToStation     = td.ArrivalStop?.Name ?? "",
                    DepartureTime = td.DepartureTime != null
                        ? DateTimeOffset.FromUnixTimeSeconds(td.DepartureTime.Value).LocalDateTime
                        : default,
                    ArrivalTime   = td.ArrivalTime != null
                        ? DateTimeOffset.FromUnixTimeSeconds(td.ArrivalTime.Value).LocalDateTime
                        : default,
                    DurationHours = step.Duration.Value / 3600.0
                });
            }

            return new ParsedLeg
            {
                DurationHours = leg.Duration.Value / 3600.0,
                HasDirectBus  = hasDirectBus,
                Steps         = steps
            };
        }

#if DEBUG
        private static void LogScoreTableCall(
            string phase, TransitLocation from, TransitLocation to, DateTime departure, string detail)
        {
            if (!OptimizerDebugTrace.IsScoreTableActive) return;
            var seq = OptimizerDebugTrace.NextCallSequence();
            var cell = OptimizerDebugTrace.FormatCellContext();
            System.Diagnostics.Debug.WriteLine(
                $"[ScoreTable-GMaps #{seq}] {phase} | {cell} | sampleDep={departure:yyyy-MM-dd HH:mm} | {detail}");
        }

        private static void LogGMapsHttp(string mode, string origin, string destination, long departureUnix, string url)
        {
            if (!OptimizerDebugTrace.IsScoreTableActive) return;
            var safeUrl = url.Contains("key=", StringComparison.OrdinalIgnoreCase)
                ? url[..url.IndexOf("key=", StringComparison.OrdinalIgnoreCase)] + "key=***"
                : url;
            System.Diagnostics.Debug.WriteLine(
                $"[ScoreTable-GMaps HTTP] mode={mode} dep={departureUnix} {origin} → {destination} | {safeUrl}");
        }

        private static void LogGMapsHttpResult(string mode, string origin, string destination, string detail)
        {
            if (!OptimizerDebugTrace.IsScoreTableActive) return;
            System.Diagnostics.Debug.WriteLine(
                $"[ScoreTable-GMaps HTTP] mode={mode} {origin} → {destination} | {detail}");
        }
#else
        private static void LogScoreTableCall(string phase, TransitLocation from, TransitLocation to, DateTime departure, string detail) { }
        private static void LogGMapsHttp(string mode, string origin, string destination, long departureUnix, string url) { }
        private static void LogGMapsHttpResult(string mode, string origin, string destination, string detail) { }
#endif

        private static string FormatLocation(TransitLocation loc)
        {
            if (loc.HasCoordinates)
                return $"{loc.Latitude.ToString(CultureInfo.InvariantCulture)},{loc.Longitude.ToString(CultureInfo.InvariantCulture)}";
            return loc.Address;
        }

        private static string BuildCacheKey(TransitLocation from, TransitLocation to, DateTime departure)
            => $"{from.Latitude},{from.Longitude}|{from.Address}|{to.Latitude},{to.Longitude}|{to.Address}|{departure:yyyyMMddHHmm}";

        private class ParsedLeg
        {
            public double DurationHours { get; set; }
            public bool HasDirectBus { get; set; }
            public List<GoogleTransitStep> Steps { get; set; } = new();
        }
    }
}

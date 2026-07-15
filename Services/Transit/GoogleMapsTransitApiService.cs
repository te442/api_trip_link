using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using API_trip_link.Settings;
using API_trip_link.Models;
using API_trip_link.Services.Optimizer;
using Microsoft.AspNetCore.WebUtilities;

namespace API_trip_link.Services.Transit
{
    //מחלקת שירות הקריאות לגוגל מפס
    public class GoogleMapsTransitApiService : ITransitApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ConcurrentDictionary<string, TransitQueryResult> _cache = new();
        private readonly ConcurrentDictionary<string, TransitDepartureBatch> _departureCache = new();
        private readonly ConcurrentDictionary<string, double> _drivingCache = new();
        private int _httpRequestCount;

        public int HttpRequestCount => _httpRequestCount;

        public void ResetHttpRequestCount() => Interlocked.Exchange(ref _httpRequestCount, 0);

        public GoogleMapsTransitApiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config     = config;
        }
        //פונקציה המחזירה אובייקט זמני נסיעה בתח"צ וברכב פרטי
        public async Task<TransitQueryResult> GetTransitTimeAsync(
            TransitLocation from,
            TransitLocation to,
            double walkingHours,
            DateTime departureTime)
        {
            //המפתח להתממשקות לשירות API
            var apiKey = _config["GoogleMaps:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return EmptyTransitResult(walkingHours);
            //מניעת חישובים לקשתות כפולות
            var cacheKey = BuildCacheKey(from, to, departureTime);
            if (_cache.TryGetValue(cacheKey, out var cached))
                return cached;

            try
            {

                var origin      = FormatLocation(from);
                var destination = FormatLocation(to);
                var departure   = new DateTimeOffset(departureTime).ToUnixTimeSeconds();

                var transitTask = FetchDirectionsLegAsync(origin, destination, "transit", departure, apiKey, alternatives: false);
                var drivingTask = FetchDirectionsLegAsync(origin, destination, "driving", departure, apiKey, alternatives: false);
                await Task.WhenAll(transitTask, drivingTask);
                var transitLeg = await transitTask;
                var drivingLeg = await drivingTask;

                if (transitLeg == null && drivingLeg == null)
                    return EmptyTransitResult(walkingHours);

                var result = new TransitQueryResult
                {
                    BusTransitHours = transitLeg?.DurationHours ?? 0,
                    CarTransitHours = drivingLeg?.DurationHours ?? 0,
                    HasDirectBus    = transitLeg?.HasDirectBus ?? false,
                    WalkingHours    = walkingHours,
                    TransitSteps    = transitLeg?.Steps ?? new()
                };

                _cache[cacheKey] = result;
                return result;
            }
            catch
            {
                return EmptyTransitResult(walkingHours);
            }
        }
        //פונקציה המקבלת מיקומים וזמן יציאה בין שני מקומות ומחזירה את אפשרויות הניסעות
        public async Task<TransitDepartureBatch> GetDepartureOptionsAsync(
            TransitLocation from,
            TransitLocation to,
            DateTime queryTime)
        {
            var apiKey = _config["GoogleMaps:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new GoogleMapsApiException("MISSING_KEY", "GoogleMaps:ApiKey לא הוגדר ב-appsettings.Development.json");
            //המפתח להתממשקות לשירות API
            var cacheKey = BuildDepartureCacheKey(from, to, queryTime);
            if (_departureCache.TryGetValue(cacheKey, out var cached))
                return cached;

            try
            {
                var origin      = FormatLocation(from);
                var destination = FormatLocation(to);
                var departure   = new DateTimeOffset(queryTime).ToUnixTimeSeconds();

                LogScoreTableCall("DepartureBatch", from, to, queryTime, "alternatives=true");
                //קריאת כל המסלולים האפשריים בתח"צ
                var routes = await FetchAllTransitRoutesAsync(origin, destination, departure, apiKey, queryTime);
                var batch = new TransitDepartureBatch { QueryTime = queryTime };
                //הכנת רשימת אפשרויות יציאה
                foreach (var route in routes)
                {
                    if (route.DepartureTime == default) continue;
                    batch.Options.Add(new TransitDepartureOption
                    {
                        DepartureTime = route.DepartureTime,
                        ArrivalTime   = route.ArrivalTime,
                        DurationHours = route.DurationHours,
                        HasDirectBus  = route.HasDirectBus,
                        TransitSteps  = route.Steps
                    });
                }
                //עבור כל קבוצה באותו הזמן נעדיף את הנסיעהעם הקו הישיר

                batch.Options = batch.Options
                    .OrderBy(o => o.DepartureTime)
                    .GroupBy(o => o.DepartureTime)
                    .Select(g => g.OrderByDescending(o => o.HasDirectBus).First())
                    .ToList();

                _departureCache[cacheKey] = batch;

                return batch;
            }
            catch
            {
                return new TransitDepartureBatch { QueryTime = queryTime };
            }
        }
        //פונקציה המחזירה את משך הנסיעה ברכב פרטי
        public async Task<double?> GetDrivingDurationHoursAsync(
            TransitLocation from,
            TransitLocation to,
            DateTime departureTime)
        {
            var apiKey = _config["GoogleMaps:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return null;

            var cacheKey = $"{Configuration.Transit.DrivingCacheKeyPrefix}{BuildCacheKey(from, to, departureTime)}";
            if (_drivingCache.TryGetValue(cacheKey, out var cached))
                return cached;

            try
            {
                var origin      = FormatLocation(from);
                var destination = FormatLocation(to);
                var departure   = new DateTimeOffset(departureTime).ToUnixTimeSeconds();
                var leg = await FetchDirectionsLegAsync(origin, destination, "driving", departure, apiKey, alternatives: false);
                if (leg == null) return null;
                _drivingCache[cacheKey] = leg.DurationHours;
                return leg.DurationHours;
            }
            catch
            {
                return null;
            }
        }

        private static TransitQueryResult EmptyTransitResult(double walkingHours)
            => new()
            {
                BusTransitHours = 0,
                CarTransitHours = 0,
                HasDirectBus    = false,
                WalkingHours    = walkingHours,
                TransitSteps    = new()
            };
        //פונקציה המבצעת את קריאת כל המסלולים האפשריים בתח"צ
        private async Task<List<ParsedTransitRoute>> FetchAllTransitRoutesAsync(
            string origin, string destination, long departureUnix, string apiKey, DateTime queryFallback)
        {
            //הכנת כתובת הבקשה פרוטוקול ועוד נתונים
            var baseUrl  = _config["GoogleMaps:BaseUrl"] ?? Configuration.Transit.DefaultGoogleMapsBaseUrl;
            var region   = _config["GoogleMaps:Region"] ?? Configuration.Transit.DefaultGoogleMapsRegion;
            var language = _config["GoogleMaps:Language"] ?? Configuration.Transit.DefaultGoogleMapsLanguage;

            var queryParams = new Dictionary<string, string?>
            {
                ["origin"]         = origin,
                ["destination"]    = destination,
                ["mode"]           = Configuration.Transit.GoogleMapsModeTransit,
                ["departure_time"] = departureUnix.ToString(CultureInfo.InvariantCulture),
                ["alternatives"]   = Configuration.Transit.GoogleMapsAlternativesEnabled,
                ["region"]         = region,
                ["language"]       = language,
                ["key"]            = apiKey
            };
            //הכנת כתובת הבקשה עם הפרמטרים
            var url = QueryHelpers.AddQueryString($"{baseUrl}{Configuration.Transit.GoogleDirectionsApiPath}", queryParams);
            //דיבוג על קריאות גוגל מפס
            LogGMapsHttp("transit+alt", origin, destination, departureUnix, url);
            //שליחת הבקשה ושמירת התוצאה
            Interlocked.Increment(ref _httpRequestCount);
            using var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new();

            var json = await response.Content.ReadAsStringAsync();
            //המרת התשובה לפורמט מותאם למערכת
            var data = JsonSerializer.Deserialize<GoogleDirectionsResponse>(json);
            if (data == null)
                return new();

            if (!IsDirectionsSuccess(data))
                return new();
            return data.Routes
                .Select(r => r.Legs.FirstOrDefault())
                .Where(leg => leg != null)
                .Select(leg => ParseTransitRoute(leg!, queryFallback))
                .Where(r => r != null)
                .Cast<ParsedTransitRoute>()
                .ToList();
        }
        //פונקציה המבצעת את קריאת נתוני נסיעה מהגוגל מפס

        private async Task<ParsedLeg?> FetchDirectionsLegAsync(
            string origin, string destination, string mode, long departureUnix, string apiKey, bool alternatives)
        {
            //הכנת כתובת הבקשה פרוטוקול 
            var baseUrl  = _config["GoogleMaps:BaseUrl"] ?? Configuration.Transit.DefaultGoogleMapsBaseUrl;
            if (!baseUrl.StartsWith(Configuration.Common.RequiredUrlScheme, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("GoogleMaps:BaseUrl must use HTTPS.");
            var region   = _config["GoogleMaps:Region"] ?? Configuration.Transit.DefaultGoogleMapsRegion;
            var language = _config["GoogleMaps:Language"] ?? Configuration.Transit.DefaultGoogleMapsLanguage;
            //הכנת פרמטרים של הבקשה 
            var queryParams = new Dictionary<string, string?>
            {
                ["origin"]         = origin,
                ["destination"]    = destination,
                ["mode"]           = mode,
                ["departure_time"] = departureUnix.ToString(CultureInfo.InvariantCulture),
                ["region"]         = region,
                ["language"]       = language,
                ["key"]            = apiKey
            };
            //הגדרת פרמטר של אופציות מסלול כאשר זה תח"צ
            if (alternatives && mode == Configuration.Transit.GoogleMapsModeTransit)
                queryParams["alternatives"] = Configuration.Transit.GoogleMapsAlternativesEnabled;
            //הכנת כתובת הבקשה עם הפרמטרים
            var url = QueryHelpers.AddQueryString($"{baseUrl}{Configuration.Transit.GoogleDirectionsApiPath}", queryParams);
            //דיבוג על קריאות גוגל מפס
            LogGMapsHttp(mode, origin, destination, departureUnix, url);
            //שליחת הבקשה ושמירת התוצאה
            Interlocked.Increment(ref _httpRequestCount);
            using var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            //המרת התשובה 
            var data = JsonSerializer.Deserialize<GoogleDirectionsResponse>(json);
            if (data == null) return null;
            //בדיקת תקינות התשובה
            if (!IsDirectionsSuccess(data))
                return null;
            //הוצאת המסלול הראשון
            var leg = data.Routes[0].Legs.FirstOrDefault();
            if (leg == null) return null;

            if (mode == Configuration.Transit.GoogleMapsModeTransit)
            {
                //החזרת נתוני הנסיעה 
                var queryFallback = DateTimeOffset.FromUnixTimeSeconds(departureUnix).LocalDateTime;
                var route = ParseTransitRoute(leg, queryFallback);
                return route == null ? null : new ParsedLeg
                {
                    DurationHours = route.DurationHours,
                    HasDirectBus  = route.HasDirectBus,
                    Steps         = route.Steps
                };
            }
            //החזרת נתוני זמני הנסיעה 
            return new ParsedLeg
            {
                //המרה לשעות
                DurationHours = leg.Duration.Value / Configuration.Common.SecondsPerHour,
                HasDirectBus  = false,
                Steps         = new()
            };
        }
        //פונקציה בודקת את תקינות תשובה מהגוגל מפס
        private static bool IsDirectionsSuccess(GoogleDirectionsResponse data)
        {
            var status = data.Status ?? "";

            if (string.Equals(status, Configuration.Transit.GoogleDirectionsStatusOk, StringComparison.OrdinalIgnoreCase))
                return data.Routes.Count > 0;

            AgentDebugLog.Write("GoogleMapsTransit", "Directions status",
                new { Status = status, data.ErrorMessage, routeCount = data.Routes.Count }, "G1");

            if (string.Equals(status, Configuration.Transit.GoogleDirectionsStatusZeroResults, StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, Configuration.Transit.GoogleDirectionsStatusNotFound, StringComparison.OrdinalIgnoreCase))
                return false;

            throw new GoogleMapsApiException(status, data.ErrorMessage);
        }
        //פונקציה הממירה נתוני תחצ לנתונים מותאמים למערכת
        private static ParsedTransitRoute? ParseTransitRoute(GoogleLeg leg, DateTime queryFallback)
        {
            //סינון שלבי התחבורה
            var transitSteps = leg.Steps
                .Where(s => string.Equals(s.TravelMode, Configuration.Transit.GoogleTravelModeTransit, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (transitSteps.Count == 0) return null;
            //שעת היציאה
            var firstTransit = transitSteps[0];
            var td = firstTransit.TransitDetails;
            DateTime departure = default;
            if (td?.DepartureTime != null && td.DepartureTime.Value > 0)
                departure = DateTimeOffset.FromUnixTimeSeconds(td.DepartureTime.Value).LocalDateTime;

            if (departure == default)
                departure = queryFallback;
            //שעת הגעה
            DateTime arrival;
            if (td?.ArrivalTime != null && td.ArrivalTime.Value > 0)
                arrival = DateTimeOffset.FromUnixTimeSeconds(td.ArrivalTime.Value).LocalDateTime;
            else if (leg.Duration?.Value > 0)
                arrival = departure.AddSeconds(leg.Duration.Value);
            else
                arrival = departure;
            //סכימת משך זמן הנסיעה בשעות
            double durationHours = leg.Duration?.Value > 0
                ? leg.Duration.Value / Configuration.Common.SecondsPerHour
                : Math.Max(0, (arrival - departure).TotalHours);
            //הכנת רשימת שלבי התחבורה
            var steps = new List<GoogleTransitStep>();
            bool hasDirectBus = false;

            foreach (var step in transitSteps)
            {
                var stepTd = step.TransitDetails;
                if (stepTd == null) continue;
                if (stepTd.NumStops == Configuration.Transit.DirectBusMaxStops) hasDirectBus = true;

                steps.Add(new GoogleTransitStep
                {
                    //אובייקט פרטי האוטובוס
                    LineName      = stepTd.Line?.ShortName ?? stepTd.Line?.Name ?? "",
                    VehicleType   = stepTd.Line?.Vehicle?.Type ?? "",
                    FromStation   = stepTd.DepartureStop?.Name ?? "",
                    ToStation     = stepTd.ArrivalStop?.Name ?? "",
                    DepartureTime = stepTd.DepartureTime != null && stepTd.DepartureTime.Value > 0
                        ? DateTimeOffset.FromUnixTimeSeconds(stepTd.DepartureTime.Value).LocalDateTime
                        : default,
                    ArrivalTime   = stepTd.ArrivalTime != null && stepTd.ArrivalTime.Value > 0
                        ? DateTimeOffset.FromUnixTimeSeconds(stepTd.ArrivalTime.Value).LocalDateTime
                        : default,
                    DurationHours = step.Duration?.Value > 0 ? step.Duration.Value / Configuration.Common.SecondsPerHour : 0
                });
            }
            //החזרת התוצאה
            return new ParsedTransitRoute
            {
                DepartureTime = departure,
                ArrivalTime   = arrival,
                DurationHours = durationHours,
                HasDirectBus  = hasDirectBus,
                Steps         = steps
            };
        }

#if DEBUG
        //פונקצית דיבוג לטבלת ציונים של המערכת
        private static void LogScoreTableCall(
            string phase, TransitLocation from, TransitLocation to, DateTime departure, string detail)
        {
            if (!OptimizerDebugTrace.IsScoreTableActive) return;
            var seq = OptimizerDebugTrace.NextCallSequence();
            var cell = OptimizerDebugTrace.FormatCellContext();
            System.Diagnostics.Debug.WriteLine(
                $"[ScoreTable-GMaps #{seq}] {phase} | {cell} | sampleDep={departure:yyyy-MM-dd HH:mm} | {detail}");
        }

        //פונקצית דיבוג  על קריאות גוגל מפס
        private static void LogGMapsHttp(string mode, string origin, string destination, long departureUnix, string url)
        {
            if (!OptimizerDebugTrace.IsScoreTableActive) return;
            var safeUrl = url.Contains("key=", StringComparison.OrdinalIgnoreCase)
                ? url[..url.IndexOf("key=", StringComparison.OrdinalIgnoreCase)] + "key=***"
                : url;
            System.Diagnostics.Debug.WriteLine(
                $"[ScoreTable-GMaps HTTP] mode={mode} dep={departureUnix} {origin} → {destination} | {safeUrl}");
        }
#else
        private static void LogScoreTableCall(string phase, TransitLocation from, TransitLocation to, DateTime departure, string detail) { }
        private static void LogGMapsHttp(string mode, string origin, string destination, long departureUnix, string url) { }
#endif
        // לשליחת המיקום לשירות גוגל מפס -פונקצית עזר 
        private static string FormatLocation(TransitLocation loc)
        {
            if (loc.HasCoordinates)
                return $"{loc.Latitude.ToString(CultureInfo.InvariantCulture)},{loc.Longitude.ToString(CultureInfo.InvariantCulture)}";
            return loc.Address;
        }
        //פונקצית עזר - לבנית מפתח יחודי לכל קריאה לשירות גוגל מפס
        private static string BuildCacheKey(TransitLocation from, TransitLocation to, DateTime departure)
            => $"{from.Latitude},{from.Longitude}|{from.Address}|{to.Latitude},{to.Longitude}|{to.Address}|{departure:yyyyMMddHHmm}";
        //פונקצית עזר- שמירת רשימת יציאות אוטובוס
        private static string BuildDepartureCacheKey(TransitLocation from, TransitLocation to, DateTime queryTime)
            => $"{Configuration.Transit.DepartureCacheKeyPrefix}{BuildCacheKey(from, to, queryTime)}";

        private class ParsedLeg
        {
            public double DurationHours { get; set; }
            public bool HasDirectBus { get; set; }
            public List<GoogleTransitStep> Steps { get; set; } = new();
        }

        private class ParsedTransitRoute
        {
            public DateTime DepartureTime { get; set; }
            public DateTime ArrivalTime   { get; set; }
            public double DurationHours   { get; set; }
            public bool HasDirectBus      { get; set; }
            public List<GoogleTransitStep> Steps { get; set; } = new();
        }
    }
}

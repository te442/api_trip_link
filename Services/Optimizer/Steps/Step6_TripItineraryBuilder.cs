using API_trip_link.Settings;
using System.Text;
using API_trip_link.Data.Repositories;
using API_trip_link.Models;
using API_trip_link.Services.Transit;

namespace API_trip_link.Services.Optimizer.Steps
{
    internal class Step6_TripItineraryBuilder : IOptimizerStep
    {
        private readonly IOptimizerDataRepository _data;
        private readonly ITransitApiService _transitApi;
        private readonly ILogger<Step6_TripItineraryBuilder> _logger;

        public Step6_TripItineraryBuilder(
            IOptimizerDataRepository data,
            ITransitApiService transitApi,
            ILogger<Step6_TripItineraryBuilder> logger)
        {
            _data = data;
            _transitApi = transitApi;
            _logger = logger;
        }

        public int StepNumber => 6;
        public string StepName => "OUTPUT";

        public async Task ExecuteAsync(OptimizationContext ctx)
        {
            var selectedIds = new HashSet<int>(ctx.BestRoute.Destinations.Select(d => d.DestinationId));

            var available = ctx.Destinations.Select(d => new AvailableDestination
            {
                DesId          = d.DestinationId,
                NameDes        = d.Name,
                Region         = ctx.TripRegion,
                VisitHours     = d.VisitDuration,
                NearestStation = d.NearestStation,
                WasSelected    = selectedIds.Contains(d.DestinationId)
            }).ToList();

            var legs        = new List<TripLeg>();
            var currentTime = ctx.Params.TripStartTime;
            string prevLabel = ctx.Params.AddressStart;

            for (int i = 0; i < ctx.BestRoute.Destinations.Count; i++)
            {
                var dest = ctx.BestRoute.Destinations[i];
                var arc  = i < ctx.BestRoute.ArcCosts.Count ? ctx.BestRoute.ArcCosts[i] : null;

                var nominalDeparture = arc?.BestDepartureTime ?? currentTime;
                var departureTime    = nominalDeparture < currentTime ? currentTime : nominalDeparture;
                var transitHours     = arc?.BusTransitHours ?? 0;
                var arrivalTime      = departureTime.AddHours(transitHours + dest.WalkingTimeHours);
                var leaveTime        = arrivalTime.AddHours(dest.VisitDuration);

                var (busLines, boardingStation, alightingStation) = await BuildTransitDisplayAsync(arc, dest, i, ctx);

                legs.Add(new TripLeg
                {
                    Order           = i + 1,
                    DesId           = dest.DestinationId,
                    DestinationName = dest.Name,
                    Region          = ctx.TripRegion,
                    ArrivalTime     = arrivalTime,
                    DepartureTime   = leaveTime,
                    StayDuration    = TimeSpan.FromHours(dest.VisitDuration),
                    Transit         = new TransitSegment
                    {
                        FromLabel         = prevLabel,
                        BoardingStation   = boardingStation,
                        AlightingStation  = alightingStation,
                        WalkingMinutes    = dest.WalkingTimeHours * Configuration.Common.MinutesPerHour,
                        BusLines          = busLines,
                        DepartureTime     = departureTime,
                        ArrivalTime       = arrivalTime,
                        TransitEfficiency = arc?.TransitEfficiency ?? 0
                    }
                });

                prevLabel   = dest.Name;
                currentTime = leaveTime;
            }

            var returnLeg = legs.Count > 0
                ? await BuildReturnLegAsync(ctx, legs[^1].DepartureTime)
                : null;

            ctx.TripPlan = new TripPlan
            {
                TripName              = ctx.TripName,
                AddressStart          = ctx.Params.AddressStart,
                TripStartTime         = ctx.Params.TripStartTime,
                TripEndTime           = ctx.Params.TripEndTime,
                TotalScore            = ctx.BestRoute.TotalScore,
                TransitEfficiency     = ctx.BestRoute.TransitEfficiency,
                AvailableDestinations = available,
                Legs                  = legs,
                ReturnLeg             = returnLeg,
                Narrative             = BuildNarrative(ctx, legs, returnLeg, available)
            };

            OptimizerLog.Info(_logger, ctx,
                "מסלול מפורט: {Legs} רגליים, {Selected} יעדים נבחרו מתוך {Available}",
                legs.Count, ctx.BestRoute.Destinations.Count, available.Count);
        }

        private async Task<TripLeg?> BuildReturnLegAsync(OptimizationContext ctx, DateTime leaveTime)
        {
            if (ctx.BestRoute.Destinations.Count == 0)
                return null;

            var lastDest = ctx.BestRoute.Destinations[^1];
            var from = new TransitLocation
            {
                DestinationId = lastDest.DestinationId,
                Address = lastDest.Name,
                Latitude = lastDest.Latitude,
                Longitude = lastDest.Longitude
            };
            var home = new TransitLocation
            {
                DestinationId = Configuration.Common.OriginDestinationId,
                Address = ctx.Params.AddressStart,
                Latitude = ctx.Params.StartLatitude,
                Longitude = ctx.Params.StartLongitude
            };

            var batch = await _transitApi.GetDepartureOptionsAsync(from, home, leaveTime);
            if (!batch.HasAnyRoute)
                return null;

            var option = batch.Options
                .Where(o => o.DepartureTime >= leaveTime)
                .OrderBy(o => o.DepartureTime)
                .FirstOrDefault()
                ?? batch.Options.OrderBy(o => o.DepartureTime).First();

            var departureTime = option.DepartureTime < leaveTime ? leaveTime : option.DepartureTime;
            var busLines = MapGoogleStepsToBusLines(option.TransitSteps);

            return new TripLeg
            {
                Order = ctx.BestRoute.Destinations.Count + 1,
                DesId = Configuration.Common.OriginDestinationId,
                DestinationName = ctx.Params.AddressStart,
                Region = "נקודת התחלה",
                ArrivalTime = option.ArrivalTime,
                DepartureTime = option.ArrivalTime,
                StayDuration = TimeSpan.Zero,
                Transit = new TransitSegment
                {
                    FromLabel = lastDest.Name,
                    BoardingStation = option.TransitSteps.Count > 0
                        ? StationFromName(option.TransitSteps[0].FromStation)
                        : lastDest.NearestStation,
                    AlightingStation = option.TransitSteps.Count > 0
                        ? StationFromName(option.TransitSteps[^1].ToStation)
                        : null,
                    WalkingMinutes = 0,
                    BusLines = busLines,
                    DepartureTime = departureTime,
                    ArrivalTime = option.ArrivalTime,
                    TransitEfficiency = 0
                }
            };
        }

        private static List<BusLineInfo> MapGoogleStepsToBusLines(IReadOnlyList<GoogleTransitStep> steps)
            => steps.Select(s => new BusLineInfo
            {
                BusNumber     = string.IsNullOrWhiteSpace(s.LineName) ? VehicleLabel(s.VehicleType) : s.LineName,
                Direction     = VehicleLabel(s.VehicleType),
                VehicleType   = s.VehicleType,
                FromStation   = s.FromStation,
                ToStation     = s.ToStation,
                DepartureTime = s.DepartureTime,
                ArrivalTime   = s.ArrivalTime
            }).ToList();

        private async Task<(List<BusLineInfo> busLines, StationInfo? boarding, StationInfo? alighting)>
            BuildTransitDisplayAsync(
                ArcCost? arc,
                OptimizerDestination dest,
                int legIndex,
                OptimizationContext ctx)
        {
            if (arc?.TransitSteps.Count > 0)
            {
                var busLines = MapTransitStepsToBusLines(arc.TransitSteps);
                var boarding = StationFromName(arc.TransitSteps[0].FromStation);
                var alighting = StationFromName(arc.TransitSteps[^1].ToStation) ?? dest.NearestStation;
                return (busLines, boarding, alighting);
            }

            var fallbackLines = await FindBusLinesAsync(dest.NearestStation);
            var fallbackBoarding = legIndex == 0
                ? null
                : ctx.BestRoute.Destinations[legIndex - 1].NearestStation;

            return (fallbackLines, fallbackBoarding, dest.NearestStation);
        }

        private static List<BusLineInfo> MapTransitStepsToBusLines(IReadOnlyList<TransitLegStep> steps)
            => steps.Select(s => new BusLineInfo
            {
                BusNumber     = string.IsNullOrWhiteSpace(s.LineName) ? VehicleLabel(s.VehicleType) : s.LineName,
                Direction     = VehicleLabel(s.VehicleType),
                VehicleType   = s.VehicleType,
                FromStation   = s.FromStation,
                ToStation     = s.ToStation,
                DepartureTime = s.DepartureTime,
                ArrivalTime   = s.ArrivalTime
            }).ToList();

        private static StationInfo? StationFromName(string stationName)
        {
            if (string.IsNullOrWhiteSpace(stationName)) return null;
            return new StationInfo { StationName = stationName };
        }

        private static string VehicleLabel(string vehicleType) => vehicleType switch
        {
            "BUS"        => "אוטובוס",
            "INTERCITY_BUS" => "אוטובוס בינעירוני",
            "SUBWAY"     => "רכבת תחתית",
            "TRAIN"      => "רכבת",
            "TRAM"       => "רכבת קלה",
            "RAIL"       => "רכבת",
            "FERRY"      => "מעבורת",
            _            => string.IsNullOrWhiteSpace(vehicleType) ? Configuration.Itinerary.DefaultVehicleTypeLabel : vehicleType
        };

        private async Task<List<BusLineInfo>> FindBusLinesAsync(StationInfo? station)
        {
            if (station == null) return new();

            var busStations = await _data.GetBusLinesForStationAsync(station.StationNum);
            if (busStations.Count == 0 && !string.IsNullOrWhiteSpace(station.StationName))
            {
                return new List<BusLineInfo>
                {
                    new()
                    {
                        BusNumber   = Configuration.Itinerary.FallbackBusNumberPlaceholder,
                        Direction   = Configuration.Itinerary.FallbackBusDirectionLabel,
                        FromStation = station.StationName,
                        ToStation   = station.StationName
                    }
                };
            }

            return busStations.Select(bs => new BusLineInfo
            {
                BusId       = bs.BusId,
                BusNumber   = bs.Bus != null ? bs.Bus.BusNumber.ToString() : bs.Bus?.BusCode ?? "",
                Direction   = bs.Bus?.Direction ?? "",
                VehicleType = Configuration.Itinerary.DefaultVehicleType,
                FromStation = station.StationName,
                ToStation   = station.StationName
            }).ToList();
        }

        private static string BuildNarrative(
            OptimizationContext ctx,
            List<TripLeg> legs,
            TripLeg? returnLeg,
            List<AvailableDestination> available)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"הטיול מתחיל ב-{ctx.Params.TripStartTime:HH:mm} מכתובת: {ctx.Params.AddressStart}.");
            sb.AppendLine($"יעדים זמינים: {available.Count} | נבחרו במסלול: {legs.Count}");
            sb.AppendLine();

            foreach (var leg in legs)
            {
                sb.AppendLine($"── קטע {leg.Order}: {leg.Transit.FromLabel} → {leg.DestinationName} ──");

                if (leg.Transit.BoardingStation != null)
                    sb.AppendLine($"עלייה: {leg.Transit.BoardingStation.StationName}");

                foreach (var bus in leg.Transit.BusLines)
                {
                    var lineLabel = string.IsNullOrWhiteSpace(bus.BusNumber) ? bus.Direction : $"קו {bus.BusNumber}";
                    sb.AppendLine($"  {lineLabel} ({bus.Direction}): {bus.FromStation} {bus.DepartureTime:HH:mm} → {bus.ToStation} {bus.ArrivalTime:HH:mm}");
                }

                if (leg.Transit.BusLines.Count == 0)
                    sb.AppendLine($"נסיעה: {leg.Transit.DepartureTime:HH:mm} → {leg.Transit.ArrivalTime:HH:mm}");

                if (leg.Transit.AlightingStation != null)
                    sb.AppendLine($"ירידה: {leg.Transit.AlightingStation.StationName}");

                if (leg.Transit.WalkingMinutes > 0)
                    sb.AppendLine($"הליכה: {leg.Transit.WalkingMinutes:F0} דקות מהתחנה ליעד");

                sb.AppendLine($"הגעה: {leg.ArrivalTime:HH:mm} | שהייה: {leg.StayDuration.TotalHours:F1} שעות | עזיבה: {leg.DepartureTime:HH:mm}");
                sb.AppendLine();
            }

            if (returnLeg != null)
            {
                sb.AppendLine($"── קטע חזור: {returnLeg.Transit.FromLabel} → {returnLeg.DestinationName} ──");

                if (returnLeg.Transit.BoardingStation != null)
                    sb.AppendLine($"עלייה: {returnLeg.Transit.BoardingStation.StationName}");

                foreach (var bus in returnLeg.Transit.BusLines)
                {
                    var lineLabel = string.IsNullOrWhiteSpace(bus.BusNumber) ? bus.Direction : $"קו {bus.BusNumber}";
                    sb.AppendLine($"  {lineLabel} ({bus.Direction}): {bus.FromStation} {bus.DepartureTime:HH:mm} → {bus.ToStation} {bus.ArrivalTime:HH:mm}");
                }

                if (returnLeg.Transit.BusLines.Count == 0)
                    sb.AppendLine($"נסיעה: {returnLeg.Transit.DepartureTime:HH:mm} → {returnLeg.Transit.ArrivalTime:HH:mm}");

                if (returnLeg.Transit.AlightingStation != null)
                    sb.AppendLine($"ירידה: {returnLeg.Transit.AlightingStation.StationName}");

                if (returnLeg.Transit.WalkingMinutes > 0)
                    sb.AppendLine($"הליכה: {returnLeg.Transit.WalkingMinutes:F0} דקות בקטע החזור");

                sb.AppendLine($"יציאה: {returnLeg.Transit.DepartureTime:HH:mm} | הגעה לנקודת ההתחלה: {returnLeg.Transit.ArrivalTime:HH:mm}");
                sb.AppendLine();
            }

            sb.AppendLine($"סה\"כ: {legs.Count} יעדים | יעילות תח\"צ: {ctx.BestRoute.TransitEfficiency:P0}");
            return sb.ToString();
        }
    }
}

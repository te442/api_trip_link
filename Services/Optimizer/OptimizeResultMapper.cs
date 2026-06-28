using API_trip_link.Settings;
using API_trip_link.Models;

namespace API_trip_link.Services.Optimizer
{
    //פעולה שממירה את תוצאת האופטמזציה למבנים המתאימים
    internal static class OptimizeResultMapper
    {
        public static OptimizeResultDto Map(OptimizationContext ctx)
        {
            var bestRoute = ctx.BestRoute;
            var plan      = ctx.TripPlan;
            var destLookup = ctx.Destinations.ToDictionary(d => d.DestinationId);

            var resultDests = bestRoute.Destinations.Select(d => new DestinationDto
            {
                DesId   = d.DestinationId,
                NameDes = d.Name,
                Region  = ctx.TripRegion,
                TimeDes = TimeSpan.FromHours(d.VisitDuration + d.WalkingTimeHours),
                Lat     = destLookup.TryGetValue(d.DestinationId, out var od) ? (decimal?)od.Latitude : null,
                Lon     = destLookup.TryGetValue(d.DestinationId, out var od2) ? (decimal?)od2.Longitude : null
            }).ToList();

            var arcCostDtos = bestRoute.ArcCosts.Select(a => new ArcCostDto
            {
                FromDestinationId  = a.FromDestinationId,
                ToDestinationId    = a.ToDestinationId,
                BestDepartureTime  = a.BestDepartureTime,
                BusTransitHours    = Math.Round(a.BusTransitHours, Configuration.Optimizer.OptimizeResultDecimalPlaces),
                CarTransitHours    = Math.Round(a.CarTransitHours, Configuration.Optimizer.OptimizeResultDecimalPlaces),
                WalkingHours       = Math.Round(a.WalkingHours, Configuration.Optimizer.OptimizeResultDecimalPlaces),
                TotalArcHours      = Math.Round(a.TotalArcHours, Configuration.Optimizer.OptimizeResultDecimalPlaces),
                TransitEfficiency  = Math.Round(a.TransitEfficiency, Configuration.Optimizer.OptimizeResultDecimalPlaces),
                HasDirectBus       = a.HasDirectBus
            }).ToList();

            var legs      = MapLegs(plan.Legs, destLookup, ctx);
            var returnLeg = plan.ReturnLeg != null
                ? MapLeg(plan.ReturnLeg, destLookup)
                : null;
            var mapPoints = BuildMapPoints(
                ctx.Params.AddressStart,
                ctx.Params.StartLatitude,
                ctx.Params.StartLongitude,
                legs,
                destLookup);
            var scoreStats = BuildScoreTableStats(ctx);

            return new OptimizeResultDto
            {
                TripId            = ctx.Request.TripId,
                TripName          = ctx.TripName,
                AddressStart      = ctx.Params.AddressStart,
                DestinationCount  = bestRoute.Destinations.Count,
                TotalScore        = Math.Round(bestRoute.TotalScore, Configuration.Optimizer.OptimizeResultDecimalPlaces),
                TimeUsed          = Math.Round(bestRoute.TotalTime, Configuration.Optimizer.OptimizeResultDecimalPlaces),
                TimeAvailable     = ctx.Params.MaxTimeFrame,
                TransitEfficiency = Math.Round(bestRoute.TransitEfficiency, Configuration.Optimizer.OptimizeResultDecimalPlaces),
                OptimalRoute      = resultDests,
                ArcCosts          = arcCostDtos,
                Narrative         = plan.Narrative,
                Legs              = legs,
                ReturnLeg         = returnLeg,
                MapPoints         = mapPoints,
                ScoreTableStats   = scoreStats,
                ScoreTableCellTrace = ctx.ScoreTableCellTrace
            };
        }

        private static ScoreTableStatsDto? BuildScoreTableStats(OptimizationContext ctx)
        {
            if (ctx.ScoreTable == null) return null;
            var (total, valid) = ctx.ScoreTable.GetStats();
            var n = ctx.ScoreTable.NodeCount;
            var m = ctx.ScoreTable.MinuteCount;
            return new ScoreTableStatsDto
            {
                NodeCount   = n,
                MinuteCount = m,
                HourCount   = m,
                TotalCells  = total,
                ValidCells  = valid,
                ValidRatio  = total > 0 ? Math.Round((double)valid / total, Configuration.Optimizer.ValidRatioDecimalPlaces) : 0,
                Description = $"Event store דליל: {total} אירועי תחבורה מתוך קיבולת לוגית {ctx.ScoreTable.GetLogicalCellCapacity()}"
            };
        }

        public static TripItineraryDto ToItineraryDto(OptimizeResultDto result) => new()
        {
            TripId            = result.TripId,
            TripName          = result.TripName ?? "",
            AddressStart      = result.AddressStart ?? "",
            DestinationCount  = result.DestinationCount,
            TotalScore        = result.TotalScore,
            TimeUsed          = result.TimeUsed,
            TimeAvailable     = result.TimeAvailable,
            TransitEfficiency = result.TransitEfficiency,
            Narrative         = result.Narrative,
            Legs              = result.Legs,
            ReturnLeg         = result.ReturnLeg,
            MapPoints         = result.MapPoints
        };

        private static List<TripLegDto> MapLegs(
            List<TripLeg> legs,
            Dictionary<int, OptimizerDestination> destLookup,
            OptimizationContext ctx)
        {
            return legs.Select(leg => MapLeg(leg, destLookup)).ToList();
        }

        private static TripLegDto MapLeg(
            TripLeg leg,
            Dictionary<int, OptimizerDestination> destLookup)
        {
            destLookup.TryGetValue(leg.DesId, out var dest);

            return new TripLegDto
            {
                Order           = leg.Order,
                DesId           = leg.DesId,
                DestinationName = leg.DestinationName,
                Region          = leg.Region,
                Lat             = dest?.Latitude,
                Lon             = dest?.Longitude,
                ImageUrl        = null,
                ArrivalTime     = leg.ArrivalTime.ToString("HH:mm"),
                DepartureTime   = leg.DepartureTime.ToString("HH:mm"),
                StayDuration    = FormatDuration(leg.StayDuration),
                Transit         = new TransitSegmentDto
                {
                    FromLabel         = leg.Transit.FromLabel,
                    BoardingStation   = leg.Transit.BoardingStation?.StationName,
                    AlightingStation  = leg.Transit.AlightingStation?.StationName,
                    WalkingMinutes    = leg.Transit.WalkingMinutes,
                    DepartureTime     = leg.Transit.DepartureTime.ToString("HH:mm"),
                    ArrivalTime       = leg.Transit.ArrivalTime.ToString("HH:mm"),
                    TransitEfficiency = leg.Transit.TransitEfficiency,
                    BusLines          = leg.Transit.BusLines.Select(b => new BusLineDto
                    {
                        BusNumber     = b.BusNumber,
                        Direction     = b.Direction,
                        VehicleType   = b.VehicleType,
                        FromStation   = b.FromStation,
                        ToStation     = b.ToStation,
                        DepartureTime = b.DepartureTime != default ? b.DepartureTime.ToString("HH:mm") : "",
                        ArrivalTime   = b.ArrivalTime != default ? b.ArrivalTime.ToString("HH:mm") : ""
                    }).ToList()
                }
            };
        }

        private static List<MapPointDto> BuildMapPoints(
            string addressStart,
            double startLatitude,
            double startLongitude,
            List<TripLegDto> legs,
            Dictionary<int, OptimizerDestination> destLookup)
        {
            var points = new List<MapPointDto>();

            if (legs.Count > 0 && (startLatitude != 0 || startLongitude != 0))
            {
                points.Add(new MapPointDto
                {
                    Order = 0,
                    Label = addressStart,
                    Lat   = startLatitude,
                    Lon   = startLongitude
                });
            }

            foreach (var leg in legs)
            {
                if (leg.Lat.HasValue && leg.Lon.HasValue && leg.Lat != 0)
                {
                    points.Add(new MapPointDto
                    {
                        Order = leg.Order,
                        Label = leg.DestinationName,
                        Lat   = leg.Lat.Value,
                        Lon   = leg.Lon.Value
                    });
                }
            }

            return points;
        }

        private static string FormatDuration(TimeSpan ts)
        {
            var h = (int)ts.TotalHours;
            var m = ts.Minutes;
            return h > 0 ? $"{h}ש' {m}ד'" : $"{m} דקות";
        }
    }
}

using API_trip_link.Models;

namespace API_trip_link.Services.Optimizer
{
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
                BusTransitHours    = Math.Round(a.BusTransitHours, 4),
                CarTransitHours    = Math.Round(a.CarTransitHours, 4),
                WalkingHours       = Math.Round(a.WalkingHours, 4),
                TotalArcHours      = Math.Round(a.TotalArcHours, 4),
                TransitEfficiency  = Math.Round(a.TransitEfficiency, 4),
                HasDirectBus       = a.HasDirectBus
            }).ToList();

            var legs      = MapLegs(plan.Legs, destLookup, ctx);
            var mapPoints = BuildMapPoints(ctx.Params.AddressStart, legs, destLookup);
            var scoreStats = BuildScoreTableStats(ctx);

            return new OptimizeResultDto
            {
                TripId            = ctx.Request.TripId,
                TripName          = ctx.TripName,
                AddressStart      = ctx.Params.AddressStart,
                DestinationCount  = bestRoute.Destinations.Count,
                TotalScore        = Math.Round(bestRoute.TotalScore, 4),
                TimeUsed          = Math.Round(bestRoute.TotalTime, 4),
                TimeAvailable     = ctx.Params.MaxTimeFrame,
                TransitEfficiency = Math.Round(bestRoute.TransitEfficiency, 4),
                OptimalRoute      = resultDests,
                ArcCosts          = arcCostDtos,
                Narrative         = plan.Narrative,
                Legs              = legs,
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
            var h = ctx.ScoreTable.HourCount;
            return new ScoreTableStatsDto
            {
                NodeCount   = n,
                HourCount   = h,
                TotalCells  = total,
                ValidCells  = valid,
                ValidRatio  = total > 0 ? Math.Round((double)valid / total, 3) : 0,
                Description = $"טבלה תלת-מימדית {n}×{n}×{h} (מקור×יעד×שעה)"
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
            MapPoints         = result.MapPoints
        };

        private static List<TripLegDto> MapLegs(
            List<TripLeg> legs,
            Dictionary<int, OptimizerDestination> destLookup,
            OptimizationContext ctx)
        {
            return legs.Select(leg =>
            {
                destLookup.TryGetValue(leg.DesId, out var dest);
                var dbDest = ctx.Destinations.FirstOrDefault(d => d.DestinationId == leg.DesId);

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
                            FromStation   = b.FromStation,
                            ToStation     = b.ToStation,
                            DepartureTime = b.DepartureTime.ToString("HH:mm"),
                            ArrivalTime   = b.ArrivalTime.ToString("HH:mm")
                        }).ToList()
                    }
                };
            }).ToList();
        }

        private static List<MapPointDto> BuildMapPoints(
            string addressStart,
            List<TripLegDto> legs,
            Dictionary<int, OptimizerDestination> destLookup)
        {
            var points = new List<MapPointDto>();

            if (legs.Count > 0 && destLookup.Count > 0)
            {
                var first = destLookup.Values.First();
                if (first.Latitude != 0 || first.Longitude != 0)
                {
                    points.Add(new MapPointDto
                    {
                        Order = 0,
                        Label = addressStart,
                        Lat   = first.Latitude,
                        Lon   = first.Longitude
                    });
                }
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

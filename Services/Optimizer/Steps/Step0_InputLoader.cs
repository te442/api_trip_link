using API_trip_link.Settings;
using API_trip_link.Data.Repositories;
using API_trip_link.Models;
using API_trip_link.Services.Optimizer;

namespace API_trip_link.Services.Optimizer.Steps
{
    internal class Step0_InputLoader : IOptimizerStep
    {
        private readonly IOptimizerDataRepository _data;
        private readonly IConfiguration _config;
        private readonly ILogger<Step0_InputLoader> _logger;

        public Step0_InputLoader(
            IOptimizerDataRepository data,
            IConfiguration config,
            ILogger<Step0_InputLoader> logger)
        {
            _data   = data;
            _config = config;
            _logger = logger;
        }

        public int StepNumber => 0;
        public string StepName => "INPUT";
//מטרה: לטעון את הנתונים מהבסיס הנתונים לפי הבקשה
        public async Task ExecuteAsync(OptimizationContext ctx)
        {
            //
            var request = ctx.Request;
            //טעינת הטיול לפי המזהה
            var trip = await _data.GetTripForOptimizationAsync(request.TripId);

            if (trip == null)
                throw new Exception($"Trip {request.TripId} not found");
            
            var nature = trip.NatureTrips?.FirstOrDefault();

            string? region    = nature?.Region;
            int?    levelId   = nature?.LevelId;
            int?    maxNumDes = nature?.MaxNumDes;

            ctx.TripName   = trip.TripName ?? "";
            ctx.TripRegion = region ?? "";
            //רשימה קטגוריות ומאפיינים
            var tripCategoryIds = trip.CategoriesToTrips?
                .Select(c => c.CategoriesId)
                .ToHashSet() ?? new HashSet<int>();

            var tripFeatureIds = trip.FeatureToTrips?
                .Select(f => f.FeatureId)
                .ToHashSet() ?? new HashSet<int>();

            //נותן רשימת יעדים מסוננים לפי אילוצים קשים
            var dbDestinations = await _data.GetDestinationsForOptimizationAsync(
                region, levelId, tripCategoryIds, tripFeatureIds);
            var visitStats = Configuration.Optimizer.VisitStatsByDestinationId;
            //הכנת אובייקט יעד כולל פרטים לאלגוריתם הראשי
            ctx.Destinations = dbDestinations.Select(d =>
            {
                var bestStation = d.StationToDestinations?
                    .OrderBy(s => s.WalkingTime ?? TimeSpan.MaxValue)
                    .FirstOrDefault();

                double walkingHours = bestStation?.WalkingTime.HasValue == true
                    ? bestStation.WalkingTime!.Value.TotalHours
                    : Configuration.Optimizer.DefaultWalkingTimeHours;

                double visitHours = d.TimeDes.HasValue
                    ? d.TimeDes.Value.TotalHours
                    : Configuration.Optimizer.DefaultVisitDurationHours;

                var (avg, std) = visitStats.TryGetValue(d.DesId, out var s) ? s : (Configuration.Optimizer.DefaultVisitCountAvg, Configuration.Optimizer.DefaultVisitCountStd);
                double dynReq  = WeightCalculator.ComputeDynamicRequirements(avg, std);

                StationInfo? stationInfo = null;
                double lat = Configuration.Common.MissingCoordinateValue, lon = Configuration.Common.MissingCoordinateValue;

                if (d.Lat.HasValue && d.Lon.HasValue)
                {
                    lat = (double)d.Lat.Value;
                    lon = (double)d.Lon.Value;
                }
                else if (bestStation?.Station != null)
                {
                    var st = bestStation.Station;
                    lat = st.Lat.HasValue ? (double)st.Lat.Value : Configuration.Common.MissingCoordinateValue;
                    lon = st.Lon.HasValue ? (double)st.Lon.Value : Configuration.Common.MissingCoordinateValue;

                    stationInfo = new StationInfo
                    {
                        StationNum  = st.StationNum,
                        StationName = st.StationName ?? "",
                        StationCode = st.StationCode ?? "",
                        Area        = st.Area ?? "",
                        Latitude    = lat,
                        Longitude   = lon
                    };
                }
                //המרת כל יעד לפורמט המתאים לאלגוריתם
                return new OptimizerDestination
                {
                    DestinationId       = d.DesId,
                    Name                = d.NameDes ?? "",
                    Latitude            = lat,
                    Longitude           = lon,
                    OpeningTime         = d.OpeningTime,
                    ClosingTime         = d.ClosingTime,
                    VisitDuration       = visitHours,
                    WalkingTimeHours    = walkingHours,
                    CrowdFactor         = Configuration.Optimizer.DefaultCrowdFactor,
                    DynamicRequirements = dynReq,
                    SoftConstraints     = Configuration.Optimizer.DefaultSoftConstraints,
                    HardConstraints     = Configuration.Optimizer.DefaultHardConstraints,
                    AvgVisitCount       = avg,
                    StdDevVisitCount    = std,
                    NearestStation      = stationInfo
                };
            }).ToList();

            if (ctx.Destinations.Count == 0)
            {
                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(region)) parts.Add($"אזור={region.Trim()}");
                if (levelId.HasValue) parts.Add($"רמה={levelId}");
                if (tripCategoryIds.Count > 0) parts.Add($"קטגוריות=[{string.Join(",", tripCategoryIds)}]");
                if (tripFeatureIds.Count > 0) parts.Add($"מאפיינים=[{string.Join(",", tripFeatureIds)}]");
                var filterDesc = parts.Count > 0 ? string.Join(", ", parts) : "ללא מסננים";
                throw new Exception($"לא נמצאו יעדים התואמים לטיול ({filterDesc}). נסי לשנות אזור, רמת קושי, קטגוריות או מאפיינים.");
            }
            //מסגרת זמן הטיול
            double maxTimeFrame = (request.TripEndTime - request.TripStartTime).TotalHours;
            var (tripStart, tripEnd, scheduleNote) =
                TripScheduleDateHelper.ClampForGoogleTransit(request.TripStartTime, request.TripEndTime);
            ctx.ScheduleAdjustmentNote = scheduleNote;

            maxTimeFrame = (tripEnd - tripStart).TotalHours;

            double configuredMinTransitEfficiency = _config.GetValue(
                "Optimizer:MinTransitEfficiency",
                Configuration.Optimizer.DefaultMinTransitEfficiency);
            double requestedMinTransitEfficiency = request.MinTransitEfficiency;
            double minTransitEfficiency = Math.Clamp(
                requestedMinTransitEfficiency >= Configuration.Optimizer.MinTransitEfficiencyFloor
                    ? requestedMinTransitEfficiency
                    : configuredMinTransitEfficiency,
                Configuration.Optimizer.MinTransitEfficiencyFloor,
                Configuration.Optimizer.MinTransitEfficiencyCeiling);
            double minReturnHoursFallback = Math.Clamp(
                _config.GetValue("Optimizer:MinReturnHoursFallback", Configuration.Optimizer.DefaultMinReturnHoursFallback),
                Configuration.Optimizer.MinReturnHoursFallbackFloor,
                Configuration.Optimizer.MinReturnHoursFallbackCeiling);

            //אובייקט פרמטרים לאלגוריתם ראשי
            ctx.Params = new OptimizerParams
            {
                TripStartTime        = tripStart,
                TripEndTime          = tripEnd,
                MaxTravelTime        = request.MaxTravelTime / Configuration.Common.MinutesPerHour,
                MaxTimeFrame         = maxTimeFrame,
                ReturnTravelTime     = request.ReturnTravelTime / Configuration.Common.MinutesPerHour,
                MinReturnHoursFallback = minReturnHoursFallback,
                MinTransitEfficiency = minTransitEfficiency,
                MaxNumDes            = maxNumDes,
                AddressStart         = trip.AddressStart ?? ""
            };
            //--הדפסת לוגים---
            AgentDebugLog.Write("Step0_InputLoader.cs:136", "Input loaded for score table",
                new { ctx.Destinations.Count, maxTimeFrame, request.TripId, MinTransitEfficiency = minTransitEfficiency },
                "H4");

            var destNames = string.Join(", ", ctx.Destinations.Select(d => d.Name));
            OptimizerLog.Info(_logger, ctx,
                "נטענו {Count} יעדים: [{Names}] | אזור={Region} | חלון={Start:HH:mm}-{End:HH:mm} | יעילות מינ={MinEff} | התחלה={Address}",
                ctx.Destinations.Count, destNames, ctx.TripRegion,
                tripStart, tripEnd, minTransitEfficiency, trip.AddressStart ?? Configuration.Optimizer.EmptyAddressPlaceholder);
            if (scheduleNote != null)
                OptimizerLog.Info(_logger, ctx, "התאמת תאריך: {Note}", scheduleNote);
        }
    }
}

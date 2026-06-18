using API_trip_link.Data.Repositories;
using API_trip_link.Models;
using API_trip_link.Services.Optimizer;

namespace API_trip_link.Services.Optimizer.Steps
{
    internal class Step0_InputLoader : IOptimizerStep
    {
        private readonly IOptimizerDataRepository _data;

        public Step0_InputLoader(IOptimizerDataRepository data)
        {
            _data = data;
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

            var tripCategoryIds = trip.CategoriesToTrips?
                .Select(c => c.CategoriesId)
                .ToHashSet() ?? new HashSet<int>();

            var tripFeatureIds = trip.FeatureToTrips?
                .Select(f => f.FeatureId)
                .ToHashSet() ?? new HashSet<int>();

            //נותן רשימת יעדים מסוננים לפי אילוצים קשים
            var dbDestinations = await _data.GetDestinationsForOptimizationAsync(
                region, levelId, tripCategoryIds, tripFeatureIds);

            var mockStats = new Dictionary<int, (double avg, double std)>
            {
                { 1,  (350, 120) }, { 2,  (200,  40) }, { 3,  (150,  80) },
                { 4,  (180,  50) }, { 5,  (300,  60) }, { 6,  (100,  30) },
                { 7,  ( 80,  20) }, { 8,  (250,  70) }, { 9,  (400, 150) },
                { 10, (500, 200) },
            };
            //הכנת אובייקט יעד כולל פרטים לאלגוריתם הראשי
            ctx.Destinations = dbDestinations.Select(d =>
            {
                var bestStation = d.StationToDestinations?
                    .OrderBy(s => s.WalkingTime ?? TimeSpan.MaxValue)
                    .FirstOrDefault();

                double walkingHours = bestStation?.WalkingTime.HasValue == true
                    ? bestStation.WalkingTime!.Value.TotalHours
                    : 0.0;

                double visitHours = d.TimeDes.HasValue
                    ? d.TimeDes.Value.TotalHours
                    : 1.5;

                double baseTransitHours = 1.0;

                var (avg, std) = mockStats.TryGetValue(d.DesId, out var s) ? s : (100, 30);
                double dynReq  = WeightCalculator.ComputeDynamicRequirements(avg, std);

                StationInfo? stationInfo = null;
                double lat = 0, lon = 0;

                if (d.Lat.HasValue && d.Lon.HasValue)
                {
                    lat = (double)d.Lat.Value;
                    lon = (double)d.Lon.Value;
                }
                else if (bestStation?.Station != null)
                {
                    var st = bestStation.Station;
                    lat = st.Lat.HasValue ? (double)st.Lat.Value : 0;
                    lon = st.Lon.HasValue ? (double)st.Lon.Value : 0;

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
                    OpeningTime         = TimeSpan.FromHours(8),
                    ClosingTime         = TimeSpan.FromHours(20),
                    VisitDuration       = visitHours,
                    WalkingTimeHours    = walkingHours,
                    TransitTimeHours    = baseTransitHours,
                    CrowdFactor         = 0.3,
                    DynamicRequirements = dynReq,
                    SoftConstraints     = 0.8,
                    HardConstraints     = 1.0,
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

            double maxTimeFrame = (request.TripEndTime - request.TripStartTime).TotalHours;
            //הכנת אובייקט פרמטרים  לאלגוריתם הראשי (זמני נסיעה/חזרה מה-UI בדקות → שעות)
            ctx.Params = new OptimizerParams
            {
                TripStartTime        = request.TripStartTime,
                TripEndTime          = request.TripEndTime,
                MaxTravelTime        = request.MaxTravelTime / 60.0,
                MaxTimeFrame         = maxTimeFrame,
                ReturnTravelTime     = request.ReturnTravelTime / 60.0,
                MinTransitEfficiency = request.MinTransitEfficiency,
                MaxNumDes            = maxNumDes,
                AddressStart         = trip.AddressStart ?? ""
            };

            AgentDebugLog.Write("Step0_InputLoader.cs:136", "Input loaded for score table",
                new { ctx.Destinations.Count, maxTimeFrame, request.TripId, MinTransitEfficiency = request.MinTransitEfficiency },
                "H4");
        }
    }
}

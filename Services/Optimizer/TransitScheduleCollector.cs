using API_trip_link.Models;
using API_trip_link.Services.Transit;
using API_trip_link.Settings;

namespace API_trip_link.Services.Optimizer
{
    internal sealed class TransitScheduleCollector
    {
        private readonly ITransitApiService _transitApi;
        private readonly int _originDepartureWindowMinutes;
        private readonly int _noResultAdvanceMinutes;

        public TransitScheduleCollector(ITransitApiService transitApi, IConfiguration config)
        {
            _transitApi = transitApi;
            //טווח בדיקה מנקודת ההתחלה
            _originDepartureWindowMinutes = Math.Clamp(
                config.GetValue("Optimizer:OriginDepartureWindowMinutes", Configuration.Optimizer.DefaultOriginDepartureWindowMinutes),
                Configuration.Optimizer.MinOriginDepartureWindowMinutes,
                Configuration.Optimizer.MaxOriginDepartureWindowMinutes);
            //קפיצה טווח קדימה במקרה ואין מסלול
            _noResultAdvanceMinutes = Math.Clamp(
                config.GetValue("Optimizer:EventNoResultAdvanceMinutes", Configuration.Optimizer.DefaultEventNoResultAdvanceMinutes),
                Configuration.Optimizer.MinEventNoResultAdvanceMinutes,
                Configuration.Optimizer.MaxEventNoResultAdvanceMinutes);
        }
        //ניהול קריאות ה API וחישוב זמני נסיעה בין שני יעדים
        public async Task<TransitScheduleCollectResult> CollectArcAsync(
            TransitLocation from,
            TransitLocation to,
            OptimizerDestination? fromDest,
            OptimizerDestination toDest,
            OptimizerParams tripParams,
            int fromIndex,
            int toIndex,
            string fromLabel,
            Action<ScoreTableCellTraceDto>? onApiQuery = null,
            Action<ScoreTableCellTraceDto>? onCell = null,
            OptimizerRejectionTracker? rejections = null)
        {
            //חלון הזמן
            var tripStart = tripParams.TripStartTime;
            var tripEnd = tripParams.TripEndTime;
            int minuteCount = ScoreTable.ComputeMinuteCount(tripStart, tripEnd);
            var midTime = tripStart.AddMinutes((tripEnd - tripStart).TotalMinutes / 2);
            //קביעת חלון זמן סריקה
            var scanEnd = fromIndex == Configuration.Common.OriginNodeIndex
                ? Min(tripEnd, tripStart.AddMinutes(_originDepartureWindowMinutes))
                : tripEnd;
            
            var eventsByMinute = new Dictionary<int, TransitEvent>();
            DateTime queryTime = tripStart;
            double? carBaseline = null;
            //לולאת סריקה בחלון הזמן
            while (queryTime <= scanEnd)
            {
                //קריאה לקבלת מסלולי נסיעה מגוגל
                int httpBefore = _transitApi.HttpRequestCount;
                var batch = await _transitApi.GetDepartureOptionsAsync(from, to, queryTime);
                var maxReturnedDeparture = GetMaxReturnedDeparture(batch, tripStart, tripStart, scanEnd);
                //רישום למעקב ודיבוג - לוג
                ReportApiQuery(
                    onApiQuery, fromIndex, toIndex, fromLabel, toDest.Name,
                    queryTime, tripStart, minuteCount, "תחבורה",
                    fromCache: _transitApi.HttpRequestCount == httpBefore,
                    isValid: batch.HasAnyRoute,
                    optionCount: batch.Options.Count,
                    carBaseline: carBaseline ?? 0);
                //אם לא התקבל מידע פממשיכים לחפש מהדקה הבאה
                if (!batch.HasAnyRoute)
                {
                    queryTime = queryTime.AddMinutes(_noResultAdvanceMinutes);
                    continue;
                }
                //קריאת API חישוב נסיעה ברכב
                if (carBaseline == null)
                {
                    httpBefore = _transitApi.HttpRequestCount;
                    carBaseline = await _transitApi.GetDrivingDurationHoursAsync(from, to, midTime) ?? 0;
                    ReportApiQuery(
                        onApiQuery, fromIndex, toIndex, fromLabel, toDest.Name,
                        midTime, tripStart, minuteCount, "רכב",
                        fromCache: _transitApi.HttpRequestCount == httpBefore,
                        isValid: carBaseline > 0,
                        optionCount: 0,
                        carBaseline: carBaseline.Value);
                }
                //פעולה האחראית אחסון הנסיעות שהתקבלו
                var lastStoredDeparture = IngestDepartureBatch(
                    batch, tripStart, tripEnd, minuteCount, carBaseline.Value,
                    fromDest, toDest, tripParams, fromIndex, toIndex, fromLabel,
                    eventsByMinute, onCell, rejections,
                    originDepartureCutoff: fromIndex == Configuration.Common.OriginNodeIndex
                        ? tripStart.AddMinutes(_originDepartureWindowMinutes)
                        : null);
                //חיפוש הבא החל מהזמן האחרון שהתקבל+ דקה
                var nextBoundary = lastStoredDeparture ?? maxReturnedDeparture;
                var nextQueryTime = nextBoundary == null
                    ? queryTime.AddMinutes(_noResultAdvanceMinutes)
                    : nextBoundary.Value.AddMinutes(Configuration.Optimizer.DepartureScanStepMinutes);

                if (nextQueryTime <= queryTime)
                    nextQueryTime = queryTime.AddMinutes(_noResultAdvanceMinutes);

                queryTime = nextQueryTime;
            }
            //החזרת התוצאות כולל האירועים- זמני הנסיעות שנקלטו
            return new TransitScheduleCollectResult
            {
                Events = eventsByMinute.Values.OrderBy(e => e.DepartureTime).ToList(),
                CarBaselineHours = carBaseline ?? 0
            };
        }
        //פעולה שבונה אובייקט למעקב בדיבוג על קריאות ה-API
        private static void ReportApiQuery(
            Action<ScoreTableCellTraceDto>? onApiQuery,
            int fromIndex,
            int toIndex,
            string fromLabel,
            string toLabel,
            DateTime queryTime,
            DateTime tripStart,
            int minuteCount,
            string apiKind,
            bool fromCache,
            bool isValid,
            int optionCount,
            double carBaseline)
        {
            onApiQuery?.Invoke(new ScoreTableCellTraceDto
            {
                I = fromIndex,
                J = toIndex,
                H = ScoreTable.TimeToMinuteIndexStatic(queryTime, tripStart, minuteCount),
                FromLabel = fromLabel,
                ToLabel = toLabel,
                DepartureTime = queryTime.ToString("HH:mm"),
                ApiKind = apiKind,
                FromCache = fromCache,
                IsValid = isValid,
                TransitionScore = optionCount,
                BusTransitHours = 0,
                WalkingHours = 0,
                TransitEfficiency = Math.Round(carBaseline, 2),
                HasDirectBus = false
            });
        }
        //פעולה האחראית על אחסון הנסיעות שהתקבלו
        private static DateTime? IngestDepartureBatch(
            TransitDepartureBatch batch,
            DateTime tripStart,
            DateTime tripEnd,
            int minuteCount,
            double carBaseline,
            OptimizerDestination? fromDest,
            OptimizerDestination toDest,
            OptimizerParams tripParams,
            int fromIndex,
            int toIndex,
            string fromLabel,
            Dictionary<int, TransitEvent> eventsByMinute,
            Action<ScoreTableCellTraceDto>? onCell,
            OptimizerRejectionTracker? rejections,
            DateTime? originDepartureCutoff = null)
        {
            DateTime? lastDeparture = null;

            foreach (var option in batch.Options)
            {
                //בדיקת תקינות התוצאה
                var departure = NormalizeDepartureToTripDate(option.DepartureTime, tripStart);
                if (!IsWithinTripDepartureWindow(departure, tripStart, tripEnd)) continue;
                if (originDepartureCutoff != null && departure > originDepartureCutoff.Value) continue;

                int minuteIndex = ScoreTable.TimeToMinuteIndexStatic(departure, tripStart, minuteCount);
                //פעולה האחראית על בניית אובייקט מסוג ArcTransitionRecord שמכיל את כל המידע על הנסיעה
                var record = BuildRecord(option, carBaseline, fromDest, toDest, tripParams, departure, fromLabel, rejections);
                var transitEvent = new TransitEvent
                {
                    DepartureTime = departure,
                    ArrivalTime = NormalizeDepartureToTripDate(option.ArrivalTime, tripStart),
                    Duration = TimeSpan.FromHours(option.DurationHours),
                    Record = record
                };
                //רישום למילון דיבוג
                if (!TryUpsert(eventsByMinute, minuteIndex, transitEvent))
                    continue;
                //פונקציה המכניסה 
                onCell?.Invoke(new ScoreTableCellTraceDto
                {
                    I = fromIndex,
                    J = toIndex,
                    H = minuteIndex,
                    FromLabel = fromLabel,
                    ToLabel = toDest.Name,
                    DepartureTime = departure.ToString("HH:mm"),
                    ApiKind = record.IsValid ? "אירוע תקף" : "אירוע שנפסל",
                    IsValid = record.IsValid,
                    TransitionScore = Math.Round(record.TransitionScore, 3),
                    BusTransitHours = Math.Round(record.ArcCost.BusTransitHours, 2),
                    WalkingHours = Math.Round(record.ArcCost.WalkingHours, 2),
                    TransitEfficiency = Math.Round(record.ArcCost.TransitEfficiency, 2),
                    HasDirectBus = record.ArcCost.HasDirectBus
                });
                //דקת היציאה המאוחרת להמשך ריצת הלולאה הראשית
                if (lastDeparture == null || departure > lastDeparture)
                    lastDeparture = departure;
            }

            return lastDeparture;
        }
        //המרת אפשרות נסיעה לאובייקט מסוג ArcTransitionRecord 
        private static ArcTransitionRecord BuildRecord(
            TransitDepartureOption option,
            double carBaselineHours,
            OptimizerDestination? fromDest,
            OptimizerDestination toDest,
            OptimizerParams tripParams,
            DateTime departureTime,
            string fromLabel,
            OptimizerRejectionTracker? rejections)
        {
            double walkingHours = toDest.WalkingTimeHours;
            double directHours = option.DurationHours + walkingHours;
            double efficiency = WeightCalculator.CalculateTransitEfficiency(directHours, carBaselineHours);

            var arc = new ArcCost
            {
                FromDestinationId = fromDest?.DestinationId ?? Configuration.Common.OriginDestinationId,
                ToDestinationId = toDest.DestinationId,
                BestDepartureTime = departureTime,
                BusTransitHours = option.DurationHours,
                CarTransitHours = carBaselineHours,
                WalkingHours = walkingHours,
                TransitEfficiency = efficiency,
                HasDirectBus = option.HasDirectBus,
                TransitSteps = MapTransitSteps(option.TransitSteps)
            };

            var (optimality, rejection) = WeightCalculator.EvaluateDestinationOptimality(
                toDest, tripParams, departureTime,
                directTravelTime: directHours,
                indirectTravelTime: carBaselineHours,
                routeMatchesTraveler: option.HasDirectBus,
                hasDeadEnd: false);

            if (rejection != null)
                rejections?.Record(fromLabel, toDest.Name, departureTime, rejection);
            //החזרת האובייקט מכיל קשת ציון ותקינות
            return new ArcTransitionRecord
            {
                ArcCost = arc,
                TransitionScore = optimality >= 0 ? optimality : 0,
                IsValid = optimality >= 0
            };
        }
        // לכל תא נשמר ציון אחד הגבוה ביותר
        private static bool TryUpsert(
            Dictionary<int, TransitEvent> eventsByMinute,
            int minuteIndex,
            TransitEvent incoming)
        {
            if (!eventsByMinute.TryGetValue(minuteIndex, out var existing))
            {
                eventsByMinute[minuteIndex] = incoming;
                return true;
            }

            if (incoming.Record.TransitionScore > existing.Record.TransitionScore)
            {
                eventsByMinute[minuteIndex] = incoming;
                return true;
            }

            if (Math.Abs(incoming.Record.TransitionScore - existing.Record.TransitionScore) < Configuration.Optimizer.TransitionScoreTieEpsilon
                && incoming.Record.ArcCost.HasDirectBus
                && !existing.Record.ArcCost.HasDirectBus)
            {
                eventsByMinute[minuteIndex] = incoming;
                return true;
            }

            return false;
        }
        //דקת היציאה המאוחרת ביותר
        private static DateTime? GetMaxReturnedDeparture(
            TransitDepartureBatch batch,
            DateTime tripStart,
            DateTime tripStartWindow,
            DateTime tripEnd)
        {
            DateTime? max = null;
            foreach (var option in batch.Options)
            {
                var departure = NormalizeDepartureToTripDate(option.DepartureTime, tripStart);
                if (!IsWithinTripDepartureWindow(departure, tripStartWindow, tripEnd)) continue;
                if (max == null || departure > max.Value)
                    max = departure;
            }

            return max;
        }
        //תקינות תאריך מקריאת ה API
        private static DateTime NormalizeDepartureToTripDate(DateTime departure, DateTime tripStart)
        {
            if (departure.Date == tripStart.Date) return departure;
            return tripStart.Date.Add(departure.TimeOfDay);
        }
        //תקין בחלון הזמן
        private static bool IsWithinTripDepartureWindow(DateTime departure, DateTime tripStart, DateTime tripEnd)
        {
            var time = departure.TimeOfDay;
            return time >= tripStart.TimeOfDay && time <= tripEnd.TimeOfDay;
        }

        private static DateTime Min(DateTime a, DateTime b) => a <= b ? a : b;
        //פעולה הממירה את רשימת הצעדים של תחבורה ציבורית לאובייקט מסוג TransitLegStep
        private static List<TransitLegStep> MapTransitSteps(List<GoogleTransitStep> steps)
            => steps.Select(s => new TransitLegStep
            {
                LineName = s.LineName,
                VehicleType = s.VehicleType,
                FromStation = s.FromStation,
                ToStation = s.ToStation,
                DepartureTime = s.DepartureTime,
                ArrivalTime = s.ArrivalTime,
                DurationHours = s.DurationHours
            }).ToList();
    }
    //מחלקה הכוללת את האירועים שנאספו ואת זמן הנסיעה ברכב
    internal sealed class TransitScheduleCollectResult
    {
        public List<TransitEvent> Events { get; set; } = new();
        public double CarBaselineHours { get; set; }
    }
}

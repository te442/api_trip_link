using API_trip_link.Models;
using API_trip_link.Services.Optimizer.Instrumentation;
namespace API_trip_link.Services.Optimizer
{
    internal class RouteBuilder
    {
        private readonly OptimizerParams _tripParams;
        private readonly ArcCostCalculator _arcCostCalculator;
        public RouteBuilder(OptimizerParams tripParams, ArcCostCalculator arcCostCalculator)
        {
            _tripParams        = tripParams;
            _arcCostCalculator = arcCostCalculator;
        }

        //פעולה הבונה את המסלול ומשבצת את היעדים
        //מקבלת event store דליל עם אירועי תחבורה ורשימת יעדים מסוננים
        //מחזירה מסלול בנוי
        public OptimizerRoute Build(
            ScoreTable? scoreTable,
            List<OptimizerDestination> allDestinations,
            List<OptimizerDestination>? allowedDestinations = null,
            ILogger? logger = null,
            int? tripId = null,
            UsageScope usageScope = UsageScope.RouteBuilder_Initial)
        {
            using var instrumentationScope = scoreTable?.Instrumentation?.BeginScope(usageScope);
            //יצירת אובייקט מסוג מסלול
            var route = new OptimizerRoute { Destinations = new(), ArcCosts = new(), TotalScore = 0, TotalTime = 0, IsValid = false };
            var currentArrival = _tripParams.TripStartTime;
            //יצירת מערך של מספרים של יעדים שכבר נשבצו מסוג hashset למניעת כפילויות
            var placedIds  = new HashSet<int>();
            //אובייקט המכיל את היעדים שאפשריים בהתאם לסינון
            var pool = allowedDestinations ?? allDestinations;
            double totalEff = 0;
            int segCount = 0;
            int candidatesEvaluated = 0;
            int acceptedArcs = 0;

            OptimizerDestination? prevDest = null;

            //לולאת שיבוץ יעדים בטיול
            while (true)
            {
                //כמות היעדים גדולה מכמות היעדים שהוגדרה בפרמטרים של הטיול
                if (_tripParams.MaxNumDes.HasValue && route.Destinations.Count >= _tripParams.MaxNumDes.Value)
                {
                    logger?.LogInformation(
                        "[Optimizer] tripId={TripId} RouteBuilder: הגעה למקסימום יעדים ({Max})",
                        tripId, _tripParams.MaxNumDes.Value);
                    break;
                }
                //יעדים שעדיין לא במסלול מתוך המאגר
                var candidates = pool.Where(d => !placedIds.Contains(d.DestinationId)).ToList();
                candidatesEvaluated += candidates.Count;
                if (candidates.Count == 0)
                {
                    logger?.LogInformation("[Optimizer] tripId={TripId} RouteBuilder: אין עוד מועמדים", tripId);
                    break;
                }

                OptimizerDestination best;
                ArcCost arc;
                if (scoreTable != null)
                {
                    //חישוב אינדקס המקור (נקודת התחלה או יעד קודם)
                    int fromIndex = prevDest == null
                        ? scoreTable.OriginIndex
                        : scoreTable.DestIdToIndex(prevDest.DestinationId);
                    //חישוב שכבת השעה הנוכחית בטבלה
                    int minuteIndex = scoreTable.TimeToMinuteIndex(currentArrival);
                    var feasibleCandidates = candidates.Where(candidate =>
                    {
                        int toIndex = scoreTable.DestIdToIndex(candidate.DestinationId);
                        var nextArc = toIndex >= 1
                            ? scoreTable.GetNextAvailable(fromIndex, toIndex, currentArrival)?.ArcCost
                            : null;
                        return nextArc != null &&
                               CanExtendRoute(candidate, nextArc, currentArrival, scoreTable, logger, tripId);
                    }).ToList();

                    if (feasibleCandidates.Count == 0)
                    {
                        var fromLabel = prevDest?.Name ?? _tripParams.AddressStart ?? "מקור";
                        logger?.LogInformation(
                            "[Optimizer] tripId={TripId} RouteBuilder: אין מועמדים שמשאירים חסם חזרה תקף מ-{From} בשעה {Time:HH:mm}",
                            tripId, fromLabel, currentArrival);
                        break;
                    }
                    //הפעלת הפונקציה לקבלת היעד בעמודה הטוב 
                    var pick = scoreTable.GetBestInColumn(fromIndex, minuteIndex, feasibleCandidates);
                    if (pick == null)
                    {
                        var fromLabel = prevDest?.Name ?? _tripParams.AddressStart ?? "מקור";
                        logger?.LogInformation(
                            "[Optimizer] tripId={TripId} RouteBuilder: אין תא תקף בטבלה מ-{From} (i={I}) בדקה {M} ({Time:HH:mm}) למועמדים: [{Candidates}]",
                            tripId, fromLabel, fromIndex, minuteIndex, currentArrival,
                            string.Join(", ", candidates.Select(c => c.Name)));
                        break;
                    }
                    //שמירת היעד הטוב ביותר ועלות הקשת מהטבלה
                    best = pick.Value.Dest;
                    arc  = pick.Value.Arc;
                    best.OptimalityScore = pick.Value.Score;
                }
                else
                {
                    best = candidates[0];
                    arc  = _arcCostCalculator.Compute(prevDest, best, currentArrival);
                    if (!CanExtendRoute(best, arc, currentArrival, scoreTable, logger, tripId))
                        break;
                }
                //מניעת כפילויות ביעדים במסלול
                if (placedIds.Contains(best.DestinationId))
                    continue;

                var readyAt = currentArrival;
                double waitHours = arc.BestDepartureTime > readyAt
                    ? (arc.BestDepartureTime - readyAt).TotalHours
                    : 0;
                double travelAndStay = arc.BusTransitHours + arc.WalkingHours + best.VisitDuration;
                double stopTime = waitHours + travelAndStay;
                double newTotal = route.TotalTime + stopTime;
                totalEff += arc.TransitEfficiency;
                segCount++;
                //הוספת היעד למסלול
                route.Destinations.Add(best);
                route.ArcCosts.Add(arc);
                acceptedArcs++;
                route.TotalTime   = newTotal;
                route.TotalScore += best.OptimalityScore;
                placedIds.Add(best.DestinationId);
                if (scoreTable != null)
                {
                    int selectedFromIndex = prevDest == null
                        ? scoreTable.OriginIndex
                        : scoreTable.DestIdToIndex(prevDest.DestinationId);
                    int selectedToIndex = scoreTable.DestIdToIndex(best.DestinationId);
                    int selectedMinuteIndex = scoreTable.TimeToMinuteIndex(arc.BestDepartureTime);
                    scoreTable.Instrumentation?.RecordSelectedArc(selectedFromIndex, selectedToIndex, selectedMinuteIndex);
                }
                logger?.LogInformation(
                    "[Optimizer] tripId={TripId} RouteBuilder: שובץ יעד #{Order} {Dest} (ציון={Score:F2}, יציאה={Dep:HH:mm}, נסיעה={Bus:F1}ש)",
                    tripId, route.Destinations.Count, best.Name, best.OptimalityScore,
                    arc.BestDepartureTime, arc.BusTransitHours);
                //קידום זמן המסלול משעת היציאה שנבחרה בטבלה + נסיעה + שהייה
                currentArrival = arc.BestDepartureTime.AddHours(travelAndStay);
                prevDest       = best;
            }
            route.TransitEfficiency = segCount > 0 ? totalEff / segCount : 0;
            route.IsValid           = route.Destinations.Count > 0 && route.TotalTime > 0;
            scoreTable?.Instrumentation?.RecordRouteBuild(route.TotalScore, candidatesEvaluated, acceptedArcs);
            logger?.LogInformation(
                "[Optimizer] tripId={TripId} RouteBuilder: סיום — {Count} יעדים, ציון={Score:F2}, זמן={Time:F1}ש, תקף={Valid}",
                tripId, route.Destinations.Count, route.TotalScore, route.TotalTime, route.IsValid);
            return route;
        }
        //פונקציה בודקת האם ניתן להוסיף יעד למסלול בחלון הזמן
        private bool CanExtendRoute(
            OptimizerDestination candidate,
            ArcCost arc,
            DateTime readyAt,
            ScoreTable? scoreTable,
            ILogger? logger,
            int? tripId)
        {
            var departure = arc.BestDepartureTime > readyAt ? arc.BestDepartureTime : readyAt;
            var arrivalAtCandidate = departure.AddHours(arc.BusTransitHours + arc.WalkingHours);
            var leaveCandidateTime = arrivalAtCandidate.AddHours(candidate.VisitDuration);
            var minReturn = scoreTable?.GetFastestReturnTime(candidate.DestinationId)
                ?? _tripParams.MinReturnHoursFallback;

            if (leaveCandidateTime.AddHours(minReturn) <= _tripParams.TripEndTime)
                return true;

            logger?.LogInformation(
                "[Optimizer] tripId={TripId} RouteBuilder: יעד {Dest} נפסל לפני הוספה — חסם חזרה " +
                "(עזיבה {Leave:HH:mm} + מינימום חזרה {Return:F1}ש > סוף {End:HH:mm})",
                tripId, candidate.Name, leaveCandidateTime, minReturn, _tripParams.TripEndTime);
            return false;
        }
    }
}



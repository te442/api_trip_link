using API_trip_link.Models;
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
        //מקבלת טבלה תלת מימדית עם כל הציונים, רשימת כל היעדים המסוננים
        //מחזירה מסלול בנוי
        public OptimizerRoute Build(
            ScoreTable? scoreTable,
            List<OptimizerDestination> allDestinations,
            List<OptimizerDestination>? allowedDestinations = null)
        {
            //יצירת אובייקט מסוג מסלול
            var route = new OptimizerRoute { Destinations = new(), ArcCosts = new(), TotalScore = 0, TotalTime = 0, IsValid = false };
            var currentArrival = _tripParams.TripStartTime;
            //יצירת מערך של מספרים של יעדים שכבר נשבצו מסוג hashset למניעת כפילויות
            var placedIds  = new HashSet<int>();
            //אובייקט המכיל את היעדים שאפשריים בהתאם לסינון
            var pool = allowedDestinations ?? allDestinations;
            double totalEff = 0;
            int segCount = 0;

            OptimizerDestination? prevDest = null;

            //לולאת שיבוץ יעדים בטיול
            while (true)
            {
                //כמות היעדים גדולה מכמות היעדים שהוגדרה בפרמטרים של הטיול
                if (_tripParams.MaxNumDes.HasValue && route.Destinations.Count >= _tripParams.MaxNumDes.Value)
                    break;
                //יעדים שעדיין לא במסלול מתוך המאגר
                var candidates = pool.Where(d => !placedIds.Contains(d.DestinationId)).ToList();
                if (candidates.Count == 0)
                    break;

                OptimizerDestination best;
                ArcCost arc;
                if (scoreTable != null)
                {
                    //חישוב אינדקס המקור (נקודת התחלה או יעד קודם)
                    int fromIndex = prevDest == null
                        ? scoreTable.OriginIndex
                        : scoreTable.DestIdToIndex(prevDest.DestinationId);
                    //חישוב שכבת השעה הנוכחית בטבלה
                    int hourIndex = scoreTable.TimeToHourIndex(currentArrival);
                    //קבלת היעד הטוב ביותר בעמודה שבטבלה בתנאי שעדיין אינו במסלול
                    var pick = scoreTable.GetBestInColumn(fromIndex, hourIndex, candidates);
                    if (pick == null) break;
                    //שמירת היעד הטוב ביותר ועלות הקשת מהטבלה
                    best = pick.Value.Dest;
                    arc  = pick.Value.Arc;
                    best.OptimalityScore = pick.Value.Score;
                }
                else
                {
                    best = candidates[0];
                    arc  = _arcCostCalculator.Compute(prevDest, best, currentArrival);
                }
                //מניעת כפילויות ביעדים במסלול
                if (placedIds.Contains(best.DestinationId))
                    continue;
                double stopTime = arc.BusTransitHours + arc.WalkingHours + best.VisitDuration;
                double newTotal = route.TotalTime + stopTime;
                //בדיקת חלון הזמן
                if (newTotal + _tripParams.ReturnTravelTime > _tripParams.MaxTimeFrame) break;
                totalEff += arc.TransitEfficiency;
                segCount++;
                //הוספת היעד למסלול
                route.Destinations.Add(best);
                route.ArcCosts.Add(arc);
                route.TotalTime   = newTotal;
                route.TotalScore += best.OptimalityScore;
                placedIds.Add(best.DestinationId);
                //קידום זמן המסלול משעת היציאה העדיפה + נסיעה + שהייה
                currentArrival = arc.BestDepartureTime.AddHours(stopTime);
                prevDest       = best;
            }
            route.TransitEfficiency = segCount > 0 ? totalEff / segCount : 0;
            route.IsValid           = route.Destinations.Count > 0 && route.TotalTime > 0;
            return route;
        }
    }
}



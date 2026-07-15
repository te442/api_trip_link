using API_trip_link.Settings;
using API_trip_link.Models;

namespace API_trip_link.Services.Optimizer
{
    internal static class WeightCalculator//פונקציה המחשבת אופטמליות ליעד
    {
        public static double ComputeDynamicRequirements(double avg, double stdDev) { 
            if (avg <= 0) return Configuration.Optimizer.DefaultDynamicRequirementsWhenNoData;
            double cv = stdDev / avg;
            return Math.Max(Configuration.Common.ScoreMin, Math.Min(Configuration.Common.ScoreMax, cv));
        }
        //מחזירה את הציון האופטימלי של היעד
        public static double CalculateDestinationOptimality(
            OptimizerDestination dest,
            OptimizerParams tripParams,
            DateTime arrivalTime,
            double directTravelTime,
            double indirectTravelTime,
            bool routeMatchesTraveler,
            bool hasDeadEnd)
            => EvaluateDestinationOptimality(
                dest, tripParams, arrivalTime,
                directTravelTime, indirectTravelTime,
                routeMatchesTraveler, hasDeadEnd).Score;

        /// <summary>מחזיר ציון וסיבת פסילה אם יש).</summary>
        public static (double Score, string? RejectionReason) EvaluateDestinationOptimality(
            OptimizerDestination dest,
            OptimizerParams tripParams,
            DateTime arrivalTime,
            double directTravelTime,
            double indirectTravelTime,
            bool routeMatchesTraveler,
            bool hasDeadEnd)
        {
            if (dest.HardConstraints < Configuration.Optimizer.HardConstraintThreshold)
                return (Configuration.Optimizer.InvalidOptimalityScore, "אילוץ קשה: HardConstraints < 1");

            double transitEfficiency = CalculateTransitEfficiency(directTravelTime, indirectTravelTime);

            if (directTravelTime <= 0)
                return (Configuration.Optimizer.InvalidOptimalityScore, $"זמן נסיעה לא תקין: directTravelTime={directTravelTime:F2}ש");

            if (transitEfficiency < tripParams.MinTransitEfficiency)
                return (Configuration.Optimizer.InvalidOptimalityScore,
                    $"יעילות תחבורה נמוכה: {transitEfficiency:F2} < סף {tripParams.MinTransitEfficiency:F2} " +
                    $"(אוטובוס+הליכה={directTravelTime:F2}ש, רכב={indirectTravelTime:F2}ש)");

            var timeReason = ExplainTimeWindowRejection(tripParams, arrivalTime, directTravelTime, dest);
            if (timeReason != null)
                return (Configuration.Optimizer.InvalidOptimalityScore, timeReason);

            double crowdScore   = Configuration.Optimizer.HardConstraintThreshold - dest.CrowdFactor;
            double dynamicScore = Configuration.Optimizer.HardConstraintThreshold - dest.DynamicRequirements;
            double softScore    = dest.SoftConstraints;

            double transitBonus = routeMatchesTraveler && !hasDeadEnd ? Configuration.Optimizer.TransitBonusDirectRoute
                                : routeMatchesTraveler ? Configuration.Optimizer.TransitBonusPartialMatch : Configuration.Optimizer.TransitBonusNone;
            double transitScore = Normalize(transitEfficiency + transitBonus);

            double rawScore = softScore    * Configuration.Optimizer.WeightSoftConstraints
                            + crowdScore   * Configuration.Optimizer.WeightCrowd
                            + dynamicScore * Configuration.Optimizer.WeightDynamic
                            + transitScore * Configuration.Optimizer.WeightTransit;

            return (Normalize(rawScore), null);
        }

        public static string? ExplainTimeWindowRejection(
            OptimizerParams tripParams,
            DateTime arrivalTime,
            double travelHours,
            OptimizerDestination dest)
        {
            var estimated    = arrivalTime.AddHours(travelHours);
            var arrivalOfDay = estimated.TimeOfDay;

            if (arrivalOfDay < dest.OpeningTime)
                return $"הגעה מוקדמת: {arrivalOfDay:hh\\:mm} לפני פתיחה {dest.OpeningTime:hh\\:mm} ({dest.Name})";

            if (arrivalOfDay >= dest.ClosingTime)
                return $"הגעה אחרי סגירה: {arrivalOfDay:hh\\:mm} ≥ {dest.ClosingTime:hh\\:mm} ({dest.Name})";

            var visitEnd  = estimated.AddHours(dest.VisitDuration);
            var returnEnd = visitEnd.AddHours(tripParams.ReturnTravelTime);

            if (returnEnd.TimeOfDay > tripParams.TripEndTime.TimeOfDay
                || returnEnd.Date > tripParams.TripEndTime.Date)
                return $"חלון זמן: חזרה משוערת {returnEnd:HH:mm} (שהייה {dest.VisitDuration:F1}ש + חזרה {tripParams.ReturnTravelTime:F1}ש) " +
                       $"אחרי סוף טיול {tripParams.TripEndTime:HH:mm} ({dest.Name})";

            return null;
        }
        //בדיקת חלון הזמן
        public static bool CheckTimeWindow(
            OptimizerParams tripParams,
            DateTime arrivalTime,
            double travelHours,
            OptimizerDestination dest)
            => ExplainTimeWindowRejection(tripParams, arrivalTime, travelHours, dest) == null;
        //יעילות תח"צ
        public static double CalculateTransitEfficiency(double publicTime, double carTime)
        {
            if (publicTime <= 0 || carTime <= 0) return 0;
            return Normalize(carTime / publicTime);
        }
        //פונקצית נירמול מחזירה ערך בין 0 ל 1
        public static double Normalize(double v) =>
            Math.Max(Configuration.Common.ScoreMin, Math.Min(Configuration.Common.ScoreMax, v));
    }
}

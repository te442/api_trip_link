using API_trip_link.Models;

namespace API_trip_link.Services.Optimizer
{
    internal static class WeightCalculator
    {
        public static double ComputeDynamicRequirements(double avg, double stdDev)
        {
            if (avg <= 0) return 0.5;
            double cv = stdDev / avg;
            return Math.Max(0.0, Math.Min(1.0, cv));
        }
        //פונקציה מחשבת את הציון האופטימלי של היעד
        //מקבלת את היעד, את הפרמטרים של הטיול, את הזמן של היציאה מהמקור, את הזמן של הנסיעה הישרה ואת הזמן של הנסיעה ברכב
        //מחזירה את הציון האופטימלי של היעד
        public static double CalculateDestinationOptimality(
            OptimizerDestination dest,
            OptimizerParams tripParams,
            DateTime arrivalTime,
            double directTravelTime,
            double indirectTravelTime,
            bool routeMatchesTraveler,
            bool hasDeadEnd)
        {
            // פסילה על אילוץ קשה
            if (dest.HardConstraints < 1.0) return -1.0;

            //חישוב יעילות תחבורה 
            double transitEfficiency = CalculateTransitEfficiency(directTravelTime, indirectTravelTime);
            if (directTravelTime <= 0 || transitEfficiency < tripParams.MinTransitEfficiency)
                return -1.0;

            // בדיקת חלון זמן: הגעה לפני סגירה + חזרה לפני סוף הטיול
            if (!CheckTimeWindow(tripParams, arrivalTime, directTravelTime, dest)) 
                return -1.0;

            //חישוב ציון אופטמלי 
            double crowdScore   = 1.0 - dest.CrowdFactor;
            double dynamicScore = 1.0 - dest.DynamicRequirements;
            double softScore    = dest.SoftConstraints;

            double transitBonus = routeMatchesTraveler && !hasDeadEnd ? 0.1
                                : routeMatchesTraveler ? 0.05 : 0.0;
            double transitScore = Normalize(transitEfficiency + transitBonus);
            //חישוב לפי אחוזים
            double rawScore = softScore    * 0.30
                            + crowdScore   * 0.10
                            + dynamicScore * 0.10
                            + transitScore * 0.50;

            return Normalize(rawScore);
        }

        public static bool CheckTimeWindow(
            OptimizerParams tripParams,
            DateTime arrivalTime,
            double travelHours,
            OptimizerDestination dest)
        {
            var estimated    = arrivalTime.AddHours(travelHours);
            var arrivalOfDay = estimated.TimeOfDay;

            bool beforeClose = arrivalOfDay < dest.ClosingTime;

            var visitEnd  = estimated.AddHours(dest.VisitDuration);
            var returnEnd = visitEnd.AddHours(tripParams.ReturnTravelTime);
            bool withinTrip = returnEnd <= tripParams.TripEndTime;

            return beforeClose && withinTrip;
        }

        public static double CalculateTransitEfficiency(double publicTime, double carTime)
        {
            if (publicTime <= 0 || carTime <= 0) return 0.5;
            return Normalize(carTime / publicTime);
        }
        //פונקצית נירמול מחזירה ערך בין 0 ל 1
        public static double Normalize(double v) => Math.Max(0.0, Math.Min(1.0, v));
    }
}

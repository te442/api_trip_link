using API_trip_link.Settings;

namespace API_trip_link.Services.Optimizer
{
    //מחלקה אחראית על תאריך ושעה תקינים למסלול בהתאם לדרישות גוגל מפס
    internal static class TripScheduleDateHelper
    {
        public static (DateTime Start, DateTime End, string? AdjustmentNote) ClampForGoogleTransit(
      DateTime tripStart,
             DateTime tripEnd)
        {
            var duration = tripEnd - tripStart;
            if (duration <= TimeSpan.Zero)
                throw new ArgumentException("זמן סיום הטיול חייב להיות מאוחר מזמן ההתחלה.");

            var today = DateTime.Now.Date;
            var minDate = today.AddDays(Configuration.Optimizer.GoogleTransitMinDaysAhead);
            var maxDate = today.AddDays(Configuration.Optimizer.GoogleTransitMaxDaysAhead);

            if (tripStart.Date < minDate)
                throw new ArgumentException(
                    $"תאריך הטיול מוקדם מדי. ניתן לבחור תאריך החל מ-{minDate:yyyy-MM-dd}.");

            if (tripStart.Date > maxDate)
                throw new ArgumentException(
                    $"תאריך הטיול רחוק מדי. ניתן לבחור תאריך עד {maxDate:yyyy-MM-dd}.");

            return (tripStart, tripEnd, null);
        
        }
    }
}

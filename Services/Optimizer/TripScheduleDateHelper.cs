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
                duration = TimeSpan.FromHours(Configuration.Optimizer.DefaultTripDurationHours);

            var today = DateTime.Now.Date;
            var minDate = today.AddDays(Configuration.Optimizer.GoogleTransitMinDaysAhead);
            var maxDate = today.AddDays(Configuration.Optimizer.GoogleTransitMaxDaysAhead);

            var original = $"{tripStart:yyyy-MM-dd HH:mm}–{tripEnd:HH:mm}";
            var targetDate = tripStart.Date;

            if (targetDate < minDate)
                targetDate = minDate;
            else if (targetDate > maxDate)
                targetDate = today.AddDays(Configuration.Optimizer.GoogleTransitFallbackDaysAhead);

            var newStart = targetDate.Add(tripStart.TimeOfDay);
            var newEnd   = newStart.Add(duration);

            if (newStart.Date == tripStart.Date && newEnd == tripEnd)
                return (tripStart, tripEnd, null);

            var note =
                $"תאריך הטיול הותאם ל-Google Transit: {original} → {newStart:yyyy-MM-dd HH:mm}–{newEnd:HH:mm} " +
                $"(לוחות אוטובוס זמינים רק {Configuration.Optimizer.GoogleTransitMinDaysAhead}–{Configuration.Optimizer.GoogleTransitMaxDaysAhead} ימים קדימה)";

            return (newStart, newEnd, note);
        }
    }
}

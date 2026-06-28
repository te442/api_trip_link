namespace API_trip_link.Services.Transit
{
    //אובייקט תוצאת נסיעה בתח"צ וברכב פרטי
    public class TransitQueryResult
    {
        public double BusTransitHours  { get; set; }
        public double CarTransitHours  { get; set; }
        public bool   HasDirectBus     { get; set; }
        public double WalkingHours     { get; set; }
        public List<GoogleTransitStep> TransitSteps { get; set; } = new();
    }

    public class TransitDepartureOption
    {
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime   { get; set; }
        public double   DurationHours { get; set; }
        public bool     HasDirectBus  { get; set; }
        public List<GoogleTransitStep> TransitSteps { get; set; } = new();
    }

    public class TransitDepartureBatch
    {
        public DateTime QueryTime { get; set; }
        public List<TransitDepartureOption> Options { get; set; } = new();
        public bool HasAnyRoute => Options.Count > 0;
    }
    //אובייקט נתוני אוטובוס למקטע
    public class GoogleTransitStep
    {
        public string LineName        { get; set; } = "";
        public string VehicleType     { get; set; } = "";
        public string FromStation     { get; set; } = "";
        public string ToStation       { get; set; } = "";
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime   { get; set; }
        public double DurationHours   { get; set; }
    }
    //אובייקט מיקום
    public class TransitLocation
    {
        public int    DestinationId { get; set; } = -1;
        public string Address       { get; set; } = "";
        public double Latitude      { get; set; }
        public double Longitude     { get; set; }

        public bool HasCoordinates => Latitude != 0 || Longitude != 0;
    }
    //ממשק להתממשקות לשירות API
    public interface ITransitApiService
    {
        /// <summary>מספר בקשות HTTP אמיתיות ל-Google (ללא cache hits).</summary>
        int HttpRequestCount { get; }

        void ResetHttpRequestCount();

        Task<TransitQueryResult> GetTransitTimeAsync(
            TransitLocation from,
            TransitLocation to,
            double walkingHours,
            DateTime departureTime);

        Task<TransitDepartureBatch> GetDepartureOptionsAsync(
            TransitLocation from,
            TransitLocation to,
            DateTime queryTime);

        Task<double?> GetDrivingDurationHoursAsync(
            TransitLocation from,
            TransitLocation to,
            DateTime departureTime);
    }
}

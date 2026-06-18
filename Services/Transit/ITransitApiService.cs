namespace API_trip_link.Services.Transit
{
    public class TransitQueryResult
    {
        public double BusTransitHours  { get; set; }
        public double CarTransitHours  { get; set; }
        public bool   HasDirectBus     { get; set; }
        public double WalkingHours     { get; set; }
        public List<GoogleTransitStep> TransitSteps { get; set; } = new();
    }

    public class GoogleTransitStep
    {
        public string LineName      { get; set; } = "";
        public string VehicleType   { get; set; } = "";
        public string FromStation   { get; set; } = "";
        public string ToStation     { get; set; } = "";
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime   { get; set; }
        public double DurationHours   { get; set; }
    }

    public class TransitLocation
    {
        public int    DestinationId { get; set; } = -1;
        public string Address       { get; set; } = "";
        public double Latitude      { get; set; }
        public double Longitude     { get; set; }

        public bool HasCoordinates => Latitude != 0 || Longitude != 0;
    }

    public interface ITransitApiService
    {
        Task<TransitQueryResult> GetTransitTimeAsync(
            TransitLocation from,
            TransitLocation to,
            double baseTransitHours,
            double walkingHours,
            DateTime departureTime);
    }
}

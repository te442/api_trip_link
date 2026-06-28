namespace API_trip_link.Models
{
    public class TripItineraryDto
    {
        public int      TripId              { get; set; }
        public string   TripName            { get; set; } = "";
        public string   AddressStart        { get; set; } = "";
        public int      DestinationCount    { get; set; }
        public double   TotalScore          { get; set; }
        public double   TimeUsed            { get; set; }
        public double   TimeAvailable       { get; set; }
        public double   TransitEfficiency   { get; set; }
        public string?  Narrative           { get; set; }
        public List<TripLegDto>    Legs      { get; set; } = new();
        public TripLegDto?         ReturnLeg { get; set; }
        public List<MapPointDto>   MapPoints { get; set; } = new();
    }

    public class TripLegDto
    {
        public int      Order            { get; set; }
        public int      DesId            { get; set; }
        public string   DestinationName  { get; set; } = "";
        public string   Region           { get; set; } = "";
        public string?  ImageUrl         { get; set; }
        public double?  Lat              { get; set; }
        public double?  Lon              { get; set; }
        public string   ArrivalTime      { get; set; } = "";
        public string   DepartureTime    { get; set; } = "";
        public string   StayDuration     { get; set; } = "";
        public TransitSegmentDto Transit { get; set; } = new();
    }

    public class TransitSegmentDto
    {
        public string  FromLabel          { get; set; } = "";
        public string? BoardingStation    { get; set; }
        public string? AlightingStation   { get; set; }
        public double  WalkingMinutes     { get; set; }
        public string  DepartureTime      { get; set; } = "";
        public string  ArrivalTime        { get; set; } = "";
        public double  TransitEfficiency  { get; set; }
        public List<BusLineDto> BusLines  { get; set; } = new();
    }

    public class BusLineDto
    {
        public string BusNumber     { get; set; } = "";
        public string Direction     { get; set; } = "";
        public string VehicleType   { get; set; } = "";
        public string FromStation   { get; set; } = "";
        public string ToStation     { get; set; } = "";
        public string DepartureTime { get; set; } = "";
        public string ArrivalTime   { get; set; } = "";
    }

    public class MapPointDto
    {
        public int     Order  { get; set; }
        public string  Label  { get; set; } = "";
        public double  Lat    { get; set; }
        public double  Lon    { get; set; }
    }
}

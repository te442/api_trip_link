namespace API_trip_link.Models
{
    internal class TripPlan
    {
        public string   TripName           { get; set; } = "";
        public string   AddressStart       { get; set; } = "";
        public DateTime TripStartTime      { get; set; }
        public DateTime TripEndTime        { get; set; }
        public double   TotalScore         { get; set; }
        public double   TransitEfficiency  { get; set; }
        public List<AvailableDestination> AvailableDestinations { get; set; } = new();
        public List<TripLeg> Legs          { get; set; } = new();
        public string   Narrative          { get; set; } = "";
    }

    internal class AvailableDestination
    {
        public int     DesId        { get; set; }
        public string  NameDes      { get; set; } = "";
        public string  Region       { get; set; } = "";
        public string  LevelType    { get; set; } = "";
        public double  VisitHours   { get; set; }
        public StationInfo? NearestStation { get; set; }
        public bool    WasSelected  { get; set; }
    }

    internal class TripLeg
    {
        public int     Order              { get; set; }
        public int     DesId              { get; set; }
        public string  DestinationName    { get; set; } = "";
        public string  Region             { get; set; } = "";
        public DateTime ArrivalTime       { get; set; }
        public DateTime DepartureTime     { get; set; }
        public TimeSpan StayDuration      { get; set; }
        public TransitSegment Transit     { get; set; } = new();
    }

    internal class TransitSegment
    {
        public string  FromLabel           { get; set; } = "";
        public StationInfo? BoardingStation  { get; set; }
        public StationInfo? AlightingStation { get; set; }
        public double  WalkingMinutes      { get; set; }
        public List<BusLineInfo> BusLines  { get; set; } = new();
        public DateTime DepartureTime      { get; set; }
        public DateTime ArrivalTime        { get; set; }
        public double  TransitEfficiency   { get; set; }
    }

    internal class BusLineInfo
    {
        public int    BusId         { get; set; }
        public string BusNumber     { get; set; } = "";
        public string Direction     { get; set; } = "";
        public string FromStation   { get; set; } = "";
        public string ToStation     { get; set; } = "";
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime   { get; set; }
    }
}

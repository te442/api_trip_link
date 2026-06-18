namespace API_trip_link.Models
{
    internal class OptimizerParams
    {
        public DateTime TripStartTime        { get; set; }
        public DateTime TripEndTime          { get; set; }
        public double   MaxTravelTime        { get; set; }
        public double   MaxTimeFrame         { get; set; }
        public double   ReturnTravelTime     { get; set; }
        public double   MinTransitEfficiency { get; set; } = 0.5;
        public int?     MaxNumDes            { get; set; }
        public string   AddressStart         { get; set; } = "";
        public double   StartLatitude        { get; set; }
        public double   StartLongitude       { get; set; }
    }
}


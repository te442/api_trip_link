namespace API_trip_link.Models
{
    internal class OptimizerDestination
    {
        public int      DestinationId       { get; set; }
        public string   Name                { get; set; } = "";
        public double   Latitude            { get; set; }
        public double   Longitude           { get; set; }
        public TimeSpan OpeningTime         { get; set; }
        public TimeSpan ClosingTime         { get; set; }
        public double   VisitDuration       { get; set; }
        public double   WalkingTimeHours    { get; set; }
        public double   CrowdFactor         { get; set; }
        public double   DynamicRequirements { get; set; }
        public double   SoftConstraints     { get; set; }
        public double   HardConstraints     { get; set; }
        public double   OptimalityScore     { get; set; }
        public double   AvgVisitCount       { get; set; }
        public double   StdDevVisitCount    { get; set; }
        public StationInfo? NearestStation  { get; set; }
    }
}

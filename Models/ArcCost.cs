namespace API_trip_link.Models
{
    internal class ArcCost
    {
        //מבנה של קשת - מעבר בין יעדים
        public int      FromDestinationId  { get; set; }
        public int      ToDestinationId    { get; set; }
        public DateTime BestDepartureTime  { get; set; }
        public double   BusTransitHours    { get; set; }
        public double   CarTransitHours    { get; set; }
        public double   WalkingHours       { get; set; }
        public double   TransitEfficiency  { get; set; }
        public bool     HasDirectBus       { get; set; }
        public List<TransitLegStep> TransitSteps { get; set; } = new();
        public double   TotalArcHours      => BusTransitHours + WalkingHours;
    }

    internal class TransitLegStep
    {
        public string   LineName      { get; set; } = "";
        public string   VehicleType   { get; set; } = "";
        public string   FromStation   { get; set; } = "";
        public string   ToStation     { get; set; } = "";
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime   { get; set; }
        public double   DurationHours { get; set; }
    }
}



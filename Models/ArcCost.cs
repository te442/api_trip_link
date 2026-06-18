namespace API_trip_link.Models
{
    internal class ArcCost
    {
        public int      FromDestinationId  { get; set; }//נקודת התחלה
        public int      ToDestinationId    { get; set; }
        public DateTime BestDepartureTime  { get; set; }//שעת יציה אופטמלית
        public double   BusTransitHours    { get; set; }//זמן הנסיעה באוטובוס
        public double   CarTransitHours    { get; set; }//זמן הנסיעה ברכב
        public double   WalkingHours       { get; set; }//זמן ההליכה
        public double   TransitEfficiency  { get; set; }//יעילות תחבורה
        public bool     HasDirectBus       { get; set; }//קו ישיר
        public double   TotalArcHours      => BusTransitHours + WalkingHours;
    }
}



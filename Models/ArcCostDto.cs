using System;

namespace API_trip_link.Models
{
    public class ArcCostDto
    {
        public int      FromDestinationId  { get; set; }
        public int      ToDestinationId    { get; set; }

        public DateTime BestDepartureTime  { get; set; }

        public double   BusTransitHours    { get; set; }

        public double   CarTransitHours    { get; set; }

        public double   WalkingHours       { get; set; }

        public double   TotalArcHours      { get; set; }

        public double   TransitEfficiency  { get; set; }

        public bool     HasDirectBus       { get; set; }
    }
}

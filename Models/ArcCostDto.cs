using System;

namespace API_trip_link.Models
{
    public class ArcCostDto
    {
        /// <summary>Origin destination ID (-1 = trip start address).</summary>
        public int      FromDestinationId  { get; set; }
        public int      ToDestinationId    { get; set; }

        /// <summary>Best sampled departure time within the ±30-min window.</summary>
        public DateTime BestDepartureTime  { get; set; }

        /// <summary>Bus (public transit) travel time in hours.</summary>
        public double   BusTransitHours    { get; set; }

        /// <summary>Private-car travel time in hours (reference baseline).</summary>
        public double   CarTransitHours    { get; set; }

        /// <summary>Walking time from arrival station to destination (hours).</summary>
        public double   WalkingHours       { get; set; }

        /// <summary>Total arc time = BusTransitHours + WalkingHours.</summary>
        public double   TotalArcHours      { get; set; }

        /// <summary>Transit efficiency = car time / bus time, clamped [0,1].</summary>
        public double   TransitEfficiency  { get; set; }

        /// <summary>Whether a direct bus (no transfer) was found for this departure.</summary>
        public bool     HasDirectBus       { get; set; }
    }
}

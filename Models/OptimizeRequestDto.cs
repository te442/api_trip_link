using System;

namespace API_trip_link.Models
{
    public class OptimizeRequestDto
    {
        public int      TripId              { get; set; }
        public DateTime TripStartTime       { get; set; }
        public DateTime TripEndTime         { get; set; }
        public double   MaxTravelTime       { get; set; }
        public double   ReturnTravelTime    { get; set; }
        public double   MinTransitEfficiency { get; set; } = 0.5;
        /// <summary>Client-generated id for live step tracking during optimization.</summary>
        public string?  TraceId             { get; set; }
    }
}

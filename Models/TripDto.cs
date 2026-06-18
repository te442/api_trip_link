using System;
using System.Collections.Generic;

namespace API_trip_link.Models
{
    public class TripDto
    {
        public int       TripId       { get; set; }
        public string    TripName     { get; set; }
        public string?   UserId       { get; set; }
        public string    UserName     { get; set; }
        public DateTime? TripDate     { get; set; }
        public string    AddressStart { get; set; }
        public TimeSpan? StartTime    { get; set; }
        public TimeSpan? EndTime      { get; set; }
        public decimal?  TripCost     { get; set; }
        public List<DestinationDto> Destinations { get; set; }
    }
}

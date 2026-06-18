using System;
using System.Collections.Generic;

namespace API_trip_link.Models
{
    public class CreateTripDto
    {
        public string    TripName     { get; set; }
        public string?   UserId       { get; set; }
        public DateTime? TripDate     { get; set; }
        public string    AddressStart { get; set; }
        public TimeSpan? StartTime    { get; set; }
        public TimeSpan? EndTime      { get; set; }
        public decimal?  TripCost     { get; set; }
        public List<int> CategoryIds  { get; set; }
        public List<int> FeatureIds   { get; set; }
        public int?      LevelId      { get; set; }
        public int?      MinNumDes    { get; set; }
        public int?      MaxNumDes    { get; set; }
        public string?   Region       { get; set; }
    }
}

using System;

namespace API_trip_link.Models
{
    public class DestinationDto
    {
        public int     DesId      { get; set; }
        public string  NameDes    { get; set; }
        public string  Region     { get; set; }
        public int?    LevelId    { get; set; }
        public string  LevelType  { get; set; }
        public int?    TravelerId { get; set; }
        public string  TravelerType { get; set; }
        public TimeSpan? TimeDes    { get; set; }
        public TimeSpan  OpeningTime { get; set; }
        public TimeSpan  ClosingTime { get; set; }
        public decimal?  Lat         { get; set; }
        public decimal?  Lon         { get; set; }
        public string?   ImageUrl    { get; set; }
        public int?      PrimaryCategoryId { get; set; }
        public List<string> Categories { get; set; } = new();
    }
}

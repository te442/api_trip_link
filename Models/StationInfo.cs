namespace API_trip_link.Models
{
    internal class StationInfo
    {
        public int    StationNum   { get; set; }
        public string StationName  { get; set; } = "";
        public string StationCode  { get; set; } = "";
        public string Area         { get; set; } = "";
        public double Latitude     { get; set; }
        public double Longitude    { get; set; }
    }
}

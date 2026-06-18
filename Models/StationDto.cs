namespace API_trip_link.Models
{
    public class StationDto
    {
        public int     StationNum  { get; set; }
        public string  StationCode { get; set; }
        public string  StationName { get; set; }
        public string  Area        { get; set; }
        public decimal? Lat         { get; set; }
        public decimal? Lon         { get; set; }
    }
}

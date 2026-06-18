namespace API_trip_link.Models
{
    public class ScoreTableStatsDto
    {
        public int    NodeCount   { get; set; }
        public int    HourCount   { get; set; }
        public int    TotalCells  { get; set; }
        public int    ValidCells  { get; set; }
        public double ValidRatio  { get; set; }
        public string Description { get; set; } = "";
    }
}

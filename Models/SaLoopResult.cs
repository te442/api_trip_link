namespace API_trip_link.Models
{
    internal class SaLoopResult
    {
        public int TotalIterations     { get; set; }
        public int AcceptedCount       { get; set; }
        public int RejectedCount       { get; set; }
        public double FinalTemperature { get; set; }
        public List<double> BestScoreProgression { get; set; } = new();
    }
}

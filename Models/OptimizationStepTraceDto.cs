namespace API_trip_link.Models
{
    public class OptimizationStepTraceDto
    {
        public int     StepNumber   { get; set; }
        public string  StepName     { get; set; } = "";
        public string  Label        { get; set; } = "";
        public string  Status       { get; set; } = "Pending";
        public string? Detail       { get; set; }
        public DateTime? StartedAt  { get; set; }
        public DateTime? FinishedAt { get; set; }
        public long?   DurationMs   { get; set; }
    }

    public class OptimizationProgressDto
    {
        public string TraceId { get; set; } = "";
        public bool   IsComplete { get; set; }
        public bool   HasError   { get; set; }
        public string? ErrorMessage { get; set; }
        public List<OptimizationStepTraceDto> Steps { get; set; } = new();
        public int ScoreTableCellsBuilt { get; set; }
        public int ScoreTableCellsTotal { get; set; }
        public List<ScoreTableCellTraceDto> ScoreTableCells { get; set; } = new();
    }

    public class ScoreTableCellTraceDto
    {
        public int    I              { get; set; }
        public int    J              { get; set; }
        public int    H              { get; set; }
        public string FromLabel      { get; set; } = "";
        public string ToLabel        { get; set; } = "";
        public string DepartureTime  { get; set; } = "";
        public bool   IsValid        { get; set; }
        public double TransitionScore { get; set; }
        public double BusTransitHours { get; set; }
        public double WalkingHours    { get; set; }
        public double TransitEfficiency { get; set; }
    }
}

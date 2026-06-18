using API_trip_link.Services.Optimizer;

namespace API_trip_link.Models
{
    internal class OptimizationContext
    {
        public OptimizeRequestDto Request { get; set; } = null!;
        public OptimizerParams Params { get; set; } = null!;
        public List<OptimizerDestination> Destinations { get; set; } = new();
        public ScoreTable? ScoreTable { get; set; }
        public OptimizerRoute InitialRoute { get; set; } = new();
        public SaLoopResult SaResult { get; set; } = new();
        public OptimizerRoute BestRoute { get; set; } = new();
        public TripPlan TripPlan { get; set; } = new();
        public string TripRegion { get; set; } = "";
        public string TripName { get; set; } = "";
        public string TraceId { get; set; } = "";
        public List<OptimizationStepTraceDto> StepTrace { get; set; } = new();
        public List<ScoreTableCellTraceDto> ScoreTableCellTrace { get; set; } = new();
    }
}

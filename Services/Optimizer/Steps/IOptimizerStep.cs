using API_trip_link.Models;

namespace API_trip_link.Services.Optimizer.Steps
{
    internal interface IOptimizerStep
    {
        int StepNumber { get; }
        string StepName { get; }
        Task ExecuteAsync(OptimizationContext ctx);
    }
}

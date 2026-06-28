using API_trip_link.Models;

namespace API_trip_link.Services.Optimizer.Steps
{
    internal interface IOptimizerStep
    {
        //ממשק המנהל את שלבי האופטמזציה
        int StepNumber { get; }
        string StepName { get; }
        Task ExecuteAsync(OptimizationContext ctx);
    }
}

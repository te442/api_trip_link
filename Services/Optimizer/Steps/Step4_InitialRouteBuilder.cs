using API_trip_link.Models;
using API_trip_link.Services.Optimizer;
using API_trip_link.Services.Transit;

namespace API_trip_link.Services.Optimizer.Steps
{
    //מחלקה בניי פתרון התחלתי
    internal class Step4_InitialRouteBuilder : IOptimizerStep
    {
        private readonly ITransitApiService _transitApi;
        private readonly ILogger<Step4_InitialRouteBuilder> _logger;

        public Step4_InitialRouteBuilder(ITransitApiService transitApi, ILogger<Step4_InitialRouteBuilder> logger)
        {
            _transitApi = transitApi;
            _logger     = logger;
        }

        public int StepNumber => 4;
        public string StepName => "INITIAL_ROUTE";

        public Task ExecuteAsync(OptimizationContext ctx)
        {
            var arcCalculator = new ArcCostCalculator(_transitApi, ctx.Params);
            var routeBuilder  = new RouteBuilder(ctx.Params, arcCalculator);

            AgentDebugLog.Write("Step4_InitialRouteBuilder.cs:23", "Using ScoreTable in route build",
                new
                {
                    scoreTableNull = ctx.ScoreTable == null,
                    nodeCount = ctx.ScoreTable?.NodeCount,
                    minuteCount = ctx.ScoreTable?.MinuteCount,
                    destinationCount = ctx.Destinations.Count
                },
                "H5");

            ctx.InitialRoute = routeBuilder.Build(
                ctx.ScoreTable, ctx.Destinations,
                logger: _logger, tripId: ctx.Request.TripId);
            return Task.CompletedTask;
        }
    }
}

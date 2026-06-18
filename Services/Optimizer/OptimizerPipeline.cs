using API_trip_link.Models;
using API_trip_link.Services.Optimizer.Steps;

namespace API_trip_link.Services.Optimizer
{
    internal class OptimizerPipeline
    {
        private readonly IReadOnlyList<IOptimizerStep> _steps;
        private readonly IOptimizationProgressStore _progress;

        public OptimizerPipeline(
            IEnumerable<IOptimizerStep> steps,
            IOptimizationProgressStore progress)
        {
            _steps   = steps.OrderBy(s => s.StepNumber).ToList();
            _progress = progress;
        }

        public async Task<OptimizationContext> RunAsync(OptimizeRequestDto request)
        {
            var ctx = new OptimizationContext
            {
                Request  = request,
                TraceId  = request.TraceId ?? ""
            };

            if (!string.IsNullOrWhiteSpace(ctx.TraceId))
                _progress.Ensure(ctx.TraceId);

            var pipelineTrace = OptimizationTraceReporter.Begin(
                ctx, _progress, -1, "PIPELINE",
                $"tripId={request.TripId}, {request.TripStartTime:HH:mm}-{request.TripEndTime:HH:mm}");

            OptimizerDebugTrace.PauseStep(-1, "PIPELINE", "START",
                $"tripId={request.TripId}, {request.TripStartTime:HH:mm}-{request.TripEndTime:HH:mm}");

            try
            {
                foreach (var step in _steps)
                {
                    var trace = OptimizationTraceReporter.Begin(
                        ctx, _progress, step.StepNumber, step.StepName,
                        $"tripId={request.TripId}");

                    OptimizerDebugTrace.PauseStep(
                        step.StepNumber, step.StepName, "START",
                        $"tripId={request.TripId}");

                    try
                    {
                        await step.ExecuteAsync(ctx);

                        OptimizationTraceReporter.End(
                            ctx, _progress, trace,
                            OptimizerDebugTrace.SummarizeAfterStep(ctx, step.StepNumber));

                        OptimizerDebugTrace.PauseStep(
                            step.StepNumber, step.StepName, "END",
                            OptimizerDebugTrace.SummarizeAfterStep(ctx, step.StepNumber));
                    }
                    catch (Exception ex)
                    {
                        OptimizationTraceReporter.Fail(ctx, _progress, trace, ex.Message);
                        throw;
                    }
                }

                OptimizationTraceReporter.End(
                    ctx, _progress, pipelineTrace,
                    OptimizerDebugTrace.SummarizeResult(ctx));

                OptimizerDebugTrace.PauseStep(-1, "PIPELINE", "END",
                    OptimizerDebugTrace.SummarizeResult(ctx));
            }
            catch (Exception ex)
            {
                OptimizationTraceReporter.Fail(ctx, _progress, pipelineTrace, ex.Message);
                if (!string.IsNullOrWhiteSpace(ctx.TraceId))
                    _progress.Fail(ctx.TraceId, ex.Message);
                throw;
            }

            return ctx;
        }
    }
}

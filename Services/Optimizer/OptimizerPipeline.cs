using API_trip_link.Models;
using API_trip_link.Services.Optimizer.Steps;

namespace API_trip_link.Services.Optimizer
{
    internal class OptimizerPipeline
    {
        private readonly IReadOnlyList<IOptimizerStep> _steps;
        private readonly IOptimizationProgressStore _progress;
        private readonly ILogger<OptimizerPipeline> _logger;

        public OptimizerPipeline(
            IEnumerable<IOptimizerStep> steps,
            IOptimizationProgressStore progress,
            ILogger<OptimizerPipeline> logger)
        {
            _steps   = steps.OrderBy(s => s.StepNumber).ToList();
            _progress = progress;
            _logger  = logger;
        }
        //פונקציה אחאית על הפעלת שלבי האופטימזציה
        public async Task<OptimizationContext> RunAsync(OptimizeRequestDto request)
        {
            var ctx = new OptimizationContext
            {
                Request  = request,
                TraceId  = request.TraceId ?? ""
            };

            if (!string.IsNullOrWhiteSpace(ctx.TraceId))
                _progress.Ensure(ctx.TraceId);

            OptimizerLog.StepStart(_logger, ctx, -1, "PIPELINE",
                $"tripId={request.TripId}, {request.TripStartTime:HH:mm}-{request.TripEndTime:HH:mm}");

            var pipelineTrace = OptimizationTraceReporter.Begin(
                ctx, _progress, -1, "PIPELINE",
                $"tripId={request.TripId}, {request.TripStartTime:HH:mm}-{request.TripEndTime:HH:mm}");

            OptimizerDebugTrace.PauseStep(-1, "PIPELINE", "START",
                $"tripId={request.TripId}, {request.TripStartTime:HH:mm}-{request.TripEndTime:HH:mm}");
            //לולאת ריצה על שלבי האופטמזציה
            try
            {
                foreach (var step in _steps)
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    OptimizerLog.StepStart(_logger, ctx, step.StepNumber, step.StepName,
                        $"tripId={request.TripId}");

                    var trace = OptimizationTraceReporter.Begin(
                        ctx, _progress, step.StepNumber, step.StepName,
                        $"tripId={request.TripId}");

                    OptimizerDebugTrace.PauseStep(
                        step.StepNumber, step.StepName, "START",
                        $"tripId={request.TripId}");

                    try
                    {
                        //הרצת שלב האופטמזציה
                        await step.ExecuteAsync(ctx);

                        sw.Stop();
                        var summary = OptimizerDebugTrace.SummarizeAfterStep(ctx, step.StepNumber);
                        OptimizerLog.StepEnd(_logger, ctx, step.StepNumber, step.StepName, summary, sw.ElapsedMilliseconds);

                        OptimizationTraceReporter.End(
                            ctx, _progress, trace,
                            OptimizerDebugTrace.SummarizeAfterStep(ctx, step.StepNumber));

                        OptimizerDebugTrace.PauseStep(
                            step.StepNumber, step.StepName, "END",
                            OptimizerDebugTrace.SummarizeAfterStep(ctx, step.StepNumber));
                    }
                    catch (Exception ex)
                    {
                        OptimizerLog.StepFailed(_logger, ctx, step.StepNumber, step.StepName, ex.Message);
                        OptimizationTraceReporter.Fail(ctx, _progress, trace, ex.Message);
                        throw;
                    }
                }
                //עדכון של סיום שלב אחד באופטימזציה

                OptimizerLog.StepEnd(_logger, ctx, -1, "PIPELINE",
                    OptimizerDebugTrace.SummarizeResult(ctx));

                OptimizationTraceReporter.End(
                    ctx, _progress, pipelineTrace,
                    OptimizerDebugTrace.SummarizeResult(ctx));

                OptimizerDebugTrace.PauseStep(-1, "PIPELINE", "END",
                    OptimizerDebugTrace.SummarizeResult(ctx));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Optimizer] tripId={TripId} trace={TraceId} PIPELINE נכשל",
                    request.TripId, ctx.TraceId ?? "-");
                OptimizationTraceReporter.Fail(ctx, _progress, pipelineTrace, ex.Message);
                if (!string.IsNullOrWhiteSpace(ctx.TraceId))
                    _progress.Fail(ctx.TraceId, ex.Message);
                throw;
            }

            return ctx;
        }
    }
}

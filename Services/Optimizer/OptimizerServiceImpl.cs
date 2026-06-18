using API_trip_link.Models;
using API_trip_link.Services;
using API_trip_link.Services.Optimizer;

namespace API_trip_link.Services.Optimizer
{
    internal class OptimizerServiceImpl : IOptimizerService
    {
        private readonly OptimizerPipeline _pipeline;
        private readonly ItineraryService _itineraryService;
        private readonly IOptimizationProgressStore _progress;

        public OptimizerServiceImpl(
            OptimizerPipeline pipeline,
            ItineraryService itineraryService,
            IOptimizationProgressStore progress)
        {
            _pipeline         = pipeline;
            _itineraryService = itineraryService;
            _progress         = progress;
        }

        public async Task<OptimizeResultDto> OptimizeTripAsync(OptimizeRequestDto request)
        {
            request.TraceId = string.IsNullOrWhiteSpace(request.TraceId)
                ? Guid.NewGuid().ToString("N")
                : request.TraceId;

            _progress.Ensure(request.TraceId);

            var ctx = await _pipeline.RunAsync(request);

            var resultTrace = OptimizationTraceReporter.Begin(
                ctx, _progress, 7, "RESULT", "מיפוי תוצאות");

            var result = OptimizeResultMapper.Map(ctx);

            OptimizationTraceReporter.End(
                ctx, _progress, resultTrace,
                OptimizerDebugTrace.SummarizeResult(ctx));

            OptimizerDebugTrace.PauseStep(7, "RESULT", "READY",
                OptimizerDebugTrace.SummarizeResult(ctx));

            var imagesTrace = OptimizationTraceReporter.Begin(
                ctx, _progress, 8, "ENRICH", "טעינת תמונות");

            try
            {
                await _itineraryService.EnrichWithImagesAsync(result);
                OptimizationTraceReporter.End(ctx, _progress, imagesTrace, "הושלם");
            }
            catch (Exception ex)
            {
                OptimizationTraceReporter.Fail(ctx, _progress, imagesTrace, ex.Message);
            }

            result.PipelineTrace = ctx.StepTrace.ToList();
            _progress.Complete(request.TraceId);
            return result;
        }

        public OptimizationProgressDto? GetProgress(string traceId) => _progress.Get(traceId);
    }
}

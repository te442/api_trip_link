using API_trip_link.Settings;
using System.Collections.Concurrent;

using API_trip_link.Models;

using API_trip_link.Services;

using API_trip_link.Services.Optimizer;



namespace API_trip_link.Services.Optimizer

{

    //מחלקה שמפעילה את כל האלגוריתם

    //כולל מעקב תמידי על כל שלבי האלגוריתם

    internal class OptimizerServiceImpl : IOptimizerService

    {

        private readonly OptimizerPipeline _pipeline;

        private readonly ItineraryService _itineraryService;

        private readonly IOptimizationProgressStore _progress;

        private readonly IOptimizeResultCache _optimizeCache;

        private readonly ILogger<OptimizerServiceImpl> _logger;

        private static readonly ConcurrentDictionary<string, byte> ActiveRuns = new();



        public OptimizerServiceImpl(

            OptimizerPipeline pipeline,

            ItineraryService itineraryService,

            IOptimizationProgressStore progress,

            IOptimizeResultCache optimizeCache,

            ILogger<OptimizerServiceImpl> logger)

        {

            _pipeline         = pipeline;

            _itineraryService = itineraryService;

            _progress         = progress;

            _optimizeCache    = optimizeCache;

            _logger           = logger;

        }



        public async Task<OptimizeResultDto> OptimizeTripAsync(OptimizeRequestDto request)

        {

            _logger.LogInformation(

                "[Optimizer] === התחלת אופטימיזציה tripId={TripId} {Start:HH:mm}-{End:HH:mm} minEff={MinEff} ===",

                request.TripId, request.TripStartTime, request.TripEndTime, request.MinTransitEfficiency);



            request.TraceId = string.IsNullOrWhiteSpace(request.TraceId)

                ? Guid.NewGuid().ToString(Configuration.Optimizer.TraceIdGuidFormat)

                : request.TraceId;



            _logger.LogInformation("[Optimizer] traceId={TraceId}", request.TraceId);

            if (!ActiveRuns.TryAdd(request.TraceId, 0))
                throw new InvalidOperationException("חישוב מסלול כבר רץ עבור הבקשה הזו. המתיני לסיום הריצה הנוכחית.");

            try
            {

            //הגדרת מעקב תמידי על כל שלבי האלגוריתם

            _progress.Ensure(request.TraceId);

            //הפעלת הpipline 

            var ctx = await _pipeline.RunAsync(request);

            //הצגת התוצאה

            var resultTrace = OptimizationTraceReporter.Begin(

                ctx, _progress, Configuration.Optimizer.StepNumberResultMapping, Configuration.Optimizer.StepNameResult, "מיפוי תוצאות");



            var result = OptimizeResultMapper.Map(ctx);



            OptimizationTraceReporter.End(

                ctx, _progress, resultTrace,

                OptimizerDebugTrace.SummarizeResult(ctx));



            OptimizerDebugTrace.PauseStep(Configuration.Optimizer.StepNumberResultMapping, Configuration.Optimizer.StepNameResult, "READY",

                OptimizerDebugTrace.SummarizeResult(ctx));



            var imagesTrace = OptimizationTraceReporter.Begin(

                ctx, _progress, Configuration.Optimizer.StepNumberImageEnrichment, Configuration.Optimizer.StepNameEnrich, "טעינת תמונות");



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



            if (result.TripId > Configuration.Common.MinValidTripId)

                _optimizeCache.Set(result.TripId, result);



            _logger.LogInformation(

                "[Optimizer] === סיום אופטימיזציה tripId={TripId} trace={TraceId}: {Count} יעדים, ציון={Score:F2}, רגליים={Legs} ===",

                result.TripId, request.TraceId, result.DestinationCount, result.TotalScore, result.Legs.Count);

            if (ctx.ScoreTable != null)
            {
                var (allocatedCells, validCells) = ctx.ScoreTable.GetStats();
                int logicalCells = ctx.ScoreTable.GetLogicalCellCapacity();
                _logger.LogInformation(
                    "[Optimizer] ScoreTable usage summary tripId={TripId} trace={TraceId}: {Summary}",
                    result.TripId, request.TraceId,
                    ctx.Instrumentation?.BuildFinalSummary(allocatedCells, logicalCells, validCells) ?? "disabled");
            }



            return result;

            }
            finally
            {
                ActiveRuns.TryRemove(request.TraceId, out _);
            }

        }



        public void EnsureProgress(string traceId) => _progress.Ensure(traceId);

        public OptimizationProgressDto? GetProgress(string traceId) => _progress.Get(traceId);

    }

}


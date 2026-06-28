using API_trip_link.Settings;
using API_trip_link.Models;
using API_trip_link.Services.Optimizer.Instrumentation;
using API_trip_link.Services.Transit;

namespace API_trip_link.Services.Optimizer.Steps
{
    internal class Step2_ScoreTableBuilder : IOptimizerStep
    {
        private readonly ITransitApiService _transitApi;
        private readonly IOptimizationProgressStore _progress;
        private readonly IConfiguration _config;
        private readonly ILogger<Step2_ScoreTableBuilder> _logger;

        public Step2_ScoreTableBuilder(
            ITransitApiService transitApi,
            IOptimizationProgressStore progress,
            IConfiguration config,
            ILogger<Step2_ScoreTableBuilder> logger)
        {
            _transitApi = transitApi;
            _progress   = progress;
            _config     = config;
            _logger     = logger;
        }

        public int StepNumber => 2;
        public string StepName => "SCORE_TABLE";
        //פעולה שבונה את מטריצת הציונים
        public async Task ExecuteAsync(OptimizationContext ctx)
        {
            //דיבוג
            AgentDebugLog.Write("Step2_ScoreTableBuilder", "Step2 started",
                new { destCount = ctx.Destinations.Count }, "H6");

            var (clampedStart, clampedEnd, clampNote) = TripScheduleDateHelper.ClampForGoogleTransit(
                ctx.Params.TripStartTime, ctx.Params.TripEndTime);
            ctx.Params.TripStartTime = clampedStart;
            ctx.Params.TripEndTime   = clampedEnd;
            if (clampNote != null)
                ctx.ScheduleAdjustmentNote = clampNote;

            var collector    = new TransitScheduleCollector(_transitApi, _config);
            var rejections   = new OptimizerRejectionTracker();
            var destinations = ctx.Destinations;
            var tripParams   = ctx.Params;
            if (_config.GetValue("Optimizer:EnableScoreTableInstrumentation", true))
                ctx.Instrumentation ??= new ScoreTableUsageInstrumentation();

            int n          = destinations.Count;
            int nodeCount  = n + 1;
            int minuteCount = ScoreTable.ComputeMinuteCount(tripParams.TripStartTime, tripParams.TripEndTime);
            int originIndex = Configuration.Common.OriginNodeIndex;

            int concurrency = Math.Clamp(
                _config.GetValue("Optimizer:ScoreTableConcurrency", Configuration.Optimizer.DefaultScoreTableConcurrency),
                Configuration.Optimizer.MinScoreTableConcurrency,
                Configuration.Optimizer.MaxScoreTableConcurrency);
            //יוצר רשימת קשתות כל קשת מציינת זוג יעד מקור ליעד
            var arcPairs = new List<(int I, int J)>();
            for (int i = 0; i < nodeCount; i++)
            for (int j = 1; j < nodeCount; j++)
                if (i != j) arcPairs.Add((i, j));
            //ממין את הקשתות לפי נקודת התחלה ולפי הסדר של i j
            arcPairs = arcPairs
                .OrderBy(p => p.I == originIndex ? 0 : 1)
                .ThenBy(p => p.I)
                .ThenBy(p => p.J)
                .ToList();
            // Event-driven estimate: each arc starts with a bounded Google event query, not a minute grid scan.
            int estimatedHttpRequests = arcPairs.Count;

            _transitApi.ResetHttpRequestCount();
            if (!string.IsNullOrWhiteSpace(ctx.TraceId))
                _progress.InitScoreTableProgress(ctx.TraceId, estimatedHttpRequests);

            OptimizerLog.Info(_logger, ctx,
                "איסוף אירועי תחבורה lazy: nodes={Nodes}, arcs={Arcs}, חלון={Minutes} דקות, concurrency={Conc}, הערכת בקשות HTTP≈{Est}",
                nodeCount, arcPairs.Count, minuteCount, concurrency, estimatedHttpRequests);

            var eventStore = new Dictionary<(int From, int To), List<TransitEvent>>();
            var eventStoreLock = new object();

            var traceLock = new object();

            void ReportRow(ScoreTableCellTraceDto row)
            {
                lock (traceLock)
                {
                    ctx.ScoreTableCellTrace.Add(row);
                    if (!string.IsNullOrWhiteSpace(ctx.TraceId))
                    {
                        _progress.AddScoreTableCell(ctx.TraceId, row);
                        _progress.SetScoreTableHttpCompleted(ctx.TraceId, _transitApi.HttpRequestCount);
                    }
                }
            }

            using (OptimizerDebugTrace.BeginScoreTable())
            {
                //אתחול סמפור לכמות המשימות שירוצו במקביל
                var gate = new SemaphoreSlim(concurrency);
                //עבור כל זוג מקור ויעד קריאת  לשירותי גוגל מפס ולקבלת מידע על זמני תחבורה בין שני היעדים
                var pairTasks = arcPairs.Select(async pair =>
                {
                    await gate.WaitAsync();
                    try
                    {
                        int i = pair.I;
                        int j = pair.J;
                        //יעדנ
                        var toDest   = destinations[j - 1];
                        //מקור בדיקה האם זו נקודת ההתחלה
                        var fromDest = i == originIndex ? null : destinations[i - 1];
                        var fromLabel = fromDest == null ? Configuration.Optimizer.OriginNodeLabel : fromDest.Name;
                        //הגדרת המיקום המדויק של קווי האורך והרוחב
                        var fromLoc = BuildLocation(fromDest, tripParams, isOrigin: fromDest == null);
                        var toLoc   = BuildLocation(toDest, tripParams, isOrigin: false);

                        var result = await collector.CollectArcAsync(
                            fromLoc, toLoc, fromDest, toDest, tripParams,
                            i, j, fromLabel,
                            onApiQuery: ReportRow,
                            onCell: ReportRow,
                            rejections: rejections);

                        if (result.Events.Count > 0)
                        {
                            lock (eventStoreLock)
                            {
                                eventStore[(i, j)] = result.Events;
                            }
                        }
                    }
                    finally
                    {
                        //שחרור המנעול
                        gate.Release();
                    }
                });
                //המתנה עד לסיום כל המשימות
                await Task.WhenAll(pairTasks);
            }
            //אובייקט עוטף
            ctx.ScoreTable = new ScoreTable(eventStore, destinations, tripParams.TripStartTime, minuteCount, ctx.Instrumentation);

            //דיבוג
            var dumpPath = Path.Combine(AppContext.BaseDirectory, Configuration.Optimizer.ScoreTableDumpFileName);
            ctx.ScoreTable.DumpToFile(dumpPath);

            var stats = ctx.ScoreTable.GetStats();
            ctx.ScoreTableCellTrace = ctx.ScoreTable.EnumerateFilledCells();
            if (!string.IsNullOrWhiteSpace(ctx.TraceId))
                _progress.SetScoreTableCells(ctx.TraceId, ctx.ScoreTableCellTrace);

            rejections.Flush(_logger, ctx.Request.TripId, ctx.TraceId, "SCORE_TABLE");
            var httpRequests = _transitApi.HttpRequestCount;
            if (!string.IsNullOrWhiteSpace(ctx.TraceId))
                _progress.SetScoreTableHttpCompleted(ctx.TraceId, httpRequests);

            OptimizerLog.Info(_logger, ctx,
                "איסוף אירועים הושלם: אירועים={Total}, תקפים={Valid}, מולאים={Filled}, בקשות HTTP={Http}, dump={Dump}",
                stats.TotalCells, stats.ValidCells, ctx.ScoreTableCellTrace.Count, httpRequests, dumpPath);

            int allocatedCells = stats.TotalCells;
            OptimizerLog.Info(_logger, ctx,
                "ScoreTable instrumentation baseline: {Summary}",
                ctx.Instrumentation?.BuildStep2Summary(
                    allocatedCells, ctx.ScoreTable.GetLogicalCellCapacity(), stats.ValidCells, ctx.ScoreTableCellTrace.Count)
                ?? "disabled");

            if (stats.ValidCells == 0)
                _logger.LogWarning(
                    "[Optimizer] tripId={TripId} trace={TraceId} אין תאים תקפים בטבלה — בדקי סף יעילות ({MinEff}), שעות פתיחה/סגירה, וחלון זמן",
                    ctx.Request.TripId, ctx.TraceId ?? "-", tripParams.MinTransitEfficiency);

            AgentDebugLog.Write("Step2_ScoreTableBuilder", "Sparse event store built",
                new
                {
                    nodeCount,
                    minuteCount,
                    concurrency,
                    httpRequests,
                    estimatedHttpRequests,
                    dimensions = $"sparse-events nodes={nodeCount}",
                    stats.TotalCells,
                    stats.ValidCells,
                    validRatio = stats.TotalCells > 0 ? Math.Round((double)stats.ValidCells / stats.TotalCells, Configuration.Optimizer.ValidRatioDecimalPlaces) : 0
                },
                "H1");
        }

        //פונקציה המחזירה מיקום יעד 
        private static TransitLocation BuildLocation(
            OptimizerDestination? dest, OptimizerParams tripParams, bool isOrigin)
        {
            if (isOrigin)
            {
                return new TransitLocation
                {
                    DestinationId = Configuration.Common.OriginDestinationId,
                    Address       = tripParams.AddressStart,
                    Latitude      = tripParams.StartLatitude,
                    Longitude     = tripParams.StartLongitude
                };
            }

            return new TransitLocation
            {
                DestinationId = dest!.DestinationId,
                Address         = dest.Name,
                Latitude        = dest.Latitude,
                Longitude       = dest.Longitude
            };
        }
    }
}

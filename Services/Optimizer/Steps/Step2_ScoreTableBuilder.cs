using API_trip_link.Models;
using API_trip_link.Services.Transit;

namespace API_trip_link.Services.Optimizer.Steps
{
    internal class Step2_ScoreTableBuilder : IOptimizerStep
    {
        private readonly ITransitApiService _transitApi;
        private readonly IOptimizationProgressStore _progress;
        private readonly IConfiguration _config;

        public Step2_ScoreTableBuilder(
            ITransitApiService transitApi,
            IOptimizationProgressStore progress,
            IConfiguration config)
        {
            _transitApi = transitApi;
            _progress   = progress;
            _config     = config;
        }

        public int StepNumber => 2;
        public string StepName => "SCORE_TABLE";

        public async Task ExecuteAsync(OptimizationContext ctx)
        {
            AgentDebugLog.Write("Step2_ScoreTableBuilder.cs:21", "Step2 started",
                new { destCount = ctx.Destinations.Count }, "H6");

            var arcCalculator = new ArcCostCalculator(_transitApi, ctx.Params);
            var destinations  = ctx.Destinations;
            var tripParams    = ctx.Params;

            int n         = destinations.Count;
            int nodeCount = n + 1;
            int hourCount = Math.Max(1, (int)Math.Ceiling(tripParams.MaxTimeFrame));
            const int originIndex = 0;

            int concurrency = Math.Clamp(_config.GetValue("Optimizer:ScoreTableConcurrency", 4), 1, 8);

            var arcPairs = new List<(int I, int J)>();
            for (int i = 0; i < nodeCount; i++)
            for (int j = 1; j < nodeCount; j++)
                if (i != j) arcPairs.Add((i, j));

            int totalCells = arcPairs.Count * hourCount;
            if (!string.IsNullOrWhiteSpace(ctx.TraceId))
                _progress.SetScoreTableTotals(ctx.TraceId, totalCells);

            var cells = new ArcTransitionRecord[nodeCount, nodeCount, hourCount];
            var traceLock = new object();

            using (OptimizerDebugTrace.BeginScoreTable())
            {
                var gate = new SemaphoreSlim(concurrency);
                var pairTasks = arcPairs.Select(async pair =>
                {
                    await gate.WaitAsync();
                    try
                    {
                        int i = pair.I;
                        int j = pair.J;
                        var toDest    = destinations[j - 1];
                        var fromDest  = i == originIndex ? null : destinations[i - 1];
                        var fromLabel = fromDest == null ? (tripParams.AddressStart ?? "מקור") : fromDest.Name;

                        for (int h = 0; h < hourCount; h++)
                        {
                            var departureTime = tripParams.TripStartTime.AddHours(h);
                            OptimizerDebugTrace.SetCell(i, j, h, fromLabel, toDest.Name, departureTime);

                            var arc = await arcCalculator.ComputeAsync(fromDest, toDest, departureTime);
                            double directHours = arc.BusTransitHours + arc.WalkingHours;
                            double optimality = WeightCalculator.CalculateDestinationOptimality(
                                toDest, tripParams, departureTime,
                                directTravelTime: directHours,
                                indirectTravelTime: arc.CarTransitHours,
                                routeMatchesTraveler: true,
                                hasDeadEnd: false);

                            var record = new ArcTransitionRecord
                            {
                                ArcCost         = arc,
                                TransitionScore = optimality >= 0 ? optimality : 0,
                                IsValid         = optimality >= 0
                            };
                            cells[i, j, h] = record;

                            var cellTrace = new ScoreTableCellTraceDto
                            {
                                I                 = i,
                                J                 = j,
                                H                 = h,
                                FromLabel         = fromLabel,
                                ToLabel           = toDest.Name,
                                DepartureTime     = departureTime.ToString("HH:mm"),
                                IsValid           = record.IsValid,
                                TransitionScore   = Math.Round(record.TransitionScore, 3),
                                BusTransitHours   = Math.Round(arc.BusTransitHours, 2),
                                WalkingHours      = Math.Round(arc.WalkingHours, 2),
                                TransitEfficiency = Math.Round(arc.TransitEfficiency, 2)
                            };

                            lock (traceLock)
                            {
                                ctx.ScoreTableCellTrace.Add(cellTrace);
                                if (!string.IsNullOrWhiteSpace(ctx.TraceId))
                                    _progress.AddScoreTableCell(ctx.TraceId, cellTrace);
                            }
                        }
                    }
                    finally
                    {
                        gate.Release();
                    }
                });

                await Task.WhenAll(pairTasks);
            }

            for (int i = 0; i < nodeCount; i++)
            for (int h = 0; h < hourCount; h++)
                cells[i, originIndex, h] = new ArcTransitionRecord { IsValid = false, TransitionScore = -1 };

            ctx.ScoreTable = new ScoreTable(cells, destinations, tripParams.TripStartTime, hourCount);

            var dumpPath = Path.Combine(AppContext.BaseDirectory, "score-table-dump.txt");
            ctx.ScoreTable.DumpToFile(dumpPath);
            AgentDebugLog.Write("Step2_ScoreTableBuilder.cs:76", "ScoreTable dumped to file",
                new { dumpPath, stats = ctx.ScoreTable.GetStats() }, "H7");

            var stats = ctx.ScoreTable.GetStats();
            AgentDebugLog.Write("Step2_ScoreTableBuilder.cs:69", "3D ScoreTable built",
                new
                {
                    nodeCount,
                    hourCount,
                    concurrency,
                    dimensions = $"{nodeCount}×{nodeCount}×{hourCount}",
                    stats.TotalCells,
                    stats.ValidCells,
                    validRatio = stats.TotalCells > 0 ? Math.Round((double)stats.ValidCells / stats.TotalCells, 3) : 0
                },
                "H1");
        }
    }
}

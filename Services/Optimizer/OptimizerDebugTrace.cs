using API_trip_link.Models;

namespace API_trip_link.Services.Optimizer
{
    /// <summary>דיבוג שלב-שלב של האופטימיזר וקריאות Google Maps ב-ScoreTable.</summary>
    internal static class OptimizerDebugTrace
    {
        private static readonly AsyncLocal<ScoreTableDebugScope?> _scope = new();

        internal sealed class ScoreTableDebugScope
        {
            public int CallSequence { get; set; }
            public int? CellI { get; set; }
            public int? CellJ { get; set; }
            public int? CellH { get; set; }
            public string FromLabel { get; set; } = "";
            public string ToLabel { get; set; } = "";
            public DateTime? CellDeparture { get; set; }
        }

        public static bool IsScoreTableActive => _scope.Value != null;

        /// <summary>עצירה בדיבוג + הודעה ב-Output. לחצי F5 (Continue) למעבר לשלב הבא.</summary>
        public static void PauseStep(int stepNumber, string stepName, string phase, string? detail = null)
        {
#if DEBUG
            var label = stepNumber < 0 ? stepName : $"Step {stepNumber}: {stepName}";
            var msg = $"[Optimizer] {label} — {phase}";
            if (!string.IsNullOrEmpty(detail)) msg += $" | {detail}";
            System.Diagnostics.Debug.WriteLine(msg);

            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Break();
                return;
            }

            if (Environment.GetEnvironmentVariable("OPTIMIZER_DEBUG_STEPS") == "1")
            {
                Console.WriteLine();
                Console.WriteLine("══════════════════════════════════════");
                Console.WriteLine(msg);
                Console.WriteLine("Enter = המשך לשלב הבא");
                Console.WriteLine("══════════════════════════════════════");
                Console.ReadLine();
            }
#endif
        }

        public static string SummarizeAfterStep(OptimizationContext ctx, int stepNumber)
        {
            return stepNumber switch
            {
                0 => $"יעדים={ctx.Destinations.Count}, אזור={ctx.TripRegion}, חלון={ctx.Params?.MaxTimeFrame:F1}ש",
                2 => ctx.ScoreTable == null
                    ? "ScoreTable=null"
                    : $"מטריצה {ctx.ScoreTable.NodeCount}×{ctx.ScoreTable.NodeCount}×{ctx.ScoreTable.HourCount}, תקפים={ctx.ScoreTable.GetStats().ValidCells}",
                4 => $"מסלול ראשוני: {ctx.InitialRoute.Destinations.Count} יעדים, ציון={ctx.InitialRoute.TotalScore:F2}",
                5 => $"מסלול סופי: {ctx.BestRoute.Destinations.Count} יעדים, ציון={ctx.BestRoute.TotalScore:F2}, " +
                     $"SA איטרציות={ctx.SaResult.TotalIterations}, מקובלים={ctx.SaResult.AcceptedCount}",
                6 => $"רגליים={ctx.TripPlan.Legs.Count}, יעדים במסלול={ctx.BestRoute.Destinations.Count}",
                _ => ""
            };
        }

        public static string SummarizeResult(OptimizationContext ctx)
        {
            var names = string.Join(" → ", ctx.BestRoute.Destinations.Select(d => d.Name));
            return $"יעדים={ctx.BestRoute.Destinations.Count}, ציון={ctx.BestRoute.TotalScore:F2}, מסלול=[{names}]";
        }

        public static IDisposable BeginScoreTable()
        {
            _scope.Value = new ScoreTableDebugScope();
            return new ScopeDisposable();
        }

        public static void SetCell(int i, int j, int h, string fromLabel, string toLabel, DateTime departure)
        {
            if (_scope.Value == null) return;
            _scope.Value.CellI = i;
            _scope.Value.CellJ = j;
            _scope.Value.CellH = h;
            _scope.Value.FromLabel = fromLabel;
            _scope.Value.ToLabel = toLabel;
            _scope.Value.CellDeparture = departure;
        }

        public static int NextCallSequence()
        {
            if (_scope.Value == null) return 0;
            return ++_scope.Value.CallSequence;
        }

        public static string FormatCellContext()
        {
            var s = _scope.Value;
            if (s == null) return "";
            return $"cell=[{s.CellI},{s.CellJ},{s.CellH}] {s.FromLabel}→{s.ToLabel} dep={s.CellDeparture:HH:mm}";
        }

        private sealed class ScopeDisposable : IDisposable
        {
            public void Dispose() => _scope.Value = null;
        }
    }
}

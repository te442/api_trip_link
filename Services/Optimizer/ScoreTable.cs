using API_trip_link.Models;

namespace API_trip_link.Services.Optimizer
{
    internal class ArcTransitionRecord
    {
        public ArcCost ArcCost         { get; set; } = null!;
        public double TransitionScore  { get; set; }
        public bool   IsValid          { get; set; }
    }

    internal class ScoreTable
    {
        private readonly ArcTransitionRecord[,,] _cells;
        private readonly List<OptimizerDestination> _destinations;
        private readonly DateTime _tripStart;

        public int NodeCount  { get; }
        public int HourCount  { get; }
        public int OriginIndex => 0;

        public ScoreTable(
            ArcTransitionRecord[,,] cells,
            List<OptimizerDestination> destinations,
            DateTime tripStart,
            int hourCount)
        {
            _cells         = cells;
            _destinations  = destinations;
            _tripStart     = tripStart;
            NodeCount      = destinations.Count + 1;
            HourCount      = hourCount;
        }

        public int DestIdToIndex(int destId)
        {
            int idx = _destinations.FindIndex(d => d.DestinationId == destId);
            return idx < 0 ? -1 : idx + 1;
        }

        public int TimeToHourIndex(DateTime time)
        {
            int h = (int)Math.Floor((time - _tripStart).TotalHours);
            return Math.Clamp(h, 0, HourCount - 1);
        }

        public ArcTransitionRecord Get(int i, int j, int h)
        {
            i = Math.Clamp(i, 0, NodeCount - 1);
            j = Math.Clamp(j, 0, NodeCount - 1);
            h = Math.Clamp(h, 0, HourCount - 1);
            return _cells[i, j, h] ?? new ArcTransitionRecord { IsValid = false, TransitionScore = -1 };
        }

        public (OptimizerDestination Dest, ArcCost Arc, double Score)? GetBestInColumn(
            int fromIndex,
            int hourIndex,
            IEnumerable<OptimizerDestination> candidates)
        {
            OptimizerDestination? bestDest  = null;
            ArcCost?              bestArc   = null;
            double                  bestScore = -1;

            foreach (var candidate in candidates)
            {
                int j = DestIdToIndex(candidate.DestinationId);
                if (j < 1) continue;

                var cell = Get(fromIndex, j, hourIndex);
                if (!cell.IsValid || cell.TransitionScore <= bestScore) continue;

                bestScore = cell.TransitionScore;
                bestDest  = candidate;
                bestArc   = cell.ArcCost;
            }

            if (bestDest == null || bestArc == null) return null;
            return (bestDest, bestArc, bestScore);
        }

        public (int TotalCells, int ValidCells) GetStats()
        {
            int total = 0, valid = 0;
            for (int i = 0; i < NodeCount; i++)
            for (int j = 0; j < NodeCount; j++)
            for (int h = 0; h < HourCount; h++)
            {
                if (j == OriginIndex || i == j) continue;
                total++;
                if (_cells[i, j, h].IsValid) valid++;
            }
            return (total, valid);
        }

        public string NodeLabel(int index)
        {
            if (index == OriginIndex) return "מקור";
            return _destinations[index - 1].Name;
        }

        public void DumpToFile(string path)
        {
            var stats = GetStats();
            var lines = new List<string>
            {
                $"ScoreTable {NodeCount}×{NodeCount}×{HourCount}",
                $"TripStart: {_tripStart:yyyy-MM-dd HH:mm}",
                $"TotalCells: {stats.TotalCells}, ValidCells: {stats.ValidCells}",
                ""
            };

            for (int h = 0; h < HourCount; h++)
            {
                var departure = _tripStart.AddHours(h);
                lines.Add($"=== שכבת זמן h={h} ({departure:HH:mm}) ===");

                for (int i = 0; i < NodeCount; i++)
                for (int j = 0; j < NodeCount; j++)
                {
                    if (j == OriginIndex || i == j) continue;

                    var cell = Get(i, j, h);
                    lines.Add(
                        $"[{i},{j},{h}] {NodeLabel(i)} → {NodeLabel(j)} | " +
                        $"valid={cell.IsValid} score={cell.TransitionScore:F3} " +
                        $"bus={cell.ArcCost.BusTransitHours:F2}h walk={cell.ArcCost.WalkingHours:F2}h " +
                        $"eff={cell.ArcCost.TransitEfficiency:F2}");
                }

                lines.Add("");
            }

            File.WriteAllText(path, string.Join(Environment.NewLine, lines));
        }
    }
}

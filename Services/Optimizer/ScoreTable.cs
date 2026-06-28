using API_trip_link.Models;
using API_trip_link.Services.Optimizer.Instrumentation;
using API_trip_link.Settings;

namespace API_trip_link.Services.Optimizer
{
    internal class ArcTransitionRecord
    {
        public ArcCost ArcCost { get; set; } = null!;
        public double TransitionScore { get; set; }
        public bool IsValid { get; set; }
    }

    internal sealed class TransitEvent
    {
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public TimeSpan Duration { get; set; }
        public ArcTransitionRecord Record { get; set; } = null!;
    }

    internal class ScoreTable
    {
        private readonly Dictionary<(int From, int To), List<TransitEvent>> _events;
        private readonly List<OptimizerDestination> _destinations;
        private readonly DateTime _tripStart;
        private readonly ScoreTableUsageInstrumentation? _instrumentation;

        public int NodeCount { get; }
        public int MinuteCount { get; }
        public int OriginIndex => Configuration.Common.OriginNodeIndex;
        public int HourCount => MinuteCount;
        public int EventCellCount => _events.Values.Sum(events => events.Count);
        public ScoreTableUsageInstrumentation? Instrumentation => _instrumentation;

        public ScoreTable(
            Dictionary<(int From, int To), List<TransitEvent>> events,
            List<OptimizerDestination> destinations,
            DateTime tripStart,
            int minuteCount,
            ScoreTableUsageInstrumentation? instrumentation = null)
        {
            _events = events.ToDictionary(
                pair => pair.Key,
                pair => pair.Value
                    .OrderBy(e => e.DepartureTime)
                    .ThenByDescending(e => e.Record.TransitionScore)
                    .ToList());
            _destinations = destinations;
            _tripStart = tripStart;
            _instrumentation = instrumentation;
            NodeCount = destinations.Count + 1;
            MinuteCount = minuteCount;
        }

        public static int ComputeMinuteCount(DateTime tripStart, DateTime tripEnd)
            => Math.Max(Configuration.Common.MinTripMinuteCount, (int)(tripEnd - tripStart).TotalMinutes + 1);

        public static int TimeToMinuteIndexStatic(DateTime time, DateTime tripStart, int minuteCount)
        {
            int m = (int)Math.Floor((time - tripStart).TotalMinutes);
            return Math.Clamp(m, 0, minuteCount - 1);
        }

        public int TimeToMinuteIndex(DateTime time)
            => TimeToMinuteIndexStatic(time, _tripStart, MinuteCount);

        public int TimeToHourIndex(DateTime time) => TimeToMinuteIndex(time);

        public DateTime MinuteIndexToTime(int minuteIndex)
            => _tripStart.AddMinutes(Math.Clamp(minuteIndex, 0, MinuteCount - 1));

        public int DestIdToIndex(int destId)
        {
            int idx = _destinations.FindIndex(d => d.DestinationId == destId);
            return idx < 0 ? -1 : idx + 1;
        }

        public ArcTransitionRecord Get(int i, int j, int m)
        {
            i = Math.Clamp(i, 0, NodeCount - 1);
            j = Math.Clamp(j, 0, NodeCount - 1);
            m = Math.Clamp(m, 0, MinuteCount - 1);

            var targetTime = MinuteIndexToTime(m);
            var cell = GetEventAtExactMinute(i, j, targetTime)?.Record;
            _instrumentation?.RecordGet(i, j, m, cell != null && cell.TransitionScore >= 0, cell?.IsValid == true);
            return cell ?? InvalidRecord();
        }

        public ArcTransitionRecord? GetNextAvailable(int fromIndex, int toIndex, DateTime currentTime)
            => GetNextEvent(fromIndex, toIndex, currentTime)?.Record;

        public double? GetFastestReturnTime(int destinationId)
        {
            int fromIndex = DestIdToIndex(destinationId);
            if (fromIndex < 1) return null;

            return _events
                .Where(pair => pair.Key.From == fromIndex && pair.Key.To == OriginIndex)
                .SelectMany(pair => pair.Value)
                .Where(e => e.Record.IsValid)
                .Select(e => (double?)e.Record.ArcCost.TotalArcHours)
                .Min();
        }

        public (OptimizerDestination Dest, ArcCost Arc, double Score)? GetBestInColumn(
            int fromIndex,
            int minuteIndex,
            IEnumerable<OptimizerDestination> candidates)
        {
            var candidateList = candidates as IReadOnlyCollection<OptimizerDestination> ?? candidates.ToList();
            _instrumentation?.RecordGetBestInColumn(candidateList.Count);

            var currentTime = MinuteIndexToTime(minuteIndex);
            OptimizerDestination? bestDest = null;
            ArcCost? bestArc = null;
            double bestScore = Configuration.Common.InvalidTransitionScore;
            DateTime bestDeparture = DateTime.MaxValue;

            foreach (var candidate in candidateList)
            {
                int toIndex = DestIdToIndex(candidate.DestinationId);
                if (toIndex < 1) continue;

                var ev = GetNextEvent(fromIndex, toIndex, currentTime);
                if (ev == null || !ev.Record.IsValid) continue;

                var score = ev.Record.TransitionScore;
                var departure = ev.Record.ArcCost.BestDepartureTime;
                if (score > bestScore ||
                    (Math.Abs(score - bestScore) < Configuration.Optimizer.TransitionScoreTieEpsilon &&
                     departure < bestDeparture))
                {
                    bestScore = score;
                    bestDeparture = departure;
                    bestDest = candidate;
                    bestArc = ev.Record.ArcCost;
                }
            }

            var hit = bestDest != null && bestArc != null;
            _instrumentation?.RecordGetBestInColumnResult(hit);
            return hit ? (bestDest!, bestArc!, bestScore) : null;
        }

        public ArcTransitionRecord? FindNearestValidCell(
            int fromIndex,
            int toIndex,
            int minuteIndex,
            int searchRadius = Configuration.Optimizer.ScoreTableNearestCellSearchRadiusMinutes)
        {
            _instrumentation?.RecordFindNearestCall();
            if (fromIndex < 0 || toIndex < 1)
            {
                _instrumentation?.RecordFindNearestResult(hit: false, offset: 0, searchedCells: 0);
                return null;
            }

            if (!_events.TryGetValue((fromIndex, toIndex), out var events) || events.Count == 0)
            {
                _instrumentation?.RecordFindNearestResult(hit: false, offset: 0, searchedCells: 0);
                return null;
            }

            var target = MinuteIndexToTime(minuteIndex);
            TransitEvent? best = null;
            var bestDistance = int.MaxValue;
            var searched = 0;

            foreach (var ev in events)
            {
                searched++;
                if (!ev.Record.IsValid) continue;

                var distance = Math.Abs((int)Math.Round((ev.DepartureTime - target).TotalMinutes));
                if (distance > searchRadius) continue;
                if (distance < bestDistance)
                {
                    best = ev;
                    bestDistance = distance;
                }
            }

            _instrumentation?.RecordFindNearestResult(best != null, bestDistance == int.MaxValue ? searchRadius : bestDistance, searched);
            return best?.Record;
        }

        public List<ScoreTableCellTraceDto> EnumerateFilledCells()
            => EnumerateCells(validOnly: false);

        public List<ScoreTableCellTraceDto> EnumerateValidCells()
            => EnumerateCells(validOnly: true);

        public (int TotalCells, int ValidCells) GetStats()
        {
            int total = EventCellCount;
            int valid = _events.Values.SelectMany(events => events).Count(e => e.Record.IsValid);
            return (total, valid);
        }

        public int GetLogicalCellCapacity()
        {
            var candidateArcs = Math.Max(0, (NodeCount - 1) * (NodeCount - 1));
            return candidateArcs * MinuteCount;
        }

        public string NodeLabel(int index)
        {
            if (index == OriginIndex) return Configuration.Optimizer.OriginNodeLabel;
            return _destinations[index - 1].Name;
        }

        public void DumpToFile(string path)
        {
            var stats = GetStats();
            var lines = new List<string>
            {
                $"Sparse event ScoreTable: nodes={NodeCount}, eventCells={stats.TotalCells}, validEvents={stats.ValidCells}",
                $"LogicalCapacity: {GetLogicalCellCapacity()} (not allocated)",
                $"TripStart: {_tripStart:yyyy-MM-dd HH:mm}",
                ""
            };

            foreach (var row in EnumerateValidCells())
            {
                var cell = Get(row.I, row.J, row.H);
                lines.Add(
                    $"[{row.I},{row.J},{row.H}] {row.FromLabel} -> {row.ToLabel} @ {row.DepartureTime} | " +
                    $"score={cell.TransitionScore:F3} bus={cell.ArcCost.BusTransitHours:F2}h " +
                    $"walk={cell.ArcCost.WalkingHours:F2}h eff={cell.ArcCost.TransitEfficiency:F2} " +
                    $"direct={cell.ArcCost.HasDirectBus}");
            }

            File.WriteAllText(path, string.Join(Environment.NewLine, lines));
        }

        private TransitEvent? GetNextEvent(int fromIndex, int toIndex, DateTime currentTime)
        {
            if (!_events.TryGetValue((fromIndex, toIndex), out var events) || events.Count == 0)
                return null;

            var left = 0;
            var right = events.Count - 1;
            var bestIndex = -1;
            while (left <= right)
            {
                var mid = left + (right - left) / 2;
                if (events[mid].DepartureTime >= currentTime)
                {
                    bestIndex = mid;
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }

            if (bestIndex < 0) return null;
            for (int i = bestIndex; i < events.Count; i++)
            {
                if (events[i].Record.IsValid)
                    return events[i];
            }

            return null;
        }

        private TransitEvent? GetEventAtExactMinute(int fromIndex, int toIndex, DateTime targetTime)
        {
            if (!_events.TryGetValue((fromIndex, toIndex), out var events) || events.Count == 0)
                return null;

            return events.FirstOrDefault(e =>
                TimeToMinuteIndex(e.DepartureTime) == TimeToMinuteIndex(targetTime));
        }

        private List<ScoreTableCellTraceDto> EnumerateCells(bool validOnly)
        {
            return _events
                .SelectMany(pair => pair.Value.Select(ev => (pair.Key.From, pair.Key.To, Event: ev)))
                .Where(x => !validOnly || x.Event.Record.IsValid)
                .Select(x =>
                {
                    var arc = x.Event.Record.ArcCost;
                    return new ScoreTableCellTraceDto
                    {
                        I = x.From,
                        J = x.To,
                        H = TimeToMinuteIndex(x.Event.DepartureTime),
                        FromLabel = NodeLabel(x.From),
                        ToLabel = NodeLabel(x.To),
                        DepartureTime = x.Event.DepartureTime.ToString("HH:mm"),
                        ApiKind = x.Event.Record.IsValid ? "אירוע תקף" : "אירוע שנפסל",
                        IsValid = x.Event.Record.IsValid,
                        TransitionScore = Math.Round(x.Event.Record.TransitionScore, Configuration.Optimizer.ScoreTraceScoreDecimals),
                        BusTransitHours = Math.Round(arc.BusTransitHours, Configuration.Optimizer.ScoreTraceHoursDecimals),
                        WalkingHours = Math.Round(arc.WalkingHours, Configuration.Optimizer.ScoreTraceHoursDecimals),
                        TransitEfficiency = Math.Round(arc.TransitEfficiency, Configuration.Optimizer.ScoreTraceHoursDecimals),
                        HasDirectBus = arc.HasDirectBus
                    };
                })
                .OrderBy(r => r.I)
                .ThenBy(r => r.J)
                .ThenBy(r => r.H)
                .ToList();
        }

        private static ArcTransitionRecord InvalidRecord()
            => new()
            {
                IsValid = false,
                TransitionScore = Configuration.Common.InvalidTransitionScore
            };
    }
}

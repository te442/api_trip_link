using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;

using API_trip_link.Settings;
//מחלקה העוקבת אחר ריצת האלגוריתם הראשי
namespace API_trip_link.Services.Optimizer.Instrumentation
{
    internal enum UsageScope
    {
        Step2,
        RouteBuilder_Initial,
        RouteBuilder_SA,
        SA_Iteration
    }

    internal sealed class ScoreTableUsageInstrumentation
    {
        private readonly AsyncLocal<UsageScope?> _currentScope = new();
        private readonly ConcurrentDictionary<UsageScope, ScopeStats> _scopeStats = new();
        private readonly ConcurrentDictionary<CellKey, byte> _uniqueCells = new();
        private readonly ConcurrentDictionary<int, long> _fromHeatmap = new();
        private readonly ConcurrentDictionary<int, long> _toHeatmap = new();
         private readonly ConcurrentDictionary<int, long> _minuteHeatmap = new();
        private readonly ConcurrentDictionary<int, SaIterationStats> _saIterations = new();
        private long _totalReads;
        private long _validReads;
        private long _invalidReads;
        private long _emptyReads;
        private long _selectedArcs;
        private long _saTotalReads;
        private long _saAccepted;
        private long _saRejected;

        public IDisposable BeginScope(UsageScope scope)
        {
            var previous = _currentScope.Value;
            _currentScope.Value = scope;
            return new ScopeHandle(this, previous);
        }

        public void RecordGet(int fromIndex, int toIndex, int minuteIndex, bool hasCell, bool isValid)
        {
            Try(() =>
            {
                var scope = _currentScope.Value;
                if (scope == null) return;

                Interlocked.Increment(ref _totalReads);
                _uniqueCells.TryAdd(new CellKey(fromIndex, toIndex, minuteIndex), 0);
                Increment(_fromHeatmap, fromIndex);
                Increment(_toHeatmap, toIndex);
                Increment(_minuteHeatmap, minuteIndex);

                if (!hasCell)
                    Interlocked.Increment(ref _emptyReads);
                else if (isValid)
                    Interlocked.Increment(ref _validReads);
                else
                    Interlocked.Increment(ref _invalidReads);

                var stats = GetScopeStats(scope.Value);
                stats.IncrementRead(hasCell, isValid);
            });
        }

        public void RecordGetBestInColumn(int candidateCount)
        {
            Try(() =>
            {
                var scope = _currentScope.Value;
                if (scope == null) return;

                var stats = GetScopeStats(scope.Value);
                Interlocked.Increment(ref stats.GetBestInColumnCalls);
                if (candidateCount >= 0)
                    Interlocked.Add(ref stats.GetBestInColumnCandidates, candidateCount);
            });
        }

        public void RecordGetBestInColumnResult(bool hit)
        {
            Try(() =>
            {
                var scope = _currentScope.Value;
                if (scope == null) return;

                var stats = GetScopeStats(scope.Value);
                if (hit)
                    Interlocked.Increment(ref stats.GetBestInColumnHits);
                else
                    Interlocked.Increment(ref stats.GetBestInColumnMisses);
            });
        }

        public void RecordFindNearestCall()
        {
            Try(() =>
            {
                var scope = _currentScope.Value;
                if (scope == null) return;

                Interlocked.Increment(ref GetScopeStats(scope.Value).FindNearestCalls);
            });
        }

        public void RecordFindNearestResult(bool hit, int offset, int searchedCells)
        {
            Try(() =>
            {
                var scope = _currentScope.Value;
                if (scope == null) return;

                var stats = GetScopeStats(scope.Value);
                if (hit)
                {
                    Interlocked.Increment(ref stats.FindNearestHits);
                    Interlocked.Add(ref stats.FindNearestHitOffsetTotal, Math.Max(0, offset));
                }
                else
                {
                    Interlocked.Increment(ref stats.FindNearestMisses);
                }

                Interlocked.Add(ref stats.FindNearestSearchedCells, Math.Max(0, searchedCells));
            });
        }

        public void RecordSelectedArc(int fromIndex, int toIndex, int minuteIndex)
        {
            Try(() =>
            {
                var scope = _currentScope.Value;
                if (scope == null) return;

                Interlocked.Increment(ref _selectedArcs);
                Interlocked.Increment(ref GetScopeStats(scope.Value).SelectedArcs);
            });
        }

        public void RecordRouteBuild(double routeScore, int candidatesEvaluated, int acceptedArcs)
        {
            Try(() =>
            {
                var scope = _currentScope.Value;
                if (scope == null) return;

                var stats = GetScopeStats(scope.Value);
                Interlocked.Increment(ref stats.RouteBuilds);
                Interlocked.Add(ref stats.CandidatesEvaluated, Math.Max(0, candidatesEvaluated));
                Interlocked.Add(ref stats.AcceptedArcs, Math.Max(0, acceptedArcs));
                stats.LastRouteScore = routeScore;
            });
        }

        public SaIterationSnapshot BeginSaIteration(int iterationIndex, double scoreBefore)
        {
            long readsBefore = Interlocked.Read(ref _totalReads);
            int uniqueBefore = _uniqueCells.Count;
            return new SaIterationSnapshot(iterationIndex, scoreBefore, readsBefore, uniqueBefore);
        }

        public void EndSaIteration(SaIterationSnapshot snapshot, double scoreAfter, bool accepted, bool bestUpdated)
        {
            Try(() =>
            {
                long readsAfter = Interlocked.Read(ref _totalReads);
                int uniqueAfter = _uniqueCells.Count;
                var stats = _saIterations.GetOrAdd(snapshot.IterationIndex, _ => new SaIterationStats());
                stats.IterationIndex = snapshot.IterationIndex;
                stats.Reads = Math.Max(0, readsAfter - snapshot.ReadsBefore);
                stats.UniqueNewCells = Math.Max(0, uniqueAfter - snapshot.UniqueBefore);
                stats.ScoreBefore = snapshot.ScoreBefore;
                stats.ScoreAfter = scoreAfter;
                stats.Accepted = accepted;
                stats.BestUpdated = bestUpdated;

                Interlocked.Increment(ref _saTotalReads);
                if (accepted)
                    Interlocked.Increment(ref _saAccepted);
                else
                    Interlocked.Increment(ref _saRejected);
            });
        }

        public string BuildStep2Summary(int allocatedCells, int logicalCells, int validCells, int filledCells)
        {
            return TryReturn(() =>
            {
                int emptyCells = Math.Max(0, allocatedCells - filledCells);
                return string.Create(CultureInfo.InvariantCulture,
                    $"allocated={allocatedCells}, logical={logicalCells}, valid={validCells}, filled={filledCells}, empty={emptyCells}");
            }, "instrumentation unavailable");
        }

        public string BuildFinalSummary(int allocatedCells, int logicalCells, int validCells)
        {
            return TryReturn(() =>
            {
                long totalReads = Interlocked.Read(ref _totalReads);
                int unique = _uniqueCells.Count;
                long validReads = Interlocked.Read(ref _validReads);
                long invalidReads = Interlocked.Read(ref _invalidReads);
                long emptyReads = Interlocked.Read(ref _emptyReads);
                int saIterations = _saIterations.Count;
                long saTotalReads = _saIterations.Values.Sum(s => s.Reads);
                long accepted = Interlocked.Read(ref _saAccepted);
                long rejected = Interlocked.Read(ref _saRejected);
                double avgUniquePerIteration = saIterations > 0
                    ? _saIterations.Values.Sum(s => s.UniqueNewCells) / (double)saIterations
                    : 0;
                double acceptedRate = accepted + rejected > 0 ? accepted / (double)(accepted + rejected) : 0;
                double rejectedRate = accepted + rejected > 0 ? rejected / (double)(accepted + rejected) : 0;

                return string.Create(CultureInfo.InvariantCulture,
                    $"reads={totalReads}, unique={unique}, validReads={validReads}, invalidReads={invalidReads}, emptyReads={emptyReads}, " +
                    $"unique/allocated={Ratio(unique, allocatedCells):F6}, unique/logical={Ratio(unique, logicalCells):F6}, unique/valid={Ratio(unique, validCells):F6}, " +
                    $"saIterations={saIterations}, saTotalReads={saTotalReads}, avgUniquePerIteration={avgUniquePerIteration:F2}, " +
                    $"acceptanceRate={acceptedRate:F4}, rejectedRate={rejectedRate:F4}");
            }, "instrumentation unavailable");
        }

        private ScopeStats GetScopeStats(UsageScope scope)
            => _scopeStats.GetOrAdd(scope, _ => new ScopeStats());

        private static void Increment(ConcurrentDictionary<int, long> map, int key)
            => map.AddOrUpdate(key, 1, (_, current) => current + 1);

        private static double Ratio(double numerator, double denominator)
            => denominator > 0 ? numerator / denominator : 0;

        private static void Try(Action action)
        {
            try
            {
                action();
            }
            catch
            {
                // Instrumentation must never affect optimizer behavior.
            }
        }

        private static T TryReturn<T>(Func<T> action, T fallback)
        {
            try
            {
                return action();
            }
            catch
            {
                return fallback;
            }
        }

        private readonly record struct CellKey(int FromIndex, int ToIndex, int MinuteIndex);

        internal readonly record struct SaIterationSnapshot(
            int IterationIndex,
            double ScoreBefore,
            long ReadsBefore,
            int UniqueBefore);

        private sealed class ScopeStats
        {
            public long Reads;
            public long ValidReads;
            public long InvalidReads;
            public long EmptyReads;
            public long GetBestInColumnCalls;
            public long GetBestInColumnCandidates;
            public long GetBestInColumnHits;
            public long GetBestInColumnMisses;
            public long FindNearestCalls;
            public long FindNearestHits;
            public long FindNearestMisses;
            public long FindNearestHitOffsetTotal;
            public long FindNearestSearchedCells;
            public long SelectedArcs;
            public long RouteBuilds;
            public long CandidatesEvaluated;
            public long AcceptedArcs;
            public double LastRouteScore;

            public void IncrementRead(bool hasCell, bool isValid)
            {
                Interlocked.Increment(ref Reads);
                if (!hasCell)
                    Interlocked.Increment(ref EmptyReads);
                else if (isValid)
                    Interlocked.Increment(ref ValidReads);
                else
                    Interlocked.Increment(ref InvalidReads);
            }
        }

        private sealed class SaIterationStats
        {
            public int IterationIndex { get; set; }
            public long Reads { get; set; }
            public int UniqueNewCells { get; set; }
            public double ScoreBefore { get; set; }
            public double ScoreAfter { get; set; }
            public bool Accepted { get; set; }
            public bool BestUpdated { get; set; }
        }

        private sealed class ScopeHandle : IDisposable
        {
            private readonly ScoreTableUsageInstrumentation _owner;
            private readonly UsageScope? _previous;
            private bool _disposed;

            public ScopeHandle(ScoreTableUsageInstrumentation owner, UsageScope? previous)
            {
                _owner = owner;
                _previous = previous;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _owner._currentScope.Value = _previous;
                _disposed = true;
            }
        }
    }
}

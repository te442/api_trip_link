using System.Collections.Concurrent;

using API_trip_link.Settings;

namespace API_trip_link.Services.Optimizer
{
    /// <summary>אוסף סיבות פסילת תאים/יעדים ומדפיס דוגמאות + סיכום.</summary>
    internal sealed class OptimizerRejectionTracker
    {
        private readonly ConcurrentDictionary<string, int> _counts = new();
        private readonly ConcurrentQueue<string> _samples = new();
        private readonly int _maxSamples;

        public OptimizerRejectionTracker(int maxSamples = Configuration.Optimizer.MaxRejectionSamples)
        {
            _maxSamples = maxSamples;
        }

        public void Record(string fromLabel, string toLabel, DateTime departure, string reason)
        {
            _counts.AddOrUpdate(reason, 1, (_, c) => c + 1);
            if (_samples.Count < _maxSamples)
                _samples.Enqueue($"{fromLabel} → {toLabel} @ {departure:HH:mm}: {reason}");
        }

        public void Flush(ILogger logger, int tripId, string? traceId, string context)
        {
            if (_counts.IsEmpty) return;

            var summary = string.Join(" | ", _counts.OrderByDescending(kv => kv.Value)
                .Select(kv => $"{kv.Key} ({kv.Value})"));

            logger.LogInformation(
                "[Optimizer] tripId={TripId} trace={TraceId} {Context} — סיכום פסילות: {Summary}",
                tripId, traceId ?? "-", context, summary);

            foreach (var sample in _samples)
                logger.LogInformation("[Optimizer] tripId={TripId} דוגמת פסילה: {Sample}", tripId, sample);
        }
    }
}

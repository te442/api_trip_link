using System.Collections.Concurrent;
using API_trip_link.Settings;
using API_trip_link.Models;

namespace API_trip_link.Services
{
    internal class OptimizeResultCache : IOptimizeResultCache
    {
        private readonly ConcurrentDictionary<int, OptimizeResultDto> _byTripId = new();

        public void Set(int tripId, OptimizeResultDto result)
        {
            if (tripId <= Configuration.Common.MinValidTripId) return;
            _byTripId[tripId] = result;
        }

        public OptimizeResultDto? Get(int tripId)
        {
            if (tripId <= Configuration.Common.MinValidTripId) return null;
            return _byTripId.TryGetValue(tripId, out var result) ? result : null;
        }
    }
}

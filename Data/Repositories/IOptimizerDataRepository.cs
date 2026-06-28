using API_trip_link.Models;

namespace API_trip_link.Data.Repositories
{
    public interface IOptimizerDataRepository//ממשק המנהל את הנתונים לאופטמזציה
    {
        Task<Trip?> GetTripForOptimizationAsync(int tripId);
        Task<List<Destination>> GetDestinationsForOptimizationAsync(
            string? region,
            int? levelId,
            IReadOnlySet<int> categoryIds,
            IReadOnlySet<int> featureIds);
        Task<List<BusStation>> GetBusLinesForStationAsync(int stationNum, int take = 3);
    }
}

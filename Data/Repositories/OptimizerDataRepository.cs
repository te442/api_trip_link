using API_trip_link.Models;
using Microsoft.EntityFrameworkCore;

namespace API_trip_link.Data.Repositories
{
    //מחלקה אחראית על האופטמזציה
    //מממשת את ממשק המנהל את הנתונים לאופטמזציה
    public class OptimizerDataRepository : IOptimizerDataRepository
    {
        private readonly TripContext _context;

        public OptimizerDataRepository(TripContext context)
        {
            _context = context;
        }
        //פעולה המחזירה טיול
        public async Task<Trip?> GetTripForOptimizationAsync(int tripId)
        {
            return await _context.Trips
                .Include(t => t.NatureTrips)
                .Include(t => t.CategoriesToTrips)
                .Include(t => t.FeatureToTrips)
                .FirstOrDefaultAsync(t => t.TripId == tripId);
        }
        //פעולה המחזירה רשימת יעדים

        public async Task<List<Destination>> GetDestinationsForOptimizationAsync(
            string? region,
            int? levelId,
            IReadOnlySet<int> categoryIds,
            IReadOnlySet<int> featureIds)
        {
            var destQuery = _context.Destinations
                .Include(d => d.DifficultyLevel)
                .Include(d => d.TypeTraveler)
                .Include(d => d.CategoriesOfDestinations)
                .Include(d => d.StationToDestinations)
                    .ThenInclude(s => s.Station)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(region))
            {
                var trimmedRegion = region.Trim();
                destQuery = destQuery.Where(d => d.Region != null && d.Region.Trim() == trimmedRegion);
            }
            //רשימת היעדים המסוננים מאילוצים קשים
            var candidates = await destQuery.ToListAsync();

            if (levelId.HasValue)
            {
                var byLevel = candidates.Where(d => d.LevelId == levelId.Value).ToList();
                if (byLevel.Count > 0)
                    candidates = byLevel;
            }

            if (categoryIds.Count > 0)
            {
                var byCategory = candidates
                    .Where(d => d.CategoriesOfDestinations != null &&
                                d.CategoriesOfDestinations.Any(c => categoryIds.Contains(c.CategoriesId)))
                    .ToList();
                if (byCategory.Count > 0)
                    candidates = byCategory;
            }

            if (featureIds.Count > 0)
            {
                var byFeature = candidates
                    .Where(d => d.StationToDestinations != null &&
                                d.StationToDestinations.Any(s =>
                                    s.FeatureId.HasValue && featureIds.Contains(s.FeatureId.Value)))
                    .ToList();
                if (byFeature.Count > 0)
                    candidates = byFeature;
            }

            return candidates;
        }
        //החזרת קווי אוטובוס לפי תחנות
        public async Task<List<BusStation>> GetBusLinesForStationAsync(int stationNum, int take = 3)
        {
            return await _context.BusStations
                .Include(bs => bs.Bus)
                .Include(bs => bs.Station)
                .Where(bs => bs.Station.StationNum == stationNum)
                .Take(take)
                .ToListAsync();
        }
    }
}

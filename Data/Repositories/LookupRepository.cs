using API_trip_link.Models;
using Microsoft.EntityFrameworkCore;

namespace API_trip_link.Data.Repositories
{
    public class LookupRepository : ILookupRepository
    {
        private readonly TripContext _context;

        public LookupRepository(TripContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryDto
                {
                    CategoriesId   = c.CategoriesId,
                    CategoriesName = c.CategoriesName.Trim()
                })
                .ToListAsync();
        }

        public async Task<List<DifficultyLevelDto>> GetLevelsAsync()
        {
            return await _context.DifficultyLevels
                .Select(l => new DifficultyLevelDto
                {
                    LevelId   = l.LevelId,
                    LevelType = l.LevelType
                })
                .ToListAsync();
        }

        public async Task<List<TravelerTypeDto>> GetTravelerTypesAsync()
        {
            return await _context.TypeTravelers
                .Select(t => new TravelerTypeDto
                {
                    TravelerId       = t.TravelerId,
                    TypeTravelerName = t.TypeTravelerName
                })
                .ToListAsync();
        }

        public async Task<List<FeatureTypeDto>> GetFeaturesAsync()
        {
            return await _context.FeatureTypes
                .Select(f => new FeatureTypeDto
                {
                    FeatureId = f.FeatureId,
                    Feature   = f.Feature
                })
                .ToListAsync();
        }

        public async Task<List<string>> GetRegionsAsync()
        {
            return await _context.Destinations
                .Where(d => d.Region != null)
                .Select(d => d.Region)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();
        }
    }
}

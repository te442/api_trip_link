using Microsoft.EntityFrameworkCore;
using API_trip_link.Data;
using API_trip_link.Models;
using API_trip_link.Services.Optimizer;

namespace API_trip_link.Services
{
   
    public class ItineraryService
    {
        private readonly TripContext _context;
        private readonly IOptimizeResultCache _optimizeCache;

        public ItineraryService(TripContext context, IOptimizeResultCache optimizeCache)
        {
            _context       = context;
            _optimizeCache = optimizeCache;
        }

        //פונקציה המחזירה תוצאה של טיול לפי מזהה טיול לאחר ריצתו באלגוריתם הראשי
        public Task<TripItineraryDto?> GetItineraryAsync(int tripId)
        {
            var cached = _optimizeCache.Get(tripId);
            if (cached == null || cached.Legs.Count == 0)
                return Task.FromResult<TripItineraryDto?>(null);

            return Task.FromResult<TripItineraryDto?>(OptimizeResultMapper.ToItineraryDto(cached));
        }

        
        public async Task EnrichWithImagesAsync(OptimizeResultDto result)
        {
            var ids = result.Legs.Select(l => l.DesId).ToList();
            var images = await _context.Destinations
                .Where(d => ids.Contains(d.DesId))
                .Select(d => new
                {
                    d.DesId,
                    d.Lat,
                    d.Lon,
                    PrimaryCategoryId = d.CategoriesOfDestinations
                        .OrderBy(c => c.CategoriesId)
                        .Select(c => (int?)c.CategoriesId)
                        .FirstOrDefault()
                })
                .ToListAsync();

            foreach (var leg in result.Legs)
            {
                var img = images.FirstOrDefault(i => i.DesId == leg.DesId);
                if (img != null)
                {
                    leg.ImageUrl = DestinationImageResolver.Resolve(img.DesId, img.PrimaryCategoryId);
                    if (img.Lat.HasValue) leg.Lat = (double)img.Lat.Value;
                    if (img.Lon.HasValue) leg.Lon = (double)img.Lon.Value;
                }
            }

            result.MapPoints = result.Legs
                .Where(l => l.Lat.HasValue && l.Lon.HasValue)
                .Select(l => new MapPointDto
                {
                    Order = l.Order,
                    Label = l.DestinationName,
                    Lat   = l.Lat!.Value,
                    Lon   = l.Lon!.Value
                }).ToList();
        }
    }
}

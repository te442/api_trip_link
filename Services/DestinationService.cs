using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using API_trip_link.Data;
using API_trip_link.Models;

namespace API_trip_link.Services
{
    public class DestinationService
    {
        private readonly TripContext _context;

        public DestinationService(TripContext context)
        {
            _context = context;
        }

        //פעולה המחזירה את כל היעדים
        public async Task<List<DestinationDto>> GetAllAsync()
        {
            var list = await QueryWithCategories().ToListAsync();
            DestinationImageResolver.ApplyDisplayImages(list);
            return list;
        }

        //פעולה המחזירה יעד לפי מזהה
        public async Task<DestinationDto> GetByIdAsync(int id)
        {
            var dest = await QueryWithCategories()
                .FirstOrDefaultAsync(d => d.DesId == id);
            if (dest != null)
                dest.ImageUrl = DestinationImageResolver.Resolve(dest.DesId, dest.PrimaryCategoryId);
            return dest;
        }

        //פעולה המחזירה רשימת יעדים לפי איזור 
        public async Task<List<DestinationDto>> GetByRegionAsync(string region)
        {
            var list = await QueryWithCategories()
                .Where(d => d.Region == region)
                .ToListAsync();
            DestinationImageResolver.ApplyDisplayImages(list);
            return list;
        }

        //פעולה המחזירה רשימת יעדים לפי רמת קושי
        public async Task<List<DestinationDto>> GetByLevelAsync(int levelId)
        {
            var list = await QueryWithCategories()
                .Where(d => d.LevelId == levelId)
                .ToListAsync();
            DestinationImageResolver.ApplyDisplayImages(list);
            return list;
        }

        //פונקציה המחזירה רשימת תחנות השייכות ליעד מסוים
        public async Task<List<StationDto>> GetStationsForDestinationAsync(int desId)
        {
            return await _context.StationToDestinations
                .Include(s => s.Station)
                .Where(s => s.DesId == desId)
                .Select(s => new StationDto
                {
                    StationNum  = s.Station.StationNum,
                    StationCode = s.Station.StationCode,
                    StationName = s.Station.StationName,
                    Area        = s.Station.Area,
                    Lat         = s.Station.Lat,
                    Lon         = s.Station.Lon
                })
                .ToListAsync();
        }

        //---map----
        private IQueryable<DestinationDto> QueryWithCategories()
            => _context.Destinations
                .Select(d => new DestinationDto
                {
                    DesId        = d.DesId,
                    NameDes      = d.NameDes,
                    Region       = d.Region,
                    LevelId      = d.LevelId,
                    LevelType    = d.DifficultyLevel.LevelType,
                    TravelerId   = d.TravelerId,
                    TravelerType = d.TypeTraveler.TypeTravelerName,
                    TimeDes      = d.TimeDes,
                    OpeningTime  = d.OpeningTime,
                    ClosingTime  = d.ClosingTime,
                    Lat          = d.Lat,
                    Lon          = d.Lon,
                    ImageUrl     = d.ImageUrl,
                    PrimaryCategoryId = d.CategoriesOfDestinations
                        .OrderBy(c => c.CategoriesId)
                        .Select(c => (int?)c.CategoriesId)
                        .FirstOrDefault(),
                    Categories   = d.CategoriesOfDestinations
                        .Select(c => c.Category.CategoriesName.Trim())
                        .ToList()
                });

    }
}

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
            return await _context.Destinations
                .Include(d => d.DifficultyLevel)
                .Include(d => d.TypeTraveler)
                .Select(d => MapToDto(d))
                .ToListAsync();
        }

        //פעולה המחזירה יעד לפי מזהה
        public async Task<DestinationDto> GetByIdAsync(int id)
        {
            var dest = await _context.Destinations
                .Include(d => d.DifficultyLevel)
                .Include(d => d.TypeTraveler)
                .FirstOrDefaultAsync(d => d.DesId == id);

            return dest == null ? null : MapToDto(dest);
        }

        //פעולה המחזירה רשימת יעדים לפי איזור 
        public async Task<List<DestinationDto>> GetByRegionAsync(string region)
        {
            return await _context.Destinations
                .Include(d => d.DifficultyLevel)
                .Include(d => d.TypeTraveler)
                .Where(d => d.Region == region)
                .Select(d => MapToDto(d))
                .ToListAsync();
        }

        //פעולה המחזירה רשימת יעדים לפי רמת קושי
        public async Task<List<DestinationDto>> GetByLevelAsync(int levelId)
        {
            return await _context.Destinations
                .Include(d => d.DifficultyLevel)
                .Include(d => d.TypeTraveler)
                .Where(d => d.LevelId == levelId)
                .Select(d => MapToDto(d))
                .ToListAsync();
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
        //פעולה הממירה אובייקט ממסד הנתונים לאובייקט יעד להעברה בין השכבות
        private static DestinationDto MapToDto(Destination d) => new DestinationDto
        {
            DesId        = d.DesId,
            NameDes      = d.NameDes,
            Region       = d.Region,
            LevelId      = d.LevelId,
            LevelType    = d.DifficultyLevel?.LevelType,
            TravelerId   = d.TravelerId,
            TravelerType = d.TypeTraveler?.TypeTravelerName,
            TimeDes      = d.TimeDes,
            Lat          = d.Lat,
            Lon          = d.Lon,
            ImageUrl     = d.ImageUrl
        };
    }
}

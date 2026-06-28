using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using API_trip_link.Data;
using API_trip_link.Models;

namespace API_trip_link.Services
{
    public class TripService
    {
        private readonly TripContext _context;

        public TripService(TripContext context)
        {
            _context = context;
        }

        //פעולה המחזירה את כל הטיולים
        //
        public async Task<List<TripDto>> GetAllTripsAsync()
        {
            return await _context.Trips
                .Include(t => t.User)
                .Include(t => t.DesOfTrips)
                    .ThenInclude(d => d.Destination)
                        .ThenInclude(dest => dest.DifficultyLevel)
                .Select(t => MapToDto(t))
                .ToListAsync();
        }

        // פעולה לקבלת טיול לפי מזהה
        public async Task<TripDto> GetTripByIdAsync(int id)
        {
            //בחירת טיול לפי מזהה  טיולים ומוסיף בשאילתא לכל טיול את המשתמש שלו ואת היעדים שלו עם רמת הקושי של כל יעד
            var trip = await _context.Trips
                .Include(t => t.User)
                .Include(t => t.DesOfTrips)
                    .ThenInclude(d => d.Destination)
                        .ThenInclude(dest => dest.DifficultyLevel)
                .FirstOrDefaultAsync(t => t.TripId == id);

            return trip == null ? null : MapToDto(trip);
        }
        //פעולה לקבלת טיולים לפי משתמש
        public async Task<List<TripDto>> GetTripsByUserAsync(string userId)
        {
            return await _context.Trips
                .Include(t => t.User)
                .Include(t => t.DesOfTrips)
                    .ThenInclude(d => d.Destination)
                .Where(t => t.UserId == userId)
                .Select(t => MapToDto(t))
                .ToListAsync();
        }

       //פעולת יצירת טיול
        public async Task<TripDto> CreateTripAsync(CreateTripDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.AddressStart))
                throw new ArgumentException("כתובת התחלה חובה — יש לבחור כתובת מהרשימה");

            //יצירת אובייקט טיול
            var trip = new Trip
            {
                TripName     = dto.TripName,
                UserId       = dto.UserId?.ToString(),
                TripDate     = dto.TripDate,
                AddressStart = dto.AddressStart ?? "",
                StartTime    = dto.StartTime,
                EndTime      = dto.EndTime,
                TripCost     = dto.TripCost ?? 0
            };
            //הוספת הטיול למסד הנתונים
            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            // Add categories
            //לוקח את הקטגוריות שהמשתמש בחר ושומר אותן בטבלת הקשר בין טיולים לקטגוריות
            if (dto.CategoryIds != null)
            {
                foreach (var catId in dto.CategoryIds)
                {
                    _context.CategoriesToTrips.Add(new CategoriesToTrip
                    {
                        TripId      = trip.TripId,
                        CategoriesId = catId
                    });
                }
            }
            //כנל לתכונות
            if (dto.FeatureIds != null)
            {
                foreach (var featId in dto.FeatureIds)
                {
                    _context.FeatureToTrips.Add(new FeatureToTrip
                    {
                        TripId    = trip.TripId,
                        FeatureId = featId
                    });
                }
            }

            // הוספת משתני אופן הטיול
            if (dto.LevelId != null || dto.MinNumDes != null || dto.MaxNumDes != null || dto.Region != null)
            {
                _context.NatureTrips.Add(new NatureTrip
                {
                    TripId    = trip.TripId,
                    LevelId   = dto.LevelId,
                    MinNumDes = dto.MinNumDes,
                    MaxNumDes = dto.MaxNumDes,
                    Region    = dto.Region
                });
            }

            await _context.SaveChangesAsync();
            //מחזירה את הטיול שנוצר
            return await GetTripByIdAsync(trip.TripId);
        }

        // פעולת מחיקת טיול
        public async Task<bool> DeleteTripAsync(int id)
        {
            //חיפוש הטיול לפי מזהה במסד נתונים
            var trip = await _context.Trips.FindAsync(id);
            if (trip == null) return false;
            //מחיקת הטיול ממסד הנתונים
            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();
            //החזרת תובה למשתמש
            return true;
        }

        //פעולה המעדכנת ושומרת את נתוני האופטמזציה כלומר את היעדים שנבחרו למסלול ואת הסדר ביקור בהם
        public async Task SaveOptimizedRouteAsync(int tripId, List<int> destinationIds)
        {
            //מחיקת היעדים הקיימים בטיול
            var existing = _context.DesOfTrips.Where(d => d.TripId == tripId);
            _context.DesOfTrips.RemoveRange(existing);

            //הוספת היעדים לטיול לפי סדר הביקור בהם
            for (int i = 0; i < destinationIds.Count; i++)
            {
                _context.DesOfTrips.Add(new DesOfTrip
                {
                    TripId      = tripId,
                    DesId       = destinationIds[i],
                    VisitNumber = i + 1
                });
            }
            //שמירת השינויים במסד הנתונים
            await _context.SaveChangesAsync();
        }

        // -----------map-----------
        //פעולה הממירה אובייקט ממסד הנתונים לאובייקט DTO להעברת נתונים בין השכבות
        private static TripDto MapToDto(Trip t) => new TripDto
        {
            TripId       = t.TripId,
            TripName     = t.TripName,
            UserId       = t.UserId,
            UserName     = t.User?.FullName,
            TripDate     = t.TripDate,
            AddressStart = t.AddressStart,
            StartTime    = t.StartTime,
            EndTime      = t.EndTime,
            TripCost     = t.TripCost,
            Destinations = t.DesOfTrips?
                .OrderBy(d => d.VisitNumber)
                .Select(d => new DestinationDto
                {
                    DesId     = d.Destination?.DesId ?? 0,
                    NameDes   = d.Destination?.NameDes,
                    Region    = d.Destination?.Region,
                    LevelId   = d.Destination?.LevelId,
                    LevelType = d.Destination?.DifficultyLevel?.LevelType,
                    TimeDes   = d.Destination?.TimeDes
                }).ToList() ?? new()
        };
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using API_trip_link.Data;
using API_trip_link.Models;

namespace API_trip_link.Services
{
    public class UserService
    {
        //שירותי המשתמשים
        private readonly TripContext _context;

        public UserService(TripContext context)
        {
            _context = context;
        }
    //פעולה המוסיפה משתמש חדש
    //מקבלת אובייקט יוזר ומוסיפה אותו למסד נתונים
    public async Task<UserDto> CreateAsync(CreateUserDto dto)
 {
    // 1. יצירת האובייקט
    var user = new User 
    { 
        UserId = Guid.NewGuid().ToString(),
        FullName = dto.FullName,
        Phone = dto.Phone
    };

    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    // 3. המרה חזרה ל-UserDto והחזרה שלו
    return new UserDto 
    { 
        UserId = user.UserId,
        FullName = user.FullName,
        Phone = user.Phone
    };
  }
        //פעולה המחזירה את כל המשתמשים
        //מחזירה רשימה של אובייקטים מסוג UserDto
        public async Task<List<UserDto>> GetAllAsync()
        {
            return await _context.Users
                .Select(u => MapToDto(u))
                .ToListAsync();
        }

        //פעולה המחזירה משתמש לפי מזהה
        //מקבלת מזהה ומחזירה אובייקט מסוג UserDto
        public async Task<UserDto> GetByIdAsync(string id)
        {
            //פעולה אסינכרונית המחזירה תגובת שרת
            var user = await _context.Users.FindAsync(id);
            return user == null ? null : MapToDto(user);
        }
        //פעולה המוחקת משתמש לפי מזהה
        //מקבלת מזהה ומוחקת אובייקט מסוג User מבסיס הנתונים
        public async Task<bool> DeleteAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);
            //אם המשתמש לא נמצא מחזירה false
            if (user == null) return false;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
        //פעולה הממירה אובייקט מסוג User לאובייקט מסוג UserDto
        private static UserDto MapToDto(User u) => new UserDto
        {
            UserId   = u.UserId,
            FullName = u.FullName,
            Phone    = u.Phone
        };
    }
}

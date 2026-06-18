using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using API_trip_link.Data;
using API_trip_link.Models;

namespace API_trip_link.Services
{
    public class AuthService
    {
        private readonly TripContext _context;
        private readonly IConfiguration _config;

        public AuthService(TripContext context, IConfiguration config)
        {
            _context = context;
            _config   = config;
        }
        //פעולה הרשמה למערכת על ידי הוספת משתמש חדש
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            //בדיקות תקינות

            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                throw new ArgumentException("אימייל וסיסמה הם שדות חובה");

            var email = dto.Email.Trim().ToLowerInvariant();
            if (await _context.Users.AnyAsync(u => u.Email == email))
                throw new InvalidOperationException("כתובת האימייל כבר רשומה במערכת");
            //יצירת המשתמש החדש

            var user = new User
            {
                UserId       = Guid.NewGuid().ToString(),
                FullName     = dto.FullName.Trim(),
                Phone        = dto.Phone?.Trim() ?? "",
                Email        = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            //החזרת תגובה

            return BuildAuthResponse(user);
        }

        //פעולת כניסה למערכת למשתמש רשום
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            //בדיקת אימות משתמש לפי המייל
            var email = dto.Email.Trim().ToLowerInvariant();
            var user  = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            //בדיקת קיום משתמש וסיסמא נכונה

            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                throw new UnauthorizedAccessException("אימייל או סיסמה שגויים");
            //בדיקת תקינות סיסמא
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("אימייל או סיסמה שגויים");

            return BuildAuthResponse(user);
        }
 
        private AuthResponseDto BuildAuthResponse(User user)
        {
            return new AuthResponseDto
            {
                Token    = GenerateJwt(user),
                UserId   = user.UserId,
                FullName = user.FullName,
                Email    = user.Email ?? ""
            };
        }

        private string GenerateJwt(User user)
        {
            var key     = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured");
            var issuer  = _config["Jwt:Issuer"] ?? "TripLinkAPI";
            var audience = _config["Jwt:Audience"] ?? "TripLinkApp";
            var expireMinutes = int.TryParse(_config["Jwt:ExpireMinutes"], out var m) ? m : 1440;

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email ?? "")
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

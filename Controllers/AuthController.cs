using Microsoft.AspNetCore.Mvc;
using API_trip_link.Models;
using API_trip_link.Services;

namespace API_trip_link.Controllers
{
    //קונטרולר המנהל את הכניסה והיצירה של המשתמשים בעת כניסתם למערכת
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        //פעולה בונה שמקבלת את השירותים ומציבה אותם במשתנים הפרימיטיביים
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        //פעולה המוסיפה משתמש חדש
      
        // POST api/auth/register
        [HttpPost("register")]
        //מקבלת אובייקט מסוג RegisterDto ומוסיפה אותו לבסיס הנתונים
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var result = await _authService.RegisterAsync(dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        //פעולה המאפשרת למשתמש להתחבר למערכת
        // POST api/auth/login
        //מקבלת אובייקט מסוג LoginDto ומאפשרת למשתמש להתחבר למערכת
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
        {
            try
            {
                var result = await _authService.LoginAsync(dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }
    }
}

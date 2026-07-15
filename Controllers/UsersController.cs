using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using API_trip_link.Models;
using API_trip_link.Services;

namespace API_trip_link.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //קונטרולר המנהל את המשתמשים
    public class UsersController : ControllerBase
    {
        private readonly UserService _service;
        public UsersController(UserService service)
        {
            _service = service;
        }

        // GET api/users
        [HttpGet]
        //פעולה המחזירה את כל המשתמשים
        public async Task<ActionResult<List<UserDto>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        // GET api/users/{id}
        //פעולה המחזירה משתמש לפי מזהה
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetById(string id)
        {
            var user = await _service.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // POST api/users
        [HttpPost]
        //מקבלת אובייקט מסוג CreateUserDto ומוסיפה אותו לבסיס הנתונים
        public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto)
        {
            var user = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = user.UserId }, user);
        }

        //פעולה המוחקת משתמש לפי מזהה
        // DELETE api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}

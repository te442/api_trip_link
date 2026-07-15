using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using API_trip_link.Models;
using API_trip_link.Services;

namespace API_trip_link.Controllers
{
    //קונטרולר המנהל את היעדים
    [ApiController]
    [Route("api/[controller]")]
    public class DestinationsController : ControllerBase
    {
        private readonly DestinationService _service;
        public DestinationsController(DestinationService service)
        {
            _service = service;
        }

        // GET api/destinations
        [HttpGet]
        //פעולה המחזירה את כל היעדים
        public async Task<ActionResult<List<DestinationDto>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }
        //פעולה המחזירה יעד לפי מזהה
        // GET api/destinations/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<DestinationDto>> GetById(int id)
        {
            var dest = await _service.GetByIdAsync(id);
            if (dest == null) return NotFound();
            return Ok(dest);
        }
        //פעולה המחזירה רשימת יעדים לפי מיקום גאוגרפי
        // GET api/destinations/region/{region}
        [HttpGet("region/{region}")]
        public async Task<ActionResult<List<DestinationDto>>> GetByRegion(string region)
        {
            return Ok(await _service.GetByRegionAsync(region));
        }
        //פעולה המחזירה רשימת יעדים לפי רמת קושי
        // GET api/destinations/level/{levelId}
        [HttpGet("level/{levelId}")]
        public async Task<ActionResult<List<DestinationDto>>> GetByLevel(int levelId)
        {
            return Ok(await _service.GetByLevelAsync(levelId));
        }
        //פעולה המחזירה את כל התחנות בהתאם ליעד על פי מזהה
        // GET api/destinations/{id}/stations
        [HttpGet("{id}/stations")]
        public async Task<ActionResult<List<StationDto>>> GetStations(int id)
        {
            return Ok(await _service.GetStationsForDestinationAsync(id));
        }
    }
}

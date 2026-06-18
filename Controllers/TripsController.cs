using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using API_trip_link.Models;
using API_trip_link.Services;

namespace API_trip_link.Controllers
{
    //קונטרולר המנהל את הטיולים
    [ApiController]
    [Route("api/[controller]")]
    public class TripsController : ControllerBase
    {
        //שירותי הטיולים
        private readonly TripService       _tripService;
        //שירותי האופטימיזציה
        private readonly IOptimizerService _optimizerService;
        //שירותי האינטרייטרי
        private readonly ItineraryService  _itineraryService;

        //פעולה בונה שמקבלת את השירותים ומציבה אותם במשתנים הפרימיטיביים
        public TripsController(
            TripService tripService,
            IOptimizerService optimizerService,
            ItineraryService itineraryService)
        {
            _tripService       = tripService;
            _optimizerService  = optimizerService;
            _itineraryService  = itineraryService;
        }

        // GET api/trips
        [HttpGet]
        //פעולה המחזירה את כל הטיולים
        //למה היא TASK- כי היא אסינכרונית
        //למה היא ACTIONRESULT - כדי לקבל את התשובה מה HTTP REQUEST
        //הפעולה מחזירה רשימה של טיולים ומחזירה אותה בפורמט של JSON
        public async Task<ActionResult<List<TripDto>>> GetAll()
        {
            //פעולה אסינכרונית שמחזירה רשימה של טיולים
            var trips = await _tripService.GetAllTripsAsync();
            return Ok(trips);
        }
        //פעולה המחזירה טיול לפי מזהה טיול
        // GET api/trips/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TripDto>> GetById(int id)
        {
            //פעולה אסינכרונית שמחזירה טיול לפי מזהה טיול
            var trip = await _tripService.GetTripByIdAsync(id);
            //אם הטיול לא נמצא מחזיר שגיאה 404
            if (trip == null) return NotFound();
            return Ok(trip);
        }
        //פעולה המחזירה רשימת טיולים לפי משתמש מזהה
        // GET api/trips/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<TripDto>>> GetByUser(string userId)
        {
            var trips = await _tripService.GetTripsByUserAsync(userId);
            return Ok(trips);
        }
        //פעולה המוסיפה טיול
        // POST api/trips
        [HttpPost]
        //למה היא [FromBody] - כי הטיול נשלח בגוף הבקשה
        //מקבלת אובייקט מסוג CreateTripDto ומוסיפה אותו 
        public async Task<ActionResult<TripDto>> Create([FromBody] CreateTripDto dto)
        {
            try
            {
                //פעולה אסינכרונית שמוסיפה אובייקט מסוג CreateTripDto לבסיס הנתונים
                var trip = await _tripService.CreateTripAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = trip.TripId }, trip);
            }
            //אם קרה שגיאה מחזירה שגיאה 400
            catch (Exception ex)
            {
                //מחזירה שגיאה 400 ומציגה את השגיאה
                return BadRequest(new { error = ex.InnerException?.Message ?? ex.Message });
            }
        }
        //פעולה המוחקת טיול לפי מזהה 
        // DELETE api/trips/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _tripService.DeleteTripAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        // GET api/trips/optimize/progress/{traceId}
        [HttpGet("optimize/progress/{traceId}")]
        public ActionResult<OptimizationProgressDto> GetOptimizeProgress(string traceId)
        {
            var progress = _optimizerService.GetProgress(traceId);
            if (progress == null) return NotFound();
            return Ok(progress);
        }

        // POST api/trips/optimize
        // מריץ את אלגוריתם האופטימיזציה ומחזיר את המסלול האופטימלי
        [HttpPost("optimize")]
        //מקבלת אובייקט מסוג OptimizeRequestDto ומריץ את אלגוריתם האופטימיזציה
        public async Task<ActionResult<OptimizeResultDto>> Optimize([FromBody] OptimizeRequestDto request)
        {
            try
            {
                //פעולה אסינכרונית שמריץ את אלגוריתם האופטימיזציה
                var result = await _optimizerService.OptimizeTripAsync(request);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                //אם קרה שגיאה מחזירה שגיאה 400 ומציגה את השגיאה
                return BadRequest(new { error = ex.Message });
            }
        }
        //פעולה המחזירה את התוצאות של אלגוריתם האופטימיזציה
        //את התוכנית המפורטת של הטיול
        // GET api/trips/{id}/itinerary
        [HttpGet("{id}/itinerary")]
        public async Task<ActionResult<TripItineraryDto>> GetItinerary(int id)
        {
            var itinerary = await _itineraryService.GetItineraryAsync(id);
            if (itinerary == null) return NotFound();
            return Ok(itinerary);
        }

        // POST api/trips/{id}/save-route
        // שומר את המסלול האופטימלי לטיול
        [HttpPost("{id}/save-route")]
        public async Task<IActionResult> SaveRoute(int id, [FromBody] List<int> destinationIds)
        {
            await _tripService.SaveOptimizedRouteAsync(id, destinationIds);
            return Ok(new { message = "Route saved successfully" });
        }
    }
}

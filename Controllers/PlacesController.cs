using System.Collections.Generic;
using System.Threading.Tasks;
using API_trip_link.Models;
using API_trip_link.Services.Transit;
using Microsoft.AspNetCore.Mvc;

namespace API_trip_link.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlacesController : ControllerBase
    {
        private readonly IPlacesAutocompleteService _placesService;

        public PlacesController(IPlacesAutocompleteService placesService)
        {
            _placesService = placesService;
        }

        // GET api/places/autocomplete?input=
        [HttpGet("autocomplete")]
        public async Task<ActionResult<List<PlaceSuggestionDto>>> Autocomplete([FromQuery] string input)
        {
            try
            {
                var results = await _placesService.AutocompleteAsync(input ?? "");
                return Ok(results);
            }
            catch (GoogleMapsApiException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}

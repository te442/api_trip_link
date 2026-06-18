using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using API_trip_link.Models;
using API_trip_link.Services;

namespace API_trip_link.Controllers
{
    /// <summary>
    /// Lookup endpoints – returns reference data for the trip-creation form:
    /// categories, difficulty levels, traveler types, feature types, and regions.
    /// </summary>
    /// קונטרולר המנהל את האילוצים השייכים לטיול
    [ApiController]
    [Route("api/[controller]")]
    public class LookupsController : ControllerBase
    {
        //שירותי האילוצים
        private readonly LookupService _lookupService;

        //פעולה בונה שמקבלת את השירותים ומציבה אותם במשתנים הפרימיטיביים
        public LookupsController(LookupService lookupService)
        {
            _lookupService = lookupService;
        }

        //פעולה המחזירה את כל הקטגוריות
        // GET api/lookups/categories
        [HttpGet("categories")]
        public async Task<ActionResult<List<CategoryDto>>> GetCategories()
        {
            //פעולה אסינכרונית המחזירה תגובת שרת
            return Ok(await _lookupService.GetCategoriesAsync());
        }

        //פעולה המחזירה את כל הרמות קושי
        // GET api/lookups/levels
        [HttpGet("levels")]
        public async Task<ActionResult<List<DifficultyLevelDto>>> GetLevels()
        {
            //פעולה אסינכרונית המחזירה תגובת שרת
            return Ok(await _lookupService.GetLevelsAsync());
        }

        //פעולה המחזירה את כל סוגי המטיילים
        // GET api/lookups/traveler-types
        [HttpGet("traveler-types")]
        public async Task<ActionResult<List<TravelerTypeDto>>> GetTravelerTypes()
        {
            //פעולה אסינכרונית המחזירה תגובת שרת
            return Ok(await _lookupService.GetTravelerTypesAsync());
        }

        //פעולה המחזירה את כל סוגי התכונות והאופי
        // GET api/lookups/features
        [HttpGet("features")]
        public async Task<ActionResult<List<FeatureTypeDto>>> GetFeatures()
        {
            //פעולה אסינכרונית המחזירה תגובת שרת
            return Ok(await _lookupService.GetFeaturesAsync());
        }

        //פעולה המחזירה את כל האזורים 
        // GET api/lookups/regions
        [HttpGet("regions")]
        public async Task<ActionResult<List<string>>> GetRegions()
        {
            //פעולה אסינכרונית המחזירה תגובת שרת
            return Ok(await _lookupService.GetRegionsAsync());
        }
    }
}

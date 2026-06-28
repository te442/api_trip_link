using API_trip_link.Models;

namespace API_trip_link.Services.Transit
{
    public interface IPlacesAutocompleteService
    {
        Task<List<PlaceSuggestionDto>> AutocompleteAsync(string input);
    }
}

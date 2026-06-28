using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using API_trip_link.Models;
using API_trip_link.Settings;

namespace API_trip_link.Services.Transit
{
    public class GooglePlacesAutocompleteService : IPlacesAutocompleteService
    {
        private const string DefaultPlacesBaseUrl = "https://places.googleapis.com";
        private const string PlacesAutocompletePath = "/v1/places:autocomplete";
        private const string PlacesFieldMask =
            "suggestions.placePrediction.placeId," +
            "suggestions.placePrediction.text.text," +
            "suggestions.placePrediction.structuredFormat.mainText.text," +
            "suggestions.placePrediction.structuredFormat.secondaryText.text";

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public GooglePlacesAutocompleteService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<List<PlaceSuggestionDto>> AutocompleteAsync(string input)
        {
            var query = input?.Trim() ?? "";
            if (query.Length < Configuration.Transit.PlacesAutocompleteMinInputLength)
                return new List<PlaceSuggestionDto>();

            var apiKey = _config["GoogleMaps:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new GoogleMapsApiException(Configuration.Transit.GoogleMapsApiStatusMissingKey, null);

            var baseUrl = _config["GooglePlaces:BaseUrl"] ?? DefaultPlacesBaseUrl;
            var language = _config["GoogleMaps:Language"] ?? Configuration.Transit.DefaultGoogleMapsLanguage;
            var url = $"{baseUrl.TrimEnd('/')}{PlacesAutocompletePath}";

            var requestBody = new GooglePlacesAutocompleteRequest
            {
                Input = query,
                LanguageCode = language,
                IncludedRegionCodes = new[] { Configuration.Transit.DefaultGoogleMapsRegion }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("X-Goog-Api-Key", apiKey);
            request.Headers.Add("X-Goog-FieldMask", PlacesFieldMask);
            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var error = Deserialize<GooglePlacesErrorResponse>(json);
                var message = error?.Error?.Message ?? response.ReasonPhrase;
                var status = response.StatusCode == System.Net.HttpStatusCode.Forbidden
                    ? Configuration.Transit.GoogleMapsApiStatusRequestDenied
                    : response.StatusCode.ToString();
                throw new GoogleMapsApiException(status, message);
            }

            var data = Deserialize<GooglePlacesAutocompleteResponse>(json);
            if (data == null)
                return new List<PlaceSuggestionDto>();

            return data.Suggestions
                .Where(s => s.PlacePrediction != null)
                .Select(s => s.PlacePrediction!)
                .Select(p => new PlaceSuggestionDto
                {
                    Description = p.Text?.Text ?? "",
                    MainText = p.StructuredFormat?.MainText?.Text ?? p.Text?.Text ?? "",
                    SecondaryText = p.StructuredFormat?.SecondaryText?.Text ?? "",
                    PlaceId = p.PlaceId ?? ""
                })
                .Where(p => !string.IsNullOrWhiteSpace(p.Description))
                .ToList();
        }

        private static T? Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }

        private sealed class GooglePlacesAutocompleteRequest
        {
            [JsonPropertyName("input")]
            public string Input { get; set; } = "";

            [JsonPropertyName("languageCode")]
            public string LanguageCode { get; set; } = "";

            [JsonPropertyName("includedRegionCodes")]
            public string[] IncludedRegionCodes { get; set; } = Array.Empty<string>();
        }

        private sealed class GooglePlacesAutocompleteResponse
        {
            [JsonPropertyName("suggestions")]
            public List<GooglePlaceSuggestion> Suggestions { get; set; } = new();
        }

        private sealed class GooglePlaceSuggestion
        {
            [JsonPropertyName("placePrediction")]
            public GooglePlacePrediction? PlacePrediction { get; set; }
        }

        private sealed class GooglePlacePrediction
        {
            [JsonPropertyName("placeId")]
            public string? PlaceId { get; set; }

            [JsonPropertyName("text")]
            public GooglePlaceText? Text { get; set; }

            [JsonPropertyName("structuredFormat")]
            public GoogleStructuredFormatting? StructuredFormat { get; set; }
        }

        private sealed class GoogleStructuredFormatting
        {
            [JsonPropertyName("mainText")]
            public GooglePlaceText? MainText { get; set; }

            [JsonPropertyName("secondaryText")]
            public GooglePlaceText? SecondaryText { get; set; }
        }

        private sealed class GooglePlaceText
        {
            [JsonPropertyName("text")]
            public string? Text { get; set; }
        }

        private sealed class GooglePlacesErrorResponse
        {
            [JsonPropertyName("error")]
            public GooglePlacesError? Error { get; set; }
        }

        private sealed class GooglePlacesError
        {
            [JsonPropertyName("message")]
            public string? Message { get; set; }
        }
    }
}

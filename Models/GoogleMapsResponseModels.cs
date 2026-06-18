using System.Text.Json.Serialization;

namespace API_trip_link.Models
{
    internal class GoogleDirectionsResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("routes")]
        public List<GoogleRoute> Routes { get; set; } = new();
    }

    internal class GoogleRoute
    {
        [JsonPropertyName("legs")]
        public List<GoogleLeg> Legs { get; set; } = new();
    }

    internal class GoogleLeg
    {
        [JsonPropertyName("duration")]
        public GoogleDuration Duration { get; set; } = new();

        [JsonPropertyName("steps")]
        public List<GoogleStep> Steps { get; set; } = new();
    }

    internal class GoogleStep
    {
        [JsonPropertyName("travel_mode")]
        public string TravelMode { get; set; } = "";

        [JsonPropertyName("duration")]
        public GoogleDuration Duration { get; set; } = new();

        [JsonPropertyName("transit_details")]
        public GoogleTransitDetails? TransitDetails { get; set; }
    }

    internal class GoogleTransitDetails
    {
        [JsonPropertyName("num_stops")]
        public int NumStops { get; set; }

        [JsonPropertyName("departure_time")]
        public GoogleTimeValue? DepartureTime { get; set; }

        [JsonPropertyName("arrival_time")]
        public GoogleTimeValue? ArrivalTime { get; set; }

        [JsonPropertyName("line")]
        public GoogleTransitLine? Line { get; set; }

        [JsonPropertyName("departure_stop")]
        public GoogleStop? DepartureStop { get; set; }

        [JsonPropertyName("arrival_stop")]
        public GoogleStop? ArrivalStop { get; set; }
    }

    internal class GoogleTransitLine
    {
        [JsonPropertyName("short_name")]
        public string ShortName { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("vehicle")]
        public GoogleVehicle? Vehicle { get; set; }
    }

    internal class GoogleVehicle
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "";
    }

    internal class GoogleStop
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }

    internal class GoogleDuration
    {
        [JsonPropertyName("value")]
        public int Value { get; set; }
    }

    internal class GoogleTimeValue
    {
        [JsonPropertyName("value")]
        public long Value { get; set; }
    }
}

namespace API_trip_link.Models
{
    public class AuthResponseDto
    {
        public string Token    { get; set; } = "";
        public string UserId   { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email    { get; set; } = "";
    }
}

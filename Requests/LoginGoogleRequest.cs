namespace OverflowBackend.Models.Requests
{
    public class LoginGoogleRequest
    {
        public string Email { get; set; }
        public string IdToken { get; set; }
        public string? Username { get; set; }
    }
}

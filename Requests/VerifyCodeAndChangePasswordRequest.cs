namespace OverflowBackend.Models.Requests
{
    public class VerifyCodeAndChangePasswordRequest
    {
        public string Email { get; set; }
        public string VerificationCode { get; set; }
        public string NewPassword {get; set;}
    }
}

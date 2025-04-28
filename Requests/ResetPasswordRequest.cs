namespace OverflowBackend.Models.Requests
{
    public class ResetPasswordRequest
    {
        public string OldPassword {get;set;}
        public string NewPassword {get;set;}
    }
}

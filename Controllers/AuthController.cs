using FacefusionBE.Filters;
using FacefusionBE.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using OverflowBackend.Models.Requests;


namespace OverflowBackend.Controllers
{

    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        [Route("api/signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            var result = await _authService.SignUp(request);
            return Ok(result);
        }

        [HttpPost]
        [Route("api/signin")]
        public async Task<IActionResult> SignIn([FromBody] SinInRequest request)
        {
            var result = await _authService.SignIn(request.Email, request.Password);
            return Ok(result);
        }


        [HttpPost]
        [Route("api/refreshToken")]
        public async Task<IActionResult> LoginGoogle([FromBody] RefreshRequest request)
        {
            var result = await _authService.RefreshToken(request.RefreshToken);
            return Ok(result);
        }


        [HttpPut]
        [AuthorizationFilter]
        [Route("api/resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _authService.ResetPassword((string)HttpContext.Items["email"], request.OldPassword, request.NewPassword);
            return Ok(result);
        }

        [HttpDelete]
        [AuthorizationFilter]
        [Route("api/deleteAccount")]
        public async Task<IActionResult> DeleteAccount()
        {
            var result = await _authService.DeleteAccount((string)HttpContext.Items["email"]);
            return Ok(result);
        }


        [HttpPut]
        [Route("api/verifyCodeAndChangePassword")]
        public async Task<IActionResult> VerifyCodeAndChangePassword(VerifyCodeAndChangePasswordRequest request)
        {
            var result = await _authService.VerifyCodeAndChangePassword(request.VerificationCode, request.Email, request.NewPassword);
            return Ok(result);
        }
    }
}

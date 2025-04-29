using FacefusionBE.DB;
using FacefusionBE.Filters;
using FacefusionBE.Response.DorelAppBackend.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacefusionBE.Controllers
{
    [Route("api")]
    [ApiController]
    public class CreditController: ControllerBase
    {
        private readonly FacefusionDBContext _context;
        public CreditController(FacefusionDBContext context)
        {
            _context = context;
        }

        [HttpGet("credit")]
        [AuthorizationFilter]
        public async Task<IActionResult> SwapFace()
        {
            var maybe = new Maybe<int>();
            var email = (string)HttpContext.Items["email"];
            var user = await _context.Users.FirstOrDefaultAsync(e => e.Email == email);
            if(user != null)
            {
                maybe.SetSuccess(user.Credit);
                return Ok(maybe); 
            }
            maybe.SetException("No user found");
            return Ok(maybe);
        }
    }
}

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using FacefusionBE.Helpers;

namespace FacefusionBE.Filters
{
    public class AuthorizationFilter : Attribute, IAuthorizationFilter
    {
        public string? Role { get; set; }

        public async void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {

                StringValues authorizationHeaderValues;
                if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out authorizationHeaderValues))
                {
                    string authorizationHeader = authorizationHeaderValues.FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        string token = authorizationHeader.Substring("Bearer ".Length).Trim();
                        if (Role == "admin")
                        {
                            if (!TokenHelper.IsAdmin(token))
                            {
                                context.Result = new StatusCodeResult(401);
                                return;
                            }
                        }
                        var username = TokenHelper.GetEmailFromToken(token);

                        if (TokenHelper.IsTokenExpired(token))
                        {
                            context.Result = new StatusCodeResult(403);
                            return;
                        }
                        if (!String.IsNullOrEmpty(username))
                        {
                            context.HttpContext.Items["email"] = username;
                            return;
                        }


                    }
                }
            }
            catch { }
            
            context.Result = new StatusCodeResult(401); ;
            return;
        }
    }
}


using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FacefusionBE.Helpers
{
    public static class TokenHelper
    {
        public static string GenerateJwtToken(string email, string sessionId, bool isRefreshToken = false, bool isAdmin = false)
        {
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");
            byte[] bytes = Encoding.UTF8.GetBytes(secretKey);
            string base64String = Convert.ToBase64String(bytes);
            var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(base64String));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var issuer = "overflowapp.xyz";
            var audience = "endpoint";
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email), // User identifier from Google
                new Claim(JwtRegisteredClaimNames.Jti, sessionId), // Unique JWT ID
                new Claim(JwtRegisteredClaimNames.Iss, issuer), // Token issuer
                new Claim(JwtRegisteredClaimNames.Aud, audience),
                new Claim("Role", isAdmin ? "admin" : "user")// Token audience
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: isRefreshToken ? DateTime.UtcNow.AddYears(1) : DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        public static string GetEmailFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jsonToken != null)
            {
                // Extract token claims
                foreach (var claim in jsonToken.Claims)
                {
                    if (claim.Type == JwtRegisteredClaimNames.Sub)
                    {
                        return claim.Value;
                    }
                }
            }
            return null;
        }

        public static string GetSessionIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jsonToken != null)
            {
                // Extract token claims
                foreach (var claim in jsonToken.Claims)
                {
                    if (claim.Type == JwtRegisteredClaimNames.Jti)
                    {
                        return claim.Value;
                    }
                }
            }
            return null;
        }

        public static bool IsAdmin(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jsonToken != null)
            {
                // Extract token claims
                foreach (var claim in jsonToken.Claims)
                {
                    if (claim.Type == "Role")
                    {
                        return claim.Value == "admin";
                    }
                }
            }
            return false;
        }

        public static bool IsTokenExpired(string tokenString)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadToken(tokenString) as JwtSecurityToken;

            if (token == null)
                throw new ArgumentException("Invalid token");

            // Check if the token has an expiration claim
            if (!token.Payload.TryGetValue(JwtRegisteredClaimNames.Exp, out var exp))
                throw new ArgumentException("Token doesn't have an expiration claim");

            // Get the expiration time from the claim
            var expirationTime = DateTimeOffset.FromUnixTimeSeconds((long)exp);

            // Check if the token is expired
            var result = expirationTime.UtcDateTime <= DateTime.UtcNow;
            return result;
        }
    }
}

using BusinessObjects.Exceptions;
using Microsoft.AspNetCore.Http;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class TokenService : ITokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TokenService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string GetUserIdFromToken()
        {
            // Get the Authorization header from the HTTP context
            var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                throw new ErrorException(StatusCodes.Status401Unauthorized, ErrorCode.UNAUTHORIZED, "Authorization token is missing or invalid");
            }

            // Extract the JWT from the header
            var token = authorizationHeader.Substring("Bearer ".Length).Trim();

            // Decode the JWT to retrieve the claims
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Extract the userId claim
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new ErrorException(StatusCodes.Status401Unauthorized, ErrorCode.UNAUTHORIZED, "User ID not found in token");
            }

            return userIdClaim;
        }
    }
}

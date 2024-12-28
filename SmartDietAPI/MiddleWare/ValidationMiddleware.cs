using BusinessObjects.Exceptions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace SmartDietAPI.MiddleWare
{
    public class ValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TokenValidationParameters _tokenValidationParams;

        public ValidationMiddleware(RequestDelegate next, TokenValidationParameters tokenValidationParams)
        {
            _next = next;
            _tokenValidationParams = tokenValidationParams;
        }

        public async Task Invoke(HttpContext context)
        {
            //var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            var tokenCookie = context.Request.Cookies.TryGetValue("accessToken", out var cookie);
            //if (cookie == null)
            //{
            //    cookie = context.Request.Headers["Authorization"];
            //    if (!string.IsNullOrEmpty(cookie) && tokenCookie.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            //    {
            //        cookie = cookie.ToString().Substring(7).Trim();
            //    }
            //}    
            if (!string.IsNullOrEmpty(cookie))
            {
                var jwtTokenHandler = new JwtSecurityTokenHandler();
                try
                {
                    // Validate the token
                    var tokenInVerification = jwtTokenHandler.ValidateToken(cookie, _tokenValidationParams, out var validatedToken);

                    if (validatedToken is JwtSecurityToken jwtSecurityToken)
                    {
                        var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                        if (!result)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            var errorResponse = new
                            {
                                error = "invalid_token",
                                error_description = "The signature key was not found or token is invalid."
                            };
                            await context.Response.WriteAsJsonAsync(errorResponse);
                            return;
                        }

                        // Check if token has expired
                        var expClaim = jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Exp)?.Value;
                        if (expClaim != null && long.TryParse(expClaim, out var exp))
                        {
                            var expDateTime = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                            if (expDateTime < DateTime.UtcNow)
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                var errorResponse = new
                                {
                                    error = "expired_token",
                                    error_description = "The token has expired."
                                };
                                await context.Response.WriteAsJsonAsync(errorResponse);
                                return;
                            }
                        }
                    }
                }
                catch (SecurityTokenException ex)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    var errorResponse = new
                    {
                        error = "invalid_token",
                        error_description = "Token validation failed. Error: " + ex.Message
                    };
                    await context.Response.WriteAsJsonAsync(errorResponse);
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    var errorResponse = new
                    {
                        error = "invalid_token",
                        error_description = "An error occurred while processing the token. Error: " + ex.Message
                    };
                    await context.Response.WriteAsJsonAsync(errorResponse);
                    return;
                }
            }

            await _next(context);
        }
    }
}

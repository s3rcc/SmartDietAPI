using BusinessObjects.Exceptions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

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
                            context.Items["Error"] = new ErrorException
                            (
                                    statusCode: 500,
                                    errorCode: ErrorCode.BADREQUEST,
                                   "Token is invalid."
                            );
                        }

                        // Check if token has expired
                        var expClaim = jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Exp)?.Value;
                        if (expClaim != null && long.TryParse(expClaim, out var exp))
                        {
                            var expDateTime = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                            if (expDateTime < DateTime.UtcNow)
                            {
                                context.Items["Error"] = new ErrorException
                                (
                                    statusCode: 500,
                                    errorCode:ErrorCode.BADREQUEST,
                                    "Token has expired."
                                );
                            }
                        }
                    }
                }
                catch (SecurityTokenException ex)
                {
                    context.Items["Error"] = new ErrorException
                    (
                        statusCode: 500,
                        errorCode: ErrorCode.BADREQUEST,
                        "Token validation failed. Error: " + ex.Message
                    );
                }
                catch (Exception ex)
                {
                    context.Items["Error"] = new ErrorException
                    (
                        statusCode: 500,
                        errorCode: ErrorCode.BADREQUEST,
                        "An error occurred while processing the token. Error: " + ex.Message
                    );
                }
            }

            await _next(context);
        }
    }
}

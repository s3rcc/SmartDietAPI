using DTOs.AuthDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace SmartDietAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpGet("Auth_Account")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            AuthResponse? result = await _authService.Login(request);
            return Ok(result);
        }
        [HttpPost("New_Account")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            await _authService.Register(request);
            return Ok("Register succesfully");
        }
        [Authorize]
        [HttpPatch("Change_password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            await _authService.ChangePassword(request);
            return Ok("Change password succesfully");
        }
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest model)
        {
            AuthResponse? result = await _authService.RefreshToken(model);
            return Ok(result);
        }

        [HttpPatch("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
        {
            await _authService.ResetPassword(model);
            return Ok("Reset password succesfully");
        }

    }
}

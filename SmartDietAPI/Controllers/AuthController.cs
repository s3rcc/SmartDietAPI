using BusinessObjects.Base;
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
        [HttpPost("Auth_Account")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            AuthResponse? result = await _authService.Login(request);
            return Ok(ApiResponse<object>.Success(result));
        }
        [HttpPost("New_Account")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            await _authService.Register(request);
            return Ok(ApiResponse<object>.Success(null, "Register successfully"));
        }
        [HttpPatch("Confirm_Email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmOtpRequest request)
        {
            await _authService.VerifyOtp(request, false);
            return Ok(ApiResponse<object>.Success(null, "Verify successfully"));
        }
        [HttpPatch("Resend_Confirmation_Email")]
        public async Task<IActionResult> ResendConfirmationEmail(EmailRequest request)
        {
            await _authService.ResendConfirmationEmail(request);
            return Ok(ApiResponse<object>.Success(null, "Email have been sent"));
        }

        [Authorize]
        [HttpPatch("Change_password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            await _authService.ChangePassword(request);
            return Ok(ApiResponse<object>.Success(null, "Change password successfully"));
        }
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(EmailRequest model)
        {
            await _authService.ForgotPassword(model);
            return Ok(ApiResponse<object>.Success(null, "OTP have been sent to your mail to verify new password."));
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            AuthResponse? result = await _authService.RefreshToken(request);
            return Ok(ApiResponse<object>.Success(result));

        }

        [HttpPatch("Confirm_OTP_ResetPassword")]
        public async Task<IActionResult> ConfirmOTPResetPassword(ConfirmOtpRequest request)
        {
            await _authService.VerifyOtp(request, true);
            return Ok(ApiResponse<object>.Success(null, "Redirect to reset password"));
        }


        [HttpPatch("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            await _authService.ResetPassword(request);
            return Ok(ApiResponse<object>.Success(null, "Reset password successfully"));
        }
        [HttpPost("signin-google")]
        public async Task<IActionResult> LoginGoogle([FromBody] TokenGoogleRequest request)
        {
            AuthResponse? result = await _authService.LoginGoogle(request);
            return Ok(ApiResponse<object>.Success(result));
        }
    }
}

using DTOs.AuthDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> Login(LoginRequest request);
        Task<AuthResponse> RefreshToken(RefreshTokenRequest request);
        Task Register(RegisterRequest request);
        Task ChangePassword(ChangePasswordRequest request);
        Task ResetPassword(ResetPasswordRequest request);
        Task ForgotPassword(EmailRequest request);
        Task<AuthResponse> LoginGoogle(TokenGoogleRequest request);
        Task VerifyOtp(ConfirmOtpRequest request, bool isResetPassword);
        Task ResendConfirmationEmail(EmailRequest request);


    }
}

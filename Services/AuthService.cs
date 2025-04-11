using AutoMapper;
using BusinessObjects.Entity;
using BusinessObjects.Exceptions;
using DTOs.AuthDTOs;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories.Interfaces;
using Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<SmartDietUser> _userManager;
        private readonly SignInManager<SmartDietUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly IEmailService _emailService;

        public AuthService(UserManager<SmartDietUser> userManager,
                           SignInManager<SmartDietUser> signInManager,
                           RoleManager<IdentityRole> roleManager,
                           IConfiguration configuration,
                           IMapper mapper,
                           IUnitOfWork unitOfWork,
                           IHttpContextAccessor contextAccessor,
                           IMemoryCache memoryCache,
                           IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _contextAccessor = contextAccessor;
            _configuration = configuration;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            _memoryCache = memoryCache;
            _emailService = emailService;
        }
        //private void SetTokenInsideCookie(string name, string value, DateTimeOffset time, HttpContext context)
        //{
        //    context.Response.Cookies.Append(name, value,
        //    new CookieOptions
        //    {
        //        Expires = time,
        //        HttpOnly = true,
        //        IsEssential = true,
        //        Secure = true,
        //        SameSite = SameSiteMode.None,
        //    });
        //}
        private async Task<SmartDietUser> CheckRefreshToken(string refreshToken)
        {
            List<SmartDietUser> users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                var token = await _userManager.GetAuthenticationTokenAsync(user, "Default", "RefreshToken");
                if (token == refreshToken)
                    return user;
            }
            throw new ErrorException(401, ErrorCode.UNAUTHORIZED, "Token not valid");
        }
        private (string token, IEnumerable<string> roles) GenerateJwtToken(SmartDietUser user)
        {
            SecurityKey key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]));
            List<Claim> claims = new List<Claim>
            {
               new Claim(ClaimTypes.NameIdentifier,user.Id),
               new Claim(ClaimTypes.Email,user.Email),
            };
            IEnumerable<string> roles = _userManager.GetRolesAsync(user).Result;
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            };
            JwtSecurityTokenHandler securityTokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = securityTokenHandler.CreateToken(securityTokenDescriptor);
            return (securityTokenHandler.WriteToken(token), roles);
        }

        private async Task<string> GenerateRefreshToken(SmartDietUser user)
        {
            string? refreshToken = Guid.NewGuid().ToString();
            string? tokenUsed = await _userManager.GetAuthenticationTokenAsync(user, "Default", "RefreshToken");
            if (tokenUsed != null)
            {
                await _userManager.RemoveAuthenticationTokenAsync(user, "Default", "RefreshToken");

            }
            await _userManager.SetAuthenticationTokenAsync(user, "Default", "RefreshToken", refreshToken);
            return refreshToken;
        }

        private string GenerateOTP()
        {
            Random random = new Random();
            string otp = random.Next(0, 10000).ToString();
            return otp;
        }

        public async Task<AuthResponse> Login(LoginRequest request)
        {
            if(request.Email == null)    
               throw new ErrorException(404, ErrorCode.NOT_FOUND, "Please input email");
            if (request.Password == null)
                throw new ErrorException(404, ErrorCode.NOT_FOUND, "Please input password");


            var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new ErrorException(404, ErrorCode.NOT_FOUND, "User not found");
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new ErrorException(StatusCodes.Status406NotAcceptable, ErrorCode.BADREQUEST, "User not confirm");
            }
            SignInResult result = await _signInManager.PasswordSignInAsync(user, request.Password, false, false);
            if (!result.Succeeded)
            {
                throw new ErrorException(401, ErrorCode.UNAUTHORIZED, "Wrong password");
            }

            _contextAccessor.HttpContext.Session.SetString("UserId", user.Id);
            (string token, IEnumerable<string> roles) = GenerateJwtToken(user);
            string refreshToken = await GenerateRefreshToken(user);

            return new AuthResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                User = new UserInfo
                {
                    Email = user.Email,
                    Roles = roles.ToList(),
                }
            };

        }

        public async Task<AuthResponse> RefreshToken(RefreshTokenRequest request)
        {
            if (request.refreshToken == null)
                throw new ErrorException(404, ErrorCode.NOT_FOUND, "Please input refreshToken");
            SmartDietUser user = await CheckRefreshToken(request.refreshToken);
            (string token, IEnumerable<string> roles) = GenerateJwtToken(user);
            string refreshToken = await GenerateRefreshToken(user);

            return new AuthResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                User = new UserInfo
                {
                    Email = user.Email,
                    Roles = roles.ToList(),
                }
            };
        }

        public async Task Register(RegisterRequest request)
        {
            if (request.Email == null)
                throw new ErrorException(404, ErrorCode.NOT_FOUND, "Please input email");
            if (request.Name == null)
                throw new ErrorException(404, ErrorCode.NOT_FOUND, "Please input name");
            if (request.Password == null)
                throw new ErrorException(404, ErrorCode.NOT_FOUND, "Please input password");
            if (!string.IsNullOrEmpty(request.PhoneNumber))
            { 
                if(request.PhoneNumber.Length != 10 || !request.PhoneNumber.All(char.IsDigit))
                {
                    throw new ErrorException(404, ErrorCode.NOT_FOUND, "Please input right number phone");
                }
            }
            SmartDietUser? user = await _userManager.FindByEmailAsync(request.Email);
            if (user != null)
            {
                throw new ErrorException(400, ErrorCode.BADREQUEST, "Email have been registed");
            }
            var newUser = _mapper.Map<SmartDietUser>(request);
            newUser.UserName = request.Email;
            IdentityResult result = await _userManager.CreateAsync(newUser, request.Password);
            if (result.Succeeded)
            {
                await _unitOfWork.Repository<UserProfile>().AddAsync(new UserProfile
                {
                    SmartDietUserId = newUser.Id,
                    FullName = request.Name,
                    ProfilePicture = "",
                    TimeZone = "UTC",
                    PreferredLanguage = "en",
                    EnableEmailNotifications = true,
                    EnableNotifications = true,
                    EnablePushNotifications = true,
                    CreatedBy = newUser.Id,
                });
                await _unitOfWork.SaveChangeAsync();
                bool roleExist = await _roleManager.RoleExistsAsync("Member");
                if (!roleExist)
                {
                    await _roleManager.CreateAsync(new IdentityRole { Name = "Member" });
                }
                await _userManager.AddToRoleAsync(newUser, "Member");
                string OTP = GenerateOTP();
                string cacheKey = $"OTP_{request.Email}";
                _memoryCache.Set(cacheKey, OTP, TimeSpan.FromMinutes(10));
                await _emailService.SendEmailAsync(request.Email, "Verify Your Account", OTP, "AccountVerification");
            }
            else
            {
                throw new ErrorException(500, ErrorCode.INTERNAL_SERVER_ERROR, $"Error when creating user {result.Errors.FirstOrDefault()?.Description}");
            }
        }
        public async Task VerifyOtp(ConfirmOtpRequest request, bool isResetPassword)
        {
            if (request.Email == null)
                throw new ErrorException(404, ErrorCode.NOT_FOUND, "Please input email");
            if (request.OTP == null || !request.OTP.All(char.IsDigit))
                throw new ErrorException(404, ErrorCode.NOT_FOUND, "Please input right OTP");
            string cacheKey = isResetPassword ? $"OTPResetPassword_{request.Email}" : $"OTP_{request.Email}";
            if (_memoryCache.TryGetValue(cacheKey, out string memory))
            {
                if (memory == request.OTP)
                {
                    SmartDietUser? user = await _userManager.FindByEmailAsync(request.Email);
                    string? token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _userManager.ConfirmEmailAsync(user, token);
                    _memoryCache.Remove(cacheKey);
                    if (isResetPassword)
                    {
                        var tokenReset = await _userManager.GeneratePasswordResetTokenAsync(user);
                        _memoryCache.Set($"ResetPassword_{user.Email}", tokenReset, TimeSpan.FromMinutes(10));
                    }
                }
                else
                {
                    throw new ErrorException(500, ErrorCode.BADREQUEST, "Otp not valid");
                }
            }
            else
            {
                throw new ErrorException(500, ErrorCode.BADREQUEST, "Otp not valid");
            }
        }
        public async Task ResendConfirmationEmail(EmailRequest request)
        {
            if (request.Email == null)
                throw new ErrorException(404, ErrorCode.NOT_FOUND, "Please input email");
            var otp = GenerateOTP();
            var cacheKey = $"OTP_{request.Email}";
            if (_memoryCache.TryGetValue(cacheKey, out var memory))
            {
                throw new ErrorException(500, ErrorCode.BADREQUEST, "OTP have been sent");
            }
            _memoryCache.Set(cacheKey, otp, TimeSpan.FromMinutes(10));
            await _emailService.SendEmailAsync(request.Email, "Verify Your Account", otp, "AccountVerification");
        }
        public async Task ChangePassword(ChangePasswordRequest request)
        {
            if (request.OldPassword == null)
                throw new ErrorException(404, ErrorCode.NOT_FOUND, "Please input old password");
            if (request.NewPassword == null)
                throw new ErrorException(404, ErrorCode.NOT_FOUND, "Please input new password");
            string userId = _contextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new ErrorException(401, ErrorCode.UNAUTHORIZED, "Something not correct");
            SmartDietUser? user = await _userManager.FindByIdAsync(userId);
            IdentityResult result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                throw new ErrorException(500, ErrorCode.INTERNAL_SERVER_ERROR, result.Errors.First().Description);
            }
        }

        public async Task ResetPassword(ResetPasswordRequest request)
        {
            if (request.Email == null)
                throw new ErrorException(404, ErrorCode.NOT_FOUND, "Please input email");
            if (request.Password == null)
                throw new ErrorException(404, ErrorCode.NOT_FOUND, "Please input password");
            var cacheKey = $"ResetPassword_{request.Email}";
            if (_memoryCache.TryGetValue(cacheKey, out string memory))
            {
                SmartDietUser user = await _userManager.FindByEmailAsync(request.Email)
                ?? throw new ErrorException(404, ErrorCode.NOT_FOUND, "User not found");

                //var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    throw new ErrorException(StatusCodes.Status406NotAcceptable, ErrorCode.BADREQUEST, "User not confirm");
                }

                IdentityResult result = await _userManager.ResetPasswordAsync(user, memory, request.Password);

                if (!result.Succeeded)
                {
                    throw new ErrorException(500, ErrorCode.BADREQUEST, result.Errors.First().Description);
                }

                _memoryCache.Remove(cacheKey);

            }
            else
            {
                throw new ErrorException(500, ErrorCode.BADREQUEST, "OTP Reset password not confirm");
            }

        }

        public async Task ForgotPassword(EmailRequest request)
        {
            if (request.Email == null)
                throw new ErrorException(404, ErrorCode.NOT_FOUND, "Please input email");
            SmartDietUser? user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new ErrorException(400, ErrorCode.BADREQUEST, "User not found");
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new ErrorException(400, ErrorCode.BADREQUEST, "User not confirm");
            }
            string OTP = GenerateOTP();
            string cacheKey = $"OTPResetPassword_{user.Email}";
            _memoryCache.Set(cacheKey, OTP, TimeSpan.FromMinutes(10));
            await _emailService.SendEmailAsync(user.Email, "Reset Your Password", OTP, "ResetPassword");

        }
        public async Task<AuthResponse> LoginGoogle(TokenGoogleRequest request)
        {
            if (request.token == null)
                throw new ErrorException(404, ErrorCode.NOT_FOUND, "Please input token");
            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(request.token);
            string email = payload.Email;
            string providerKey = payload.Subject;
            SmartDietUser? user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = _mapper.Map<SmartDietUser>(new { email });
                user.Email = email;
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    bool roleExist = await _roleManager.RoleExistsAsync("Member");
                    if (!roleExist)
                    {
                        await _roleManager.CreateAsync(new IdentityRole { Name = "Member" });
                    }
                    await _userManager.AddToRoleAsync(user, "Member");
                    UserLoginInfo? info = new("Google", providerKey, "Google");
                    IdentityResult identityResult = await _userManager.AddLoginAsync(user, info);
                    if (!identityResult.Succeeded)
                    {
                        throw new ErrorException(500, ErrorCode.INTERNAL_SERVER_ERROR, $"Error when created user {identityResult.Errors.First().Description}");
                    }
                }
                else
                {
                    throw new ErrorException(500, ErrorCode.INTERNAL_SERVER_ERROR, $"Error when created user {result.Errors.First().Description}");
                }
            }
            (string token, IEnumerable<string> roles) = GenerateJwtToken(user);
            string refreshToken = await GenerateRefreshToken(user);


            return new AuthResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                User = new UserInfo
                {
                    Email = user.Email,
                    Roles = roles.ToList()
                }
            };
        }


    }
}

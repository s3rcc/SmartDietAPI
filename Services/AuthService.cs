using AutoMapper;
using BusinessObjects.Entity;
using BusinessObjects.Exceptions;
using DTOs.AuthDTOs;
using MailKit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<SmartDietUser> _userManager;
        private readonly SignInManager<SmartDietUser> _signInManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<SmartDietUser> userManager,
                           SignInManager<SmartDietUser> signInManager,
                           RoleManager<IdentityRole<Guid>> roleManager,
                           IConfiguration configuration,
                           IMapper mapper,
                           IUnitOfWork unitOfWork,
                           IHttpContextAccessor contextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _contextAccessor = contextAccessor;
            _configuration = configuration;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            
        }

        private async Task<SmartDietUser> CheckRefreshToken(string refreshToken)
        {
            List<SmartDietUser> users = await _userManager.Users.ToListAsync();
            foreach(var user in users)
            {
                var token = await _userManager.GetAuthenticationTokenAsync(user, "Default", "RefreshToken");
                if(token == refreshToken)
                {
                    return user;
                }    
            }
            throw new ErrorException(401, ErrorCode.UNAUTHORIZED, "Token not valid");
        }
        private (string token, IEnumerable<string> roles) GenerateJwtToken(SmartDietUser user)
        {
            byte[] key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
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
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.Sha256)
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
            string otp =  random.Next(0, 10000).ToString();
            return otp;
        }

        public async Task<AuthResponse> Login(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email)
            ??  throw new ErrorException(404,ErrorCode.NOT_FOUND,"User not found");
            SignInResult result = await _signInManager.PasswordSignInAsync(user,request.Password, false,false);
            if (!result.Succeeded)
            {
                throw new ErrorException(401, ErrorCode.UNAUTHORIZED, "Wrong password");
            }
            _contextAccessor.HttpContext.Session.SetString("UserId",user.Id);
            (string token, IEnumerable<string> roles)  = GenerateJwtToken(user);
            string refreshToken = await GenerateRefreshToken(user);
            return new AuthResponse {
                    AccessToken = token,
                    RefreshToken = refreshToken,
                    TokenType = "Jwt",
                    AuthType = "Bearer",
                    ExpiresIn = DateTime.UtcNow.AddHours(1),
                    User = new UserInfo
                    {
                        Email = user.Email,
                        Roles = roles.ToList(),
                    }
            };
            
        }

        public async Task<AuthResponse> RefreshToken(RefreshTokenRequest request)
        {
            SmartDietUser user = await CheckRefreshToken(request.refreshToken);
            (string token, IEnumerable<string> roles) = GenerateJwtToken(user);
            string refreshToken = await GenerateRefreshToken(user);
            return new AuthResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                AuthType = "Bearer",
                TokenType = "Jwt",
                ExpiresIn = DateTime.UtcNow.AddHours(1),
                User = new UserInfo
                {
                    Email = user.Email,
                    Roles = roles.ToList(),
                }
            };
        }

        public async Task Register(RegisterRequest request)
        {
            SmartDietUser? user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new ErrorException(400, ErrorCode.BADREQUEST, "Email have been registed");
            var newUser = _mapper.Map<SmartDietUser>(request);
            newUser.UserName = request.Email;
            IdentityResult result = await _userManager.CreateAsync(newUser, request.Password);
            if (result.Succeeded)
            {
                bool roleExist = await _roleManager.RoleExistsAsync("Member");
                if (!roleExist)
                {
                    await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = "Member" });
                }
                await _userManager.AddToRoleAsync(newUser, "Member");
            }
            else
            {
                throw new ErrorException(500, ErrorCode.INTERNAL_SERVER_ERROR, $"Error when creating user {result.Errors.FirstOrDefault()?.Description}");
            }
        }

        public async Task ChangePassword(ChangePasswordRequest request)
        {
            string userId = _contextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new ErrorException(401, ErrorCode.UNAUTHORIZED, "Something not correct");
            SmartDietUser? user = await _userManager.FindByIdAsync(userId);
            IdentityResult result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
                if(!result.Succeeded)
            {
                throw new ErrorException(500, ErrorCode.INTERNAL_SERVER_ERROR, result.Errors.First().Description);
            }
        }

        public async Task ResetPassword(ResetPasswordRequest request)
        {
            SmartDietUser user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new ErrorException(404, ErrorCode.NOT_FOUND, "User not found");
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            IdentityResult result = await _userManager.ResetPasswordAsync(user, token, request.Password);
            if (!result.Succeeded) 
            {
                throw new ErrorException(500, ErrorCode.BADREQUEST, result.Errors.First().Description);
            }
        }

        public async Task ForgotPassword(string email)
        {
            SmartDietUser? user = await _userManager.FindByEmailAsync(email)
            ?? throw new ErrorException(400, ErrorCode.BADREQUEST, "User not found");

        }

    }
}

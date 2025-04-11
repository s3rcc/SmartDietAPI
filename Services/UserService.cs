using AutoMapper;
using BusinessObjects.Base;
using BusinessObjects.Entity;
using BusinessObjects.Exceptions;
using DTOs.AuthDTOs;
using DTOs.DishDTOs;
using DTOs.UserProfileDTos;
using Google.Apis.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class UserService : IUserService
    {
        private readonly IEmailService _emailService ;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<SmartDietUser> _userManager;
        private readonly SignInManager<SmartDietUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IMapper _mapper;

        public UserService(
            IUnitOfWork unitOfWork, 
            UserManager<SmartDietUser> userManager, 
            SignInManager<SmartDietUser> signInManager, 
            RoleManager<IdentityRole> roleManager,
            IHttpContextAccessor contextAccessor,
            IMapper mapper,
            IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _contextAccessor = contextAccessor;
            _mapper = mapper;
            _emailService = emailService;
        }

        public async Task UpdateUserProfiles(UpdateUserProfileRequest input)
        {
            string? userId = _contextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                throw new ErrorException(StatusCodes.Status401Unauthorized, ErrorCode.UNAUTHORIZED, "Token not valid");

            SmartDietUser? user = await _userManager.Users.Include(x => x.UserProfile).FirstOrDefaultAsync(x => x.Id == userId) ??
                throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "User not exist");

            UserProfile userProfile = await _unitOfWork.Repository<UserProfile>().GetByIdAsync(user.UserProfile.Id);
            if (userProfile.DeletedBy != null)
                throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "User have been deleted");

            _mapper.Map(input, userProfile);
            userProfile.LastUpdatedBy = user.Id;
            userProfile.LastUpdatedTime = DateTime.UtcNow;

            await _unitOfWork.Repository<UserProfile>().UpdateAsync(userProfile);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task DeleteUser(string userId)
        {
            if(string.IsNullOrEmpty(userId))
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "UserId cannot be null. ");  

            string? currentUser = _contextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                throw new ErrorException(StatusCodes.Status401Unauthorized, ErrorCode.UNAUTHORIZED, "Token not valid. ");

            var user = await _userManager.FindByIdAsync(currentUser)
                ?? throw new Exception("Please login.");

            var roles = await _userManager.GetRolesAsync(user);

            if (!roles.Contains("Admin") && currentUser != userId)
                throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "User does not have permission. ");

            SmartDietUser? userExist = await _userManager.Users.Include(x => x.UserProfile).FirstOrDefaultAsync(x => x.Id == userId) ??
                throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "User not found. ");

            UserProfile userProfile = await _unitOfWork.Repository<UserProfile>().GetByIdAsync(userExist.UserProfile.Id);
            if(userProfile.DeletedBy != null)
               throw new ErrorException(StatusCodes.Status404NotFound,ErrorCode.NOT_FOUND,"User have been deleted. ");

            userProfile.DeletedBy = currentUser;
            userProfile.DeletedTime = DateTime.UtcNow;
            
            await _unitOfWork.Repository<UserProfile>().UpdateAsync(userProfile);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task<UserProfileResponse> GetUserProfile()
        {
            string? userId = _contextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                throw new ErrorException(StatusCodes.Status401Unauthorized, ErrorCode.UNAUTHORIZED, "Token not valid");

            SmartDietUser? userExist = await _userManager.Users.Include(x => x.UserProfile).FirstOrDefaultAsync(x => x.Id == userId) ??
                throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "User not found. ");

            UserProfile userProfile = await _unitOfWork.Repository<UserProfile>().GetByIdAsync(userExist.UserProfile.Id);
            if (userProfile.DeletedBy != null)
                throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "User have been deleted");
            
            var response = _mapper.Map<UserProfileResponse>(userProfile);
            return response;
        }
        private string CreateNumericPassword()
        {
            const int length = 6; 
            const string digits = "0123456789"; 
            Random random = new Random();
            char[] password = new char[length];

            for (int i = 0; i < length; i++)
            {
                password[i] = digits[random.Next(digits.Length)];
            }

            return new string(password);
        }
        public async Task<IEnumerable<UserProfileResponse>> GetAllUserProfile()
        {
            try
            {
                var userProfile = await _unitOfWork.Repository<UserProfile>().GetAllAsync();
                return _mapper.Map<IEnumerable<UserProfileResponse>>(userProfile);
            }
            catch
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, "An error occured");
            }
        }
        public async Task<BasePaginatedList<UserProfileResponse>> GetAllUserProfileAsync(int pageIndex, int pageSize, string? searchTerm)
        {
            try
            {
                BasePaginatedList<UserProfile> userProfile = await _unitOfWork.Repository<UserProfile>().GetAllWithPaginationAsync(
                    pageIndex,
                    pageSize,
                    searchTerm: x => string.IsNullOrEmpty(searchTerm) || x.FullName.Contains(searchTerm),
                    orderBy: x => x.OrderBy(d => d.FullName));
                if (userProfile == null || !userProfile.Items.Any())
                {
                    return new BasePaginatedList<UserProfileResponse>(new List<UserProfileResponse>(), 0, pageIndex, pageSize);
                }
                var responses = _mapper.Map<List<UserProfileResponse>>(userProfile.Items);
                return new BasePaginatedList<UserProfileResponse>(
                    responses,
                    responses.Count,
                    pageIndex,
                    pageSize);
            }
            catch
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, "An error occured");
            }
        }

        public async Task AddUserWithRoleAsync(RegisterUserWithRoleRequest request)
        {
            string? currentUser = _contextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
             ?? throw new ErrorException(StatusCodes.Status401Unauthorized, ErrorCode.UNAUTHORIZED, "Token not valid. ");

            SmartDietUser? userExists = await _userManager.FindByEmailAsync(request.Email);
            if (userExists != null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Email is already exist. ");
            }
            SmartDietUser? newUser = _mapper.Map<SmartDietUser>(request);
            newUser.EmailConfirmed = true;
            newUser.UserName = request.Email;
            string passwordChars = CreateNumericPassword();

            IdentityResult? result = await _userManager.CreateAsync(newUser, passwordChars);
            if (result.Succeeded)
            {
                IdentityRole? role = await _roleManager.FindByIdAsync(request.RoleID);
                await _userManager.AddToRoleAsync(newUser, role.Name);
                await _emailService.SendEmailAsync(request.Email, "Account employee",
                      $"Your password: {passwordChars}", "NewStaff");
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
            }
            else
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, result.Errors.First().Description);
            }

        }
    }
}

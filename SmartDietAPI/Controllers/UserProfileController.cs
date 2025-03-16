using BusinessObjects.Base;
using DTOs.UserProfileDTos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartDietAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : Controller
    {
        private readonly IUserProfileService _userProfileService;

        public UserProfileController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        [Authorize]
        // Get user profile by ID
        [HttpGet()]
        public async Task<IActionResult> GetUserProfile()
        {
            var result = await _userProfileService.GetUserProfileAsync();
            return Ok(ApiResponse<object>.Success(result));
        }
        [Authorize]
        // Create a new user profile
        [HttpPost("create")]
        public async Task<IActionResult> CreateUserProfile( UserProfileDTO userProfileDTO)
        {
            await _userProfileService.CreateUserProfileAsync(userProfileDTO);
            return Ok(ApiResponse<object>.Success(null, "User profile created successfully", 201));
        }
        [Authorize]
        // Update an existing user profile
        [HttpPut()]
        public async Task<IActionResult> UpdateUserProfile(UserProfileDTO userProfileDTO)
        {
            await _userProfileService.UpdateUserProfileAsync(userProfileDTO);
            return Ok(ApiResponse<object>.Success(null, "User profile updated successfully"));
        }
    }
}
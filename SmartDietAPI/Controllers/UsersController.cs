using BusinessObjects.Base;
using DTOs.UserProfileDTos;
using Google.Apis.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Interfaces;

namespace SmartDietAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpPost("add-user-with-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUserWithRoleAsync(RegisterUserWithRoleRequest request)
        {
            await _userService.AddUserWithRoleAsync(request);
            return Ok(ApiResponse<object>.Success("Create user successfully"));
        }
        [HttpPut("update-user-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileRequest request)
        {
            await _userService.UpdateUserProfiles(request);
            return Ok(ApiResponse<object>.Success("Update profile successfully"));

        }
        [HttpDelete("{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser( string userId)
        {

            await _userService.DeleteUser(userId);
            return Ok(ApiResponse<object>.Success("Delete user successfully"));
        }
        [HttpGet("profile")]
        //[Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            var userProfile = await _userService.GetUserProfile();
            return Ok(ApiResponse<object>.Success(userProfile));
        }
        [HttpGet("all")]
        //[Authorize]
        public async Task<IActionResult> GetUserProfiles([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTearm = null)
        {
            var result = await _userService.GetAllUserProfileAsync(pageIndex, pageSize, searchTearm);
            return Ok(ApiResponse<object>.Success(result));
        }
    }
}

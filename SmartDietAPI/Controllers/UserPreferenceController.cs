using BusinessObjects.Base;
using DTOs.FavoriteMealDTOs;
using DTOs.UserPreferenceDTOs;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Interfaces;

namespace SmartDietAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserPreferenceController : Controller
    {
        private readonly IUserPreferenceService _userPreferenceService;

        public UserPreferenceController(IUserPreferenceService userPreferenceService)
        {
            _userPreferenceService = userPreferenceService;
        }

        [HttpGet()]
        public async Task<IActionResult> GetUserPreferenceById()
        {
            var result = await _userPreferenceService.GetUserPreferenceByIdAsync();
            return Ok(ApiResponse<object>.Success(result));
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddUserPreference(UserPreferenceDTO userPreferenceDto)
        {
            await _userPreferenceService.CreateUserPreferenceAsync(userPreferenceDto);
            return Ok(ApiResponse<object>.Success(null, "Preference created successfully", 201));
        }

        [HttpPut()]
        public async Task<IActionResult> UpdateUserPreference(UserPreferenceDTO userPreferenceDto)
        {
            await _userPreferenceService.UpdateUserPreferenceAsync(userPreferenceDto);
            return Ok(ApiResponse<object>.Success(null, "Preference updated successfully"));
        }
    }
}

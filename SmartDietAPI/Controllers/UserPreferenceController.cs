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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserPreferenceById(string id)
        {
            var result = await _userPreferenceService.GetUserPreferenceByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddUserPreference(UserPreferenceDTO userPreferenceDto)
        {
            await _userPreferenceService.CreateUserPreferenceAsync(userPreferenceDto);
            return Ok();
        }

        [HttpPut("{userPreferenceId}")]
        public async Task<IActionResult> UpdateUserPreference(string userPreferenceId, UserPreferenceDTO userPreferenceDto)
        {
            await _userPreferenceService.UpdateUserPreferenceAsync(userPreferenceId, userPreferenceDto);
            return Ok();
        }
    }
}

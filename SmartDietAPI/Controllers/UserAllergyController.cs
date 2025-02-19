using BusinessObjects.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace SmartDietAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserAllergyController : ControllerBase
    {
        private IUserAllergyService _userAllergyService;
        public UserAllergyController(IUserAllergyService userAllergyService)
        {
            _userAllergyService = userAllergyService;
        }

        [HttpGet]
        public async Task<IActionResult> UserAllergies() 
        {
            var result = await _userAllergyService.GetUserAllergies();
            return Ok(ApiResponse<object>.Success(result));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserAllergies(List<string> foodIdsToAdd, List<string> foodIdsToDelete)
        {
            await _userAllergyService.UpdateUserAllergies(foodIdsToAdd, foodIdsToDelete);
            return Ok(ApiResponse<object>.Success(null, "Allergies updated successfully"));
        }
        
    }
}

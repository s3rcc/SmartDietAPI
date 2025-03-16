using BusinessObjects.Base;
using DTOs.UserAllergyDTOs;
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

        //[HttpGet]
        //public async Task<IActionResult> UserAllergies() 
        //{
        //    var result = await _userAllergyService.GetUserAllergies();
        //    return Ok(ApiResponse<object>.Success(result));
        //}
        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddUserAllergies([FromBody] List<UserAllergyDTO> userAllergyDTOs)
        {
            await _userAllergyService.AddUserAllergies(userAllergyDTOs);
            return Ok(ApiResponse<object>.Success(null, "Allergies added successfully", 201));
        }
        [Authorize]
        // Xóa các dị ứng của người dùng dựa trên danh sách FoodId
        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveUserAllergies([FromBody] List<string> foodIds)
        {
            await _userAllergyService.RemoveUserAllergies(foodIds);
            return Ok(ApiResponse<object>.Success(null, "Allergies removed successfully"));
        }


    }
}

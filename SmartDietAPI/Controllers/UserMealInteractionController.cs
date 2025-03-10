using BusinessObjects.Base;
using DTOs.MealDTOs;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace SmartDietAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserMealInteractionController : ControllerBase
    {
        private readonly IUserMealInteractionService _service;

        public UserMealInteractionController(IUserMealInteractionService service)
        {
            _service = service;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllUserMealInteractionsAsync();
            return Ok(ApiResponse<object>.Success(result));
        }

        [HttpGet("meal/{mealId}")]
        public async Task<IActionResult> GetByMealId(string mealId)
        {
            var result = await _service.GetUserMealInteractionByMealIdAsync(mealId);
            return Ok(ApiResponse<object>.Success(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _service.GetUserMealInteractionByIdAsync(id);
            return Ok(ApiResponse<object>.Success(result));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(UserMealInteractionDTO dto)
        {
            await _service.CreateUserMealInteractionAsync(dto);
            return Ok(ApiResponse<object>.Success(null, "Created successfully", 201));
        }

        [HttpPost]
        public async Task<IActionResult> CreateInteraction(UserMealInteractionDTO dto)
        {
            var result = await _service.CreateMealInteractionAsync(dto);
            return Ok(ApiResponse<object>.Success(result, "Created successfully", 201));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, UserMealInteractionDTO dto)
        {
            await _service.UpdateUserMealInteractionAsync(id, dto);
            return Ok(ApiResponse<object>.Success(null, "Updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _service.DeleteUserMealInteractionAsync(id);
            return Ok(ApiResponse<object>.Success(null, "Deleted successfully"));
        }
    }
}

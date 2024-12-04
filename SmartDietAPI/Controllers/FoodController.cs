using BusinessObjects.Base;
using BusinessObjects.Entity;
using DTOs.FoodDTOs;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace SmartDietAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodController : Controller
    {
        private readonly IFoodService _foodService;
        public FoodController(IFoodService foodService)
        {
            _foodService = foodService;
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetFoods([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTearm = null)
        {
            var result = await _foodService.GetAllFoodsAsync(pageIndex, pageSize, searchTearm);
            return Ok(ApiResponse<object>.Success(result));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFoodById(string id)
        {
            var result = await _foodService.GetFoodByIdAsync(id);
            return Ok(ApiResponse<object>.Success(result));
        }
        [HttpPost("create")]
        public async Task<IActionResult> AddFood(FoodDTO foodDTO)
        {
            await _foodService.CreateFoodAsync(foodDTO);
            return Ok(ApiResponse<object>.Success(null, "Food created successfully", 201));
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFood(string id, FoodDTO foodDTO)
        {
            await _foodService.UpdateFoodAsync(id, foodDTO);
            return Ok(ApiResponse<object>.Success(null, "Food updated successfully"));
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFood(string id)
        {
            await _foodService.DeleteFoodAsync(id);
            return Ok(ApiResponse<object>.Success(null, "Food deleted successfully"));
        }
    }
}

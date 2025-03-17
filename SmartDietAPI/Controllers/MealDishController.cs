using BusinessObjects.Base;
using DTOs.MealDishDTOs;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.Threading.Tasks;

namespace SmartDietAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MealDishController : ControllerBase
    {
        private readonly IMealDishService _mealDishService;

        public MealDishController(IMealDishService mealDishService)
        {
            _mealDishService = mealDishService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateMealDishes([FromBody] IEnumerable<MealDishDTO> mealDishDTOs)
        {
            var result = await _mealDishService.CreateMealDishesAsync(mealDishDTOs);
            return Ok(ApiResponse<object>.Success(result, "MealDishes created successfully", 201));
        }

        [HttpDelete("{mealDishId}")]
        public async Task<IActionResult> DeleteMealDish(string mealDishId)
        {
            await _mealDishService.DeleteMealDishAsync(mealDishId);
            return Ok(ApiResponse<object>.Success(null, "MealDish deleted successfully"));
        }

        [HttpGet("meal/{mealId}")]
        public async Task<IActionResult> GetMealDishesByMealId(string mealId)
        {
            var result = await _mealDishService.GetMealDishesByMealIdAsync(mealId);
            return Ok(ApiResponse<object>.Success(result));
        }
    }
} 
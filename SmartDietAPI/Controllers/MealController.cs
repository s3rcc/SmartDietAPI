using BusinessObjects.Base;
using DTOs.MealDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Interfaces;
using System.Security.Permissions;
using Newtonsoft.Json;

namespace SmartDietAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MealController : Controller
    {
        private readonly IMealService _mealService;
        public MealController(IMealService mealService)
        {
            _mealService = mealService;
        }
        [Authorize]
        [HttpGet("all")]
        public async Task<IActionResult> GetMeals([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
        {
            var result = await _mealService.GetAllMealsAsync(pageIndex, pageSize, searchTerm);
            return Ok(ApiResponse<object>.Success(result));
        }
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMealById(string id)
        {
            var result = await _mealService.GetMealByIdAsync(id);
            return Ok(ApiResponse<object>.Success(result));
        }
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> AddMeal([FromForm] MealDTO mealDTO
            , [FromForm] string? dishIds
            )
        {
            var dishIdsList = JsonConvert.DeserializeObject<List<string>>(dishIds);
            mealDTO.DishIds = dishIdsList;
            await _mealService.CreateMealAsync(mealDTO);
            return Ok(ApiResponse<object>.Success(null, "Meal created successfully", 201));
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMeal(string id, [FromForm] MealDTO mealDTO
            , [FromForm] string? dishIds
            )
        {
            // Deserialize DishIds từ JSON string
            var dishIdsList = JsonConvert.DeserializeObject<List<string>>(dishIds);
            mealDTO.DishIds = dishIdsList;
            await _mealService.UpdateMealAsync(id, mealDTO);
            return Ok(ApiResponse<object>.Success(null, "Meal updated successfully"));
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeal(string id)
        {
            await _mealService.DeleteMealAsync(id);
            return Ok(ApiResponse<object>.Success(null, "Meal deleted successfully"));
        }
    }
}

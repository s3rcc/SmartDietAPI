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
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFoodById(string id)
        {
            var result = await _foodService.GetFoodByIdAsync(id);
            return Ok(result);
        }
        [HttpPost("create")]
        public async Task<IActionResult> AddFood(FoodDTO foodDTO)
        {
            await _foodService.CreateFoodAsync(foodDTO);
            return Ok();
        }
        [HttpPut("{foodId}")]
        public async Task<IActionResult> UpdateFood(string foodId, FoodDTO foodDTO)
        {
            await _foodService.UpdateFoodAsync(foodId, foodDTO);
            return Ok();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFood(string id)
        {
            await _foodService.DeleteFoodAsync(id);
            return Ok();
        }
    }
}

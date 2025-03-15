using BusinessObjects.Base;
using DTOs.DishDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Interfaces;

namespace SmartDietAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DishController : Controller
    {
        private readonly IDishService _dishService;
        public DishController(IDishService dishService)
        {
            _dishService = dishService;
        }
        [Authorize]
        [HttpGet("all")]
        public async Task<IActionResult> GetDishes([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
        {
            var result = await _dishService.GetAllDishesAsync(pageIndex, pageSize, searchTerm);
            return Ok(ApiResponse<object>.Success(result));
        }
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDishById(string id)
        {
            var result = await _dishService.GetDishByIdAsync(id);
            return Ok(ApiResponse<object>.Success(result));
        }
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> AddDish(DishDTO dishDTO, List<DishIngredientDTO> dishIngredientDTOs)
        {
            await _dishService.CreateDishAsync(dishDTO, dishIngredientDTOs);
            return Ok(ApiResponse<object>.Success(null, "Dish created successfully", 201));
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDish(string id, DishDTO dishDTO, List<DishIngredientDTO> dishIngredientDTOs)
        {
            await _dishService.UpdateDishAsync(id, dishDTO, dishIngredientDTOs);
            return Ok(ApiResponse<object>.Success(null, "Dish updated successfully"));
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDish(string id)
        {
            await _dishService.DeleteDishAsync(id);
            return Ok(ApiResponse<object>.Success(null, "Dish deleted successfully"));
        }
    }
}

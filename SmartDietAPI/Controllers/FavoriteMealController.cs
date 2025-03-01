using BusinessObjects.Base;
using DTOs.FavoriteMealDTOs;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace SmartDietAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoriteMealController : Controller
    {
        private readonly IFavoriteMealService _favoriteMealService;

        public FavoriteMealController(IFavoriteMealService favoriteMealService)
        {
            _favoriteMealService = favoriteMealService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetFavoriteMeals(
            [FromQuery] int pageIndex = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? searchTerm = null)
        {
            var result = await _favoriteMealService.GetAllFavoriteMealsAsync(pageIndex, pageSize, searchTerm);
            return Ok(ApiResponse<object>.Success(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFavoriteMealById(string id)
        {
            var result = await _favoriteMealService.GetFavoriteMealByIdAsync(id);
            return Ok(ApiResponse<object>.Success(result));
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddFavoriteMeal(FavoriteMealDTO favoriteMealDTO)
        {
            await _favoriteMealService.CreateFavoriteMealAsync(favoriteMealDTO);
            return Ok(ApiResponse<object>.Success(null, "Favorite meal added successfully", 201));
        }

        [HttpPut("{favoriteMealId}")]
        public async Task<IActionResult> UpdateFavoriteMeal(string favoriteMealId, FavoriteMealDTO favoriteMealDTO)
        {
            await _favoriteMealService.UpdateFavoriteMealAsync(favoriteMealId, favoriteMealDTO);
            return Ok(ApiResponse<object>.Success(null, "Favorite meal updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFavoriteMeal(string id)
        {
            await _favoriteMealService.DeleteFavoriteMealAsync(id);
            return Ok(ApiResponse<object>.Success(null, "Favorite meal deleted successfully"));
        }
    }
}

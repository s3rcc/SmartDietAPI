using BusinessObjects.Base;
using DTOs.FavoriteDishDTOs;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace SmartDietAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoriteDishController : ControllerBase
    {
        private readonly IFavoriteDishService _favoriteDishService;

        public FavoriteDishController(IFavoriteDishService favoriteDishService)
        {
            _favoriteDishService = favoriteDishService;
        }

        [HttpGet()]
        public async Task<IActionResult> All()
        {
            var result = await _favoriteDishService.GetAllFavoriteDishesAsync();
            return Ok(ApiResponse<object>.Success(result));
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetFavoriteDishes(
            [FromQuery] int pageIndex = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? searchTerm = null)
        {
            var result = await _favoriteDishService.GetAllFavoriteDishesAsync(pageIndex, pageSize, searchTerm);
            return Ok(ApiResponse<object>.Success(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFavoriteDishById(string id)
        {
            var result = await _favoriteDishService.GetFavoriteDishByIdAsync(id);
            return Ok(ApiResponse<object>.Success(result));
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddFavoriteDish(FavoriteDishDTO favoriteDishDTO)
        {
            await _favoriteDishService.CreateFavoriteDishAsync(favoriteDishDTO);
            return Ok(ApiResponse<object>.Success(null, "Favorite dish added successfully", 201));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFavoriteDish(string id, FavoriteDishDTO favoriteDishDTO)
        {
            await _favoriteDishService.UpdateFavoriteDishAsync(id, favoriteDishDTO);
            return Ok(ApiResponse<object>.Success(null, "Favorite dish updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFavoriteDish(string id)
        {
            await _favoriteDishService.DeleteFavoriteDishAsync(id);
            return Ok(ApiResponse<object>.Success(null, "Favorite dish deleted successfully"));
        }
    }

}

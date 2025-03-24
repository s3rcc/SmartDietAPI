using Microsoft.AspNetCore.Mvc;
using BusinessObjects.Base;
using Microsoft.AspNetCore.Authorization;

namespace SmartDietAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DishRecommendationController : ControllerBase
    {
        private readonly IDishRecommendationService _recommendationService;

        public DishRecommendationController(IDishRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }
        [Authorize]
        [HttpGet("recommendations")]
        public async Task<IActionResult> GetRecommendations()
        {
            var recommendations = await _recommendationService.GetRecommendedDishesAsync();
            return Ok(ApiResponse<object>.Success(recommendations));
        }
        [Authorize]
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateRecommendations()
        {
            var recommendations = await _recommendationService.GenerateRecommendationsAsync();
            return Ok(ApiResponse<object>.Success(recommendations));
        }
        [Authorize]
        [HttpPost("regenerate")]
        public async Task<IActionResult> RegenerateRecommendations()
        {
            var recommendations = await _recommendationService.RegenerateRecommendationsAsync();
            return Ok(ApiResponse<object>.Success(recommendations));
        }
        [Authorize]
        [HttpGet("history")]
        public async Task<IActionResult> GetRecommendationHistory()
        {
            var history = await _recommendationService.GetRecommendationHistoryAsync();
            return Ok(ApiResponse<object>.Success(history));
        }
    }
}

using BusinessObjects.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace SmartDietAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MealRecommendationController : Controller
    {
        private readonly IRecommendationService _recommendationService;

        public MealRecommendationController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }
        [Authorize]
        [HttpGet("recommendations")]
        public async Task<IActionResult> GetRecommendations()
        {
            var recommendations = await _recommendationService.GetRecommendedMealsAsync();
            return Ok(ApiResponse<object>.Success(recommendations));
        }
        [Authorize]
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateRecommendations()
        {
            var recommendations = await _recommendationService.GenerateRecommendationsAsync();
            return Ok(ApiResponse<object>.Success(null, "Generated successfully", 201));
        }
        [Authorize]
        [HttpPost("regenerate")]
        public async Task<IActionResult> RegenerateRecommendations()
        {
            var recommendations = await _recommendationService.RegenerateRecommendationsAsync();
            return Ok(ApiResponse<object>.Success(null, "Generated successfully", 201));
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

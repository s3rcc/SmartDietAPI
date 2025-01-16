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

        [HttpGet("recommendations/{userId}")]
        public async Task<IActionResult> GetRecommendations(string userId)
        {
            var recommendations = await _recommendationService.GetRecommendedMealsAsync(userId);
            return Ok(recommendations);
        }

        [HttpPost("generate/{userId}")]
        public async Task<IActionResult> GenerateRecommendations(string userId)
        {
            var recommendations = await _recommendationService.GenerateRecommendationsAsync(userId);
            return Ok(recommendations);
        }

        [HttpPost("regenerate/{userId}")]
        public async Task<IActionResult> RegenerateRecommendations(string userId)
        {
            var recommendations = await _recommendationService.RegenerateRecommendationsAsync(userId);
            return Ok(recommendations);
        }

        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetRecommendationHistory(string userId)
        {
            var history = await _recommendationService.GetRecommendationHistoryAsync(userId);
            return Ok(history);
        }
    }
}

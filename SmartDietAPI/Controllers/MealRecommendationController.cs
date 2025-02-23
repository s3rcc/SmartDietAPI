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

        [HttpGet("recommendations")]
        public async Task<IActionResult> GetRecommendations()
        {
            var recommendations = await _recommendationService.GetRecommendedMealsAsync();
            return Ok(recommendations);
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateRecommendations()
        {
            var recommendations = await _recommendationService.GenerateRecommendationsAsync();
            return Ok(recommendations);
        }

        [HttpPost("regenerate")]
        public async Task<IActionResult> RegenerateRecommendations()
        {
            var recommendations = await _recommendationService.RegenerateRecommendationsAsync();
            return Ok(recommendations);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetRecommendationHistory()
        {
            var history = await _recommendationService.GetRecommendationHistoryAsync();
            return Ok(history);
        }
    }
}

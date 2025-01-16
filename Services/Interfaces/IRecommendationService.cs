using DTOs.MealDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IRecommendationService
    {
        Task<IEnumerable<MealResponse>> GetRecommendedMealsAsync(string userId);
        Task<IEnumerable<MealResponse>> GenerateRecommendationsAsync(string userId);
        Task<IEnumerable<MealResponse>> RegenerateRecommendationsAsync(string userId);
        Task<IEnumerable<MealResponse>> GetRecommendationHistoryAsync(string userId);
    }
}

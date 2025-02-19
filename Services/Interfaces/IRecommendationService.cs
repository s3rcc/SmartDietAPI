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
        Task<IEnumerable<MealResponse>> GetRecommendedMealsAsync();
        Task<IEnumerable<MealResponse>> GenerateRecommendationsAsync();
        Task<IEnumerable<MealResponse>> RegenerateRecommendationsAsync();
        Task<IEnumerable<MealResponse>> GetRecommendationHistoryAsync();
    }
}

using DTOs.DishDTOs;

public interface IDishRecommendationService
{
    Task<IEnumerable<DishResponse>> GenerateRecommendationsAsync();
    Task<IEnumerable<DishResponse>> RegenerateRecommendationsAsync();
    Task<IEnumerable<DishResponse>> GetRecommendedDishesAsync();
    Task<IEnumerable<DishResponse>> GetRecommendationHistoryAsync();
} 
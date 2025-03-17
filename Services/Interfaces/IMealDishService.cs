using DTOs.MealDishDTOs;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IMealDishService
    {
        Task<IEnumerable<MealDishResponse>> CreateMealDishesAsync(IEnumerable<MealDishDTO> mealDishDTOs);
        Task DeleteMealDishAsync(string mealDishId);
        Task<IEnumerable<MealDishResponse>> GetMealDishesByMealIdAsync(string mealId);
    }
} 
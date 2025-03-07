using BusinessObjects.Base;
using DTOs.MealDTOs;
using DTOs.MealDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IMealService
    {
        Task<IEnumerable<MealResponse>> GetAllMealsAsync();
        Task<BasePaginatedList<MealResponse>> GetAllMealsAsync(int pageIndex, int pageSize, string? searchTerm);
        Task<MealResponse> GetMealByIdAsync(string id);
        Task CreateMealAsync(MealDTO Meal);
        Task UpdateMealAsync(string id, MealDTO foodDTO);
        Task DeleteMealAsync(string id);
    }
}

using BusinessObjects.Base;
using DTOs.DishDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IDishService
    {
        Task<IEnumerable<DishResponse>> GetAllDishesAsync();
        Task<DishResponse> GetDishByIdAsync(string dishId);
        Task<BasePaginatedList<DishResponse>> GetAllDishesAsync(int pageIndex, int pageSize, string? searchTerm);
        Task CreateDishAsync(DishDTO dishDTO, List<DishIngredientDTO> dishIngredientDTOs);
        Task UpdateDishAsync(string dishId, DishDTO dishDTO, List<DishIngredientDTO> dishIngredientDTOs);
        Task DeleteDishAsync(string dishId);
    }
}

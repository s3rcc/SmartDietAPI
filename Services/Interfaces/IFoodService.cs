using BusinessObjects.Base;
using BusinessObjects.Entity;
using DTOs.FoodDTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IFoodService
    {
        Task<FoodResponse> GetDeletedFoodByIdAsync(string id);
        Task<IEnumerable<FoodResponse>> GetDeletedFoodsAsync();
        Task<IEnumerable<FoodResponse>> GetAllFoodsAsync();
        Task<BasePaginatedList<FoodResponse>> GetAllFoodsAsync(int pageIndex, int pageSize, string? searchTerm);
        Task<FoodResponse> GetFoodByIdAsync(string id);
        Task CreateFoodAsync(FoodDTO food);
        Task UpdateFoodAsync(string foodId, FoodDTO foodDTO);
        Task DeleteFoodAsync(string foodId);
    }
}

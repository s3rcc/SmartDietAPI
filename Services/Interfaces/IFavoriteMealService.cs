using BusinessObjects.Base;
using DTOs.FavoriteMealDTOs;

namespace Services.Interfaces
{
    public interface IFavoriteMealService
    {
        Task<FavoriteMealResponse> GetFavoriteMealByIdAsync(string id);
        Task<IEnumerable<FavoriteMealResponse>> GetAllFavoriteMealsAsync();
        Task<BasePaginatedList<FavoriteMealResponse>> GetAllFavoriteMealsAsync(int pageIndex, int pageSize, string? searchTerm);
        Task CreateFavoriteMealAsync(FavoriteMealDTO favoriteMeal);
        Task UpdateFavoriteMealAsync(string favoriteMealId, FavoriteMealDTO favoriteMealDTO);
        Task DeleteFavoriteMealAsync(string favoriteMealId);
    }

}

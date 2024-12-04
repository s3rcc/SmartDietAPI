using BusinessObjects.Base;
using DTOs.FavoriteDishDTOs;

namespace Services.Interfaces
{
    public interface IFavoriteDishService
    {
        Task<FavoriteDishResponse> GetFavoriteDishByIdAsync(string id);
        Task<IEnumerable<FavoriteDishResponse>> GetAllFavoriteDishesAsync();
        Task<BasePaginatedList<FavoriteDishResponse>> GetAllFavoriteDishesAsync(int pageIndex, int pageSize, string? searchTerm);
        Task CreateFavoriteDishAsync(FavoriteDishDTO favoriteDishDTO);
        Task UpdateFavoriteDishAsync(string favoriteDishId, FavoriteDishDTO favoriteDishDTO);
        Task DeleteFavoriteDishAsync(string favoriteDishId);
    }

}

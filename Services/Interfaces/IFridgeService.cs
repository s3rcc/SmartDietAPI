using DTOs.FridgeDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IFridgeService
    {
        Task<FridgeRespose> GetFridgeByIdAsync(string id);
        Task<IEnumerable<FridgeRespose>> GetAllUserFrige();
        Task<IEnumerable<FridgeItemResponse>> GetAllItemsInFridge(string id);
        Task<FridgeItemResponse> GetItemById(string id);
        Task CreateFridgeAsync(FridgeDTO fridgeDTO);
        Task UpdateFridgeAsync(string Id, FridgeDTO fridgeDTO);
        Task DeleteFridgeAsync(string Id);
        Task AddItemsToFridge(string fridgeId, List<FridgeItemDTO> fridgeItemDTOs);
        Task UpdateItemInFridge(string itemId, FridgeItemDTO fridgeItemDTO);
        Task RemoveItemsFromFridge(string fridgeId, string itemId);
    }
}

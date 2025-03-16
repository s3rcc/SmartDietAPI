using BusinessObjects.Base;
using DTOs.FridgeDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace SmartDietAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FridgeController : ControllerBase
    {
        private readonly IFridgeService _fridgeService;
        public FridgeController(IFridgeService fridgeService)
        {
            _fridgeService = fridgeService;
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Fridge()
        {
            var result = await _fridgeService.GetAllUserFrige();
            return Ok(ApiResponse<object>.Success(result));
        }
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> Fridge(string id)
        {
            var result = await _fridgeService.GetFridgeByIdAsync(id);
            return Ok(ApiResponse<object>.Success(result));
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddFridge(FridgeDTO fridgeDTO)
        {
            await _fridgeService.CreateFridgeAsync(fridgeDTO);
            return Ok(ApiResponse<object>.Success(null, "Fridge created successfully", 201));
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFridge(string id, FridgeDTO fridgeDTO)
        {
            await _fridgeService.UpdateFridgeAsync(id, fridgeDTO);
            return Ok(ApiResponse<object>.Success(null, "Fridge updated successfully"));
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFridge(string id)
        {
            await _fridgeService.DeleteFridgeAsync(id);
            return Ok(ApiResponse<object>.Success(null, "Fridge deleted successfully"));
        }
        [Authorize]
        [HttpGet("items")]
        public async Task<IActionResult> AllFrideItem(string id)
        {
            var result = await _fridgeService.GetAllItemsInFridge(id);
            return Ok(ApiResponse<object>.Success(result));
        }
        [Authorize]
        [HttpGet("items/{id}")]
        public async Task<IActionResult> FrideItem(string id)
        {
            var result = await _fridgeService.GetItemById(id);
            return Ok(ApiResponse<object>.Success(result));
        }
        [Authorize]
        [HttpPost("items")]
        public async Task<IActionResult> AddItemFridge(string fridgeId, List<FridgeItemDTO> fridgeItemDTOs)
        {
            await _fridgeService.AddItemsToFridge(fridgeId, fridgeItemDTOs);
            return Ok(ApiResponse<object>.Success(null, "Item created successfully", 201));
        }
        [Authorize]
        [HttpPut("items/{id}")]
        public async Task<IActionResult> UpdateItemFridge(string id, FridgeItemDTO fridgeItemDTO)
        {
            await _fridgeService.UpdateItemInFridge(id, fridgeItemDTO);
            return Ok(ApiResponse<object>.Success(null, "Item updated successfully"));
        }
        [Authorize]
        [HttpDelete("{fridgeId}/item/{id}")]
        public async Task<IActionResult> DeleteItemFridge(string fridgeId, string id)
        {
            await _fridgeService.RemoveItemsFromFridge(fridgeId,id);
            return Ok(ApiResponse<object>.Success(null, "Item deleted successfully"));
        }
    }
}

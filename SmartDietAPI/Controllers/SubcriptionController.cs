using BusinessObjects.Base;
using DTOs.DishDTOs;
using DTOs.SubcriptionDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Interfaces;

namespace SmartDietAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubcriptionController : ControllerBase
    {
        private ISubcriptionService _service;
        public SubcriptionController(ISubcriptionService service)
        {
            _service = service;
        }
        [Authorize]
        [HttpGet("all")]
        public async Task<IActionResult> GetDishes()
        {
            var result = await _service.GetAllSubcriptionAsync();
            return Ok(ApiResponse<object>.Success(result));
        }
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubcriptionById(string id)
        {
            var result = await _service.GetSubcriptionByIdAsync(id);
            return Ok(ApiResponse<object>.Success(result));
        }
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> AddSubcription(SubcriptionRequest request)
        {
            await _service.CreateSubcriptionAsync(request);
            return Ok(ApiResponse<object>.Success(null, "Subscription created successfully", 201));
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubcription(string id, SubcriptionRequest request)
        {
            await _service.UpdateSubcriptionAsync(id, request);
            return Ok(ApiResponse<object>.Success(null, "Subscription updated successfully"));
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDish(string id)
        {
            await _service.DeleteSubcriptionAsync(id);
            return Ok(ApiResponse<object>.Success(null, "Subscription deleted successfully"));
        }
    }
}

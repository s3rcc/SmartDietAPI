using BusinessObjects.Base;
using DTOs.DishDTOs;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace SmartDietAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserDishInteractionController : ControllerBase
    {
        private readonly IUserDishInteractionService _service;

        public UserDishInteractionController(IUserDishInteractionService service)
        {
            _service = service;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllUserDishInteractionsAsync();
            return Ok(ApiResponse<object>.Success(result));
        }

        [HttpGet("dish/{dishId}")]
        public async Task<IActionResult> GetByDishId(string dishId)
        {
            var result = await _service.GetUserDishInteractionByDishIdAsync(dishId);
            return Ok(ApiResponse<object>.Success(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _service.GetUserDishInteractionByIdAsync(id);
            return Ok(ApiResponse<object>.Success(result));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(UserDishInteractionDTO dto)
        {
            await _service.CreateUserDishInteractionAsync(dto);
            return Ok(ApiResponse<object>.Success(null, "Created successfully", 201));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, UserDishInteractionDTO dto)
        {
            await _service.UpdateUserDishInteractionAsync(id, dto);
            return Ok(ApiResponse<object>.Success(null, "Updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _service.DeleteUserDishInteractionAsync(id);
            return Ok(ApiResponse<object>.Success(null, "Deleted successfully"));
        }
    }
}

using BusinessObjects.Entity;
using DTOs.FavoriteDishDTOs;
using DTOs.UserFeedbackDTOs;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Interfaces;

namespace SmartDietAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserFeedbackController : Controller
    {
        private readonly IUserFeedbackService _userFeedbackService;

        public UserFeedbackController(IUserFeedbackService userFeedbackService)
        {
            _userFeedbackService = userFeedbackService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUserFeedbacks()
        {
            var result = await _userFeedbackService.GetAllUserFeedbackAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserFeedbackBySmartDietUserIds(string id)
        {
            var result = await _userFeedbackService.GetUserFeedbackBySmartDietUserIdAsync(id);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUserFeedback(UserFeedbackDTO userFeedback)
        {
            await _userFeedbackService.CreateUserFeedbackAsync(userFeedback);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserFeedback(string id)
        {
            await _userFeedbackService.DeleteUserFeedbackAsync(id);
            return Ok();
        }
    }
}

using BusinessObjects.FixedData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartDietAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnumController : ControllerBase
    {
        [Authorize]
        [HttpGet("food-model")]
        public IActionResult GetFoodModelEnums()
        {
            var result = new
            {
                PreservationTypes = Enum.GetNames(typeof(PreservationType)),
                FoodCategories = Enum.GetNames(typeof(FoodCategory))
            };

            return Ok(result);
        }

        [HttpGet("dish-model")]
        public IActionResult GetDishModelEnums()
        {
            var result = new
            {
                DifficultyLevels = Enum.GetNames(typeof(DifficultyLevel)),
                RegionTypes = Enum.GetNames(typeof(RegionType)),
                DietTypes = Enum.GetNames(typeof(DietType))
            };

            return Ok(result);
        }

        [HttpGet("meal-model")]
        public IActionResult GetMealModelEnums()
        {
            var result = new
            {
                DietTypes = Enum.GetNames(typeof(DietType))
            };

            return Ok(result);
        }
    }
}
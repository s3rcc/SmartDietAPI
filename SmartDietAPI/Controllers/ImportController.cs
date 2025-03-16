using Microsoft.AspNetCore.Mvc;
using Services;
using BusinessObjects.Base;
using BusinessObjects.Entity;
using DTOs.ExcelDTOs;
using OfficeOpenXml;
using Microsoft.AspNetCore.Authorization;

namespace SmartDietAPI.Controllers
{
    [Route("api/import")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly IExcelImportService<Meal> _mealImportService;
        private readonly IExcelImportService<Dish> _dishImportService;
        private readonly IExcelImportService<Food> _foodImportService;

        private readonly IFileHandlerService _fileHandlerService;
        private readonly IWebHostEnvironment _env;

        public ImportController(
            IExcelImportService<Meal> mealImportService,
            IExcelImportService<Dish> dishImportService,
            IExcelImportService<Food> foodImportService,
            IFileHandlerService fileHandlerService,
            IWebHostEnvironment env)
        {
            _mealImportService = mealImportService;
            _dishImportService = dishImportService;
            _foodImportService = foodImportService;
            _fileHandlerService = fileHandlerService;
            _env = env;
        }
        [Authorize]
        [HttpPost("meals")]
        public async Task<IActionResult> ImportMeals(IFormFile file)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only .xlsx files are allowed");

            try
            {
                var filePath = await _fileHandlerService.SaveUploadedFileAsync(file, "meals.xlsx");
                var result = await _mealImportService.ImportFromExcel<MealExcelDTO>(filePath);
                
                return Ok(ApiResponse<object>.Success(result, "Meals imported successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(
                    errorCode: "IMPORT_MEALS_ERROR", 
                    message: $"Import failed: {ex.Message}", 
                    statusCode: 500
                ));
            }
        }
        [Authorize]
        [HttpPost("foods")]
        public async Task<IActionResult> ImportFoods(IFormFile file)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only .xlsx files are allowed");

            try
            {
                var filePath = await _fileHandlerService.SaveUploadedFileAsync(file, "foods.xlsx");
                var result = await _foodImportService.ImportFromExcel<FoodExcelDTO>(filePath);

                return Ok(ApiResponse<object>.Success(result, "Foods imported successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(
                    errorCode: "IMPORT_FOODS_ERROR",
                    message: $"Import failed: {ex.Message}",
                    statusCode: 500
                ));
            }
        }
        [Authorize]
        [HttpPost("dishes")]
        public async Task<IActionResult> ImportDishs(IFormFile file)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only .xlsx files are allowed");

            try
            {
                var filePath = await _fileHandlerService.SaveUploadedFileAsync(file, "dishes.xlsx");
                var result = await _dishImportService.ImportFromExcel<DishExcelDTO>(filePath);

                return Ok(ApiResponse<object>.Success(result, "Dishes imported successfully", 201));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(
                    errorCode: "IMPORT_FOODS_ERROR",
                    message: $"Import failed: {ex.Message}",
                    statusCode: 500
                ));
            }
        }
        [Authorize]
        [HttpGet("template/{fileName}")]
        public IActionResult GetImportTemplate(string fileName)
        {
            try
            {
                // Validate filename
                if (string.IsNullOrWhiteSpace(fileName) || 
                    fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    return BadRequest(ApiResponse<object>.Error(
                        "INVALID_FILENAME", 
                        "Invalid file name format", 
                        400
                    ));
                }

                var sanitizedFileName = Path.GetFileNameWithoutExtension(fileName);
                var fullFileName = $"{sanitizedFileName}.xlsx";
                var filePath = Path.Combine(_env.ContentRootPath, "Data", fullFileName);

                Console.WriteLine("\n\n\n"+filePath);

                if (!System.IO.File.Exists(filePath))
                    return NotFound(ApiResponse<object>.Error(
                        "FILE_NOT_FOUND", 
                        $"Template file '{sanitizedFileName}' not found", 
                        404
                    ));

                Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
                Response.Headers.Append("Pragma", "no-cache");
                Response.Headers.Append("Expires", "0");

                return PhysicalFile(
                    filePath,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"{sanitizedFileName}.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(
                    "TEMPLATE_ERROR",
                    $"Failed to retrieve template: {ex.Message}",
                    500
                ));
            }
        }
    }
} 
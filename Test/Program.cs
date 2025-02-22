// See https://aka.ms/new-console-template for more information
using BusinessObjects.Entity;
using DTOs.ExcelDTOs;
using Repositories;
using Repositories.Interfaces;
using Services;
using Microsoft.EntityFrameworkCore;
using DataAccessObjects; // Contains SmartDietDbContext
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // Initialize EPPlus license FIRST
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // Create DbContext with SQL Server provider
        var optionsBuilder = new DbContextOptionsBuilder<SmartDietDbContext>()
            .UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

        using var dbContext = new SmartDietDbContext(optionsBuilder.Options);

        // Create UnitOfWork instead of direct repository
        var unitOfWork = new UnitOfWork(dbContext);
        var importService = new ExcelImportService<Meal>(unitOfWork);

        // Get the full path to the Excel file
        var excelPath = Path.Combine(Directory.GetCurrentDirectory(),"Data", "meals.xlsx");

        // Verify file exists before processing
        if (!File.Exists(excelPath))
        {
            Console.WriteLine($"File not found: {excelPath}");
            return;
        }

        var result = await importService.ImportFromExcel<MealExcelDTO>(excelPath);

        if (result.Errors.Any())
        {
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Lỗi dòng: {error.RowData} - Lý do: {error.ErrorMessage}");
            }
        }

        Console.WriteLine($"Import thành công {result.SuccessCount}/{result.TotalProcessed} bản ghi");
    }
}
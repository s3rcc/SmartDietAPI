using System.Reflection;
using OfficeOpenXml;
using BusinessObjects.Base;
using BusinessObjects.Entity;
using Repositories.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Services
{
    public interface IExcelImportService<T> where T : BaseEntity, new()
    {
        Task<ExcelImportResult<T>> ImportFromExcel<TDto>(string filePath) where TDto : class;

    }

    public class ExcelImportService<T> : IExcelImportService<T> where T : BaseEntity, new()
    {
        private readonly IUnitOfWork _unitOfWork;
        private const int RequiredColumnIndex = 1; // Column A

        public ExcelImportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ExcelImportResult<T>> ImportFromExcel<TDto>(string filePath) where TDto : class
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Excel file not found", filePath);
            }

            var result = new ExcelImportResult<T>();
            var validEntities = new List<T>();
            var errors = new List<ExcelImportError>();
            int rowCount = 0;

            FileInfo fileInfo = new FileInfo(filePath);
            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                rowCount = worksheet.Dimension.Rows;

                var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var cellAValue = worksheet.Cells[row, 1].Value?.ToString();
                        
                        // Check for duplicates within the file
                        if (cellAValue != null && seenNames.Contains(cellAValue))
                        {
                            throw new ValidationException($"Duplicate name '{cellAValue}' found in file");
                        }
                        seenNames.Add(cellAValue!);

                        // Check if column A (index 1) is empty
                        if (string.IsNullOrWhiteSpace(cellAValue))
                        {
                            throw new ValidationException("Empty row - Column A is required");
                        }

                        var requiredValue = worksheet.Cells[row, RequiredColumnIndex].Value?.ToString();
                        if (string.IsNullOrWhiteSpace(requiredValue))
                        {
                            throw new ValidationException($"Required column {RequiredColumnIndex} is empty");
                        }

                        var dto = MapExcelRowToDto<TDto>(worksheet, row);
                        ValidateDto(dto);

                        var entity = MapDtoToEntity(dto);

                        if (await IsDuplicate(entity))
                            throw new ValidationException($"Duplicate name '{cellAValue}' exists in database");

                        validEntities.Add(entity);
                    }
                    catch (Exception ex)
                    {
                        errors.Add(new ExcelImportError
                        {
                            RowNumber = row,
                            RowData = GetRowData(worksheet, row),
                            ErrorMessage = ex.Message
                        });
                    }
                }
            }

            if (validEntities.Any())
            {
                await _unitOfWork.Repository<T>().AddRangeAsync(validEntities.AsEnumerable());
                result.SuccessCount = validEntities.Count;
                await _unitOfWork.SaveChangeAsync();
            }

            result.TotalProcessed = rowCount - 1;
            result.Errors = errors;

            return result;
        }

        private TDto MapExcelRowToDto<TDto>(ExcelWorksheet worksheet, int row)
        {
            var dto = Activator.CreateInstance<TDto>();
            var properties = typeof(TDto).GetProperties();

            foreach (var prop in properties)
            {
                var columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
                if (columnAttr != null)
                {
                    var value = worksheet.Cells[row, columnAttr.Index].Value?.ToString();
                    prop.SetValue(dto, value);
                }
            }
            return dto;
        }

        private void ValidateDto(object dto)
        {
            var validationContext = new ValidationContext(dto);
            Validator.ValidateObject(dto, validationContext, true);
        }

        private T MapDtoToEntity<TDto>(TDto dto)
        {
            var entity = new T();
            var dtoProperties = typeof(TDto).GetProperties();
            var entityProperties = typeof(T).GetProperties();

            foreach (var dtoProp in dtoProperties)
            {
                var entityProp = entityProperties.FirstOrDefault(p => p.Name == dtoProp.Name);
                if (entityProp != null && entityProp.CanWrite)
                {
                    var value = dtoProp.GetValue(dto);
                    entityProp.SetValue(entity, value);
                }
            }

            // Set BaseEntity defaults
            entity.CreatedTime = DateTime.UtcNow;
            entity.CreatedBy = "ExcelSystemImport";

            return entity;
        }

        private async Task<bool> IsDuplicate(T entity)
        {
            // Use reflection to check for Name property
            var nameProperty = typeof(T).GetProperty("Name");
            if (nameProperty == null) return false;

            var entityName = nameProperty.GetValue(entity)?.ToString();
            if (string.IsNullOrEmpty(entityName)) return false;

            // Build expression dynamically
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, nameProperty);
            var constant = Expression.Constant(entityName);
            var equality = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(equality, parameter);

            return await _unitOfWork.Repository<T>().AnyAsync(lambda);
        }

        private string GetRowData(ExcelWorksheet worksheet, int row)
        {
            return string.Join(", ", worksheet.Cells[row, 1, row, worksheet.Dimension.End.Column]
                .Select(c => c.Value?.ToString()));
        }
    }
}
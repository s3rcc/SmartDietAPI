using System.Reflection;
using OfficeOpenXml;
using BusinessObjects.Base;
using BusinessObjects.Entity;
using Repositories.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.IO;
using Services.Interfaces;

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
        private readonly ICloudinaryService _cloudinaryService;
        public ExcelImportService(IUnitOfWork unitOfWork, ICloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
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
                            throw new ValidationException($"<<< Duplicate name '{cellAValue}' found in file >>>");
                        }
                        seenNames.Add(cellAValue!);

                        // Check if column A (index 1) is empty
                        if (string.IsNullOrWhiteSpace(cellAValue))
                        {
                            throw new ValidationException("<<< Empty row - Column A is required >>>");
                        }

                        var requiredValue = worksheet.Cells[row, RequiredColumnIndex].Value?.ToString();
                        if (string.IsNullOrWhiteSpace(requiredValue))
                        {
                            throw new ValidationException($"<<< Required column {RequiredColumnIndex} is empty >>>");
                        }

                        var dto = await MapExcelRowToDto<TDto>(worksheet, row);
                        ValidateDto(dto);

                        var entity = MapDtoToEntity(dto);

                        if (await IsDuplicate(entity))
                            throw new ValidationException($"<<< Duplicate name '{cellAValue}' exists in database >>>");

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

        private async Task<TDto> MapExcelRowToDto<TDto>(ExcelWorksheet worksheet, int row) where TDto : class
        {
            if (_cloudinaryService == null)
            {
                throw new ArgumentNullException(nameof(_cloudinaryService), "CloudinaryService is not initialized.");
            }

            var dto = Activator.CreateInstance<TDto>();
            var properties = typeof(TDto).GetProperties();

            foreach (var prop in properties)
            {
                var columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
                if (columnAttr != null)
                {
                    var cell = worksheet.Cells[row, columnAttr.Index];
                    var value = cell.Value?.ToString();

                    // Xử lý đặc biệt cho cột Image
                    if (prop.Name == "Image" && !string.IsNullOrEmpty(value))
                    {
                        string imageUrl;
                        
                        // Lấy URL từ hyperlink nếu có, ngược lại sử dụng giá trị cell
                        if (cell.Hyperlink != null)
                        {
                            imageUrl = cell.Hyperlink.AbsoluteUri;
                        }
                        else
                        {
                            imageUrl = value;
                        }
                        
                        try
                        {
                            // Kiểm tra xem URL có phải từ Google Drive không
                            if (imageUrl.Contains("drive.google.com"))
                            {
                                Console.WriteLine($"Detected Google Drive URL: {imageUrl}");
                                value = await _cloudinaryService.UploadImageFromGoogleDriveAsync(imageUrl);
                            }
                            else
                            {
                                value = await _cloudinaryService.UploadImageFromUrlAsync(imageUrl);
                            }
                            Console.WriteLine($"Uploaded image successfully: {value}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to upload image: {ex.Message}");
                            // Không throw exception ở đây - để cell tiếp tục xử lý
                            // Có thể đặt một giá trị mặc định cho image
                            value = "https://res.cloudinary.com/dtsjztbus/image/upload/default_image.jpg";
                        }
                    }

                    // Xử lý giá trị trống cho StorageGuidelines
                    if (prop.Name == "StorageGuidelines" && string.IsNullOrWhiteSpace(value))
                    {
                        value = "Hiện tại không có hướng dẫn bảo quản";
                    }

                    if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
                    {
                        if (int.TryParse(value, out int intValue))
                            prop.SetValue(dto, intValue);
                        else if (prop.PropertyType == typeof(int?))
                            prop.SetValue(dto, null);
                    }
                    else if (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?))
                    {
                        if (decimal.TryParse(value, out decimal decimalValue))
                            prop.SetValue(dto, decimalValue);
                        else if (prop.PropertyType == typeof(decimal?))
                            prop.SetValue(dto, null);
                    }
                    else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                    {
                        if (DateTime.TryParse(value, out DateTime dateValue))
                            prop.SetValue(dto, dateValue);
                    }
                    else
                    {
                        prop.SetValue(dto, value);
                    }
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
                    
                    // Xử lý đặc biệt cho RegionType - xử lý bitwise cho flags enum
                    if (entityProp.Name == "RegionType" && value is string regionTypeStr)
                    {
                        // Nếu giá trị là "0", đặt RegionType là None
                        if (regionTypeStr.Trim() == "0")
                        {
                            entityProp.SetValue(entity, BusinessObjects.FixedData.RegionType.None);
                            continue;
                        }

                        // Xử lý nhiều giá trị được phân tách bằng dấu phẩy hoặc khoảng trắng
                        string[] values = regionTypeStr.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        int result = 0;

                        foreach (var val in values)
                        {
                            if (int.TryParse(val.Trim(), out int flagValue))
                            {
                                result |= flagValue;
                            }
                        }

                        entityProp.SetValue(entity, (BusinessObjects.FixedData.RegionType)result);
                        continue;
                    }
                    // Xử lý enum nullable
                    else if (IsNullableEnum(entityProp.PropertyType) && value is string nullableEnumString)
                    {
                        Type underlyingType = Nullable.GetUnderlyingType(entityProp.PropertyType);
                        
                        if (string.IsNullOrWhiteSpace(nullableEnumString))
                        {
                            entityProp.SetValue(entity, null);
                        }
                        else if (Enum.TryParse(underlyingType, nullableEnumString, true, out object nullableEnumValue))
                        {
                            entityProp.SetValue(entity, nullableEnumValue);
                        }
                        else
                        {
                            throw new ValidationException($"Giá trị '{nullableEnumString}' không hợp lệ cho {entityProp.Name}");
                        }
                    }
                    // Xử lý enum thường
                    else if (entityProp.PropertyType.IsEnum && value is string enumString)
                    {
                        if (Enum.TryParse(entityProp.PropertyType, enumString, true, out object enumValue))
                        {
                            entityProp.SetValue(entity, enumValue);
                        }
                        else
                        {
                            throw new ValidationException($"Giá trị '{enumString}' không hợp lệ cho {entityProp.Name}");
                        }
                    }
                    // Xử lý các kiểu dữ liệu khác
                    else
                    {
                        entityProp.SetValue(entity, value);
                    }
                }
            }

            entity.CreatedTime = DateTime.UtcNow;
            entity.CreatedBy = "ExcelSystemImport";
            return entity;
        }

        private bool IsNullableEnum(Type type)
        {
            Type underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType != null && underlyingType.IsEnum;
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

        private async Task<Stream> DownloadImageFromGoogleDriveAsync(string url)
        {
            try
            {
                // Log bắt đầu tải ảnh
                Console.WriteLine($"Bắt đầu tải ảnh từ URL: {url}");

                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(url);

                // Log kết quả response
                Console.WriteLine($"Response status code: {response.StatusCode}");

                // Đảm bảo response thành công
                response.EnsureSuccessStatusCode();

                // Log thành công
                Console.WriteLine("Tải ảnh thành công.");

                return await response.Content.ReadAsStreamAsync();
            }
            catch (HttpRequestException ex)
            {
                // Log lỗi HTTP
                Console.WriteLine($"Lỗi HTTP khi tải ảnh: {ex.Message}");
                throw new Exception($"Không thể tải ảnh từ URL: {url}", ex);
            }
            catch (Exception ex)
            {
                // Log lỗi tổng quát
                Console.WriteLine($"Lỗi khi tải ảnh: {ex.Message}");
                throw new Exception($"Lỗi không xác định khi tải ảnh từ URL: {url}", ex);
            }
        }

        public async Task<string> UploadImageFromStreamAsync(Stream imageStream, string fileName)
        {
            if (imageStream == null || imageStream.Length == 0)
            {
                throw new ArgumentException("Image stream cannot be null or empty");
            }

            var formFile = new FormFile(imageStream, 0, imageStream.Length, "image", fileName);
            return await _cloudinaryService.UploadImageAsync(formFile);
        }
    }
}
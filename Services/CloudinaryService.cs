using BusinessObjects.Exceptions;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        public CloudinaryService(IConfiguration configuration)
        {
            var cloudinaryConfig = configuration.GetSection("Cloudinary");
            var account = new Account(
                cloudinaryConfig["CloudName"],
                cloudinaryConfig["ApiKey"],
                cloudinaryConfig["ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file.Length == 0) throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Error!!! File empty");

            // Chuyển đổi IFormFile sang một MemoryStream để upload lên Cloudinary
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation().Quality(80).Crop("limit")
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            return uploadResult.SecureUrl.AbsoluteUri;
        }
        public async Task DeleteImageAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId);
            var result = _cloudinary.Destroy(deletionParams); 

            if (result.Result != "ok")
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Error!!! Can't delete");
            }
        }

        public async Task<string> UploadImageFromStreamAsync(Stream imageStream, string fileName)
        {
            if (imageStream == null || imageStream.Length == 0)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Error!!! Image stream is empty or null");
            }

            try
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(fileName, imageStream),
                    Transformation = new Transformation().Quality(80).Crop("limit")
                };
                
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                
                if (uploadResult == null || uploadResult.SecureUrl == null)
                {
                    throw new ErrorException(StatusCodes.Status500InternalServerError, "Error!!! Failed to upload image to Cloudinary");
                }
                
                return uploadResult.SecureUrl.AbsoluteUri;
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, $"Error uploading image: {ex.Message}");
            }
        }

        public async Task<string> UploadImageFromUrlAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Error!!! Image URL is empty or null");
            }

            try
            {
                // Tải ảnh từ URL
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();
                
                var imageStream = await response.Content.ReadAsStreamAsync();
                
                // Lấy tên file từ URL hoặc sử dụng tên mặc định
                string fileName = "image.jpg";
                if (Uri.TryCreate(imageUrl, UriKind.Absolute, out Uri uri))
                {
                    var pathSegments = uri.Segments;
                    if (pathSegments.Length > 0)
                    {
                        var lastSegment = pathSegments.Last();
                        if (!string.IsNullOrEmpty(lastSegment) && !lastSegment.EndsWith("/"))
                        {
                            fileName = lastSegment;
                        }
                    }
                }
                
                // Sử dụng hàm upload từ stream đã tạo
                return await UploadImageFromStreamAsync(imageStream, fileName);
            }
            catch (HttpRequestException ex)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, $"Error downloading image from URL: {ex.Message}");
            }
        }

        public async Task<string> UploadImageFromGoogleDriveAsync(string googleDriveUrl)
        {
            if (string.IsNullOrEmpty(googleDriveUrl))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Error!!! Google Drive URL is empty or null");
            }

            try
            {
                // Trích xuất ID file từ URL Google Drive
                string fileId = ExtractGoogleDriveFileId(googleDriveUrl);
                if (string.IsNullOrEmpty(fileId))
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Invalid Google Drive URL format");
                }

                // Tạo URL trực tiếp để tải file từ Google Drive
                string directDownloadUrl = $"https://drive.google.com/uc?export=download&id={fileId}";
                
                // Log URL trực tiếp để debug
                Console.WriteLine($"Attempting to download from direct URL: {directDownloadUrl}");

                // Tải ảnh từ URL trực tiếp
                using var httpClient = new HttpClient();
                // Đặt User-Agent để tránh bị chặn
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36");
                
                var response = await httpClient.GetAsync(directDownloadUrl);
                Console.WriteLine($"Response status: {response.StatusCode}");
                response.EnsureSuccessStatusCode();
                
                var imageStream = await response.Content.ReadAsStreamAsync();
                Console.WriteLine($"Stream length: {imageStream.Length}");
                
                // Tạo tên file từ ID Google Drive
                string fileName = $"{fileId}.jpg";
                
                // Sử dụng hàm upload từ stream đã tạo
                return await UploadImageFromStreamAsync(imageStream, fileName);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request Error: {ex.Message}");
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, $"Error downloading image from Google Drive: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                throw new ErrorException(StatusCodes.Status500InternalServerError, $"Error processing Google Drive image: {ex.Message}");
            }
        }

        private string ExtractGoogleDriveFileId(string googleDriveUrl)
        {
            // Xử lý URL dạng https://drive.google.com/file/d/FILE_ID/view
            if (googleDriveUrl.Contains("/file/d/"))
            {
                int startIndex = googleDriveUrl.IndexOf("/file/d/") + 8;
                int endIndex = googleDriveUrl.IndexOf("/", startIndex);
                if (endIndex == -1)
                {
                    // Nếu không có ký tự / ở cuối, lấy đến hết URL
                    return googleDriveUrl.Substring(startIndex);
                }
                else
                {
                    return googleDriveUrl.Substring(startIndex, endIndex - startIndex);
                }
            }
            // Xử lý URL dạng https://drive.google.com/open?id=FILE_ID
            else if (googleDriveUrl.Contains("?id="))
            {
                int startIndex = googleDriveUrl.IndexOf("?id=") + 4;
                int endIndex = googleDriveUrl.IndexOf("&", startIndex);
                if (endIndex == -1)
                {
                    return googleDriveUrl.Substring(startIndex);
                }
                else
                {
                    return googleDriveUrl.Substring(startIndex, endIndex - startIndex);
                }
            }
            return null;
        }
    }
}

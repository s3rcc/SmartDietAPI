using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file);
        Task DeleteImageAsync(string publicId);
        
        Task<string> UploadImageFromStreamAsync(Stream imageStream, string fileName);
        
        Task<string> UploadImageFromUrlAsync(string imageUrl);
        Task<string> UploadImageFromGoogleDriveAsync(string googleDriveUrl);
    }
}

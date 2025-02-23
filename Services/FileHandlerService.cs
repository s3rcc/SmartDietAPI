using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;


public interface IFileHandlerService
{
    Task<string> SaveUploadedFileAsync(IFormFile file, string fileName);
}

public class FileHandlerService : IFileHandlerService
{
    private readonly IWebHostEnvironment _env;

    public FileHandlerService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveUploadedFileAsync(IFormFile file, string fileName)
    {
        var dataPath = Path.Combine(_env.ContentRootPath, "Data");
        Directory.CreateDirectory(dataPath);
        
        var filePath = Path.Combine(dataPath, fileName);
        
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        
        return filePath;
    }
} 
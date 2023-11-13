using Microsoft.AspNetCore.Mvc;

namespace PdfExtractor.Controllers;

[ApiController]
[Route("[controller]")]
public class FileController : ControllerBase
{
    private readonly IWebHostEnvironment _hostEnvironment;

    public FileController(IWebHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }

    [HttpPost("upload")]
public string Upload(IFormFileCollection files)
{
    try
    {
        // Generate a unique folder name for the current request
        string uniqueDirName = $"Request_{Guid.NewGuid()}";

        // Construct the folder path
        var uniqueDirPath = Path.Combine(_hostEnvironment.WebRootPath, uniqueDirName);

        // Check if the folder already exists; create it if not
        if (!Directory.Exists(uniqueDirPath))
        {
            Directory.CreateDirectory(uniqueDirPath);
        }

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                // Construct the file path within the unique folder
                var filePath = Path.Combine(uniqueDirPath, file.FileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                file.CopyTo(stream);

                Console.WriteLine($"{file.FileName}-{file.ContentType} is uploaded successfully!");
            }
        }

        return $"{files.Count} file(s) uploaded successfully!";
    }
    catch (Exception ex)
    {
        return $"Exception occurred ! - {ex.Message}";
    }
}

}
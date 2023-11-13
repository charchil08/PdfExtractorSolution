using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
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
    public IList<string> Upload(IFormFileCollection files)
    {
        IList<string> extractedTexts = new List<string>();
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

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    extractedTexts.Add(ExtractTextFromPDF(filePath));
                    Console.WriteLine($"{file.FileName}-{file.ContentType} is uploaded successfully!");
                }
            }

            return extractedTexts;
        }
        catch (Exception ex)
        {
            return new List<string>() { $"Exception occurred ! - {ex.Message}" };
        }
    }


    private static string ExtractTextFromPDF(string filePath)
    {
        var extractedText = new StringBuilder();

        try
        {
            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(filePath)))
            {
                int numPages = pdfDocument.GetNumberOfPages();
                for (int pageNum = 1; pageNum <= numPages; pageNum++)
                {
                    PdfPage page = pdfDocument.GetPage(pageNum);
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

                    // Extract text from the page
                    PdfCanvasProcessor parser = new PdfCanvasProcessor(strategy);
                    parser.ProcessPageContent(page);

                    // Append the page text to the result using StringBuilder
                    extractedText.Append(strategy.GetResultantText());
                }
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions here
            extractedText.Append("Error: " + ex.Message);
        }

        return extractedText.ToString();
    }

}
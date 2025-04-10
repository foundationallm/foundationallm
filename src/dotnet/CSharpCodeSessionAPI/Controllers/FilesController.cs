using FoundationaLLM.CSharpCodeSession.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.CSharpCodeSession.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FilesController(ILogger<FilesController> logger) : ControllerBase
    {
        private readonly ILogger<FilesController> _logger = logger;

        private const string ROOT_PATH = "\\mnt\\data";

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var filePath = NormalizePath(Path.Combine(ROOT_PATH, file.FileName));
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            return Ok(new UploadFileResponse
            {
                FileName = Path.GetFileName(filePath)
            });
        }

        private static string NormalizePath(string path) =>
            Path.GetFullPath(new Uri(path).LocalPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}

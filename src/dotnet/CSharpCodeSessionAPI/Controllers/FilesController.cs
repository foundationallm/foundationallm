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
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var filePath = Path.Combine(ROOT_PATH, file.FileName);

            // Normalize and validate file path
            var normalizedPath = Path.GetFullPath(filePath);
            if (!normalizedPath.StartsWith(Path.GetFullPath(ROOT_PATH)))
                return BadRequest("Invalid file path.");

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return Ok(new UploadFileResponse
            {
                FileName = Path.GetFileName(filePath)
            });
        }

        [HttpGet]
        public IActionResult ListFiles()
        {
            var rootPath = Path.GetFullPath(ROOT_PATH);

            return Ok(new ListFilesResponse
            {
                Files = [.. Directory
                    .EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)
                    .Select(path => Path.GetRelativePath(rootPath, path))
                ]
            });
        }

        [HttpPost("download")]
        public IActionResult DownloadFile([FromBody] DownloadFileRequest downloadFileRequest)
        {
            if (downloadFileRequest == null || string.IsNullOrWhiteSpace(downloadFileRequest.FileName))
                return BadRequest("Invalid file name.");

            var filePath = Path.Combine(ROOT_PATH, downloadFileRequest.FileName);

            // Normalize and validate file path
            var normalizedPath = Path.GetFullPath(filePath);
            if (!normalizedPath.StartsWith(Path.GetFullPath(ROOT_PATH)))
                return BadRequest("Invalid file path.");

            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found.");

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return File(stream, "application/octet-stream", Path.GetFileName(filePath));
        }

        [HttpPost("delete")]
        public IActionResult DeleteFiles()
        {
            Directory.Delete(ROOT_PATH, true);
            Directory.CreateDirectory(ROOT_PATH);
            return Ok(new StatusResponse());
        }
    }
}

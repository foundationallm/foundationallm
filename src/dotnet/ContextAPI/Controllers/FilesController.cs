using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Context.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Context.API.Controllers
{
    /// <summary>
    /// Provdes methods for managing files.
    /// </summary>
    /// <param name="fileService">The <see cref="IFileService"/> file service.</param>
    /// <param name="callContext">>The <see cref="ICallContext"/> call context associated with the current request.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    [ApiController]
    [APIKeyAuthentication]
    [Route("instances/{instanceId}")]
    public class FilesController(
        IFileService fileService,
        ICallContext callContext,
        ILogger<FilesController> logger)
    {
        private readonly IFileService _fileService = fileService;
        private readonly ICallContext _callContext = callContext;
        private readonly ILogger<FilesController> _logger = logger;


    }
}

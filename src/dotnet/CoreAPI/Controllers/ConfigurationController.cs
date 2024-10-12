using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides methods for retrieving and managing configuration.
    /// </summary>
    [Authorize(Policy = "DefaultPolicy")]
    [ApiController]
    [Route("instances/{instanceId}/[controller]")]
    public class ConfigurationController : ControllerBase
    {
        private readonly ICallContext _callContext;
        private readonly InstanceSettings _instanceSettings;
        private readonly ICoreService _coreService;
        private readonly ILogger<FilesController> _logger;

        /// <summary>
        /// The controller for managing files.
        /// </summary>
        /// <param name="callContext">The <see cref="ICallContext"/> call context of the request being handled.</param>
        /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
        /// <param name="coreService">The <see cref="ICoreService"/> core service.</param>
        /// <param name="logger"></param>
        /// <exception cref="ResourceProviderException"></exception>
        public ConfigurationController(
            ICallContext callContext,
            IOptions<InstanceSettings> instanceOptions,
            ICoreService coreService,
            ILogger<FilesController> logger)
        {
            _callContext = callContext;
            _instanceSettings = instanceOptions.Value;
            _coreService = coreService;
            _logger = logger;
        }

        /// <summary>
        /// Returns the core configuration.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetCoreConfiguration(string instanceId) =>
            new OkObjectResult(await _coreService.GetCoreConfiguration(instanceId, _callContext.CurrentUserIdentity!));
    }
}

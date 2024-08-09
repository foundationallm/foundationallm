﻿using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Models.Infrastructure;
using FoundationaLLM.Orchestration.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Orchestration.API.Controllers
{
    /// <summary>
    /// Provides methods for checking the status of the service.
    /// </summary>
    /// <param name="orchestrationService">The <see cref="IOrchestrationService"/> that provides orchestration capabilities.</param>
    [ApiController]
    [Route("instances/{instanceId}/[controller]")]
    public class StatusController(
        IOrchestrationService orchestrationService) : ControllerBase
    {
        private readonly IOrchestrationService _orchestrationService = orchestrationService;

        /// <summary>
        /// Returns the status of the Orchestration API service.
        /// </summary>
        [HttpGet(Name = "GetServiceStatus")]
        public async Task<IActionResult> Get(string instanceId) =>
            new OkObjectResult(await _orchestrationService.GetStatus(instanceId));

        /// <summary>
        /// Returns the allowed HTTP methods for the Orchestration API service.
        /// </summary>
        [HttpOptions]
        public IActionResult Options()
        {
            HttpContext.Response.Headers.Append("Allow", new[] { "GET", "POST", "OPTIONS" });

            return Ok();
        }
    }
}

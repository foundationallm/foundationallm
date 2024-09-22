using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Vectorization.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Vectorization.API.Controllers
{
    /// <summary>
    /// Methods for managing vectorization requests.
    /// </summary>
    /// <param name="vectorizationRequestProcessor">The vectorization request processor.</param>
    /// <remarks>
    /// Constructor for the vectorization request controller.
    /// </remarks>
    [ApiController]
    [APIKeyAuthentication]
    [Route("instances/{instanceId}")]
    public class VectorizationRequestController(        
        IVectorizationRequestProcessor vectorizationRequestProcessor) : ControllerBase
    {
        /// <summary>
        /// Handles an incoming vectorization request by starting a new vectorization pipeline.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="vectorizationRequest">The <see cref="VectorizationRequest"/> that must be processed.</param>
        /// <returns></returns>
        [HttpPost("vectorization-requests")]
        public async Task<IActionResult> ProcessRequest(string instanceId, [FromBody] VectorizationRequest vectorizationRequest)
            => new OkObjectResult(await vectorizationRequestProcessor.ProcessRequest(instanceId, vectorizationRequest, DefaultAuthentication.ServiceIdentity));

    }
}

using Asp.Versioning;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Gatekeeper.API.Controllers
{
    /// <summary>
    /// Wrapper for Gatekeeper Integration API service.
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [APIKeyAuthentication]
    [Route("[controller]")]
    public class IntegrationController : ControllerBase
    {
        private readonly IGatekeeperIntegrationAPIService _gatekeeperIntegrationAPIService;

        /// <summary>
        /// Constructor for the Gatekeeper API integration controller.
        /// </summary>
        /// <param name="gatekeeperIntegrationAPIService"></param>
        public IntegrationController(
            IGatekeeperIntegrationAPIService gatekeeperIntegrationAPIService)
        {
            _gatekeeperIntegrationAPIService = gatekeeperIntegrationAPIService;
        }

        /// <summary>
        /// Analyze text to identify PII (personally identifiable information) entities.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <returns>A list of PII entities identified in the analyzed text.</returns>
        [HttpPost("analyze")]
        public async Task<List<string>> AnalyzeText(string text)
        {
            return await _gatekeeperIntegrationAPIService.AnalyzeText(text);
        }

        /// <summary>
        /// Anonymize text with identified PII (personally identifiable information) entities.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <returns>The anonymized text.</returns>
        [HttpPost("anonymize")]
        public async Task<string> AnonymizeText(string text)
        {
            return await _gatekeeperIntegrationAPIService.AnonymizeText(text);
        }
    }
}
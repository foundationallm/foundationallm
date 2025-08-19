using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Core.Services
{
    /// <summary>
    /// Provides functionality to restrict access to resource providers and resource types in the Core API.
    /// </summary>
    /// <param name="logger">The logger used for logging.</param>
    /// <remarks>This class implements the <see cref="IResourcePathAvailabilityCheckerService"/> interface, allowing
    /// API controllers to limit the availability of resource providers and resource types.</remarks>
    public class CoreResourcePathAvailabilityCheckerService(
        ILogger<CoreResourcePathAvailabilityCheckerService> logger) : IResourcePathAvailabilityCheckerService
    {
        private readonly ILogger<CoreResourcePathAvailabilityCheckerService> _logger = logger;
        private readonly Dictionary<HttpMethod, Dictionary<string, HashSet<string>>> _availabilityMap =
            new()
            {
                [HttpMethod.Get] = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
                {
                    [ResourceProviderNames.FoundationaLLM_AIModel] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        AIModelResourceTypeNames.AIModels
                    }
                }
            };

        /// <inheritdoc/>
        public bool IsResourcePathAvailable(
            HttpMethod method,
            ResourcePath resourcePath)
        {
            try
            {
                var isAvailable =
                    _availabilityMap.TryGetValue(method, out var resourceProviders)
                    && resourceProviders.TryGetValue(resourcePath.ResourceProvider!, out var resourceTypes)
                    && resourceTypes.Contains(resourcePath.MainResourceTypeName!);

                if (!isAvailable)
                    _logger.LogWarning(
                        "The resource path {ResourcePath} was blocked for HTTP method {HttpMethod}.",
                        resourcePath.RawResourcePath,
                        method);

                return isAvailable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking the availability of the resource path: {ResourcePath}", resourcePath.RawResourcePath);
                return false;
            }
        }
    }
}

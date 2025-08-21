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
        private readonly Dictionary<HttpMethod, Dictionary<string, Dictionary<string, ResourceTypeAvailability>>> _availabilityMap =
            new()
            {
                [HttpMethod.Get] = new Dictionary<string, Dictionary<string, ResourceTypeAvailability>>(StringComparer.OrdinalIgnoreCase)
                {
                    [ResourceProviderNames.FoundationaLLM_AIModel] = new Dictionary<string, ResourceTypeAvailability>(StringComparer.OrdinalIgnoreCase)
                    {
                        [AIModelResourceTypeNames.AIModels] = new ResourceTypeAvailability
                        {
                            IsResourceTypeAvailable = true
                        }
                    },
                    [ResourceProviderNames.FoundationaLLM_Agent] = new Dictionary<string, ResourceTypeAvailability>(StringComparer.OrdinalIgnoreCase)
                    {
                        [AgentResourceTypeNames.Agents] = new ResourceTypeAvailability
                        {
                            IsResourceTypeAvailable = true
                        }
                    },
                    [ResourceProviderNames.FoundationaLLM_Prompt] = new Dictionary<string, ResourceTypeAvailability>(StringComparer.OrdinalIgnoreCase)
                    {
                        [PromptResourceTypeNames.Prompts] = new ResourceTypeAvailability
                        {
                            IsResourceTypeAvailable = true
                        }
                    }
                },
                [HttpMethod.Post] = new Dictionary<string, Dictionary<string, ResourceTypeAvailability>>(StringComparer.OrdinalIgnoreCase)
                {
                    [ResourceProviderNames.FoundationaLLM_Agent] = new Dictionary<string, ResourceTypeAvailability>(StringComparer.OrdinalIgnoreCase)
                    {
                        [AgentResourceTypeNames.Agents] = new ResourceTypeAvailability
                        {
                            IsResourceTypeAvailable = true,
                            AvailableActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                            {
                                ResourceProviderActions.CheckName
                            }
                        },
                        [AgentResourceTypeNames.AgentTemplates] = new ResourceTypeAvailability
                        {
                            IsResourceTypeAvailable = false,
                            AvailableActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                            {
                                ResourceProviderActions.CreateNew
                            }
                        },
                    },
                    [ResourceProviderNames.FoundationaLLM_Prompt] = new Dictionary<string, ResourceTypeAvailability>(StringComparer.OrdinalIgnoreCase)
                    {
                        [PromptResourceTypeNames.Prompts] = new ResourceTypeAvailability
                        {
                            IsResourceTypeAvailable = true
                        }
                    },
                },
            };

        /// <inheritdoc/>
        public bool IsResourcePathAvailable(
            HttpMethod method,
            ResourcePath resourcePath)
        {
            try
            {
                var isAvailable = true;

                // Check if the method and the resource provider are registered in the availability map.
                if (_availabilityMap.TryGetValue(method, out var resourceProviders)
                    && resourceProviders.TryGetValue(resourcePath.ResourceProvider!, out var currentResourceTypesAvailability))
                {
                    foreach (var resourceTypeInstance in resourcePath.ResourceTypeInstances)
                    {
                        //  Check if the resource type is registered in the availability map.
                        if (currentResourceTypesAvailability is null
                            || !currentResourceTypesAvailability.TryGetValue(resourceTypeInstance.ResourceTypeName, out var resourceTypeAvailability))
                        {
                            isAvailable = false;
                            break;
                        }

                        // NOTE: An action on a resource type can be available even if the resource type itself is not available.
                        // This is to allow for actions that can be performed on a resource type that is not available for general use.

                        bool hasAction = !string.IsNullOrWhiteSpace(resourceTypeInstance.Action);
                        bool isActionAvailable =
                            hasAction
                            && resourceTypeAvailability.AvailableActions.Contains(resourceTypeInstance.Action!);

                        // Check if the action is available.
                        if (hasAction && !isActionAvailable)
                        {
                            isAvailable = false;
                            break;
                        }

                        // Check if the resource type is available.
                        // We only need to check the availability of the resource type if there is no action.
                        // If an action is present it's either unavailable (already checked above) or available, and the resource type availability is not relevant.
                        if (!hasAction
                            && !resourceTypeAvailability.IsResourceTypeAvailable)
                        {
                            isAvailable = false;
                            break;
                        }

                        currentResourceTypesAvailability = resourceTypeAvailability.AvailableSubordinateResourceTypes;
                    }
                }
                else
                    isAvailable = false;

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

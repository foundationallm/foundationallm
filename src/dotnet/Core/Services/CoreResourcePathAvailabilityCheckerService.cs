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
                            IsResourceTypeAvailable = true,
                            AvailableSubordinateResourceTypes = new Dictionary<string, ResourceTypeAvailability>(StringComparer.OrdinalIgnoreCase)
                            {
                                [AgentResourceTypeNames.AgentFiles] = new ResourceTypeAvailability
                                {
                                    IsResourceTypeAvailable = true
                                }
                            }
                        }
                    },
                    [ResourceProviderNames.FoundationaLLM_Prompt] = new Dictionary<string, ResourceTypeAvailability>(StringComparer.OrdinalIgnoreCase)
                    {
                        [PromptResourceTypeNames.Prompts] = new ResourceTypeAvailability
                        {
                            IsResourceTypeAvailable = true
                        }
                    },
                    [ResourceProviderNames.FoundationaLLM_Authorization] = new Dictionary<string, ResourceTypeAvailability>(StringComparer.OrdinalIgnoreCase)
                    {
                        [AuthorizationResourceTypeNames.RoleDefinitions] = new ResourceTypeAvailability
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
                            },
                            AvailableSubordinateResourceTypes = new Dictionary<string, ResourceTypeAvailability>(StringComparer.OrdinalIgnoreCase)
                            {
                                [AgentResourceTypeNames.AgentFiles] = new ResourceTypeAvailability
                                {
                                    IsResourceTypeAvailable = true
                                }
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
                [HttpMethod.Delete] = new Dictionary<string, Dictionary<string, ResourceTypeAvailability>>(StringComparer.OrdinalIgnoreCase)
                {
                    [ResourceProviderNames.FoundationaLLM_Agent] = new Dictionary<string, ResourceTypeAvailability>(StringComparer.OrdinalIgnoreCase)
                    {
                        [AgentResourceTypeNames.Agents] = new ResourceTypeAvailability
                        {
                            IsResourceTypeAvailable = false,
                            AvailableSubordinateResourceTypes = new Dictionary<string, ResourceTypeAvailability>(StringComparer.OrdinalIgnoreCase)
                            {
                                [AgentResourceTypeNames.AgentFiles] = new ResourceTypeAvailability
                                {
                                    IsResourceTypeAvailable = true
                                }
                            }
                        }
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
                var isAvailable = true;

                // Check if the method and the resource provider are registered in the availability map.
                if (_availabilityMap.TryGetValue(method, out var resourceProviders)
                    && resourceProviders.TryGetValue(resourcePath.ResourceProvider!, out var currentResourceTypesAvailability))
                {
                    ResourceTypeAvailability lastResourceTypeAvailability = null!;

                    // Move to the last resource type in the path and update the currentResourceTypesAvailability value while iterating through the resource types.
                    for (int i = 0; i < resourcePath.ResourceTypeInstances.Count; i++)
                    {
                        var resourceTypeInstance = resourcePath.ResourceTypeInstances[i];
                        if (currentResourceTypesAvailability is null
                            || !currentResourceTypesAvailability.TryGetValue(resourceTypeInstance.ResourceTypeName, out var resourceTypeAvailability))
                        {
                            isAvailable = false;
                            break;
                        }

                        lastResourceTypeAvailability = resourceTypeAvailability;
                        currentResourceTypesAvailability = resourceTypeAvailability.AvailableSubordinateResourceTypes;
                    }

                    var lastResourceTypeInstance = resourcePath.ResourceTypeInstances.Last();

                    // At this point lastResourceTypeAvailability and lastResourceTypeInstance are set to the last resource type in the path.
                    // They are all we need to decide whether the resource path is available or not.

                    // First check we have not already decided the resource path is unavailable.
                    if (isAvailable)
                    {
                        // NOTE: An action on a resource type can be available even if the resource type itself is not available.
                        // This is to allow for actions that can be performed on a resource type that is not available for general use.

                        bool hasAction = !string.IsNullOrWhiteSpace(lastResourceTypeInstance.Action);
                        bool isActionAvailable =
                            hasAction
                            && lastResourceTypeAvailability.AvailableActions.Contains(lastResourceTypeInstance.Action!);

                        // The resource path is not available if:
                        // 1. The action is specified and it is not available for the resource type.
                        // 2. The action is not specified and the resource type is not available.
                        if ((hasAction && !isActionAvailable)
                            || (!hasAction && !lastResourceTypeAvailability.IsResourceTypeAvailable))
                            isAvailable = false;
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

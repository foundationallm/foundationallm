using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Authorization;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Core.Services
{
    /// <summary>
    /// Provides functionality to restrict access to resource providers and resource types in the Core API.
    /// </summary>
    /// <param name="logger">The logger used for logging.</param>
    /// <remarks>This class implements the <see cref="IManagementCapabilitiesService"/> interface, allowing
    /// API controllers to limit the availability of resource providers and resource types.</remarks>
    public class CoreManagementCapabilitiesService(
        ILogger<CoreManagementCapabilitiesService> logger) : IManagementCapabilitiesService
    {
        private readonly ILogger<CoreManagementCapabilitiesService> _logger = logger;
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
                    },
                    [ResourceProviderNames.FoundationaLLM_Configuration] = new Dictionary<string, ResourceTypeAvailability>(StringComparer.OrdinalIgnoreCase)
                    {
                        [ConfigurationResourceTypeNames.AppConfigurationSets] = new ResourceTypeAvailability
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
                                ResourceProviderActions.CheckName,
                                ResourceProviderActions.SetOwner
                            },
                            AvailableSubordinateResourceTypes = new Dictionary<string, ResourceTypeAvailability>(StringComparer.OrdinalIgnoreCase)
                            {
                                [AgentResourceTypeNames.AgentFiles] = new ResourceTypeAvailability
                                {
                                    IsResourceTypeAvailable = true
                                },
                                [AgentResourceTypeNames.AgentFileToolAssociations] = new ResourceTypeAvailability
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
                    [ResourceProviderNames.FoundationaLLM_Authorization] = new Dictionary<string, ResourceTypeAvailability>(StringComparer.OrdinalIgnoreCase)
                    {
                        [AuthorizationResourceTypeNames.RoleAssignments] = new ResourceTypeAvailability
                        {
                            IsResourceTypeAvailable = true,
                            AvailableActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                            {
                                ResourceProviderActions.Filter
                            }
                        },
                        [AuthorizationResourceTypeNames.SecurityPrincipals] = new ResourceTypeAvailability
                        {
                            IsResourceTypeAvailable = false,
                            AvailableActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                            {
                                ResourceProviderActions.Filter
                            }
                        }
                    }
                },
                [HttpMethod.Delete] = new Dictionary<string, Dictionary<string, ResourceTypeAvailability>>(StringComparer.OrdinalIgnoreCase)
                {
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
                    [ResourceProviderNames.FoundationaLLM_Authorization] = new Dictionary<string, ResourceTypeAvailability>(StringComparer.OrdinalIgnoreCase)
                    {
                        [AuthorizationResourceTypeNames.RoleAssignments] = new ResourceTypeAvailability
                        {
                            IsResourceTypeAvailable = true
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

        /// <inheritdoc/>
        public bool IsValidRequestPayload(
            object requestPayload) =>
        requestPayload.GetType() switch
        {
            Type t when t == typeof(RoleAssignmentQueryParameters) =>
                ValidateRoleAssignmentQueryParameters((requestPayload as RoleAssignmentQueryParameters)!),
            Type t when t == typeof(RoleAssignment) =>
                ValidateRoleAssignment((requestPayload as RoleAssignment)!),
            _ => false
        };

        private bool ValidateRoleAssignmentQueryParameters(
            RoleAssignmentQueryParameters queryParameters)
        {
            try
            {
                var resourcePath = ResourcePath.GetResourcePath(queryParameters.Scope!);

                if (resourcePath.IsInstancePath)
                {
                    if (queryParameters.SecurityPrincipalIds is null
                        || queryParameters.SecurityPrincipalIds.Count != 1
                        || queryParameters.SecurityPrincipalIds[0] != SecurityPrincipalVariableNames.CurrentUserIds)
                    {
                        _logger.LogWarning("The RoleAssignmentQueryParameters.Scope value is invalid: {Scope}. " +
                            "The instance scope is allowed only when the security principal ids are set to retrieved using the CURRENT_USER_IDS variable.",
                            queryParameters.Scope);
                        return false;
                    }
                }
                else
                {
                    if (resourcePath.ResourceProvider != ResourceProviderNames.FoundationaLLM_Agent
                        || resourcePath.MainResourceTypeName != AgentResourceTypeNames.Agents
                        || resourcePath.ResourceTypeInstances.Count != 1)
                    {
                        _logger.LogWarning("The RoleAssignmentQueryParameters.Scope value is invalid: {Scope}. Only individual agent resources are allowed.",
                            queryParameters.Scope);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while validating the RoleAssignmentQueryParameters with scope {Scope}",
                    queryParameters.Scope);
                return false;
            }
        }

        private bool ValidateRoleAssignment(
            RoleAssignment roleAssignment)
        {
            try
            {
                var resourcePath = ResourcePath.GetResourcePath(roleAssignment.Scope!);
                if (resourcePath.IsRootPath
                    || resourcePath.IsInstancePath
                    || resourcePath.ResourceProvider != ResourceProviderNames.FoundationaLLM_Agent
                    || resourcePath.MainResourceTypeName != AgentResourceTypeNames.Agents
                    || resourcePath.ResourceTypeInstances.Count != 1)
                {
                    _logger.LogWarning("The RoleAssignment.Scope value is invalid: {Scope}",
                        roleAssignment.Scope);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while validating the RoleAssignment with scope {Scope}",
                    roleAssignment.Scope);
                return false;
            }
        }
    }
}

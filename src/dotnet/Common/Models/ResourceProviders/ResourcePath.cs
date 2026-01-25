using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Collections.Immutable;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Provides the logic for handling FoundationaLLM resource identifiers.
    /// </summary>
    public class ResourcePath
    {
        private readonly string? _instanceId;
        private readonly string? _resourceProvider;
        private readonly List<ResourceTypeInstance> _resourceTypeInstances;
        private readonly bool _isRootPath;
        private readonly string _rawResourcePath;

        private const string INSTANCE_TOKEN = "instances";
        private const string RESOURCE_PROVIDER_TOKEN = "providers";

        /// <summary>
        /// The instance id of the resource identifier. Can be null if the resource path does not contain an instance id.
        /// </summary>
        public string? InstanceId => _instanceId;

        /// <summary>
        /// The resource provider of the resource identifier. Can be null if the resource path does not contain a resource provider.
        /// </summary>
        public string? ResourceProvider => _resourceProvider;

        /// <summary>
        /// The resource type instances of the resource identifier.
        /// </summary>
        public List<ResourceTypeInstance> ResourceTypeInstances => _resourceTypeInstances;

        /// <summary>
        /// A flag denoting if the resource path contains subordinate resources or not.
        /// </summary>
        public bool HasSubordinateResourceId =>
            _resourceTypeInstances.Count > 1
            && _resourceTypeInstances.First().ResourceId != null
            && _resourceTypeInstances.Last().ResourceId != null
            && _resourceTypeInstances.First().ResourceType != _resourceTypeInstances.Last().ResourceType;

        /// <summary>
        /// Indicates whether the resource path is a root path ("/") or not.
        /// </summary>
        public bool IsRootPath => _isRootPath;

        /// <summary>
        /// Indicates whether the resource path is an instance path or not (i.e., only contains the FoundationaLLM instance identifier).
        /// </summary>
        public bool IsInstancePath =>
            !(_instanceId == null)
            && (_resourceProvider == null);

        /// <summary>
        /// Indicates whether the resource path refers to a resource type (does not contain a resource name).
        /// </summary>
        public bool IsResourceTypePath =>
            _resourceTypeInstances != null
            && _resourceTypeInstances.Count > 0
            && _resourceTypeInstances.Last().ResourceId == null;

        /// <summary>
        /// Gets the name of the main resource type of the path.
        /// </summary>
        /// <remarks>
        /// The main resource type is the first resource type in the path. In the case of nested resources, this will be the resource type of the main resource.
        /// </remarks>
        public string? MainResourceTypeName =>
            _resourceTypeInstances == null || _resourceTypeInstances.Count == 0
            ? null
            : _resourceTypeInstances[0].ResourceTypeName;

        /// <summary>
        /// Gets the object type of the main resource type of the path.
        /// </summary>
        /// <remarks>
        /// The main resource type is the first resource type in the path. In the case of nested resources, this will be the resource type of the main resource.
        /// </remarks>
        public Type? MainResourceType =>
            _resourceTypeInstances == null || _resourceTypeInstances.Count == 0
            ? null
            : _resourceTypeInstances[0].ResourceType;

        /// <summary>
        /// Gets the resource id of the main resource type of the path.
        /// </summary>
        public string? MainResourceId =>
            _resourceTypeInstances == null || _resourceTypeInstances.Count == 0
            ? null
            : _resourceTypeInstances[0].ResourceId;

        /// <summary>
        /// Gets the resource type name of the resource identified by the path.
        /// </summary>
        /// <remarks>
        /// This is the last resource type name in the path. If the path refers to nested resources, this will be the resource type name of the last resource.
        /// Otherwise, it will be the resource type name of the main resource (and hence identical to <see cref="MainResourceTypeName"/>).
        /// </remarks>
        public string? ResourceTypeName =>
            _resourceTypeInstances == null || _resourceTypeInstances.Count == 0
            ? null
            : _resourceTypeInstances.Last().ResourceTypeName;

        /// <summary>
        /// Gets the resource type of the resource identified by the path.
        /// </summary>
        /// <remarks>
        /// This is the last resource type in the path. If the path refers to nested resources, this will be the resource type of the last resource.
        /// Otherwise, it will be the resource type of the main resource (and hence identical to <see cref="MainResourceType"/>).
        /// </remarks>
        public Type? ResourceType =>
            _resourceTypeInstances == null || _resourceTypeInstances.Count == 0
            ? null
            : _resourceTypeInstances.Last().ResourceType;

        /// <summary>
        /// Gets the resource id of the resource identified by the path.
        /// </summary>
        /// <remarks>
        /// This is the last resource id in the path. If the path refers to nested resources, this will be the resource id of the last resource.
        /// Otherwise, it will be the resource id of the main resource (and hence identical to <see cref="MainResourceId"/>).
        /// </remarks>
        public string? ResourceId =>
            _resourceTypeInstances == null || _resourceTypeInstances.Count == 0
            ? null
            : _resourceTypeInstances.Last().ResourceId;

        /// <summary>
        /// Indicates whether the resource path contains a valid resource id or not.
        /// </summary>
        public bool HasResourceId =>
            _resourceTypeInstances != null
            && _resourceTypeInstances.Count > 0
            && !string.IsNullOrWhiteSpace(_resourceTypeInstances.Last().ResourceId);

        /// <summary>
        /// Gets the resource type name of the parent resource of the path.
        /// </summary>
        public string? ParentResourceTypeName =>
            _resourceTypeInstances is null
            || _resourceTypeInstances.Count <= 1
            ? null
            : _resourceTypeInstances[^2].ResourceTypeName;

        /// <summary>
        /// Gets the resource type of the parent resource of the path.
        /// </summary>
        public Type? ParentResourceType =>
            _resourceTypeInstances is null
            || _resourceTypeInstances.Count <= 1
            ? null
            : _resourceTypeInstances[^2].ResourceType;

        /// <summary>
        /// Gets the resource id of the parent resource of the path.
        /// </summary>
        public string? ParentResourceId =>
            _resourceTypeInstances is null
            || _resourceTypeInstances.Count <= 1
            ? null
            : _resourceTypeInstances[^2].ResourceId;

        /// <summary>
        /// Gets the action (if any) specified in the resource path.
        /// </summary>
        public string? Action =>
            _resourceTypeInstances == null || _resourceTypeInstances.Count == 0
            ? null
            : _resourceTypeInstances.Last().Action;

        /// <summary>
        /// Indicates whether the resource path contains a valid action or not.
        /// </summary>
        public bool HasAction =>
            _resourceTypeInstances != null
            && _resourceTypeInstances.Count > 0
            && !string.IsNullOrWhiteSpace(_resourceTypeInstances.Last().Action);

        /// <summary>
        /// Gets the raw resource path which was used to create the resource path object.
        /// </summary>
        public string RawResourcePath =>
            _rawResourcePath;

        /// <summary>
        /// Gets the resource object identifier associated with the resource path.
        /// </summary>
        /// <remarks>
        /// Only fully qualified resource paths can be converted to object identifiers.
        /// </remarks>
        public string? ObjectId =>
            string.IsNullOrWhiteSpace(_instanceId) || string.IsNullOrWhiteSpace(_rawResourcePath)
            ? null
            : _rawResourcePath;

        /// <summary>
        /// Gets the resource object identifier associated with the resource path without the action name.
        /// </summary>
        /// <remarks>
        /// Only fully qualified resource paths can be converted to object identifiers.
        /// </remarks>
        public string? ObjectIdWithoutAction =>
            string.IsNullOrWhiteSpace(_instanceId) || string.IsNullOrWhiteSpace(_rawResourcePath)
            ? null
            : HasAction
                ? _rawResourcePath.Substring(0, _rawResourcePath.LastIndexOf('/'))
                : _rawResourcePath;

        /// <summary>
        /// Gets the parent object identifier of the resource path.
        /// </summary>
        public string? ParentObjectId =>
             _resourceTypeInstances is null
            || _resourceTypeInstances.Count <= 1
            ? null
            : _resourceTypeInstances[^2].ObjectId;

        /// <summary>
        /// Creates a new resource identifier from a resource path optionally allowing an action.
        /// </summary>
        /// <param name="resourcePath">The resource path used to create the resource identifier.</param>
        /// <param name="allowedResourceProviders">Provides a list of allowed resource providers.</param>
        /// <param name="allowedResourceTypes">Provides a dictionary of <see cref="ResourceTypeDescriptor"/> used to validate resource types.</param>
        /// <param name="allowAction">Optional parameter that indicates whether an action name is allowed as part of the resource identifier.</param>
        public ResourcePath(
            string resourcePath,
            ImmutableList<string> allowedResourceProviders,
            Dictionary<string, ResourceTypeDescriptor> allowedResourceTypes,
            bool allowAction = true)
        {
            if (!resourcePath.StartsWith('/'))
                resourcePath = "/" + resourcePath;
            _rawResourcePath = resourcePath;

            ParseResourcePath(
                resourcePath,
                allowedResourceProviders,
                allowedResourceTypes,
                allowAction,
                out _isRootPath,
                out _instanceId,
                out _resourceProvider,
                out _resourceTypeInstances);
        }

        /// <summary>
        /// Tries to parse a resource path and create a resource identifier from it.
        /// </summary>
        /// <param name="resourcePath">The resource path used to create the resource identifier.</param>
        /// <param name="allowedResourceProviders">Provides a list of allowed resource providers.</param>
        /// <param name="allowedResourceTypes">Provides a dictionary of <see cref="ResourceTypeDescriptor"/> used to validate resource types.</param>
        /// <param name="allowAction">Optional parameter that indicates whether an action name is allowed as part of the resource identifier.</param>
        /// <param name="resourcePathInstance">The parsed resource path.</param>
        /// <returns>True if the resource path is parsed successfully.</returns>
        public static bool TryParse(
            string resourcePath,
            ImmutableList<string> allowedResourceProviders,
            Dictionary<string, ResourceTypeDescriptor> allowedResourceTypes,
            bool allowAction,
            out ResourcePath? resourcePathInstance)
        {
            try
            {
                resourcePathInstance = new ResourcePath(
                    resourcePath,
                    allowedResourceProviders,
                    allowedResourceTypes,
                    allowAction);
                return true;
            }
            catch
            {
                resourcePathInstance = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to parse an object instance from a URL-encoded resource path.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="urlEncodedResourcePath">The URL-encoded representation of the resource path.</param>
        /// <param name="resourcePathInstance">The parsed resource path.</param>
        /// <returns>true if the string was successfully parsed and the object was created; otherwise, false.</returns>
        /// <remarks>The format of the URL-encoded string representation must be {resource_provider}|{resource_type}|{resource_name}.
        /// For example, an agent named MAA-01 will be identified by <code>FoundationaLLM.Agent|agents|MAA-01</code></remarks>
        public static bool TryParseFromURLEncodedString(
            string instanceId,
            string urlEncodedResourcePath,
            out ResourcePath? resourcePathInstance)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(instanceId);

            resourcePathInstance = null;
            if (string.IsNullOrWhiteSpace(urlEncodedResourcePath))
                return false;
            try
            {
                var tokens = urlEncodedResourcePath.Split('|');
                if (tokens.Length != 3
                    || tokens.Any(t => string.IsNullOrWhiteSpace(t)))
                    return false;

                var resourcePath = $"/instances/{instanceId}/providers/{tokens[0]}/{tokens[1]}/{tokens[2]}";
                resourcePathInstance = ResourcePath.GetResourcePath(resourcePath);
                return true;
            }
            catch
            {
                
                return false;
            }
        }

        /// <summary>
        /// Tries to retrieve the identifier of the FoundationaLLM instance from a resource path.
        /// </summary>
        /// <param name="resourcePath">The resource path to analyze.</param>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <returns>True if a valid intance identifier is retrieved successfully.</returns>
        public static bool TryParseInstanceId(string resourcePath, out string? instanceId)
        {
            instanceId = GetInstanceId(resourcePath);
            return instanceId != null;
        }

        /// <summary>
        /// Tries to retrieve the name of the resource provider from a resource path.
        /// </summary>
        /// <param name="resourcePath">The resource path to analyze.</param>
        /// <param name="resourceProvider">The resource provider name.</param>
        /// <returns>True if a valid resource provider name is retrieved successfully.</returns>
        public static bool TryParseResourceProvider(string resourcePath, out string? resourceProvider)
        {
            resourceProvider = GetResourceProvider(resourcePath);
            return resourceProvider != null;
        }

        /// <summary>
        /// Computes the object id of the resource identifier.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id. Provide this if the resource does not have a fully-qualified resource identifier.</param>
        /// <param name="resourceProvider">The name of the resource provider. Provide this if the resource does not have a fully-qualified resource identifier.</param>
        /// <returns>The object id.</returns>
        public string GetObjectId(
            string? instanceId,
            string? resourceProvider)
        {
            if (_instanceId == null && _resourceProvider == null)
            {
                if (_resourceTypeInstances == null
                    || _resourceTypeInstances.Count == 0)
                    throw new ResourceProviderException("The resource path cannot be converted to a fully qualified resource identifier.",
                        StatusCodes.Status400BadRequest);
                else
                    return $"/instances/{instanceId}/providers/{resourceProvider}/{string.Join("/",
                        _resourceTypeInstances.Select(i => i.ResourceId == null ? $"{i.ResourceTypeName}" : $"{i.ResourceTypeName}/{i.ResourceId}").ToArray())}";
            }
            else
            {
                if (_instanceId == null
                    || _resourceProvider == null
                    || _resourceTypeInstances == null
                    || _resourceTypeInstances.Count == 0)
                    throw new ResourceProviderException("The resource path does not represent a fully qualified resource identifier.",
                        StatusCodes.Status400BadRequest);
                else
                    return $"/instances/{_instanceId}/providers/{_resourceProvider}/{string.Join("/",
                        _resourceTypeInstances.Select(i => i.ResourceId == null ? $"{i.ResourceTypeName}" : $"{i.ResourceTypeName}/{i.ResourceId}").ToArray())}";
            }
        }

        /// <summary>
        /// Computes the object id of the resource type associated with the resource identifier.
        /// </summary>
        /// <returns>The resource type object id.</returns>
        /// <exception cref="ResourceProviderException"></exception>
        public string GetResourceTypeObjectId()
        {
            if (_instanceId == null
                || _resourceProvider == null
                || _resourceTypeInstances == null
                || _resourceTypeInstances.Count == 0)
                throw new ResourceProviderException($"Cannot extract a resource type object id from the {_rawResourcePath} resource path.",
                    StatusCodes.Status400BadRequest);
            else
            {
                var resourceTypePath = _resourceTypeInstances.Count == 1
                    ? _resourceTypeInstances[0].ResourceTypeName
                    : string.Join("/", [.. _resourceTypeInstances.Select(i => i.ResourceTypeName)]);

                return $"/instances/{_instanceId}/providers/{_resourceProvider}/{resourceTypePath}";
            }
        }

        /// <summary>
        /// Computes the object id of the resource identifier.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="resourceProviderName">The name of the resource provider.</param>
        /// <param name="resourceTypeName">The name of the resource type.</param>
        /// <param name="resourceName">The name of the resource.</param>
        /// <returns>The object id.</returns>
        public static string GetObjectId(
            string instanceId,
            string resourceProviderName,
            string resourceTypeName,
            string resourceName) =>
            $"/instances/{instanceId}/providers/{resourceProviderName}/{resourceTypeName}/{resourceName}";

        /// <summary>
        /// Computes the object id of the resource identifier.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="resourceProviderName">The name of the resource provider.</param>
        /// <param name="mainResourceTypeName">The name of the main resource type.</param>
        /// <param name="mainResourceName">The name of the main resource.</param>
        /// <param name="resourceTypeName">The name of the resource type.</param>
        /// <param name="resourceName">The name of the resource.</param>
        /// <returns>The object id.</returns>
        public static string GetObjectId(
            string instanceId,
            string resourceProviderName,
            string mainResourceTypeName,
            string mainResourceName,
            string resourceTypeName,
            string resourceName) =>
            $"/instances/{instanceId}/providers/{resourceProviderName}/{mainResourceTypeName}/{mainResourceName}/{resourceTypeName}/{resourceName}";

        /// <summary>
        /// Determines whether the resource path includes another specified resource path.
        /// </summary>
        /// <param name="other">The <see cref="ResourcePath"/> to check for inclusion.</param>
        /// <param name="allowEqual">Indicates whether an equal resource path is considered to be included or not.</param>
        /// <returns>True if the specified resource path is included in the resource path.</returns>
        public bool IncludesResourcePath(ResourcePath other, bool allowEqual = true)
        {
            if (_isRootPath)
                // The other path is included only if it is root.
                return
                    other.IsRootPath && allowEqual;

            if (IsInstancePath)
            {
                // An instance path includes a root path.
                if (other.IsRootPath)
                    return true;

                // An instance path includes another instance path for the same instance id.
                if (other.IsInstancePath)
                    return (_instanceId == other.InstanceId) && allowEqual;
            }

            // A full path includes a root path or an instance path.
            if (other.IsRootPath || other.IsInstancePath)
                return true;

            // A full path does not include a full path that is longer than it.
            if (other.ResourceTypeInstances.Count > _resourceTypeInstances.Count)
                return false;

            for (int i = 0; i < other.ResourceTypeInstances.Count; i++)
            {
                if (!_resourceTypeInstances[i].Includes(other.ResourceTypeInstances[i]))
                    return false;
            }

            return
                allowEqual
                || StringComparer.OrdinalIgnoreCase.Compare(_rawResourcePath, other.RawResourcePath) != 0;
        }

        /// <summary>
        /// Determines whether the resource path matches exactly (including order) the resource types of another specified resource path.
        /// </summary>
        /// <param name="other">The <see cref="ResourcePath"/> to be matched.</param>
        /// <returns>True if the resource path matches exactly (including order) the resource types of the other resource path.</returns>
        public bool MatchesResourceTypes(ResourcePath other)
        {
            if (_resourceTypeInstances.Count == 0
                || other.ResourceTypeInstances.Count == 0
                || _resourceTypeInstances.Count != other.ResourceTypeInstances.Count)
                return false;

            for (int i = 0; i < _resourceTypeInstances.Count; i++)
                if (!StringComparer.OrdinalIgnoreCase.Equals(
                    _resourceTypeInstances[i].ResourceTypeName,
                    other.ResourceTypeInstances[i].ResourceTypeName))
                    return false;

            return true;
        }

        /// <summary>
        /// Parses a resource path.
        /// </summary>
        /// <param name="resourcePath">The resource path to be parsed.</param>
        /// <returns>A <see cref="ResourcePath"/> object containing the parsed resource path.</returns>
        public static ResourcePath GetResourcePath(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
                throw new ResourcePathException("The resource path to parse cannot be an empty or whitespace string.");

            _ = TryParseResourceProvider(resourcePath, out var resourceProvider);

            var allowedResourceProviders = ImmutableList<string>.Empty;
            var allowedResourceTypes = new Dictionary<string, ResourceTypeDescriptor>();

            if (resourceProvider != null)
            {
                allowedResourceProviders = allowedResourceProviders.Add(resourceProvider);
                allowedResourceTypes = GetAllowedResourceTypes(resourceProvider);
            }

            if (!TryParse(
                resourcePath,
                allowedResourceProviders,
                allowedResourceTypes,
                true,
                out ResourcePath? parsedResourcePath)
                || parsedResourcePath is null)
                throw new ResourcePathException($"The resource path [{resourcePath}] is invalid.");

            return parsedResourcePath!;
        }

        /// <summary>
        /// Joins a resource path with a subordinate resource path.
        /// </summary>
        /// <param name="resourcePath">The main resource path.</param>
        /// <param name="subordinateResourcePath">The subordinate resource path.</param>
        /// <returns>The combined resource path.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string Join(string resourcePath, string subordinateResourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
                throw new ArgumentNullException(nameof(resourcePath));
            if (string.IsNullOrWhiteSpace(subordinateResourcePath))
                throw new ArgumentNullException(nameof(subordinateResourcePath));
            return $"{resourcePath.TrimEnd('/')}/{subordinateResourcePath.TrimStart('/')}";
        }

        /// <summary>
        /// Retrieves the allowed resource types for a specified resource provider.
        /// </summary>
        /// <param name="resourceProvider">The name of the resource provider.</param>
        public static Dictionary<string, ResourceTypeDescriptor> GetAllowedResourceTypes(string resourceProvider) =>
            resourceProvider switch
            {
                ResourceProviderNames.FoundationaLLM_Agent => AgentResourceProviderMetadata.AllowedResourceTypes,
                ResourceProviderNames.FoundationaLLM_DataSource => DataSourceResourceProviderMetadata.AllowedResourceTypes,
                ResourceProviderNames.FoundationaLLM_Prompt => PromptResourceProviderMetadata.AllowedResourceTypes,
                ResourceProviderNames.FoundationaLLM_Configuration => ConfigurationResourceProviderMetadata.AllowedResourceTypes,
                ResourceProviderNames.FoundationaLLM_Attachment => AttachmentResourceProviderMetadata.AllowedResourceTypes,
                ResourceProviderNames.FoundationaLLM_Authorization => AuthorizationResourceProviderMetadata.AllowedResourceTypes,
                ResourceProviderNames.FoundationaLLM_AIModel => AIModelResourceProviderMetadata.AllowedResourceTypes,
                ResourceProviderNames.FoundationaLLM_AzureAI => AzureAIResourceProviderMetadata.AllowedResourceTypes,
                ResourceProviderNames.FoundationaLLM_AzureOpenAI => AzureOpenAIResourceProviderMetadata.AllowedResourceTypes,
                ResourceProviderNames.FoundationaLLM_Conversation => ConversationResourceProviderMetadata.AllowedResourceTypes,
                ResourceProviderNames.FoundationaLLM_DataPipeline => DataPipelineResourceProviderMetadata.AllowedResourceTypes,
                ResourceProviderNames.FoundationaLLM_Plugin => PluginResourceProviderMetadata.AllowedResourceTypes,
                ResourceProviderNames.FoundationaLLM_Vector => VectorResourceProviderMetadata.AllowedResourceTypes,
                ResourceProviderNames.FoundationaLLM_Context => ContextResourceProviderMetadata.AllowedResourceTypes,
                _ => []
            };

        private void ParseResourcePath(
            string resourcePath,
            ImmutableList<string> allowedResourceProviders,
            Dictionary<string, ResourceTypeDescriptor> allowedResourceTypes,
            bool allowAction,
            out bool rootPath,
            out string? instanceId,
            out string? resourceProvider,
            out List<ResourceTypeInstance> resourceTypeInstances)
        {
            try
            {
                instanceId = null;
                resourceProvider = null;
                resourceTypeInstances = [];
                rootPath = false;

                // Check if we deal with a root path ("/").
                if (resourcePath == "/")
                {
                    rootPath = true;
                    return;
                }

                var tokens = GetPathTokens(resourcePath);

                int currentIndex = 0;

                // Retrieve the instance id if present.
                if (tokens[0] == INSTANCE_TOKEN)
                {
                    instanceId = tokens[1];

                    // The instance id must be a valid GUID.
                    if (!Guid.TryParse(instanceId, out _))
                        throw new ResourcePathException($"The FoundationaLLM instance identifier is invalid.");
                    currentIndex = 2;
                }
                else
                    throw new ResourcePathException($"All resource paths must include the FoundationaLLM instance identifier.");

                // An instance identifier is a valid resource path.
                if (currentIndex >= tokens.Length)
                    return;

                // Retrieve the resource provider if present.
                if (tokens[currentIndex] == RESOURCE_PROVIDER_TOKEN)
                {
                    resourceProvider = tokens[currentIndex + 1];

                    // Raise an exception if the resource provider name is not a valid FoundationaLLM resource provider.
                    if (!allowedResourceProviders.Contains(resourceProvider))
                        throw new ResourcePathException($"The resource provider name is invalid.");

                    currentIndex += 2;
                }
                else if (currentIndex != 0)
                    // If an instance id token is present, the resource provider token must be present as well.
                    throw new ResourcePathException($"The resource provider is missing.");

                // There must be at least one resource type after instance id and/or resource provider.
                if (currentIndex >= tokens.Length)
                    throw new ResourcePathException($"The main resource type is missing.");

                var currentResourceTypes = allowedResourceTypes;

                while (currentIndex < tokens.Length)
                {
                    if (currentResourceTypes == null)
                        throw new ResourcePathException("Missing allowed resource types.");

                    var sharedResourceType = default(ResourceTypeDescriptor);

                    if (!currentResourceTypes.TryGetValue(tokens[currentIndex], out ResourceTypeDescriptor? currentResourceType)
                        && !SharedResourceProviderMetadata.AllowedResourceTypes.TryGetValue(tokens[currentIndex], out sharedResourceType))
                        throw new ResourcePathException($"{tokens[currentIndex]} is not a valid resource type.");

                    currentResourceType ??= sharedResourceType;

                    var resourceTypeInstance = new ResourceTypeInstance(
                        tokens[currentIndex],
                        currentResourceType!.ResourceType,
                        string.Join('/', [string.Empty, ..tokens.Take(currentIndex + 1)]));
                    resourceTypeInstances.Add(resourceTypeInstance);

                    if (currentIndex + 1 == tokens.Length)
                        // No more tokens left, which means we have a resource type instance without actions or subtypes.
                        // This will be used by resource providers to retrieve all resources of a specific resource type.
                        break;

                    // Check if the next token is an action or a resource id.
                    // The only way to determine is by matching the token with the list of supported actions.
                    // If there is not match, the token is considered to be a resource identifier.
                    var action = currentResourceType.Actions.FirstOrDefault(a => a.Name == tokens[currentIndex + 1]);
                    if (action != null)
                    {
                        // The next token is an action

                        // If actions are not allowed or the action is not allowed on a resource type, raise an exception.
                        if (!allowAction
                            || !action.AllowedOnResourceType)
                            throw new ResourcePathException($"The action {action.Name} is not allowed.");

                        resourceTypeInstance.Action = tokens[currentIndex + 1];

                        // It must be the last token
                        if (currentIndex + 2 == tokens.Length)
                            break;
                        else
                            throw new ResourcePathException($"The structure of the resource path is invalid (unexpected number of tokens).");
                    }
                    else
                    {
                        // The next token is a resource identifier
                        resourceTypeInstance.ResourceId = tokens[currentIndex + 1];

                        // Check if the token after it exists and is an action.
                        if (currentIndex + 2 <= tokens.Length - 1)
                        {
                            // Check if the next token is an action or a resource id.
                            // The only way to determine is by matching the token with the list of supported actions.
                            // If there is not match, the token is considered to be a resource identifier.
                            var action2 = currentResourceType.Actions.FirstOrDefault(a => a.Name == tokens[currentIndex + 2]);
                            if (action2 != null)
                            {
                                // The token represents an action.

                                // If actions are not allowed or the action is not allowed on a resource, raise an exception.
                                if (!allowAction
                                    || !action2.AllowedOnResource)
                                    throw new ResourcePathException($"The action {action2.Name} is not allowed.");

                                resourceTypeInstance.Action = tokens[currentIndex + 2];

                                // It must be the last token
                                if (currentIndex + 2 == tokens.Length - 1)
                                    break;
                                else
                                    throw new ResourcePathException($"The structure of the resource path is invalid (unexpected number of tokens).");
                            }
                        }
                    }

                    currentResourceTypes = currentResourceType.SubTypes;
                    currentIndex += 2;
                }
            }
            catch (Exception ex)
            {
                throw new ResourceProviderException(
                    $"The resource path [{resourcePath}] is invalid.",
                    ex,
                    StatusCodes.Status400BadRequest);
            }
        }

        private static string[] GetPathTokens(string resourcePath)
        {
            // Resource path cannot be null, empty or whitespace.
            if (string.IsNullOrWhiteSpace(resourcePath))
                throw new ResourcePathException("The resource path cannot be null, empty or whitespace.");

            // Remove leading slash if present.
            if (resourcePath.StartsWith('/'))
                resourcePath = resourcePath[1..];

            var tokens = resourcePath.Split('/', StringSplitOptions.TrimEntries);

            // None of the tokens can be null, empty or whitespace.
            if (tokens.Any(t => string.IsNullOrWhiteSpace(t)))
                throw new ResourcePathException("The resource path contains invalid tokens.");

            return tokens;
        }

        private static string? GetResourceProvider(
            string resourcePath)
        {
            try
            {
                var tokens = GetPathTokens(resourcePath);

                var startIndex = tokens[0] == INSTANCE_TOKEN
                    ? 2
                    : 0;

                if (startIndex + 1 < tokens.Length
                    && tokens[startIndex] == RESOURCE_PROVIDER_TOKEN)
                {
                    if (ResourceProviderNames.All.Contains(tokens[startIndex + 1]))
                        return tokens[startIndex + 1];
                }
            }
            catch
            {
            }

            return null;
        }

        private static string? GetInstanceId(
            string resourcePath)
        {
            try
            {
                var tokens = GetPathTokens(resourcePath);

                if (tokens[0] == INSTANCE_TOKEN)
                {
                    if (Guid.TryParse(tokens[1], out _))
                        return tokens[1];
                }
            }
            catch
            {
            }

            return null;
        }
    }
}

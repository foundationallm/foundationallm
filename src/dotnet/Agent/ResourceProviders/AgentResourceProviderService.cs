using FluentValidation;
using FoundationaLLM.Agent.Models.Resources;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Constants.AzureAI;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.DataPipelines;
using FoundationaLLM.Common.Constants.Events;
using FoundationaLLM.Common.Constants.OpenAI;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentAccessTokens;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentFiles;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentTemplates;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Models.ResourceProviders.AzureAI;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Common.Services.ResourceProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FoundationaLLM.Agent.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Agent resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="cacheOptions">The options providing the <see cref="ResourceProviderCacheSettings"/> with settings for the resource provider cache.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationServiceClient"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="agentTemplateService">The agent template service used to create agents from templates.</param>
    /// <param name="cosmosDBService">The <see cref="IAzureCosmosDBService"/> providing Cosmos DB services.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to provide loggers for logging.</param>
    /// <param name="proxyMode">Indicates whether the resource provider is running in proxy mode.</param>
    public class AgentResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IOptions<ResourceProviderCacheSettings> cacheOptions,
        IAuthorizationServiceClient authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Agent)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IAgentTemplateService agentTemplateService,
        IAzureCosmosDBService cosmosDBService,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory,
        bool proxyMode = false)
        : ResourceProviderServiceBase<AgentReference>(
            instanceOptions.Value,
            cacheOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            loggerFactory.CreateLogger<AgentResourceProviderService>(),
            eventTypesToSubscribe: [
                EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand
            ],
            useInternalReferencesStore: true,
            proxyMode: proxyMode)
    {
        private readonly IAzureCosmosDBService _cosmosDBService = cosmosDBService;
        private readonly IAgentTemplateService _agentTemplateService = agentTemplateService;

        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            AgentResourceProviderMetadata.AllowedResourceTypes;

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Agent;

        /// <inheritdoc/>
        protected override async Task InitializeInternal()
        {
            _agentTemplateService.SetResourceProviderServices(
                _serviceProvider.GetServices<IResourceProviderService>());
            await Task.CompletedTask;
        }

        #region Resource provider support for Management API

        /// <inheritdoc/>
        protected override async Task<object> GetResourcesAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options = null) =>
            resourcePath.ResourceTypeName switch
            {
                AgentResourceTypeNames.Agents => await LoadResources<AgentBase>(
                    resourcePath.ResourceTypeInstances[0],
                    authorizationResult,
                    options ?? new ResourceProviderGetOptions
                    {
                        IncludeRoles = resourcePath.IsResourceTypePath,
                    }),
                AgentResourceTypeNames.Workflows => await LoadResources<Workflow>(
                    resourcePath.ResourceTypeInstances[0],
                    authorizationResult,
                    options ?? new ResourceProviderGetOptions
                    {
                        IncludeRoles = resourcePath.IsResourceTypePath,
                    }),
                AgentResourceTypeNames.Tools => await LoadResources<Tool>(
                   resourcePath.ResourceTypeInstances[0],
                   authorizationResult,
                   options ?? new ResourceProviderGetOptions
                   {
                       IncludeRoles = resourcePath.IsResourceTypePath,
                   }),
                AgentResourceTypeNames.AgentAccessTokens => await LoadAgentAccessTokens(resourcePath)!,
                AgentResourceTypeNames.AgentFiles => await LoadAgentFiles(resourcePath, userIdentity, options)!,
                AgentResourceTypeNames.AgentFileToolAssociations => await LoadAgentFileToolAssociations(resourcePath, userIdentity)!,
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        /// <inheritdoc/>
        protected override async Task<object> UpsertResourceAsync(
            ResourcePath resourcePath,
            string? serializedResource,
            ResourceProviderFormFile? formFile,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            Func<object, bool>? requestPayloadValidator = null) =>

            resourcePath.ResourceTypeName switch
            {
                AgentResourceTypeNames.Agents => await UpdateAgent(
                    resourcePath,
                    JsonSerializer.Deserialize<AgentBase>(serializedResource!)
                        ?? throw new ResourceProviderException(
                            "The object definition is invalid.",
                            StatusCodes.Status400BadRequest),
                    authorizationResult,
                    userIdentity),
                AgentResourceTypeNames.AgentFiles => await UpdateAgentFile(resourcePath, formFile!, userIdentity),
                AgentResourceTypeNames.AgentAccessTokens => await UpdateAgentAccessToken(resourcePath, serializedResource!, authorizationResult, userIdentity),
                AgentResourceTypeNames.AgentFileToolAssociations => await UpdateFileToolAssociations(resourcePath, serializedResource!, userIdentity),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        /// <inheritdoc/>
        protected override async Task<object> ExecuteActionAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction,
            UnifiedUserIdentity userIdentity,
            Func<object, bool>? requestPayloadValidator = null) =>
            resourcePath.ResourceTypeName switch
            {
                SharedResourceTypeNames.Management => resourcePath.Action switch
                {
                    ResourceProviderActions.TriggerCommand => await ExecuteManagementAction(
                        resourcePath,
                        authorizationResult,
                        serializedAction),
                    _ => throw new ResourceProviderException($"The action {resourcePath.Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                AgentResourceTypeNames.Agents => resourcePath.Action switch
                {
                    ResourceProviderActions.CheckName => await CheckAgentName(authorizationResult, serializedAction),
                    ResourceProviderActions.Purge => await PurgeResource<AgentBase>(resourcePath),
                    ResourceProviderActions.SetDefault => await SetDefaultResource<AgentBase>(resourcePath),
                    ResourceProviderActions.SetOwner => await SetAgentOwnerUser(
                        resourcePath,
                        authorizationResult,
                        serializedAction,
                        userIdentity),
                    _ => throw new ResourceProviderException($"The action {resourcePath.Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                AgentResourceTypeNames.AgentAccessTokens => resourcePath.Action switch
                {
                    ResourceProviderActions.Validate => await ValidateAgentAccessToken(
                        resourcePath,
                        JsonSerializer.Deserialize<AgentAccessTokenValidationRequest>(serializedAction)!,
                        userIdentity),

                    _ => throw new ResourceProviderException($"The action {resourcePath.Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                AgentResourceTypeNames.AgentTemplates => resourcePath.Action switch
                {
                    ResourceProviderActions.CreateNew => await CreateNewAgentFromTemplate(
                        resourcePath,
                        authorizationResult,
                        JsonSerializer.Deserialize<AgentCreationFromTemplateRequest>(serializedAction)!,
                        userIdentity),
                    _ => throw new ResourceProviderException($"The action {resourcePath.Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                _ => throw new ResourceProviderException()
            };

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            switch (resourcePath.ResourceTypeName)
            {
                case AgentResourceTypeNames.Agents:
                    await DeleteResource<AgentBase>(resourcePath);
                    break;
                case AgentResourceTypeNames.AgentAccessTokens:
                    await DeleteAgentAccessToken(resourcePath);
                    break;
                case AgentResourceTypeNames.AgentFiles:
                    await DeleteAgentFile(resourcePath, userIdentity);
                    break;
                default:
                    throw new ResourceProviderException(
                        $"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest);
            };
            await SendResourceProviderEvent(
                    EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand);
        }

        #endregion

        #region Resource provider strongly typed operations

        /// <inheritdoc/>
        protected override async Task<T> GetResourceAsyncInternal<T>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options = null,
            ResourceBase? parentResourceInstance = null)
        {
            switch (resourcePath.ResourceTypeName)
            {
                case AgentResourceTypeNames.Agents:
                case AgentResourceTypeNames.Workflows:
                case AgentResourceTypeNames.Tools:
                    return (await LoadResource<T>(resourcePath.ResourceId!))!;
                case AgentResourceTypeNames.AgentFiles:
                    var agentPrivateFile = await _cosmosDBService.GetAgentFile(_instanceSettings.Id, resourcePath.MainResourceId!, resourcePath.ResourceId!)
                        ?? throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.MainResourceTypeName} was not found.");

                    return (await LoadAgentFile(agentPrivateFile, loadContent: options?.LoadContent ?? false)) as T
                        ?? throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.MainResourceTypeName} could not be loaded.");
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.MainResourceTypeName} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest);
            }
        }

        /// <inheritdoc/>
        protected override async Task<TResult> UpsertResourceAsyncInternal<T, TResult>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            T resource,
            UnifiedUserIdentity userIdentity,
            ResourceProviderUpsertOptions? options = null) =>
            resource.GetType() switch
            {
                Type t when t == typeof(GenericAgent) =>
                    ((await UpdateAgent(
                        resourcePath,
                        (resource as AgentBase)!,
                        authorizationResult,
                        userIdentity,
                        includeResourceInResponse: true)).ToResourceProviderUpsertResult<AgentBase>() as TResult)!,
                _ => throw new ResourceProviderException($"The resource type {resource.GetType().Name} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #endregion

        #region Resource management

        private async Task<ResourceNameCheckResult> CheckAgentName(
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction)
        {
            if (!authorizationResult.Authorized
                && !authorizationResult.HasRequiredRole)
                throw new ResourceProviderException("Access is not authorized.", StatusCodes.Status403Forbidden);

            return await CheckResourceName<AgentBase>(
                JsonSerializer.Deserialize<ResourceName>(serializedAction)!);
        }

        private async Task<ResourceProviderUpsertResult> UpdateAgent(
            ResourcePath resourcePath,
            AgentBase agent,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            bool includeResourceInResponse = false)
        {
            var existingAgentReference = await _resourceReferenceStore!.GetResourceReference(agent.Name);
            var existingAgent = existingAgentReference is not null
                ? await LoadResource<AgentBase>(existingAgentReference)
                : null;

            if (existingAgentReference is not null
                && !authorizationResult.Authorized)
            {
                // The resource already exists and the user is not authorized to update it.
                // Irrespective of whether the user has the required role or not, we need to throw an exception in the case of existing resources.
                // The required role only allows the user to create a new resource.
                // This check is needed because it's only here that we can determine if the resource exists.
                _logger.LogWarning("Access to the resource path {ResourcePath} was not authorized for user {UserName} : userId {UserId}.",
                    resourcePath.RawResourcePath, userIdentity!.Username, userIdentity!.UserId);
                throw new ResourceProviderException("Access is not authorized.", StatusCodes.Status403Forbidden);
            }

            if (resourcePath.ResourceTypeInstances[0].ResourceId != agent.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            var agentReference = new AgentReference
            {
                Name = agent.Name!,
                Type = agent.Type!,
                Filename = $"/{_name}/{agent.Name}.json",
                Deleted = false
            };

            agent.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);

            if (agent.Workflow is AzureOpenAIAssistantsAgentWorkflow)
            {
                agent.Properties ??= [];

                (var openAIAssistantId, var openAIAssistantVectorStoreId, var workflowBase, var agentAIModel, var agentPrompt, var agentAIModelAPIEndpoint)
                    = await ResolveAgentProperties(agent, userIdentity);

                var workflow = (workflowBase as AzureOpenAIAssistantsAgentWorkflow)!;
                var gatewayClient = await GetGatewayServiceClient(
                    resourcePath.InstanceId!,
                    userIdentity);

                if (string.IsNullOrWhiteSpace(openAIAssistantId))
                {
                    // The agent uses the Azure OpenAI Assistants workflow
                    // but it does not have an associated assistant.
                    // Proceed to create the Azure OpenAI Assistants assistant.

                    _logger.LogInformation(
                        "Starting to create the Azure OpenAI assistant for agent {AgentName}",
                        agent.Name);


                    #region Create Azure OpenAI Assistants assistant                   

                    Dictionary<string, object> parameters = new()
                        {
                            { OpenAIAgentCapabilityParameterNames.CreateOpenAIAssistant, true },
                            { OpenAIAgentCapabilityParameterNames.OpenAIEndpoint, agentAIModelAPIEndpoint.Url },
                            { OpenAIAgentCapabilityParameterNames.OpenAIModelDeploymentName, agentAIModel.DeploymentName! },
                            { OpenAIAgentCapabilityParameterNames.OpenAIAssistantPrompt, (agentPrompt as MultipartPrompt)!.Prefix! }
                        };

                    var agentCapabilityResult = await gatewayClient!.CreateAgentCapability(
                        _instanceSettings.Id,
                        AgentCapabilityCategoryNames.OpenAIAssistants,
                        $"FoundationaLLM - {agent.Name}",
                        parameters);

                    var newOpenAIAssistantId = default(string);

                    if (agentCapabilityResult.TryGetValue(OpenAIAgentCapabilityParameterNames.OpenAIAssistantId, out var newOpenAIAssistantIdObject)
                        && newOpenAIAssistantIdObject != null)
                        newOpenAIAssistantId = ((JsonElement)newOpenAIAssistantIdObject!).Deserialize<string>();

                    var newOpenAIAssistantVectorStoreId = default(string);
                    if (agentCapabilityResult.TryGetValue(OpenAIAgentCapabilityParameterNames.OpenAIVectorStoreId, out var newOpenAIAssistantVectorStoreIdObject)
                        && newOpenAIAssistantVectorStoreIdObject != null)
                        newOpenAIAssistantVectorStoreId = ((JsonElement)newOpenAIAssistantVectorStoreIdObject!).Deserialize<string>();

                    if (string.IsNullOrWhiteSpace(newOpenAIAssistantId))
                        throw new ResourceProviderException($"Could not create an Azure OpenAI assistant for the agent {agent} which requires it.",
                            StatusCodes.Status500InternalServerError);
                    if (string.IsNullOrWhiteSpace(newOpenAIAssistantVectorStoreId))
                        throw new ResourceProviderException($"Could not create an Azure OpenAI assistant vector store id for the agent {agent} which requires it.",
                            StatusCodes.Status500InternalServerError);

                    _logger.LogInformation(
                        $"The Azure OpenAI assistant {newOpenAIAssistantId} for agent {agent.Name} was created successfully with Vector Store: {newOpenAIAssistantVectorStoreId}.",
                        newOpenAIAssistantId, agent.Name);

                    workflow.VectorStoreId = newOpenAIAssistantVectorStoreId;
                    workflow.AssistantId = newOpenAIAssistantId;

                    #endregion
                }
                else
                {
                    // Verify if the assistant has a vector store id.                   
                    if (string.IsNullOrEmpty(workflow.VectorStoreId))
                    {
                        // Add vector store to existing assistant
                        Dictionary<string, object> vectorStoreParameters = new()
                        {
                            { OpenAIAgentCapabilityParameterNames.CreateOpenAIAssistantVectorStore, true },
                            { OpenAIAgentCapabilityParameterNames.OpenAIEndpoint, agentAIModelAPIEndpoint.Url },
                            { OpenAIAgentCapabilityParameterNames.OpenAIModelDeploymentName, agentAIModel.DeploymentName! },
                            { OpenAIAgentCapabilityParameterNames.OpenAIAssistantId, openAIAssistantId }
                        };

                        // Pass the existing assistant id as the capability name
                        var agentCapabilityResult = await gatewayClient!.CreateAgentCapability(
                            _instanceSettings.Id,
                            AgentCapabilityCategoryNames.OpenAIAssistants,
                            string.Empty,
                            vectorStoreParameters);

                        var newOpenAIAssistantVectorStoreId = default(string);
                        if (agentCapabilityResult.TryGetValue(OpenAIAgentCapabilityParameterNames.OpenAIVectorStoreId, out var newOpenAIAssistantVectorStoreIdObject)
                            && newOpenAIAssistantVectorStoreIdObject != null)
                            newOpenAIAssistantVectorStoreId = ((JsonElement)newOpenAIAssistantVectorStoreIdObject!).Deserialize<string>();
                        if (string.IsNullOrWhiteSpace(newOpenAIAssistantVectorStoreId))
                            throw new ResourceProviderException($"Could not create an Azure OpenAI assistant vector store id for the agent {agent} which requires it.",
                                StatusCodes.Status500InternalServerError);

                        workflow.VectorStoreId = newOpenAIAssistantVectorStoreId;
                    }

                    // Always update the assistant prompt.
                    Dictionary<string, object> instructionsParameters = new()
                    {
                        { OpenAIAgentCapabilityParameterNames.UpdateOpenAIAssistantInstructions, true },
                        { OpenAIAgentCapabilityParameterNames.OpenAIEndpoint, agentAIModelAPIEndpoint.Url },
                        { OpenAIAgentCapabilityParameterNames.OpenAIAssistantId, openAIAssistantId },
                        { OpenAIAgentCapabilityParameterNames.OpenAIAssistantPrompt, (agentPrompt as MultipartPrompt)!.Prefix! }
                    };

                    await gatewayClient!.CreateAgentCapability(
                        _instanceSettings.Id,
                        AgentCapabilityCategoryNames.OpenAIAssistants,
                        string.Empty,
                        instructionsParameters);
                }
            }

            if (agent.Workflow is AzureAIAgentServiceAgentWorkflow)
            {
                agent.Properties ??= [];

                (var agentId, var vectorStoreId, var workflowBase, var agentAIModel, var agentPrompt, var project)
                    = await ResolveAgentServiceProperties(agent, userIdentity);

                var workflow = (workflowBase as AzureAIAgentServiceAgentWorkflow)!;
                var gatewayClient = await GetGatewayServiceClient(
                    resourcePath.InstanceId!,
                    userIdentity);

                if(string.IsNullOrWhiteSpace(workflow.ProjectConnectionString))
                    workflow.ProjectConnectionString = project.ProjectConnectionString;

                if (string.IsNullOrWhiteSpace(agentId))
                {
                    // The agent uses the Azure AI Agent Service workflow.
                    // but it does not have an associated assistant.
                    // Proceed to create the Azure AI Agent Service agent.

                    _logger.LogInformation(
                        "Starting to create the Azure AI Agent Service agent for agent {AgentName}",
                        agent.Name);


                    #region Create Azure AI Agent Service agent                   

                    Dictionary<string, object> parameters = new()
                        {
                            { AzureAIAgentServiceCapabilityParameterNames.CreateAgent, true },
                            { AzureAIAgentServiceCapabilityParameterNames.ProjectConnectionString, project.ProjectConnectionString },
                            { AzureAIAgentServiceCapabilityParameterNames.AzureAIModelDeploymentName, agentAIModel.DeploymentName!},
                            { AzureAIAgentServiceCapabilityParameterNames.AgentPrompt, (agentPrompt as MultipartPrompt)!.Prefix! }
                        };

                    var agentCapabilityResult = await gatewayClient!.CreateAgentCapability(
                        _instanceSettings.Id,
                        AgentCapabilityCategoryNames.AzureAIAgents,
                        $"FoundationaLLM - {agent.Name}",
                        parameters);

                    var newAgentId = default(string);

                    if (agentCapabilityResult.TryGetValue(AzureAIAgentServiceCapabilityParameterNames.AgentId, out var newAgentObject)
                        && newAgentObject != null)
                        newAgentId = ((JsonElement)newAgentObject!).Deserialize<string>();

                    var newVectorStoreId = default(string);
                    if (agentCapabilityResult.TryGetValue(AzureAIAgentServiceCapabilityParameterNames.VectorStoreId, out var newVectorStoreObject)
                        && newVectorStoreObject != null)
                        newVectorStoreId = ((JsonElement)newVectorStoreObject!).Deserialize<string>();

                    if (string.IsNullOrWhiteSpace(newAgentId))
                        throw new ResourceProviderException($"Could not create an Azure AI Agent Service agent for the agent {agent} which requires it.",
                            StatusCodes.Status500InternalServerError);
                    if (string.IsNullOrWhiteSpace(newVectorStoreId))
                        throw new ResourceProviderException($"Could not create an Azure AI Agent Service agent vector store for the agent {agent} which requires it.",
                            StatusCodes.Status500InternalServerError);

                    _logger.LogInformation(
                        $"The Azure AI Agent Service Agent with ID: {newAgentId} for agent {agent.Name} was created successfully with Vector Store: {newVectorStoreId}.",
                        newAgentId, agent.Name);

                    workflow.VectorStoreId = newVectorStoreId;
                    workflow.AgentId = newAgentId;

                    #endregion
                }
                else
                {
                    // Verify if the agent has a vector store id.                   
                    if (string.IsNullOrEmpty(workflow.VectorStoreId))
                    {
                        // Add vector store to existing assistant
                        Dictionary<string, object> parameters = new()
                        {
                            { AzureAIAgentServiceCapabilityParameterNames.CreateVectorStore, true },
                            { AzureAIAgentServiceCapabilityParameterNames.ProjectConnectionString, project.ProjectConnectionString },
                            { AzureAIAgentServiceCapabilityParameterNames.AgentId, agentId }
                        };

                        // Pass the existing assistant id as the capability name
                        var agentCapabilityResult = await gatewayClient!.CreateAgentCapability(
                            _instanceSettings.Id,
                            AgentCapabilityCategoryNames.AzureAIAgents,
                            string.Empty,
                            parameters);

                        var newVectorStoreId = default(string);
                        if (agentCapabilityResult.TryGetValue(AzureAIAgentServiceCapabilityParameterNames.VectorStoreId, out var newVectorStoreObject)
                            && newVectorStoreObject != null)
                            newVectorStoreId = ((JsonElement)newVectorStoreObject!).Deserialize<string>();
                        if (string.IsNullOrWhiteSpace(newVectorStoreId))
                            throw new ResourceProviderException($"Could not create an Azure AI Agent Service vector store id for the agent {agent} which requires it.",
                                StatusCodes.Status500InternalServerError);

                        workflow.VectorStoreId = newVectorStoreId;
                    }
                }
            }

            var validator = _resourceValidatorFactory.GetValidator(agentReference.ResourceType);
            if (validator is IValidator agentValidator)
            {
                var context = new ValidationContext<object>(agent);
                var validationResult = await agentValidator.ValidateAsync(context);
                if (!validationResult.IsValid)
                {
                    throw new ResourceProviderException($"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}",
                        StatusCodes.Status400BadRequest);
                }
            }

            // Ensure the agent has a valid virtual security group identifier.
            var virtualSecurityGroupGenerated = false;
            if (string.IsNullOrWhiteSpace(agent.VirtualSecurityGroupId))
            {
                if (existingAgent is not null
                    && !string.IsNullOrWhiteSpace(existingAgent.VirtualSecurityGroupId))
                {
                    // Use the existing virtual security group identifier.
                    agent.VirtualSecurityGroupId = existingAgent.VirtualSecurityGroupId;
                }
                else
                {
                    // Generate a new virtual security group identifier.
                    agent.VirtualSecurityGroupId = Guid.NewGuid().ToString().ToLower();
                    virtualSecurityGroupGenerated = true;
                }
            }

            UpdateBaseProperties(agent, userIdentity, isNew: existingAgentReference is null);
            if (existingAgentReference is null)
                await CreateResource<AgentBase>(agentReference, agent);
            else
                await SaveResource<AgentBase>(existingAgentReference, agent);

            var upsertResult = new ResourceProviderUpsertResult
            {
                ObjectId = agent!.ObjectId,
                ResourceExists = existingAgentReference is not null,
                Resource = includeResourceInResponse
                    ? agent
                    : null
            };

            if (virtualSecurityGroupGenerated)
            {
                try
                {
                    // Assign the agent's virtual security group identifier read permissions to the agent.
                    var roleAssignmentName = Guid.NewGuid().ToString();
                    var roleAssignmentDescription = $"Reader role for the {agent.Name} agent's virtual security group";
                    var roleAssignmentResult = await _authorizationServiceClient.CreateRoleAssignment(
                        _instanceSettings.Id,
                        new RoleAssignmentCreateRequest()
                        {
                            Name = roleAssignmentName,
                            Description = roleAssignmentDescription,
                            ObjectId = $"/instances/{resourcePath.InstanceId}/providers/{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleAssignments}/{roleAssignmentName}",
                            PrincipalId = agent.VirtualSecurityGroupId,
                            PrincipalType = PrincipalTypes.Group,
                            RoleDefinitionId = $"/providers/{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleDefinitions}/{RoleDefinitionNames.Reader}",
                            Scope = upsertResult.ObjectId
                        },
                        userIdentity);

                    if (!roleAssignmentResult.Success)
                    {
                        _logger.LogWarning("Failed to assign the agent's virtual security group identifier read permissions to the agent {AgentName}.",
                            agent.Name);
                    }

                    // With the introduction of the more complex prompt structure,
                    // the approach of assigning permissions to a single main prompt is no longer valid.
                    //

                    //if (!agent.InheritableAuthorizableActions.Contains(
                    //    AuthorizableActionNames.FoundationaLLM_Prompt_Prompts_Read))
                    //{
                    //    _logger.LogWarning("The agent {AgentName} does not specify {ActionName} as an inheritable authorizable action. Explicit role assignments must be set on all referenced resources for the virtual security group identifier {VirtualSecurityGroupId}.",
                    //        agent.Name,
                    //        AuthorizableActionNames.FoundationaLLM_Prompt_Prompts_Read,
                    //        agent.VirtualSecurityGroupId!);
                    //}

                    
                    //if (!string.IsNullOrWhiteSpace(agent.Workflow?.MainPromptObjectId))
                    //{
                    //    roleAssignmentName = Guid.NewGuid().ToString();
                    //    roleAssignmentResult = await _authorizationServiceClient.CreateRoleAssignment(
                    //        _instanceSettings.Id,
                    //        new RoleAssignmentCreateRequest()
                    //        {
                    //            Name = roleAssignmentName,
                    //            Description = roleAssignmentDescription,
                    //            ObjectId = $"/instances/{resourcePath.InstanceId}/providers/{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleAssignments}/{roleAssignmentName}",
                    //            PrincipalId = agent.VirtualSecurityGroupId,
                    //            PrincipalType = PrincipalTypes.Group,
                    //            RoleDefinitionId = $"/providers/{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleDefinitions}/{RoleDefinitionNames.Reader}",
                    //            Scope = agent.Workflow.MainPromptObjectId
                    //        },
                    //        userIdentity);

                    //    if (!roleAssignmentResult.Success)
                    //    {
                    //        _logger.LogWarning("Failed to assign the agent's virtual security group identifier read permissions to the prompt {PromptObjectId}.",
                    //            agent.Workflow.MainPromptObjectId);
                    //    }
                    //}
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An error occurred while assigning the agent's virtual security group identifier read permissions to the agent {AgentName}.",
                        agent.Name);
                }
            }

            return upsertResult;
        }

        private async Task<ResourceProviderActionResult> SetAgentOwnerUser(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction,
            UnifiedUserIdentity userIdentity)
        {
            var agentOwnerUpdateRequest = JsonSerializer.Deserialize<AgentOwnerUpdateRequest>(serializedAction)
                ?? throw new ResourceProviderException("The agent owner update request is invalid.",
                    StatusCodes.Status400BadRequest);
            if (string.IsNullOrWhiteSpace(agentOwnerUpdateRequest.OwnerUserId))
                throw new ResourceProviderException("The agent owner update request is invalid.",
                    StatusCodes.Status400BadRequest);

            var agent = await LoadResource<AgentBase>(resourcePath.MainResourceId!)
                ?? throw new ResourceProviderException($"The resource {resourcePath.MainResourceId!} of type {resourcePath.MainResourceTypeName} was not found.",
                    StatusCodes.Status404NotFound);

            if (agent.OwnerUserId is null
                || agent.OwnerUserId != agentOwnerUpdateRequest.OwnerUserId)
            {
                // Set or replace owner user only if different from the current one.

                var roleAssignments = await _authorizationServiceClient.GetRoleAssignments(
                resourcePath.InstanceId!,
                new RoleAssignmentQueryParameters
                {
                    Scope = agent.ObjectId,
                    SecurityPrincipalIds = [userIdentity.UserId!, .. userIdentity.GroupIds]
                },
                userIdentity);

                if (!roleAssignments.Any(ra =>
                    ra.RoleDefinitionId == RoleDefinitionIds.Owner))
                    throw new ResourceProviderException("The specified owner user does not have the Owner role assigned on the agent.",
                        StatusCodes.Status400BadRequest);

                agent.OwnerUserId = agentOwnerUpdateRequest.OwnerUserId;
                await SaveResource<AgentBase>(agent);
            }

            return new ResourceProviderActionResult(
                agent.ObjectId!,
                true);
        }

        private async Task<List<ResourceProviderGetResult<AgentAccessToken>>> LoadAgentAccessTokens(
            ResourcePath resourcePath)
        {
            var agentClientSecretKey = new AgentClientSecretKey
            {
                InstanceId = resourcePath.InstanceId!,
                ContextId = string.Empty,
                Id = string.Empty,
                ClientSecret = string.Empty
            };
            agentClientSecretKey.SetContextId(resourcePath.MainResourceId!);

            var secretKeys = await _authorizationServiceClient.GetSecretKeys(
                agentClientSecretKey.InstanceId,
                agentClientSecretKey.ContextId);

            return [.. secretKeys.Select(k => new ResourceProviderGetResult<AgentAccessToken>()
            {
                Actions = [],
                Roles = [],
                Resource = new AgentAccessToken()
                {
                    Id = new Guid(k.Id),
                    Name = k.Id,
                    Description = k.Description,
                    Active = k.Active,
                    ExpirationDate = k.ExpirationDate
                }
            })];
        }

        private async Task<ResourceProviderUpsertResult> UpdateAgentAccessToken(
            ResourcePath resourcePath,
            string serializedAgentAccessToken,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity)
        {
            var agentAccessToken = JsonSerializer.Deserialize<AgentAccessToken>(serializedAgentAccessToken)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            if (!authorizationResult.Authorized
                || !authorizationResult.HasRequiredRole)
            {
                // Agent access keys can be created or updated only by users with the required role and authorization on the agent.
                _logger.LogWarning("Access to the resource path {ResourcePath} was not authorized for user {UserName} : userId {UserId}.",
                    resourcePath.RawResourcePath, userIdentity!.Username, userIdentity!.UserId);
                throw new ResourceProviderException("Access is not authorized.", StatusCodes.Status403Forbidden);
            }

            var agentClientSecretKey = new AgentClientSecretKey
            {
                InstanceId = resourcePath.InstanceId!,
                ContextId = string.Empty,
                Id = resourcePath.ResourceId!,
                ClientSecret = string.Empty
            };
            agentClientSecretKey.SetContextId(resourcePath.MainResourceId!);

            var secretKeyValue = await _authorizationServiceClient.UpsertSecretKey(_instanceSettings.Id, new SecretKey()
            {
                Id = agentClientSecretKey.Id,
                InstanceId = agentClientSecretKey.InstanceId,
                ContextId = agentClientSecretKey.ContextId,
                Description = agentAccessToken.Description!,
                Active = agentAccessToken.Active,
                ExpirationDate = agentAccessToken.ExpirationDate!.Value
            });

            agentAccessToken.ObjectId = resourcePath.RawResourcePath;

            return new ResourceProviderUpsertResult
            {
                ObjectId = agentAccessToken!.ObjectId,
                ResourceExists = string.IsNullOrWhiteSpace(secretKeyValue),
                Resource = secretKeyValue
            };
        }

        private async Task DeleteAgentAccessToken(ResourcePath resourcePath)
        {
            var agentClientSecretKey = new AgentClientSecretKey
            {
                InstanceId = resourcePath.InstanceId!,
                ContextId = string.Empty,
                Id = resourcePath.ResourceId!,
                ClientSecret = string.Empty
            };
            agentClientSecretKey.SetContextId(resourcePath.MainResourceId!);

            await _authorizationServiceClient.DeleteSecretKey(
                agentClientSecretKey.InstanceId,
                agentClientSecretKey.ContextId,
                agentClientSecretKey.Id);

            await SendResourceProviderEvent(EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand);
        }

        private async Task<AgentAccessTokenValidationResult> ValidateAgentAccessToken(
            ResourcePath resourcePath,
            AgentAccessTokenValidationRequest agentAccessTokenValidationRequest,
            UnifiedUserIdentity userIdentity)
        {
            var fallbackResult = new AgentAccessTokenValidationResult()
            {
                Valid = false
            };

            if (!ClientSecretKey.TryParse(agentAccessTokenValidationRequest.AccessToken, out var clientSecretKey)
                || clientSecretKey == null)
                return fallbackResult;

            var agentClientSecretKey = AgentClientSecretKey.FromClientSecretKey(clientSecretKey);
            var accessTokens = await LoadAgentAccessTokens(resourcePath);
            var matchingAccessToken = accessTokens.FirstOrDefault(x => x.Resource.Id == new Guid(agentClientSecretKey.Id))?.Resource;
            if (matchingAccessToken != null)
            {
                if (matchingAccessToken.ExpirationDate.HasValue && matchingAccessToken.ExpirationDate.Value < DateTime.UtcNow)
                {
                    _logger.LogWarning("An expired agent access token was used for the agent {AgentName} in instance {InstanceId}.",
                        agentClientSecretKey.AgentName,
                        resourcePath.InstanceId);
                    return fallbackResult;
                }
                if (!matchingAccessToken.Active)
                {
                    _logger.LogWarning("An inactive agent access token was used for the agent {AgentName} in instance {InstanceId}.",
                        agentClientSecretKey.AgentName,
                        resourcePath.InstanceId);
                    return fallbackResult;
                }
            }

            if (!StringComparer.OrdinalIgnoreCase.Equals(agentClientSecretKey.AgentName, resourcePath.MainResourceId))
                return fallbackResult;

            var result = await _authorizationServiceClient.ValidateSecretKey(
                resourcePath.InstanceId!,
                agentClientSecretKey.ContextId,
                agentAccessTokenValidationRequest.AccessToken);

            if (result.Valid)
            {
                // Set virtual identity.
                var agent = await GetResourceAsync<AgentBase>($"/instances/{resourcePath.InstanceId}/providers/{_name}/{AgentResourceTypeNames.Agents}/{agentClientSecretKey.AgentName}", userIdentity);
                var upn =
                    $"aat_{agentClientSecretKey.AgentName}_{agentClientSecretKey.Id}@foundationallm.internal_";

                // Warn if the agent's virtual security group identifier is not set.
                if (string.IsNullOrEmpty(agent.VirtualSecurityGroupId))
                {
                    _logger.LogWarning("An agent access token was used for the agent {AgentName} in instance {InstanceId} but the agent's virtual security group identifier is not set.",
                        agentClientSecretKey.AgentName,
                        resourcePath.InstanceId);
                }

                return new AgentAccessTokenValidationResult()
                {
                    Valid = result.Valid,
                    VirtualIdentity = new UnifiedUserIdentity()
                    {
                        UserId = agentClientSecretKey.Id,
                        Name = agentClientSecretKey.Id,
                        Username = agentClientSecretKey.Id,
                        UPN = upn,
                        GroupIds = string.IsNullOrWhiteSpace(agent.VirtualSecurityGroupId)
                            ? []
                            : [
                                agent.VirtualSecurityGroupId,
                                Common.Constants.Authentication.GroupIDs.AllAgentsVirtualSecurityGroup
                            ],
                    }
                };
            }

            return fallbackResult;
        }

        private async Task<List<ResourceProviderGetResult<AgentFile>>> LoadAgentFiles(
            ResourcePath resourcePath, UnifiedUserIdentity userIdentity, ResourceProviderGetOptions? options = null)
        {
            var agentFiles = new List<AgentFileReference>();

            if (resourcePath.ResourceId != null)
                agentFiles = [await _cosmosDBService.GetAgentFile(_instanceSettings.Id, resourcePath.MainResourceId!, resourcePath.ResourceId!)];
            else
                agentFiles = await _cosmosDBService.GetAgentFiles(_instanceSettings.Id, resourcePath.MainResourceId!);

            var results = new List<AgentFile>();
            foreach (var agentFile in agentFiles)
                results.Add(await LoadAgentFile(agentFile, options != null && options!.LoadContent));

            return results.Select(r => new ResourceProviderGetResult<AgentFile>
            {
                Resource = r,
                Roles = [],
                Actions = []
            }).ToList();
        }

        private async Task<AgentFile> LoadAgentFile(AgentFileReference agentFileReference, bool loadContent = false)
        {
            var agentFile = new AgentFile
            {
                ObjectId = agentFileReference.ObjectId,
                Name = agentFileReference.Name,
                DisplayName = agentFileReference.OriginalFilename,
                Type = agentFileReference.Type,
                ContentType = agentFileReference.ContentType,
                AgentObjectId = ResourcePath.GetObjectId(agentFileReference.InstanceId, _name, AgentResourceTypeNames.Agents, agentFileReference.AgentName)
            };

            if (loadContent)
            {
                var fileContent = await _storageService.ReadFileAsync(
                    _storageContainerName,
                    agentFileReference.Filename,
                    default);
                agentFile.Content = fileContent.ToArray();
            }

            return agentFile;
        }

        private async Task<ResourceProviderUpsertResult> UpdateAgentFile(ResourcePath resourcePath, ResourceProviderFormFile formFile, UnifiedUserIdentity userIdentity)
        {
            if (formFile.BinaryContent.Length == 0)
                throw new ResourceProviderException("The attached file is not valid.",
                    StatusCodes.Status400BadRequest);

            AgentFileReference agentPrivateFile = null!;
            var agent = await LoadResource<AgentBase>(resourcePath.MainResourceId!);
            if (agent!.HasGenericWorkflow()
                && agent.Workflow!.ClassName == AgentWorkflowNames.FoundationaLLMFunctionCallingWorkflow)
            {
                // Use the Context API to store the file.

                var contextServiceClient = GetContextServiceClient(userIdentity);
                var contextServiceResult = await contextServiceClient.CreateFileForAgent(
                    resourcePath.InstanceId!,
                    agent.Name,
                    formFile.FileName,
                    formFile.ContentType!,
                    new MemoryStream(formFile.BinaryContent.ToArray()));

                if (!contextServiceResult.IsSuccess)
                    throw new ResourceProviderException(
                        $"Failed to add the file {formFile.FileName} to the FoundationaLLM file store.",
                        StatusCodes.Status500InternalServerError);
                var objectId = ResourcePath.GetObjectId(_instanceSettings.Id, _name, AgentResourceTypeNames.Agents, resourcePath.MainResourceId!, AgentResourceTypeNames.AgentFiles, contextServiceResult.Result!.Id);

                agentPrivateFile = new AgentFileReference
                {
                    Id = contextServiceResult.Result!.Id,
                    Name = contextServiceResult.Result.Id,
                    ObjectId =  objectId,
                    OriginalFilename = formFile.FileName,
                    ContentType = formFile.ContentType!,
                    Type = AgentTypes.AgentFile,
                    Filename = contextServiceResult.Result.FilePath,
                    Size = formFile.BinaryContent.Length,
                    UPN = userIdentity.UPN ?? string.Empty,
                    InstanceId = resourcePath.InstanceId!,
                    AgentName = resourcePath.MainResourceId!,
                    Deleted = false,
                };
            }
            else
            {
                var uniqueId = $"af-{Guid.NewGuid()}";
                var extension = GetFileExtension(formFile.FileName);
                var fullName = $"{uniqueId}{extension}";
                var filePath = $"/{_name}/{resourcePath.InstanceId}/{resourcePath.MainResourceId}/private-file-store/{fullName}";
                var objectId = ResourcePath.GetObjectId(_instanceSettings.Id, _name, AgentResourceTypeNames.Agents, resourcePath.MainResourceId!, AgentResourceTypeNames.AgentFiles, uniqueId);

                agentPrivateFile = new AgentFileReference
                {
                    Id = uniqueId,
                    Name = uniqueId,
                    ObjectId = objectId,
                    OriginalFilename = formFile.FileName,
                    ContentType = formFile.ContentType!,
                    Type = AgentTypes.AgentFile,
                    Filename = filePath,
                    Size = formFile.BinaryContent.Length,
                    UPN = userIdentity.UPN ?? string.Empty,
                    InstanceId = resourcePath.InstanceId!,
                    AgentName = resourcePath.MainResourceId!,
                    Deleted = false
                };

                await _storageService.WriteFileAsync(
                    _storageContainerName,
                    agentPrivateFile.Filename,
                    new MemoryStream(formFile.BinaryContent.ToArray()),
                    agentPrivateFile.ContentType ?? default,
                    default);
            }

            await _cosmosDBService.CreateAgentFile(agentPrivateFile);

            return new ResourceProviderUpsertResult<AgentFile>
            {
                ObjectId = agentPrivateFile!.ObjectId!,
                ResourceExists = false,
                Resource = new AgentFile
                {
                    ObjectId = agentPrivateFile.ObjectId,
                    Name = agentPrivateFile.Name,
                    DisplayName = formFile.FileName,
                    Type = agentPrivateFile.Type,
                    ContentType = agentPrivateFile.ContentType,
                    AgentObjectId = ResourcePath.GetObjectId(agentPrivateFile.InstanceId, _name, AgentResourceTypeNames.Agents, agentPrivateFile.AgentName)
                }
            };
        }

        private async Task DeleteAgentFile(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            var attachment = await _cosmosDBService.GetAgentFile(_instanceSettings.Id, resourcePath.MainResourceId!, resourcePath.ResourceId!)
                        ?? throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.MainResourceTypeName} was not found.");

            var filePath = $"/{_name}/{_instanceSettings.Id}/{resourcePath.MainResourceId!}/Associations.json";
            await EnsureAgentFileToolAssociationsFileExists(filePath);

            var fileContent = await _storageService.ReadFileAsync(_storageContainerName, filePath, default);
            var existingAssociations = JsonSerializer.Deserialize<List<AgentFileToolAssociation>>(Encoding.UTF8.GetString(fileContent.ToArray()))!;

            var fileObjectId = resourcePath.RawResourcePath;
            var fileToolAssociation = existingAssociations.Where(x => x.FileObjectId == fileObjectId).SingleOrDefault();

            if (fileToolAssociation != null)
            {
                // Create a temporary resource path for the file tool association to pass to the RemoveFileToolAssociation method.
                // The resource identifier is not relevant, so we just use a random GUID to ensire the format is correct.
                if (!ResourcePath.TryParse(
                    $"/instances/{_instanceSettings.Id}/providers/{_name}/{AgentResourceTypeNames.Agents}/{resourcePath.MainResourceId}/{AgentResourceTypeNames.AgentFileToolAssociations}/{Guid.NewGuid()}",
                    [_name],
                    AgentResourceProviderMetadata.AllowedResourceTypes,
                    false,
                    out var toolAssociationResourcePath))
                        throw new ResourceProviderException("The object definition is invalid.",
                            StatusCodes.Status400BadRequest);

                var associatedTools = fileToolAssociation.AssociatedResourceObjectIds!.Keys.ToList();
                foreach (var toolObjectId in associatedTools)
                    await RemoveFileToolAssociation(fileObjectId, toolObjectId, fileToolAssociation, toolAssociationResourcePath!, userIdentity);

                existingAssociations.Remove(fileToolAssociation);

                await _storageService.WriteFileAsync(
                    _storageContainerName,
                    filePath,
                    JsonSerializer.Serialize(existingAssociations),
                    default,
                    default);
            }

            await _cosmosDBService.DeleteAgentFile(attachment);
        }

        private async Task<List<ResourceProviderGetResult<AgentFileToolAssociation>>> LoadAgentFileToolAssociations(
            ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            if (!ResourcePath.TryParse(
                $"/instances/{_instanceSettings.Id}/providers/{_name}/{AgentResourceTypeNames.Agents}/{resourcePath.MainResourceId}/{AgentResourceTypeNames.AgentFiles}",
                [_name],
                AgentResourceProviderMetadata.AllowedResourceTypes,
                false,
                out var agentFilesResourcePath))
                throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            var agentFileResources = await LoadAgentFiles(agentFilesResourcePath!, userIdentity, new ResourceProviderGetOptions() { LoadContent = false });
            var agentFiles = agentFileResources.Select(x => x.Resource).ToList();

            var filePath = $"/{_name}/{_instanceSettings.Id}/{resourcePath.MainResourceId!}/Associations.json";
            await EnsureAgentFileToolAssociationsFileExists(filePath);

            var fileContent = await _storageService.ReadFileAsync(_storageContainerName, filePath, default);
            var existingAssociations = JsonSerializer.Deserialize<List<AgentFileToolAssociation>>(Encoding.UTF8.GetString(fileContent.ToArray()))!;

            foreach (var agentFile in agentFiles)
            {
                if (!existingAssociations.Any(x => x.FileObjectId == agentFile.ObjectId))
                {
                    existingAssociations.Add(new AgentFileToolAssociation()
                    {
                        Name = Guid.NewGuid().ToString(),
                        FileObjectId = agentFile.ObjectId!,
                        CreatedBy = userIdentity.UPN,
                        CreatedOn = DateTimeOffset.UtcNow,
                        AssociatedResourceObjectIds = [],
                    });
                }
            }

            // Ensure file associations are always consistent.
            // Remove any file associations that do not have a corresponding agent file.
            if (existingAssociations.Any(x => !agentFiles.Any(f => f.ObjectId == x.FileObjectId)))
            {
                existingAssociations.RemoveAll(x => !agentFiles.Any(f => f.ObjectId == x.FileObjectId));
                await _storageService.WriteFileAsync(
                    _storageContainerName,
                    filePath,
                    JsonSerializer.Serialize(existingAssociations),
                    default,
                    default);
            }

            return [.. existingAssociations.Select(fileToolAssociation =>
            new ResourceProviderGetResult<AgentFileToolAssociation>()
            {
                Resource = fileToolAssociation,
                Actions = [],
                Roles = []
            })];
        }

        private async Task<ResourceProviderUpsertResult> UpdateFileToolAssociations(
            ResourcePath resourcePath,
            string serializedResource,
            UnifiedUserIdentity userIdentity)
        {
            var errors = new List<AgentFileToolAssociationError>();

            if (!ResourcePath.TryParse(
                $"/instances/{_instanceSettings.Id}/providers/{_name}/{AgentResourceTypeNames.Agents}/{resourcePath.MainResourceId}/{AgentResourceTypeNames.AgentFiles}",
                [_name],
                AgentResourceProviderMetadata.AllowedResourceTypes,
                false,
                out var agentFilesResourcePath))
                throw new ResourceProviderException("The agent files resource path cannot be derived from the request.",
                    StatusCodes.Status400BadRequest);

            var agentFileResources = await LoadAgentFiles(agentFilesResourcePath!, userIdentity, new ResourceProviderGetOptions() { LoadContent = false });
            var agentFiles = agentFileResources.Select(x => x.Resource).ToList();

            var agentFileToolAssociationRequest = JsonSerializer.Deserialize<AgentFileToolAssociationRequest>(serializedResource)
                ?? throw new ResourceProviderException("The update agent file tool association request is invalid.",
                    StatusCodes.Status400BadRequest);
            if (resourcePath.ResourceId != WellKnownResourceIdentifiers.AllResources
                && (
                    agentFileToolAssociationRequest.AgentFileToolAssociations.Count != 1
                    || !agentFileToolAssociationRequest.AgentFileToolAssociations.ContainsKey($"{agentFilesResourcePath!.RawResourcePath}/{resourcePath.ResourceId}")
                ))
                throw new ResourceProviderException("The update agent file tool association request is invalid.",
                    StatusCodes.Status400BadRequest);

            var filePath = $"/{_name}/{_instanceSettings.Id}/{resourcePath.MainResourceId!}/Associations.json";
            await EnsureAgentFileToolAssociationsFileExists(filePath);

            var fileContent = await _storageService.ReadFileAsync(_storageContainerName, filePath, default);
            var existingAssociations = JsonSerializer.Deserialize<List<AgentFileToolAssociation>>(Encoding.UTF8.GetString(fileContent.ToArray()))!;

            foreach (var fileObjectId in agentFileToolAssociationRequest.AgentFileToolAssociations.Keys)
            {
                if (!agentFileToolAssociationRequest.AgentFileToolAssociations.TryGetValue(fileObjectId, out var toolAssociations))
                    continue;

                if (!agentFiles.Any(x => x.ObjectId == fileObjectId))
                {
                    errors.Add(new AgentFileToolAssociationError()
                    {
                        FileObjectId = fileObjectId,
                        ToolObjectId = string.Empty,
                        ErrorMessage = $"The file was not found in the agent private storage."
                    });

                    continue;
                }

                foreach (var toolObjectId in toolAssociations.Keys)
                {
                    if (!toolAssociations.TryGetValue(toolObjectId, out bool enabled))
                        continue;

                    var fileToolAssociation = existingAssociations.Where(x => x.FileObjectId == fileObjectId).SingleOrDefault();
                    if (fileToolAssociation == null)
                    {
                        fileToolAssociation = new AgentFileToolAssociation()
                        {
                            Name = Guid.NewGuid().ToString(),
                            FileObjectId = fileObjectId,
                            CreatedBy = userIdentity.UPN,
                            CreatedOn = DateTimeOffset.UtcNow
                        };
                        existingAssociations.Add(fileToolAssociation);
                    }
                    fileToolAssociation.AssociatedResourceObjectIds ??= [];

                    var exists = fileToolAssociation != null && fileToolAssociation.AssociatedResourceObjectIds.ContainsKey(toolObjectId);

                    if (enabled && !exists)
                    {
                        try
                        {
                            await AddFileToolAssociation(fileObjectId, toolObjectId, fileToolAssociation!, resourcePath, userIdentity);
                            fileToolAssociation!.UpdatedBy = userIdentity.UPN;
                            fileToolAssociation!.UpdatedOn = DateTimeOffset.UtcNow;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "Error when adding the association between the {File} file and the {Tool} tool.",
                                fileObjectId, toolObjectId);

                            errors.Add(new AgentFileToolAssociationError()
                            {
                                FileObjectId = fileObjectId,
                                ToolObjectId = toolObjectId,
                                ErrorMessage = $"Error when adding the association between the {fileObjectId} file and the {toolObjectId} tool."
                            });
                        }
                    }

                    if (!enabled && exists)
                    {
                        try
                        {
                            await RemoveFileToolAssociation(fileObjectId, toolObjectId, fileToolAssociation!, resourcePath, userIdentity);
                            fileToolAssociation!.UpdatedBy = userIdentity.UPN;
                            fileToolAssociation!.UpdatedOn = DateTimeOffset.UtcNow;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "Error when removing the association between the {File} file and the {Tool} tool.",
                                fileObjectId, toolObjectId);

                            errors.Add(new AgentFileToolAssociationError()
                            {
                                FileObjectId = fileObjectId,
                                ToolObjectId = toolObjectId,
                                ErrorMessage = $"Error when removing the association between the {fileObjectId} file and the {toolObjectId} tool."
                            });
                        }
                    }
                }
            }

            if (resourcePath.ResourceId == WellKnownResourceIdentifiers.AllResources)
            {
                // The request refers to all the file associations of the agent, so we need to
                // ensure that file associations that are not present in the request are removed.
                var associationNamesToRemove = new List<string>();
                foreach (var existingAssociation in existingAssociations.Where(ea =>
                    !agentFileToolAssociationRequest.AgentFileToolAssociations.ContainsKey(ea.FileObjectId)))
                {
                    foreach (var toolObjectId in existingAssociation.AssociatedResourceObjectIds!.Keys)
                    {
                        try
                        {
                            await RemoveFileToolAssociation(
                                existingAssociation.FileObjectId,
                                toolObjectId,
                                existingAssociation,
                                resourcePath,
                                userIdentity);
                            // Only remove associations that were successfully processed for deletion.
                            // If an error occurs, the association will not be removed and will be retried next time.
                            associationNamesToRemove.Add(existingAssociation.Name);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "Error when removing the association between the {File} file and the {Tool} tool.",
                                existingAssociation.FileObjectId, toolObjectId);

                            errors.Add(new AgentFileToolAssociationError()
                            {
                                FileObjectId = existingAssociation.FileObjectId,
                                ToolObjectId = toolObjectId,
                                ErrorMessage = $"Error when removing the association between the {existingAssociation.FileObjectId} file and the {toolObjectId} tool."
                            });
                        }
                    }
                }
                existingAssociations.RemoveAll(x => associationNamesToRemove.Contains(x.Name));
            }

            await _storageService.WriteFileAsync(
                _storageContainerName,
                filePath,
                JsonSerializer.Serialize(existingAssociations),
                default,
                default);

            return new ResourceProviderUpsertResult()
            {
                ObjectId = resourcePath.RawResourcePath,
                ResourceExists = true,
                Resource = new AgentFileToolAssociationResult()
                {
                    Success = errors.Count == 0,
                    Errors = errors,
                    AgentFileToolAssociations = existingAssociations,
                }
            };
        }

        private async Task EnsureAgentFileToolAssociationsFileExists(string filePath)
        {
            var fileExists = await _storageService.FileExistsAsync(_storageContainerName, filePath, default);

            if (!fileExists)
                await _storageService.WriteFileAsync(_storageContainerName, filePath,
                    JsonSerializer.Serialize(new List<AgentFileToolAssociation>()), default, default);
        }

        private async Task AddFileToolAssociation(
            string fileObjectId,
            string toolObjectId,
            AgentFileToolAssociation fileToolAssociation,
            ResourcePath resourcePath,
            UnifiedUserIdentity userIdentity)
        {
            // check if the agent file reference contains the requested tool association
            if (fileToolAssociation.AssociatedResourceObjectIds?.Any(x => x.Value.HasObjectRole(ResourceObjectIdPropertyValues.ToolAssociation)
                && x.Value.ObjectId == toolObjectId) ?? false)
                throw new ResourceProviderException($"The agent file {fileObjectId} is already associated with the tool {toolObjectId}.",
                    StatusCodes.Status400BadRequest);

            var agentObjectId = ResourcePath.GetObjectId(_instanceSettings.Id, _name, AgentResourceTypeNames.Agents, resourcePath.MainResourceId!);
            var agent = await GetResourceAsync<AgentBase>(agentObjectId, userIdentity);
            var toolResource = await GetResourceAsync<Tool>(
                toolObjectId,
                userIdentity,
                parentResourceInstance: agent);

            switch (toolResource.Name)
            {
                case AgentToolNames.OpenAIAssistantsFileSearchTool:
                    var gatewayClient = await GetGatewayServiceClient(
                        resourcePath.InstanceId!,
                        userIdentity);

                    (var openAIAssistantId, var openAIAssistantVectorStoreId, var workflow, var agentAIModel, var agentPrompt, var agentAIModelAPIEndpoint)
                        = await ResolveAgentProperties(agent, userIdentity);

                    // check reference for the Azure OpenAI File ID
                    if (string.IsNullOrWhiteSpace(fileToolAssociation.OpenAIFileId))
                    {
                        Dictionary<string, object> parameters = new()
                        {
                            { OpenAIAgentCapabilityParameterNames.OpenAIEndpoint, agentAIModelAPIEndpoint.Url },
                            { OpenAIAgentCapabilityParameterNames.CreateOpenAIFile, true },
                            { OpenAIAgentCapabilityParameterNames.AgentFileObjectId, fileObjectId! }
                        };

                        var agentCapabilityResult = await gatewayClient!.CreateAgentCapability(
                            _instanceSettings.Id,
                            AgentCapabilityCategoryNames.OpenAIAssistants,
                            string.Empty,
                            parameters);

                        agentCapabilityResult.TryGetValue(OpenAIAgentCapabilityParameterNames.OpenAIFileId, out var openAIFileIdObject);
                        fileToolAssociation.OpenAIFileId = ((JsonElement)openAIFileIdObject!).Deserialize<string>();
                    }

                    // Add the tool association to the agent file reference
                    _ = await AddFileToAssistantsVectorStore(
                        _instanceSettings.Id,
                        agentAIModelAPIEndpoint.Url,
                        agentAIModel.DeploymentName!,
                        openAIAssistantVectorStoreId,
                        fileToolAssociation.OpenAIFileId!,
                        userIdentity);

                    // Add the tool association to the agent file reference
                    fileToolAssociation.AssociatedResourceObjectIds!.Add(toolObjectId, new ResourceObjectIdProperties
                    {
                        ObjectId = toolObjectId,
                        Properties = new Dictionary<string, object>
                        {
                            { ResourceObjectIdPropertyNames.ObjectRole, ResourceObjectIdPropertyValues.ToolAssociation }
                        }
                    });

                    break;

                case AgentToolNames.OpenAIAssistantsCodeInterpreterTool:
                    var gatewayClientCI = await GetGatewayServiceClient(
                        resourcePath.InstanceId!,
                        userIdentity);

                    (var openAIAssistantIdCI, var openAIAssistantVectorStoreIdCI, var workflowCI, var agentAIModelCI, var agentPromptCI, var agentAIModelAPIEndpointCI)
                        = await ResolveAgentProperties(agent, userIdentity);

                    // check reference for the Azure OpenAI File ID
                    if (string.IsNullOrWhiteSpace(fileToolAssociation.OpenAIFileId))
                    {
                        Dictionary<string, object> parameters = new()
                        {
                            { OpenAIAgentCapabilityParameterNames.OpenAIEndpoint, agentAIModelAPIEndpointCI.Url },
                            { OpenAIAgentCapabilityParameterNames.CreateOpenAIFile, true },
                            { OpenAIAgentCapabilityParameterNames.AgentFileObjectId, fileObjectId! }
                        };

                        var agentCapabilityResult = await gatewayClientCI!.CreateAgentCapability(
                            _instanceSettings.Id,
                            AgentCapabilityCategoryNames.OpenAIAssistants,
                            string.Empty,
                            parameters);

                        agentCapabilityResult.TryGetValue(OpenAIAgentCapabilityParameterNames.OpenAIFileId, out var openAIFileIdObject);
                        fileToolAssociation.OpenAIFileId = ((JsonElement)openAIFileIdObject!).Deserialize<string>();
                    }

                    // Add the tool association to the agent file reference
                    _ = await AddFileToAssistantsCodeInterpreter(
                        _instanceSettings.Id,
                        agentAIModelAPIEndpointCI.Url,
                        agentAIModelCI.DeploymentName!,
                        openAIAssistantIdCI!,
                        fileToolAssociation.OpenAIFileId!,
                        userIdentity);

                    // Add the tool association to the agent file reference
                    fileToolAssociation.AssociatedResourceObjectIds!.Add(toolObjectId, new ResourceObjectIdProperties
                    {
                        ObjectId = toolObjectId,
                        Properties = new Dictionary<string, object>
                        {
                            { ResourceObjectIdPropertyNames.ObjectRole, ResourceObjectIdPropertyValues.ToolAssociation }
                        }
                    });

                    break;

                case AgentToolNames.FoundationaLLMKnowledgeSearchTool:

                    await RunDataPipeline(
                        _instanceSettings.Id,
                        fileObjectId,
                        agent,
                        ContentItemActions.AddOrUpdate,
                        userIdentity);

                    // Add the tool association to the agent file reference
                    fileToolAssociation.AssociatedResourceObjectIds!.Add(toolObjectId, new ResourceObjectIdProperties
                    {
                        ObjectId = toolObjectId,
                        Properties = new Dictionary<string, object>
                            {
                                { ResourceObjectIdPropertyNames.ObjectRole, ResourceObjectIdPropertyValues.ToolAssociation }
                            }
                    });

                    break;

                default:
                    throw new ResourceProviderException(
                        $"The tool {toolResource.Name} is not supported for file association by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest);
            };
        }

        private async Task RemoveFileToolAssociation(
            string fileObjectId,
            string toolObjectId,
            AgentFileToolAssociation fileToolAssociation,
            ResourcePath resourcePath,
            UnifiedUserIdentity userIdentity)
        {
            // Ensure the agent file reference contains the requested tool association
            if (!fileToolAssociation.AssociatedResourceObjectIds?.Any(x => x.Value.HasObjectRole(ResourceObjectIdPropertyValues.ToolAssociation)
                && x.Value.ObjectId == toolObjectId) ?? false)
                throw new ResourceProviderException($"The agent file {fileObjectId} is not associated with the tool {toolObjectId}.",
                    StatusCodes.Status400BadRequest);

            var agentObjectId = ResourcePath.GetObjectId(_instanceSettings.Id, _name, AgentResourceTypeNames.Agents, resourcePath.MainResourceId!);
            var agent = await GetResourceAsync<AgentBase>(agentObjectId, userIdentity);
            var toolResource = await GetResourceAsync<Tool>(
                toolObjectId,
                userIdentity,
                parentResourceInstance: agent);

            switch (toolResource.Name)
            {
                case AgentToolNames.OpenAIAssistantsFileSearchTool:
                    var gatewayClient = await GetGatewayServiceClient(
                        resourcePath.InstanceId!,
                        userIdentity);

                    (var openAIAssistantId, var openAIAssistantVectorStoreId, var workflow, var agentAIModel, var agentPrompt, var agentAIModelAPIEndpoint)
                        = await ResolveAgentProperties(agent, userIdentity);

                    _ = await RemoveFileFromAssistantsVectorStore(
                            _instanceSettings.Id,
                            agentAIModelAPIEndpoint.Url,
                            agentAIModel.DeploymentName!,
                            openAIAssistantVectorStoreId,
                            fileToolAssociation.OpenAIFileId!,
                            userIdentity);

                    // Remove the tool association from the agent file reference
                    fileToolAssociation.AssociatedResourceObjectIds!.Remove(toolObjectId);

                    break;

                case AgentToolNames.OpenAIAssistantsCodeInterpreterTool:
                    var gatewayClientCI = await GetGatewayServiceClient(
                        resourcePath.InstanceId!,
                        userIdentity);

                    (var openAIAssistantIdCI, var openAIAssistantVectorStoreIdCI, var workflowCI, var agentAIModelCI, var agentPromptCI, var agentAIModelAPIEndpointCI)
                        = await ResolveAgentProperties(agent, userIdentity);

                    _ = await RemoveFileFromAssistantsCodeInterpreter(
                            _instanceSettings.Id,
                            agentAIModelAPIEndpointCI.Url,
                            agentAIModelCI.DeploymentName!,
                            openAIAssistantIdCI,
                            fileToolAssociation.OpenAIFileId!,
                            userIdentity);

                    // Remove the tool association from the agent file reference
                    fileToolAssociation.AssociatedResourceObjectIds!.Remove(toolObjectId);

                    break;

                case AgentToolNames.FoundationaLLMKnowledgeSearchTool:
                    await RunDataPipeline(
                        _instanceSettings.Id,
                        fileObjectId,
                        agent,
                        ContentItemActions.Remove,
                        userIdentity);

                    // Remove the tool association from the agent file reference
                    fileToolAssociation.AssociatedResourceObjectIds!.Remove(toolObjectId);

                    break;

                default:
                    throw new ResourceProviderException($"The tool {toolResource.Name} is not supported by the {_name} resource provider.",
                                                                        StatusCodes.Status400BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the GatewayServiceClient.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="userIdentity">Identity of the user.</param>
        /// <returns></returns>
        private async Task<GatewayServiceClient> GetGatewayServiceClient(
            string instanceId,
            UnifiedUserIdentity userIdentity)
        {
            var gatewayClient = new GatewayServiceClient(
                       await _serviceProvider.GetRequiredService<IHttpClientFactoryService>()
                           .CreateClient(instanceId, HttpClientNames.GatewayAPI, userIdentity),
                       _serviceProvider.GetRequiredService<ILogger<GatewayServiceClient>>());
            return gatewayClient;
        }

        /// <summary>
        /// Resolves the agent properties.
        /// </summary>
        /// <param name="agent">Agent Resource</param>
        /// <param name="userIdentity">Identity of the caller</param>
        /// <returns>openAIAssistantId, openAIAssistantVectorStoreId, AIModel resource, Prompt resource, APIEndpointConfiguration resource.</returns>
        private async Task<(string, string, AgentWorkflowBase, AIModelBase, PromptBase, APIEndpointConfiguration)> ResolveAgentProperties(AgentBase agent, UnifiedUserIdentity userIdentity)
        {
            agent.Properties ??= [];

            var workflow = (agent.Workflow as AzureOpenAIAssistantsAgentWorkflow)!;
            var openAIAssistantId = workflow.AssistantId;
            var openAiAssistantVectorStoreId = workflow.VectorStoreId;


            var agentAIModel = await GetResourceProviderServiceByName(ResourceProviderNames.FoundationaLLM_AIModel)
                            .GetResourceAsync<AIModelBase>(workflow.MainAIModelObjectId!, userIdentity);
            var agentPrompt = await GetResourceProviderServiceByName(ResourceProviderNames.FoundationaLLM_Prompt)
                            .GetResourceAsync<PromptBase>(workflow.MainPromptObjectId!, userIdentity);

            APIEndpointConfiguration agentAIModelAPIEndpoint = await GetResourceProviderServiceByName(ResourceProviderNames.FoundationaLLM_Configuration)
                    .GetResourceAsync<APIEndpointConfiguration>(agentAIModel.EndpointObjectId!, userIdentity);

            return (openAIAssistantId!, openAiAssistantVectorStoreId!, workflow!, agentAIModel, agentPrompt, agentAIModelAPIEndpoint);
        }

        private async Task<(string, string, AgentWorkflowBase, AIModelBase, PromptBase, AzureAIProject)> ResolveAgentServiceProperties(AgentBase agent, UnifiedUserIdentity userIdentity)
        {
            agent.Properties ??= [];

            var workflow = (agent.Workflow as AzureAIAgentServiceAgentWorkflow)!;
            var agentId = workflow.AgentId;
            var vectorStoreId = workflow.VectorStoreId;

            var agentAIModel = await GetResourceProviderServiceByName(ResourceProviderNames.FoundationaLLM_AIModel)
                            .GetResourceAsync<AIModelBase>(workflow.MainAIModelObjectId!, userIdentity);
            var agentPrompt = await GetResourceProviderServiceByName(ResourceProviderNames.FoundationaLLM_Prompt)
                            .GetResourceAsync<PromptBase>(workflow.MainPromptObjectId!, userIdentity);
            var project = await GetResourceProviderServiceByName(ResourceProviderNames.FoundationaLLM_AzureAI)
                    .GetResourceAsync<AzureAIProject>(workflow.AIProjectObjectId!, userIdentity);            

            return (agentId!, vectorStoreId!, workflow!, agentAIModel, agentPrompt, project);
        }

        private async Task RunDataPipeline(
            string instanceId,
            string fileObjectId,
            AgentBase agent,
            string contentAction,
            UnifiedUserIdentity userIdentity)
        {
            var dataPipelineResourceProvider =
                GetResourceProviderServiceByName(ResourceProviderNames.FoundationaLLM_DataPipeline);
            var knowledgeSearchSettings = agent!.Tools
                .Select(t => t.GetKnowledgeSearchSettings())
                .Where(s => s != null)
                .SingleOrDefault()
                ?? throw new OrchestrationException(
                    $"The agent {agent.Name} does not have the required knowledge search settings set.");
            var fileResourcePath = ResourcePath.GetResourcePath(fileObjectId);
            var fileId =
                $"/instances/{instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Context}/files/{fileResourcePath.ResourceId!}";

            var newDataPipelineRun = DataPipelineRun.Create(
                knowledgeSearchSettings.FileUploadDataPipelineObjectId,
                DataPipelineTriggerNames.DefaultManualTrigger,
                new()
                {
                    { DataPipelineTriggerParameterNames.DataSourceContextFileContextFileObjectId, fileId},
                    { DataPipelineTriggerParameterNames.DataSourceContextFileContentAction, contentAction },
                    { DataPipelineTriggerParameterNames.StageEmbedKnowledgeUnitObjectId, knowledgeSearchSettings.AgentPrivateStoreKnowledgeUnitObjectId },
                    { DataPipelineTriggerParameterNames.StageIndexKnowledgeUnitObjectId, knowledgeSearchSettings.AgentPrivateStoreKnowledgeUnitObjectId },
                    // By convention, the vector store id for the agent's private files is the agent's name.
                    { DataPipelineTriggerParameterNames.StageIndexVectorStoreId, agent.Name },
                    { DataPipelineTriggerParameterNames.StageRemovalKnowledgeUnitObjectId, knowledgeSearchSettings.AgentPrivateStoreKnowledgeUnitObjectId },
                    // By convention, the vector store id for the agent's private files is the agent's name.
                    { DataPipelineTriggerParameterNames.StageRemovalVectorStoreId, agent.Name }
                },
                userIdentity.UPN!,
                DataPipelineRunProcessors.Frontend);

            var success = await PollingResourceRunner<DataPipelineRun>.Start(
                instanceId,
                dataPipelineResourceProvider,
                newDataPipelineRun,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(300),
                _logger,
                ServiceContext.ServiceIdentity!);

            if (!success)
                throw new ResourceProviderException(
                    $"The execution of the data pipeline for file {fileObjectId} failed.");
        }

        /// <summary>
        /// Adds file to the assistant-level vector store.
        /// Assumes the file is already uploaded and the OpenAI File ID is available.
        /// </summary>
        /// <param name="instanceId">Identifies the FoundationaLLM instance.</param>
        /// <param name="apiEndpointUrl">The API endpoint URL of the OpenAI service.</param>
        /// <param name="deploymentName">The deployment name of the model in the OpenAI service.</param>
        /// <param name="vectorStoreId">The assistant vector store ID.</param>
        /// <param name="fileId">The OpenAI FileId indicating the file to add to the vector store.</param>
        /// <param name="userIdentity">The identity of the user.</param>
        /// <returns>Returns true if successful, false otherwise.</returns>
        private async Task<bool> AddFileToAssistantsVectorStore(
            string instanceId,
            string apiEndpointUrl,
            string deploymentName,
            string vectorStoreId,
            string fileId,
            UnifiedUserIdentity userIdentity)
        {
            var gatewayClient = await GetGatewayServiceClient(
                instanceId,
                userIdentity);

            Dictionary<string, object> parameters = new()
            {
                { OpenAIAgentCapabilityParameterNames.OpenAIEndpoint, apiEndpointUrl },
                { OpenAIAgentCapabilityParameterNames.OpenAIModelDeploymentName, deploymentName },
                { OpenAIAgentCapabilityParameterNames.AddOpenAIFileToVectorStore, true },
                { OpenAIAgentCapabilityParameterNames.OpenAIVectorStoreId, vectorStoreId },
                { OpenAIAgentCapabilityParameterNames.OpenAIFileId, fileId }
            };
            var vectorizationResult = await gatewayClient!.CreateAgentCapability(
                            instanceId,
                            AgentCapabilityCategoryNames.OpenAIAssistants,
                            string.Empty,
                            parameters);

            vectorizationResult.TryGetValue(OpenAIAgentCapabilityParameterNames.OpenAIFileActionOnVectorStoreSuccess, out var vectorizationSuccessObject);
            var vectorizationSuccess = ((JsonElement)vectorizationSuccessObject!).Deserialize<bool>();
            if (!vectorizationSuccess)
                throw new OrchestrationException($"The vectorization of file id {fileId} into the vector store with id {vectorStoreId} failed.");
            return vectorizationSuccess;
        }

        /// <summary>
        /// Removes a file from the assistant-level vector store.
        /// Assumes the file is already uploaded and the OpenAI File ID is available.
        /// </summary>
        /// <param name="instanceId">Identifies the FoundationaLLM instance.</param>
        /// <param name="apiEndpointUrl">The API endpoint URL of the OpenAI service.</param>
        /// <param name="deploymentName">The deployment name of the model in the OpenAI service.</param>
        /// <param name="vectorStoreId">The assistant vector store ID.</param>
        /// <param name="fileId">The OpenAI FileId indicating the file to remove from the vector store.</param>
        /// <param name="userIdentity">The identity of the user.</param>
        /// <returns>Returns true if successful, false otherwise.</returns>
        private async Task<bool> RemoveFileFromAssistantsVectorStore(
            string instanceId,
            string apiEndpointUrl,
            string deploymentName,
            string vectorStoreId,
            string fileId,
            UnifiedUserIdentity userIdentity)
        {
            var gatewayClient = await GetGatewayServiceClient(
                instanceId,
                userIdentity);

            Dictionary<string, object> parameters = new()
            {
                { OpenAIAgentCapabilityParameterNames.OpenAIEndpoint, apiEndpointUrl },
                { OpenAIAgentCapabilityParameterNames.OpenAIModelDeploymentName, deploymentName },
                { OpenAIAgentCapabilityParameterNames.RemoveOpenAIFileFromVectorStore, true },
                { OpenAIAgentCapabilityParameterNames.OpenAIVectorStoreId, vectorStoreId },
                { OpenAIAgentCapabilityParameterNames.OpenAIFileId, fileId }
            };
            var removalResult = await gatewayClient!.CreateAgentCapability(
                            instanceId,
                            AgentCapabilityCategoryNames.OpenAIAssistants,
                            string.Empty,
                            parameters);

            removalResult.TryGetValue(OpenAIAgentCapabilityParameterNames.OpenAIFileActionOnVectorStoreSuccess, out var vectorStoreSuccessObject);
            var removalSuccess = ((JsonElement)vectorStoreSuccessObject!).Deserialize<bool>();
            if (!removalSuccess)
                throw new OrchestrationException($"The removal of file id {fileId} from the vector store with id {vectorStoreId} failed.");
            return removalSuccess;
        }

        /// <summary>
        /// Adds file to the assistant code interpreter resources.
        /// Assumes the file is already uploaded and the OpenAI File ID is available.
        /// </summary>
        /// <param name="instanceId">Identifies the FoundationaLLM instance.</param>
        /// <param name="apiEndpointUrl">The API endpoint URL of the OpenAI service.</param>
        /// <param name="deploymentName">The deployment name of the model in the OpenAI service.</param>
        /// <param name="assistantId">The unique identifier of the OpenAI Assistant.</param>
        /// <param name="fileId">The OpenAI FileId indicating the file to add to the code interpreter resources.</param>
        /// <param name="userIdentity">The identity of the user.</param>
        /// <returns>Returns true if successful, false otherwise.</returns>
        private async Task<bool> AddFileToAssistantsCodeInterpreter(
            string instanceId,
            string apiEndpointUrl,
            string deploymentName,
            string assistantId,
            string fileId,
            UnifiedUserIdentity userIdentity)
        {
            var gatewayClient = await GetGatewayServiceClient(
                instanceId,
                userIdentity);

            Dictionary<string, object> parameters = new()
            {
                { OpenAIAgentCapabilityParameterNames.OpenAIEndpoint, apiEndpointUrl },
                { OpenAIAgentCapabilityParameterNames.OpenAIModelDeploymentName, deploymentName },
                { OpenAIAgentCapabilityParameterNames.AddOpenAIFileToCodeInterpreter, true },
                { OpenAIAgentCapabilityParameterNames.OpenAIAssistantId, assistantId },
                { OpenAIAgentCapabilityParameterNames.OpenAIFileId, fileId }
            };
            var additionResult = await gatewayClient!.CreateAgentCapability(
                            instanceId,
                            AgentCapabilityCategoryNames.OpenAIAssistants,
                            string.Empty,
                            parameters);

            additionResult.TryGetValue(OpenAIAgentCapabilityParameterNames.OpenAIFileActionOnCodeInterpreterSuccess, out var codeInterpreterSuccessObject);
            var additionSuccess = ((JsonElement)codeInterpreterSuccessObject!).Deserialize<bool>();
            if (!additionSuccess)
                throw new OrchestrationException($"The addition of file id {fileId} into the code interpreter resources of the assistant with id {assistantId} failed.");
            return additionSuccess;
        }

        /// <summary>
        /// Removes a file from the assistant code interpreter resources.
        /// Assumes the file is already uploaded and the OpenAI File ID is available.
        /// </summary>
        /// <param name="instanceId">Identifies the FoundationaLLM instance.</param>
        /// <param name="apiEndpointUrl">The API endpoint URL of the OpenAI service.</param>
        /// <param name="deploymentName">The deployment name of the model in the OpenAI service.</param>
        /// <param name="assistantId">The unique identifier of the OpenAI Assistant.</param>
        /// <param name="fileId">The OpenAI FileId indicating the file to remove from the code interpreter.</param>
        /// <param name="userIdentity">The identity of the user.</param>
        /// <returns>Returns true if successful, false otherwise.</returns>
        private async Task<bool> RemoveFileFromAssistantsCodeInterpreter(
            string instanceId,
            string apiEndpointUrl,
            string deploymentName,
            string assistantId,
            string fileId,
            UnifiedUserIdentity userIdentity)
        {
            var gatewayClient = await GetGatewayServiceClient(
                instanceId,
                userIdentity);

            Dictionary<string, object> parameters = new()
            {
                { OpenAIAgentCapabilityParameterNames.OpenAIEndpoint, apiEndpointUrl },
                { OpenAIAgentCapabilityParameterNames.OpenAIModelDeploymentName, deploymentName },
                { OpenAIAgentCapabilityParameterNames.RemoveOpenAIFileFromCodeInterpreter, true },
                { OpenAIAgentCapabilityParameterNames.OpenAIAssistantId, assistantId },
                { OpenAIAgentCapabilityParameterNames.OpenAIFileId, fileId }
            };
            var removalResult = await gatewayClient!.CreateAgentCapability(
                            instanceId,
                            AgentCapabilityCategoryNames.OpenAIAssistants,
                            string.Empty,
                            parameters);

            removalResult.TryGetValue(OpenAIAgentCapabilityParameterNames.OpenAIFileActionOnCodeInterpreterSuccess, out var codeInterpreterSuccessObject);
            var removalSuccess = ((JsonElement)codeInterpreterSuccessObject!).Deserialize<bool>();
            if (!removalSuccess)
                throw new OrchestrationException($"The removal of file id {fileId} from the code interpreter resources for assistant with id {assistantId} failed.");
            return removalSuccess;
        }

        private async Task<ResourceProviderUpsertResult> CreateNewAgentFromTemplate(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            AgentCreationFromTemplateRequest agentCreationRequest,
            UnifiedUserIdentity userIdentity)
        {
            var agentTemplate = await LoadResource<AgentTemplate>(
                resourcePath.MainResourceId!)
                ?? throw new ResourceProviderException(
                    $"The agent template {resourcePath.MainResourceId} does not exist.",
                    StatusCodes.Status404NotFound);

            var agentTemplateFolderPath = string.Join('/', [
                _storageRootPath ?? string.Empty,
                _name,
                resourcePath.InstanceId!,
                agentTemplate.Name
            ]);

            var agentTemplateFilesPaths = await _storageService.GetFilePathsAsync(
                _storageContainerName,
                agentTemplateFolderPath);

            var fileLoadTasks = agentTemplateFilesPaths
                .Select(filePath =>
                    _storageService.ReadFileAsync(
                        _storageContainerName,
                        filePath,
                        default))
                .ToArray();

            await Task.WhenAll(fileLoadTasks);

            var agentTemplateFiles = agentTemplateFilesPaths.Zip(fileLoadTasks, (filePath, fileContent) =>
                    new
                    {
                        FileName = Path.GetFileName(filePath),
                        Content = fileContent.Result.ToString()
                    })
                .ToDictionary(
                    x => x.FileName,
                    x => x.Content);

            var result = await _agentTemplateService.CreateAgent(
                resourcePath.InstanceId!,
                agentCreationRequest,
                agentTemplateFiles,
                userIdentity);

            return result;
        }

        #endregion

        #region Utils
        private static string GetFileExtension(string fileName) =>
            Path.GetExtension(fileName);

        private IContextServiceClient GetContextServiceClient(
            UnifiedUserIdentity userIdentity) =>
            new ContextServiceClient(
                new OrchestrationContext { CurrentUserIdentity = userIdentity },
                _serviceProvider.GetRequiredService<IHttpClientFactoryService>(),
                _serviceProvider.GetRequiredService<ILogger<ContextServiceClient>>());

        #endregion

        #region Schema upgrades

        /// <inheritdoc/>
        protected override List<ResourceReferenceListSchemaUpgrade<AgentReference>> GetResourceReferenceSchemaUpgrades() =>
            [
                new ResourceReferenceListSchemaUpgrade<AgentReference>
                {
                    SchemaVersion = 2,
                    ResourceReferenceUpgradeAction = UpgradeAgentResourceSchema_V1_to_V2
                }
            ];

        private async Task UpgradeAgentResourceSchema_V1_to_V2(
            AgentReference agentReference,
            IStorageService storage,
            ILogger logger)
        {
            if (agentReference.Type == "knowledge-management"
                && !agentReference.Deleted)
            {
                logger.LogInformation("Upgrading Agent resource {AgentName} from schema version 1 to 2...",
                    agentReference.Name);

                agentReference.Type = AgentTypes.GenericAgent;

                var agentFile = await storage.ReadFileAsync(
                    _storageContainerName,
                    agentReference.Filename,
                    CancellationToken.None);

                var jsonNode = JsonNode.Parse(agentFile.ToString());
                jsonNode!["type"] = AgentTypes.GenericAgent;

                await storage.WriteFileAsync(
                    _storageContainerName,
                    agentReference.Filename,
                    jsonNode!.ToJsonString(),
                    "application/json",
                    CancellationToken.None);

                logger.LogInformation("Successfully upgraded Agent resource {AgentName} from schema version 1 to 2...",
                    agentReference.Name);
            }
        }

        #endregion
    }
}

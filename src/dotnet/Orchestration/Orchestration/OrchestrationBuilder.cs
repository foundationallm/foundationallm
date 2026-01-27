using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Constants.Templates;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.CodeExecution;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Models.ResourceProviders.AzureAI;
using FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using FoundationaLLM.Common.Models.ResourceProviders.Vector;
using FoundationaLLM.Orchestration.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Orchestration.Core.Orchestration
{
    /// <summary>
    /// Builds an orchestration for a FoundationaLLM agent.
    /// </summary>
    public class OrchestrationBuilder
    {
        /// <summary>
        /// Builds the orchestration used to handle a synchronous completion operation or start an asynchronous completion operation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance ID.</param>
        /// <param name="agentName">The unique name of the agent for which the orchestration is built.</param>
        /// <param name="originalRequest">The <see cref="CompletionRequest"/> request for which the orchestration is built.</param>
        /// <param name="callContext">The call context of the request being handled.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> used to retrieve app settings from configuration.</param>
        /// <param name="resourceProviderServices">A dictionary of <see cref="IResourceProviderService"/> resource providers hashed by resource provider name.</param>
        /// <param name="llmOrchestrationServiceManager">The <see cref="ILLMOrchestrationServiceManager"/> that manages internal and external orchestration services.</param>
        /// <param name="userProfileService">The user profile service that manages user profiles.</param>
        /// <param name="cosmosDBService">The <see cref="IAzureCosmosDBService"/> used to interact with the Cosmos DB database.</param>
        /// <param name="templatingService">The <see cref="ITemplatingService"/> used to render templates.</param>
        /// <param name="contextServiceClient">The <see cref="IContextServiceClient"/> client used to call the Context API.</param>
        /// <param name="userPromptRewriteService">The <see cref="IUserPromptRewriteService"/> used to rewrite user prompts.</param>
        /// <param name="semanticCacheService">The <see cref="ISemanticCacheService"/> used to cache and retrieve completion responses.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> provding dependency injection services for the current scope.</param>
        /// <param name="loggerFactory">The logger factory used to create new loggers.</param>
        /// <param name="completionRequestObserver">An optional observer for completion requests.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<OrchestrationBase?> Build(
            string instanceId,
            string agentName,
            CompletionRequest originalRequest,
            IOrchestrationContext callContext,
            IConfiguration configuration,
            Dictionary<string, IResourceProviderService> resourceProviderServices,
            ILLMOrchestrationServiceManager llmOrchestrationServiceManager,
            IUserProfileService userProfileService,
            IAzureCosmosDBService cosmosDBService,
            ITemplatingService templatingService,
            IContextServiceClient contextServiceClient,
            IUserPromptRewriteService userPromptRewriteService,
            ISemanticCacheService semanticCacheService,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            Func<LLMCompletionRequest, CompletionRequest, Task>? completionRequestObserver = null)
        {
            var logger = loggerFactory.CreateLogger<OrchestrationBuilder>();

            var result = await LoadAgent(
                instanceId,
                agentName,
                originalRequest,
                resourceProviderServices,
                templatingService,
                contextServiceClient,
                callContext.CurrentUserIdentity!,
                logger);

            if (result.Agent == null) return null;

            var vectorStoreId = await EnsureAgentCapabilities(
                instanceId,
                result.Agent,
                originalRequest.SessionId!,
                result.ExplodedObjectsManager,
                resourceProviderServices,
                callContext.CurrentUserIdentity!,
                logger);

            if (result.Agent.AgentType == typeof(GenericAgent))
            {
                var orchestrator = !string.IsNullOrWhiteSpace(result.Agent.Workflow?.WorkflowHost)
                    ? result.Agent.Workflow.WorkflowHost
                    : LLMOrchestrationServiceNames.LangChain;

                if (originalRequest.LongRunningOperation)
                {
                    await cosmosDBService.PatchOperationsItemPropertiesAsync<LongRunningOperationContext>(
                        originalRequest.OperationId!,
                        originalRequest.OperationId!,
                        new Dictionary<string, object?>
                        {
                            { "/orchestrator", orchestrator! },
                            { "/agentWorkflowMainAIModelAPIEndpoint", result.APIEndpointConfiguration!.Url }
                        });
                }

                var userProfile = await userProfileService.GetUserProfileForUserAsync(
                    instanceId,
                    callContext.CurrentUserIdentity!.UPN!);
                var persistCompletionRequest = userProfile?.Flags.GetValueOrDefault(UserProfileFlags.PersistOrchestrationCompletionRequests, false) ?? false;

                result.ExplodedObjectsManager.TryAdd(
                    CompletionRequestObjectsKeys.TracingTraceCompletionRequest,
                    persistCompletionRequest);

                var orchestrationService = llmOrchestrationServiceManager.GetService(
                    instanceId,
                    orchestrator!,
                    serviceProvider,
                    callContext);

                var kmOrchestration = new AgentOrchestration(
                    instanceId,
                    result.Agent.ObjectId!,
                    (GenericAgent)result.Agent,
                    originalRequest.SessionId!,
                    result.APIEndpointConfiguration!.Url,
                    result.ExplodedObjectsManager.GetExplodedObjects() ?? [],
                    callContext,
                    orchestrationService,
                    userPromptRewriteService,
                    semanticCacheService,
                    loggerFactory.CreateLogger<OrchestrationBase>(),
                    serviceProvider.GetRequiredService<IHttpClientFactoryService>(),
                    resourceProviderServices,
                    vectorStoreId,
                    null,
                    contextServiceClient,
                    persistCompletionRequest ? completionRequestObserver : null);

                return kmOrchestration;
            }

            return null;
        }

        /// <summary>
        /// Builds the orchestration used to retrieve the status of an asynchronous completion operation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="operationId">The asynchronous completion operation identifier.</param>
        /// <param name="callContext">The call context of the request being handled.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> used to retrieve app settings from configuration.</param>
        /// <param name="resourceProviderServices">A dictionary of <see cref="IResourceProviderService"/> resource providers hashed by resource provider name.</param>
        /// <param name="llmOrchestrationServiceManager">The <see cref="ILLMOrchestrationServiceManager"/> that manages internal and external orchestration services.</param>
        /// <param name="cosmosDBService">The <see cref="IAzureCosmosDBService"/> used to interact with the Cosmos DB database.</param>
        /// <param name="semanticCacheService">The <see cref="ISemanticCacheService"/> used to cache and retrieve completion responses.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> provding dependency injection services for the current scope.</param>
        /// <param name="loggerFactory">The logger factory used to create new loggers.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<OrchestrationBase?> BuildForStatus(
            string instanceId,
            string operationId,
            IOrchestrationContext callContext,
            IConfiguration configuration,
            Dictionary<string, IResourceProviderService> resourceProviderServices,
            ILLMOrchestrationServiceManager llmOrchestrationServiceManager,
            IAzureCosmosDBService cosmosDBService,
            ISemanticCacheService semanticCacheService,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory)
        {
            var operationContext = await cosmosDBService.GetLongRunningOperationContextAsync(operationId);

            var orchestrationService = llmOrchestrationServiceManager.GetService(instanceId, operationContext.Orchestrator!, serviceProvider, callContext);

            var kmOrchestration = new AgentOrchestration(
                instanceId,
                ResourcePath.GetObjectId(
                    instanceId,
                    ResourceProviderNames.FoundationaLLM_Agent,
                    AgentResourceTypeNames.Agents,
                    operationContext.AgentName),
                null,
                operationContext.SessionId!,
                operationContext.AgentWorkflowMainAIModelAPIEndpoint!,
                null,
                callContext,
                orchestrationService,
                null,
                semanticCacheService,
                loggerFactory.CreateLogger<OrchestrationBase>(),
                serviceProvider.GetRequiredService<IHttpClientFactoryService>(),
                resourceProviderServices,
                null,
                operationContext,
                null);

            return kmOrchestration;
        }

        private static async Task<(
            AgentBase? Agent,
            AIModelBase? AIModel,
            APIEndpointConfiguration? APIEndpointConfiguration,
            ExplodedObjectsManager ExplodedObjectsManager
            )>
            LoadAgent(
                string instanceId,
                string agentName,
                CompletionRequest originalRequest,
                Dictionary<string, IResourceProviderService> resourceProviderServices,
                ITemplatingService templatingService,
                IContextServiceClient contextServiceClient,
                UnifiedUserIdentity currentUserIdentity,
                ILogger<OrchestrationBuilder> logger)
        {
            if (string.IsNullOrWhiteSpace(agentName))
                throw new OrchestrationException("The agent name provided in the completion request is invalid.");

            if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Agent, out var agentResourceProvider))
                throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_Agent} was not loaded.");
            if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Prompt, out var promptResourceProvider))
                throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_Prompt} was not loaded.");
            if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_DataSource, out var dataSourceResourceProvider))
                throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_DataSource} was not loaded.");
            if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_AIModel, out var aiModelResourceProvider))
                throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_AIModel} was not loaded.");
            if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Configuration, out var configurationResourceProvider))
                throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_Configuration} was not loaded.");
            if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_AzureAI, out var azureAIResourceProvider))
                throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_AzureAI} was not loaded.");
            if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_DataPipeline, out var dataPipelineResourceProvider))
                throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_AzureAI} was not loaded.");
            if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Vector, out var vectorResourceProvider))
                throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_AzureAI} was not loaded.");
            if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Context, out var contextResourceProvider))
                throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_Context} was not loaded.");

            var explodedObjectsManager = new ExplodedObjectsManager();

            var agentBase = await agentResourceProvider.GetResourceAsync<AgentBase>(
                instanceId,
                agentName,
                currentUserIdentity);

            var agentWorkflow = agentBase.Workflow;
            AIModelBase? mainAIModel = null;
            APIEndpointConfiguration? mainAIModelAPIEndpointConfiguration = null;

            foreach (var resourceObjectId in agentWorkflow!.ResourceObjectIds.Values)
            {
                var resourcePath = ResourcePath.GetResourcePath(resourceObjectId.ObjectId);
                switch (resourcePath.MainResourceTypeName)
                {
                    case AIModelResourceTypeNames.AIModels:
                        // Check if the AI model is the main model, if so check for overrides.
                        if (resourceObjectId.Properties.TryGetValue(ResourceObjectIdPropertyNames.ObjectRole, out var aiModelObjectRole)
                            && ((JsonElement)aiModelObjectRole).GetString() == ResourceObjectIdPropertyValues.MainModel)
                        {

                            var retrievedAIModel = await aiModelResourceProvider.GetResourceAsync<AIModelBase>(
                                    resourceObjectId.ObjectId,
                                    currentUserIdentity,
                                    parentResourceInstance: agentBase);
                            var retrievedAPIEndpointConfiguration = await configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(
                                                    retrievedAIModel.EndpointObjectId!,
                                                    currentUserIdentity,
                                                    parentResourceInstance: agentBase);

                            mainAIModel = retrievedAIModel;
                            mainAIModelAPIEndpointConfiguration = retrievedAPIEndpointConfiguration;

                            // Agent Workflow AI Model overrides.
                            if (resourceObjectId.Properties.TryGetValue(ResourceObjectIdPropertyNames.ModelParameters, out var modelParameters)
                                && modelParameters != null)
                            {
                                // Allowing the override only for the keys that are supported.
                                var modelParamsDict = JsonSerializer.Deserialize<Dictionary<string, object>>(((JsonElement)modelParameters).GetRawText());
                                foreach (var key in modelParamsDict!.Keys.Where(k => ModelParametersKeys.All.Contains(k)))
                                {
                                    retrievedAIModel.ModelParameters[key] = modelParamsDict[key];
                                }
                            }

                            explodedObjectsManager.TryAdd(
                                retrievedAIModel.ObjectId!,
                                retrievedAIModel);
                            explodedObjectsManager.TryAdd(
                                retrievedAIModel.EndpointObjectId!,
                                retrievedAPIEndpointConfiguration);
                        }

                        break;                    
                    case AgentResourceTypeNames.Workflows:

                        var retrievedWorkflow = await agentResourceProvider.GetResourceAsync<Workflow>(
                            resourceObjectId.ObjectId,
                            currentUserIdentity,
                            parentResourceInstance: agentBase);

                        explodedObjectsManager.TryAdd(
                            retrievedWorkflow.ObjectId!,
                            retrievedWorkflow);

                        break;
                    case AzureAIResourceTypeNames.Projects:
                        var retrievedProject = await azureAIResourceProvider.GetResourceAsync<AzureAIProject>(
                                resourceObjectId.ObjectId,
                                currentUserIdentity,
                                parentResourceInstance: agentBase);
                        explodedObjectsManager.TryAdd(
                            retrievedProject.ObjectId!,
                            retrievedProject);
                        break;
                }
            }

            if (agentWorkflow is AzureOpenAIAssistantsAgentWorkflow azureOpenAIAssistantsWorkflow)
            {
                explodedObjectsManager.TryAdd(
                    CompletionRequestObjectsKeys.OpenAIAssistantsAssistantId,
                    azureOpenAIAssistantsWorkflow.AssistantId
                        ?? throw new OrchestrationException("The OpenAI Assistants assistant identifier was not found in the agent workflow."));
            }

            if (agentWorkflow is AzureAIAgentServiceAgentWorkflow azureAIAgentServiceWorkflow)
            {
                explodedObjectsManager.TryAdd(
                    CompletionRequestObjectsKeys.AzureAIAgentServiceAgentId,
                    azureAIAgentServiceWorkflow.AgentId
                        ?? throw new OrchestrationException("The Azure AI Agent Service agent identifier was not found in the agent workflow."));
            }

            var gatewayAPIEndpointConfiguration = await configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(
                instanceId,
                "GatewayAPI",
                currentUserIdentity,
                parentResourceInstance: agentBase);

            explodedObjectsManager.TryAdd(
                CompletionRequestObjectsKeys.GatewayAPIEndpointConfiguration,
                gatewayAPIEndpointConfiguration);

            var contextAPIEnpointConfiguration = await configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(
                instanceId,
                ServiceNames.ContextAPI,
                currentUserIdentity,
                parentResourceInstance: agentBase);

            explodedObjectsManager.TryAdd(
                CompletionRequestObjectsKeys.ContextAPIEndpointConfiguration,
                contextAPIEnpointConfiguration);

            explodedObjectsManager.TryAdd(
                CompletionRequestObjectsKeys.InstanceId,
                instanceId);

            explodedObjectsManager.TryAdd(
                CompletionRequestObjectsKeys.AgentName,
                agentBase.Name);

            explodedObjectsManager.TryAdd(
                CompletionRequestObjectsKeys.AgentObjectId,
                agentBase.ObjectId!);

            #region Tools

            List<string> toolNames = [];
            StringBuilder toolList = new();
            StringBuilder toolRouterPrompts = new();

            foreach (var tool in agentBase.Tools)
            {
                toolNames.Add(tool.Name);
                toolList.Append($"- {tool.Name}: {tool.Description}\n");

                // Build the tool parameters dictionary.
                Dictionary<string, object> toolParameters = [];

                if (tool.TryGetPropertyValue<bool>(
                        AgentToolPropertyNames.CodeSessionRequired, out bool codeSessionRequired)
                    && codeSessionRequired)
                {
                    if (!tool.TryGetPropertyValue<string>(
                            AgentToolPropertyNames.CodeSessionEndpointProvider, out string? codeSessionProvider)
                        || string.IsNullOrWhiteSpace(codeSessionProvider))
                        throw new OrchestrationException(
                            $"The tool {tool.Name} requires a code session, but the code session provider is not specified or is invalid.");

                    if (!tool.TryGetPropertyValue<string>(
                            AgentToolPropertyNames.CodeSessionLanguage, out string? codeSessionLanguage)
                        || string.IsNullOrWhiteSpace(codeSessionLanguage))
                        throw new OrchestrationException(
                            $"The tool {tool.Name} requires a code session, but the code session language is not specified or is invalid.");

                    var endpointProviderOverride = default(CodeSessionEndpointProviderOverride);
                    if (tool.TryGetPropertyValue<string>(
                            AgentToolPropertyNames.CodeSessionEndpointProviderOverrides, out var endpointProviderOverrideString)
                        && !string.IsNullOrWhiteSpace(endpointProviderOverrideString))
                    {
                        try
                        {
                            var overrides = JsonSerializer.Deserialize<List<CodeSessionEndpointProviderOverride>>(
                                endpointProviderOverrideString!);
                            if (overrides is not null)
                            {
                                // Check if there is an active override for the current user.
                                endpointProviderOverride = overrides
                                    .FirstOrDefault(o => o.UPN.Equals(
                                        currentUserIdentity.UPN!,
                                        StringComparison.OrdinalIgnoreCase)
                                    && o.Enabled);
                            }
                        }
                        catch (JsonException)
                        {
                            logger.LogWarning(
                                "The code session endpoint provider overrides could not be deserialized. The format is invalid.");
                        }
                    }

                    var contextServiceResponse = await contextServiceClient.CreateCodeSession(
                        instanceId,
                        agentName,
                        originalRequest.SessionId
                            ?? originalRequest.OperationId
                            ?? $"temporary-{Guid.NewGuid().ToString().ToLower()}",
                        tool.Name,
                        codeSessionProvider,
                        codeSessionLanguage,
                        endpointProviderOverride: endpointProviderOverride);

                    if (contextServiceResponse.TryGetValue(out var codeSession))
                    {
                        toolParameters.Add(
                            AgentToolPropertyNames.CodeSessionEndpoint,
                            codeSession.Endpoint);
                        toolParameters.Add(
                            AgentToolPropertyNames.CodeSessionId,
                            codeSession.SessionId);
                    }
                    else
                        throw new OrchestrationException($"The Context API was not able to create code session: {contextServiceResponse.Error?.Detail ?? "N/A"}");
                }

                // Map the metadata filter values to possible values from the completion request.

                List<(string Key, bool Required)> completionRequestMetadataKeys = [];

                if (tool.TryGetPropertyValue<Dictionary<string, object>>(
                        AgentToolPropertyNames.VectorStoreMetadataFilter, out var metadataFilter)
                    && metadataFilter is not null)
                    completionRequestMetadataKeys.AddRange(
                        GetCompletionRequestMetadataKeys(
                            metadataFilter.Values));

                if (tool.TryGetPropertyValue<List<Dictionary<string, object>>>(
                        AgentToolPropertyNames.KnowledgeUnitVectorStoreFilters, out var knowledgeUnitVectorStoreFilters)
                    && knowledgeUnitVectorStoreFilters is not null)
                    foreach (var knowledgeUnitVectorStoreFilter in knowledgeUnitVectorStoreFilters)
                        if (knowledgeUnitVectorStoreFilter.TryGetValue(AgentToolPropertyNames.VectorStoreMetadataFilter, out object knowldegeUnitMetadataFilter))
                        {
                            if (knowldegeUnitMetadataFilter is not null
                                && knowldegeUnitMetadataFilter is JsonElement je
                                && je.ValueKind == JsonValueKind.Object)
                                completionRequestMetadataKeys.AddRange(
                                    GetCompletionRequestMetadataKeys(
                                        je.Deserialize<Dictionary<string, object>>()!.Values));
                        }
                        else
                            throw new OrchestrationException(
                                $"Each knowledge unit vector store filter must contain a {AgentToolPropertyNames.VectorStoreMetadataFilter} property (even if it's set to null).");

                if (completionRequestMetadataKeys.Count > 0
                        && originalRequest.Metadata is null)
                    throw new OrchestrationException(
                        "The completion request metadata is required for the tool, but it was not provided in the completion request.");

                foreach (var (Key, Required) in completionRequestMetadataKeys)
                {
                    if (originalRequest.Metadata!.TryGetValue(Key, out var value))
                    {
                        toolParameters.Add(
                            Required
                                ? $"{AgentToolPropertyValueSources.CompletionRequestMetadata_Required}:{Key}"
                                : $"{AgentToolPropertyValueSources.CompletionRequestMetadata_Optional}:{Key}",
                            value);
                    }
                    else
                    {
                        // If the metadata key is required and is not found in the completion request, throw an exception.
                        if (Required)
                            throw new OrchestrationException($"The metadata key {Key} is required and it was missing from the completion request.");
                    }
                }

                explodedObjectsManager.TryAdd(
                tool.Name,
                toolParameters);

                // Ensure all resource object identifiers are exploded.
                foreach (var resourceObjectId in tool.ResourceObjectIds.Values)
                {
                    var resourcePath = ResourcePath.GetResourcePath(resourceObjectId.ObjectId);

                    // No need to explode objects that have already been exploded.
                    if (explodedObjectsManager.HasKey(resourceObjectId.ObjectId))
                        continue;

                    switch (resourcePath.MainResourceTypeName)
                    {
                        case AIModelResourceTypeNames.AIModels:

                            var aiModel = await aiModelResourceProvider.GetResourceAsync<AIModelBase>(
                                resourceObjectId.ObjectId,
                                currentUserIdentity,
                                parentResourceInstance: agentBase);

                            explodedObjectsManager.TryAdd(
                                resourceObjectId.ObjectId,
                                aiModel);

                            // TODO: Improve handling to allow each tool to override model parameters separately
                            if (!string.IsNullOrEmpty(aiModel.EndpointObjectId) && !explodedObjectsManager.HasKey(aiModel.EndpointObjectId))
                            {
                                var aiModelEndpoint = await configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(
                                    aiModel.EndpointObjectId!,
                                    currentUserIdentity,
                                    parentResourceInstance: agentBase);

                                explodedObjectsManager.TryAdd(
                                    aiModel.EndpointObjectId!,
                                    aiModelEndpoint);
                            }

                            break;

                        case ConfigurationResourceTypeNames.APIEndpointConfigurations:
                            var apiEndpoint = await configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(
                                resourceObjectId.ObjectId,
                                currentUserIdentity,
                                parentResourceInstance: agentBase);

                            explodedObjectsManager.TryAdd(
                                resourceObjectId.ObjectId,
                                apiEndpoint);

                            break;

                        case DataPipelineResourceTypeNames.DataPipelines:
                            
                            // No need to send in the details of the data pipeline.
                            break;

                        case VectorResourceTypeNames.VectorDatabases:

                            // No need to send in the details of the vector database anymore.

                            //var vectorDatabase = await vectorResourceProvider.GetResourceAsync<VectorDatabase>(
                            //    resourceObjectId.ObjectId,
                            //    currentUserIdentity,
                            //    parentResourceInstance: agentBase);

                            //explodedObjectsManager.TryAdd(
                            //    resourceObjectId.ObjectId,
                            //    vectorDatabase);

                            //if (!explodedObjectsManager.HasKey(vectorDatabase.APIEndpointConfigurationObjectId))
                            //{
                            //    // Explode the object only if it hasn't been exploded yet.

                            //    var vectorDatabaseApiEndpoint = await configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(
                            //        vectorDatabase.APIEndpointConfigurationObjectId,
                            //        currentUserIdentity,
                            //        parentResourceInstance: agentBase);

                            //    explodedObjectsManager.TryAdd(
                            //        vectorDatabase.APIEndpointConfigurationObjectId,
                            //        vectorDatabaseApiEndpoint);
                            //}

                            break;

                        case PromptResourceTypeNames.Prompts:
                            var prompt = await promptResourceProvider.GetResourceAsync<PromptBase>(
                                resourceObjectId.ObjectId,
                                currentUserIdentity,
                                parentResourceInstance: agentBase);                            
                            
                            if (prompt is MultipartPrompt multipartPrompt)
                            {
                                if(multipartPrompt is not null)
                                {
                                    if (resourceObjectId.HasObjectRole(ResourceObjectIdPropertyValues.RouterPrompt))
                                    {
                                        toolRouterPrompts.Append(multipartPrompt.Prefix +
                                               (string.IsNullOrEmpty(multipartPrompt.Suffix)
                                                    ? string.Empty : multipartPrompt.Suffix) + "\n");
                                    }
                                    else
                                    {
                                        // prompt template token replacement                                        
                                        multipartPrompt.Prefix = templatingService.Transform(multipartPrompt.Prefix!);
                                        multipartPrompt.Suffix = templatingService.Transform(multipartPrompt.Suffix!);
                                        explodedObjectsManager.TryAdd(
                                            resourceObjectId.ObjectId,
                                                prompt);                                        
                                    }
                                }
                                
                            }                            
                            break;

                        case ContextResourceTypeNames.KnowledgeSources:
                            // No need to send in the details of the knowledge source.
                            break;

                        case ContextResourceTypeNames.KnowledgeUnits:

                            // No need to send in the details of the knowledge unit anymore.

                            //var knowledgeUnit = await contextResourceProvider.GetResourceAsync<KnowledgeUnit>(
                            //    resourceObjectId.ObjectId,
                            //    currentUserIdentity,
                            //    parentResourceInstance: agentBase);

                            //explodedObjectsManager.TryAdd(
                            //    resourceObjectId.ObjectId,
                            //    knowledgeUnit);

                            break;

                        case DataSourceResourceTypeNames.DataSources:

                            // No need to send in the details of the data source anymore.

                            //var dataSource = await dataSourceResourceProvider.GetResourceAsync<DataSourceBase>(
                            //    resourceObjectId.ObjectId,
                            //    currentUserIdentity,
                            //    parentResourceInstance: agentBase);

                            //explodedObjectsManager.TryAdd(
                            //    dataSource.ObjectId!,
                            //    dataSource);
                            break;

                        default:
                            throw new OrchestrationException($"Unknown resource type '{resourcePath.MainResourceTypeName}'.");
                    }
                }
            }

            explodedObjectsManager.TryAdd(
                CompletionRequestObjectsKeys.ToolNames,
                toolNames);

            #endregion

            #region Build workflow prompts

            var tokenReplacements = new Dictionary<string, string>();

            // If tools exist on the agent, prepare for the potential of tools list token replacements in the prompt.

            if (toolList.Length > 0)
                tokenReplacements.Add(TemplateVariables.ToolList, toolList.ToString());

            if (toolRouterPrompts.Length > 0)
                tokenReplacements.Add(TemplateVariables.ToolRouterPrompts, toolRouterPrompts.ToString());

            #region Main prompt and router prompt processing

            var mainPromptObjectId = agentWorkflow!.MainPromptObjectId;
            var retrievedMainPrompt = await promptResourceProvider.GetResourceAsync<PromptBase>(
                                        mainPromptObjectId!,
                                        currentUserIdentity,
                                        parentResourceInstance: agentBase);
            if (retrievedMainPrompt is MultipartPrompt mainPrompt)
            {
                if (mainPrompt is not null)
                {
                    mainPrompt.Prefix = templatingService.Transform(mainPrompt.Prefix!, tokenReplacements);
                    mainPrompt.Suffix = templatingService.Transform(mainPrompt.Suffix!, tokenReplacements);

                    explodedObjectsManager.TryAdd(
                            mainPromptObjectId!,
                            mainPrompt);
                }
            }

            #endregion

            #region Router prompt processing

            var routerPromptObjectId = agentWorkflow!.RouterPromptObjectId;
            if (routerPromptObjectId is not null)
            {
                var retrievedRouterPrompt = await promptResourceProvider.GetResourceAsync<PromptBase>(
                    routerPromptObjectId!,
                    currentUserIdentity,
                    parentResourceInstance: agentBase);

                if (retrievedRouterPrompt is MultipartPrompt routerPrompt
                    && routerPrompt is not null)
                {
                    routerPrompt.Prefix = templatingService.Transform(routerPrompt.Prefix!, tokenReplacements);
                    routerPrompt.Suffix = templatingService.Transform(routerPrompt.Suffix!, tokenReplacements);

                    explodedObjectsManager.TryAdd(
                            routerPromptObjectId!,
                            routerPrompt);
                }
            }

            #endregion

            #region Files prompt processing

            var filesPromptObjectId = agentWorkflow!.FilesPromptObjectId;
            if (filesPromptObjectId is not null)
            {
                var retrievedFilesPrompt = await promptResourceProvider.GetResourceAsync<PromptBase>(
                    filesPromptObjectId,
                    currentUserIdentity,
                    parentResourceInstance: agentBase);

                if (retrievedFilesPrompt is MultipartPrompt filesPrompt
                    && filesPrompt is not null)
                {
                    filesPrompt.Prefix = templatingService.Transform(filesPrompt.Prefix!, tokenReplacements);
                    filesPrompt.Suffix = templatingService.Transform(filesPrompt.Suffix!, tokenReplacements);

                    explodedObjectsManager.TryAdd(
                            filesPromptObjectId!,
                            filesPrompt);
                }
            }

            // Leave out files that are supposed to be embedded in the request.
            var fileHistory = originalRequest.FileHistory?
                                .Where(f => !f.EmbedContentInRequest)
                                .Select(f => $"{f.OriginalFileName}")
                                .ToArray() ?? [];

            explodedObjectsManager.TryAdd(
                CompletionRequestObjectsKeys.WorkflowInvocationConversationFiles,
                fileHistory);

            // Leave out files that are supposed to be embedded in the request.
            var attachedFiles = originalRequest.FileHistory?
                                .Where(f => f.CurrentMessageAttachment && !f.EmbedContentInRequest)
                                .Select(f => $"{f.OriginalFileName}")
                                .ToArray() ?? [];

            explodedObjectsManager.TryAdd(
                CompletionRequestObjectsKeys.WorkflowInvocationAttachedFiles,
                attachedFiles);

            #endregion

            #region Final prompt processing

            var finalPromptObjectId = agentWorkflow!.FinalPromptObjectId;
            if (finalPromptObjectId is not null)
            {
                var retrievedFinalPrompt = await promptResourceProvider.GetResourceAsync<PromptBase>(
                    finalPromptObjectId,
                    currentUserIdentity,
                    parentResourceInstance: agentBase);
                if (retrievedFinalPrompt is MultipartPrompt finalPrompt
                    && finalPrompt is not null)
                {
                    finalPrompt.Prefix = templatingService.Transform(finalPrompt.Prefix!, tokenReplacements);
                    finalPrompt.Suffix = templatingService.Transform(finalPrompt.Suffix!, tokenReplacements);
                    explodedObjectsManager.TryAdd(
                        finalPromptObjectId!,
                        finalPrompt);
                }
            }

            #endregion

            #endregion

            return (agentBase, mainAIModel, mainAIModelAPIEndpointConfiguration, explodedObjectsManager);
        }

        private static List<(string Key, bool Required)> GetCompletionRequestMetadataKeys(
            IEnumerable<object> values) =>
                [.. values
                    .Where(v =>
                        v is JsonElement je
                        && je.ValueKind == JsonValueKind.String)
                    .Select(v =>
                        {
                            var s = ((JsonElement) v).GetString();
                            if (string.IsNullOrEmpty(s))
                                return new {
                                    Ignore = true,
                                    Key = string.Empty,
                                    Required = false
                                };

                            var tokens = s.Split(':', 2);
                            if (tokens.Length != 2)
                                return new {
                                    Ignore = true,
                                    Key = string.Empty,
                                    Required = false
                                };

                            if (tokens[0] == AgentToolPropertyValueSources.CompletionRequestMetadata_Required)
                                return new {
                                    Ignore = false,
                                    Key = tokens[1],
                                    Required = true
                                };
                            else if (tokens[0] == AgentToolPropertyValueSources.CompletionRequestMetadata_Optional)
                                return new {
                                    Ignore = false,
                                    Key = tokens[1],
                                    Required = false
                                };
                            else

                            return new {
                                    Ignore = true,
                                    Key = string.Empty,
                                    Required = false
                                };
                        })
                    .Where(x => !x.Ignore)
                    .Select(s => (s.Key, s.Required))
                ];

        private static async Task<string?> EnsureAgentCapabilities(
            string instanceId,
            AgentBase agent,
            string conversationId,
            ExplodedObjectsManager explodedObjectsManager,
            Dictionary<string, IResourceProviderService> resourceProviderServices,
            UnifiedUserIdentity currentUserIdentity,
            ILogger<OrchestrationBuilder> logger)
        {
            if (agent.Workflow is AzureOpenAIAssistantsAgentWorkflow)
            {
                if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_AzureOpenAI, out var azureOpenAIResourceProvider))
                    throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_AzureOpenAI} was not loaded.");

                var mainAIModelObjectId = agent.Workflow.MainAIModelObjectId;
                explodedObjectsManager.TryGet<AIModelBase>(mainAIModelObjectId!, out AIModelBase? aiModel);
                explodedObjectsManager.TryGet<APIEndpointConfiguration>(aiModel!.EndpointObjectId!, out APIEndpointConfiguration? apiEndpointConfiguration);
                explodedObjectsManager.TryGet<string>(CompletionRequestObjectsKeys.OpenAIAssistantsAssistantId, out string? openAIAssistantsAssistantId);

                var resourceProviderUpsertOptions = new ResourceProviderUpsertOptions
                {
                    Parameters = new()
                    {
                        { AzureOpenAIResourceProviderUpsertParameterNames.AgentObjectId, agent.ObjectId! },
                        { AzureOpenAIResourceProviderUpsertParameterNames.ConversationId, conversationId },
                        { AzureOpenAIResourceProviderUpsertParameterNames.OpenAIAssistantId, openAIAssistantsAssistantId! },
                        { AzureOpenAIResourceProviderUpsertParameterNames.MustCreateOpenAIAssistantThread, false }
                    }
                };

                var existsResult =
                    await azureOpenAIResourceProvider.ResourceExistsAsync<AzureOpenAIConversationMapping>(instanceId, conversationId, currentUserIdentity);

                if (existsResult.Exists && existsResult.Deleted)
                    throw new OrchestrationException($"The conversation mapping for conversation {conversationId} was deleted but not purged. It cannot be used for active conversations.");

                var conversationMapping = existsResult.Exists
                    ? await azureOpenAIResourceProvider.GetResourceAsync<AzureOpenAIConversationMapping>(instanceId, conversationId, currentUserIdentity)  // No need for parentResourceInstance, access is granted based on policy.
                    : new AzureOpenAIConversationMapping
                    {
                        Name = conversationId,
                        Id = conversationId,
                        UPN = currentUserIdentity.UPN!,
                        InstanceId = instanceId,
                        ConversationId = conversationId,
                        OpenAIEndpoint = apiEndpointConfiguration!.Url,
                        OpenAIAssistantsAssistantId = openAIAssistantsAssistantId!,

                    };

                string? vectorStoreId;

                if (string.IsNullOrWhiteSpace(conversationMapping.OpenAIAssistantsThreadId))
                {
                    // We're either in the case of creating a new conversation mapping or the OpenAI thread identifier is missing.
                    // This can happen if previous attempts of creating the OpenAI thread failed.
                    // Either way we need to force an update to ensure we're attempting to create the OpenAI thread.

                    resourceProviderUpsertOptions.Parameters[AzureOpenAIResourceProviderUpsertParameterNames.MustCreateOpenAIAssistantThread] = true;

                    // We need to update the conversation mapping.
                    // We will rely on the upsert operation result to fill in the OpenAI assistant-related properties.
                    // We expect to get back valid values for the OpenAI Assistants thread identifier and OpenAI vector store identifier.

                    var result = await azureOpenAIResourceProvider.UpsertResourceAsync<AzureOpenAIConversationMapping, AzureOpenAIConversationMappingUpsertResult>(
                        instanceId,
                        conversationMapping,
                        currentUserIdentity,
                        resourceProviderUpsertOptions);

                    if (string.IsNullOrWhiteSpace(result.NewOpenAIAssistantThreadId))
                        throw new OrchestrationException("The OpenAI assistant thread ID was not returned.");
                    else
                        explodedObjectsManager.TryAdd(
                            CompletionRequestObjectsKeys.OpenAIAssistantsThreadId,
                            result.NewOpenAIAssistantThreadId);

                    vectorStoreId = result.NewOpenAIVectorStoreId;
                }
                else
                {
                    explodedObjectsManager.TryAdd(
                        CompletionRequestObjectsKeys.OpenAIAssistantsAssistantId,
                        conversationMapping.OpenAIAssistantsAssistantId!);
                    explodedObjectsManager.TryAdd(
                        CompletionRequestObjectsKeys.OpenAIAssistantsThreadId,
                        conversationMapping.OpenAIAssistantsThreadId!);
                    vectorStoreId = conversationMapping.OpenAIVectorStoreId;
                }

                return vectorStoreId;
            }

            if (agent.Workflow is AzureAIAgentServiceAgentWorkflow)
            {
                if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_AzureAI, out var azureAIResourceProvider))
                    throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_AzureAI} was not loaded.");

                var workflow = agent.Workflow as AzureAIAgentServiceAgentWorkflow;

                explodedObjectsManager.TryGet<string>(CompletionRequestObjectsKeys.AzureAIAgentServiceAgentId, out string? azureAIAgentId);
                
                var resourceProviderUpsertOptions = new ResourceProviderUpsertOptions
                {
                    Parameters = new()
                    {
                        { AzureAIResourceProviderUpsertParameterNames.AgentObjectId, agent.ObjectId! },
                        { AzureAIResourceProviderUpsertParameterNames.ConversationId, conversationId },
                        { AzureAIResourceProviderUpsertParameterNames.AzureAIAgentId, azureAIAgentId! },
                        { AzureAIResourceProviderUpsertParameterNames.MustCreateAzureAIAgentThread, false }
                    }
                };

                var existsResult =
                    await azureAIResourceProvider.ResourceExistsAsync<AzureAIAgentConversationMapping>(instanceId, conversationId, currentUserIdentity);

                if (existsResult.Exists && existsResult.Deleted)
                    throw new OrchestrationException($"The conversation mapping for conversation {conversationId} was deleted but not purged. It cannot be used for active conversations.");

                var conversationMapping = existsResult.Exists
                    ? await azureAIResourceProvider.GetResourceAsync<AzureAIAgentConversationMapping>(instanceId, conversationId, currentUserIdentity) // No need for parentResourceInstance, access is granted based on policy.
                    : new AzureAIAgentConversationMapping
                    {
                        Name = conversationId,
                        Id = conversationId,
                        UPN = currentUserIdentity.UPN!,
                        InstanceId = instanceId,
                        ConversationId = conversationId,
                        ProjectConnectionString = workflow!.ProjectConnectionString!,
                        AzureAIAgentId = azureAIAgentId!
                    };

                string? vectorStoreId;

                if (string.IsNullOrWhiteSpace(conversationMapping.AzureAIAgentThreadId))
                {
                    // We're either in the case of creating a new conversation mapping or the Azure AI Agent Service thread identifier is missing.
                    // This can happen if previous attempts of creating the Azure AI Agent Service thread failed.
                    // Either way we need to force an update to ensure we're attempting to create the Azure AI Agent Service thread.

                    resourceProviderUpsertOptions.Parameters[AzureAIResourceProviderUpsertParameterNames.MustCreateAzureAIAgentThread] = true;

                    // We need to update the conversation mapping.
                    // We will rely on the upsert operation result to fill in the Azure AI Agent Service related properties.
                    // We expect to get back valid values for the Azure AI Agent Service thread identifier and Azure AI Agent Service vector store identifier.

                    var result = await azureAIResourceProvider.UpsertResourceAsync<AzureAIAgentConversationMapping, AzureAIAgentConversationMappingUpsertResult>(
                        instanceId,
                        conversationMapping,
                        currentUserIdentity,
                        resourceProviderUpsertOptions);

                    if (string.IsNullOrWhiteSpace(result.NewAzureAIAgentThreadId))
                        throw new OrchestrationException("The Azure AI Agent Service thread ID was not returned.");
                    else
                        explodedObjectsManager.TryAdd(
                            CompletionRequestObjectsKeys.AzureAIAgentServiceThreadId,
                            result.NewAzureAIAgentThreadId);

                    vectorStoreId = result.NewAzureAIAgentVectorStoreId;
                }
                else
                {
                    explodedObjectsManager.TryAdd(
                        CompletionRequestObjectsKeys.AzureAIAgentServiceAgentId,
                        conversationMapping.AzureAIAgentId);
                    explodedObjectsManager.TryAdd(
                        CompletionRequestObjectsKeys.AzureAIAgentServiceThreadId,
                        conversationMapping.AzureAIAgentThreadId!);
                    vectorStoreId = conversationMapping.AzureAIAgentVectorStoreId;
                }
                return vectorStoreId;
            }           

            // The default vector store identifier is derived directly from the conversation identifier.

            return conversationId;
        }
    }
}

using Azure.AI.OpenAI;
using FoundationaLLM.AzureOpenAI.Models;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.OpenAI;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI;
using FoundationaLLM.Common.Services.ResourceProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FoundationaLLM.AzureOpenAI.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.AzureOpenAI resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationService"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class AzureOpenAIResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IAuthorizationService authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_AzureOpenAI)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IServiceProvider serviceProvider,
        ILogger<AzureOpenAIResourceProviderService> logger)
        : ResourceProviderServiceBase<AzureOpenAIResourceReference>(
            instanceOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            logger,
            eventNamespacesToSubscribe: null,
            useInternalReferencesStore: true)
    {
        private readonly SemaphoreSlim _localLock = new(1, 1);

        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            AzureOpenAIResourceProviderMetadata.AllowedResourceTypes;

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_AzureOpenAI;

        protected override async Task InitializeInternal() =>
            await Task.CompletedTask;

        #region Resource provider support for Management API

        /// <inheritdoc/>
        protected override async Task<object> GetResourcesAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options = null) =>
            resourcePath.MainResourceTypeName switch
            {
                AzureOpenAIResourceTypeNames.AssistantUserContexts => await LoadResources<AssistantUserContext>(
                    resourcePath.ResourceTypeInstances[0],
                    authorizationResult),
                AzureOpenAIResourceTypeNames.FileUserContexts => await LoadResources<FileUserContext>(
                    resourcePath.ResourceTypeInstances[0],
                    authorizationResult),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.MainResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        /// <inheritdoc/>
        protected override async Task<object> ExecuteActionAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction,
            UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeName switch
            {
                AzureOpenAIResourceTypeNames.AssistantUserContexts => resourcePath.Action switch
                {
                    ResourceProviderActions.CheckName => await CheckResourceName<AssistantUserContext>(
                        JsonSerializer.Deserialize<ResourceName>(serializedAction)!),
                    ResourceProviderActions.Purge => await PurgeResource<AssistantUserContext>(resourcePath),
                    _ => throw new ResourceProviderException(
                        $"The action {resourcePath.Action} is not supported for the resource type {AzureOpenAIResourceTypeNames.AssistantUserContexts} by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                _ => throw new ResourceProviderException()
            };

        #endregion

        #region Resource provider strongly typed operations

        /// <inheritdoc/>
        protected override async Task<T> GetResourceAsyncInternal<T>(ResourcePath resourcePath, ResourcePathAuthorizationResult authorizationResult, UnifiedUserIdentity userIdentity, ResourceProviderGetOptions? options = null) =>
            resourcePath.ResourceTypeName switch
            {
                AzureOpenAIResourceTypeNames.AssistantUserContexts => (await LoadResource<T>(
                    resourcePath.MainResourceId!))!,
                AzureOpenAIResourceTypeNames.FilesContent => ((await LoadFileContent(
                    resourcePath.MainResourceId!,
                    resourcePath.RawResourcePath!)) as T)!,
                AzureOpenAIResourceTypeNames.FileUserContexts => (await LoadResource<T>(resourcePath.MainResourceId!) as T)!,
                _ => throw new ResourceProviderException(
                    $"The {resourcePath.MainResourceTypeName} resource type is not supported by the {_name} resource provider.")
            };

        /// <inheritdoc/>
        protected override async Task<TResult> UpsertResourceAsyncInternal<T, TResult>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            T resource,
            UnifiedUserIdentity userIdentity,
            ResourceProviderUpsertOptions? options = null) =>
            resource switch
            {
                AssistantUserContext assistantUserContext => ((await UpdateAssistantUserContext(assistantUserContext, userIdentity, options)) as TResult)!,
                FileUserContext fileUserContext => ((await UpdateFileUserContext(fileUserContext, userIdentity, options)) as TResult)!,
                _ => throw new ResourceProviderException(
                    $"The type {nameof(T)} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        private async Task<FileContent> LoadFileContent(string fileUserContextName, string openAIFileIdObjectId)
        {
            var fileUserContext = await LoadResource<FileUserContext>(fileUserContextName);
            if (!fileUserContext!.TryGetFileMapping(openAIFileIdObjectId, out var agentFileUserContext, out var fileMapping))
                throw new ResourceProviderException(
                    $"Could not find the agent files mapping for file {openAIFileIdObjectId} in the {fileUserContextName} file user context.",
                    StatusCodes.Status404NotFound);

            var azureOpenAIClient = new AzureOpenAIClient(new Uri(agentFileUserContext!.Endpoint), DefaultAuthentication.AzureCredential);
            var fileClient = azureOpenAIClient.GetFileClient();

            // Retrieve using the OpenAI file ID.           
            var result = await fileClient.DownloadFileAsync(fileMapping!.OpenAIFileId);

            return new FileContent
            {
                Name = fileMapping!.OpenAIFileId!,
                OriginalFileName = fileMapping!.OriginalFileName,
                ContentType = fileMapping.ContentType,
                BinaryContent = result.Value.ToMemory()
            };
        }

        #endregion

        #region Resource management

        private async Task<AssistantUserContextUpsertResult> UpdateAssistantUserContext(
            AssistantUserContext assistantUserContext,
            UnifiedUserIdentity userIdentity,
            ResourceProviderUpsertOptions? options = null)
        {
            #region Load and validate upsert options

            var agentObjectId = options?.Parameters.GetValueOrDefault(AzureOpenAIResourceProviderUpsertParameterNames.AgentObjectId) as string
                ?? throw new ResourceProviderException(
                    $"The {_name} resource provider requires the {AzureOpenAIResourceProviderUpsertParameterNames.AgentObjectId} parameter to update the {assistantUserContext.Name} assistant user context.",
                    StatusCodes.Status400BadRequest);

            var conversationId = options?.Parameters.GetValueOrDefault(AzureOpenAIResourceProviderUpsertParameterNames.ConversationId) as string
                ?? throw new ResourceProviderException(
                    $"The {_name} resource provider requires the {AzureOpenAIResourceProviderUpsertParameterNames.ConversationId} parameter to update the {assistantUserContext.Name} assistant user context.",
                    StatusCodes.Status400BadRequest);

            var mustCreateAssistant = options?.Parameters.GetValueOrDefault(AzureOpenAIResourceProviderUpsertParameterNames.MustCreateAssistant) as bool?
                ?? throw new ResourceProviderException(
                    $"The {_name} resource provider requires the {AzureOpenAIResourceProviderUpsertParameterNames.MustCreateAssistant} parameter to update the {assistantUserContext.Name} assistant user context.",
                    StatusCodes.Status400BadRequest);

            var mustCreateAssistantThread = options?.Parameters.GetValueOrDefault(AzureOpenAIResourceProviderUpsertParameterNames.MustCreateAssistantThread) as bool?
                ?? throw new ResourceProviderException(
                    $"The {_name} resource provider requires the {AzureOpenAIResourceProviderUpsertParameterNames.MustCreateAssistantThread} parameter to update the {assistantUserContext.Name} assistant user context.",
                    StatusCodes.Status400BadRequest);

            var fileUserContextName = $"{assistantUserContext.UserPrincipalName.NormalizeUserPrincipalName()}-file-{_instanceSettings.Id.ToLower()}";

            #endregion

            // The assistant user context must always contain:
            // - An agent assistant user context for the agent.
            // - A conversation mapping for the conversation.

            if (!assistantUserContext.AgentAssistants.TryGetValue(agentObjectId, out var agentAssistantUserContext))
                throw new ResourceProviderException(
                    $"The Assistant user context {assistantUserContext.Name} is missing the agent assistant user context for the agent {agentObjectId}.",
                    StatusCodes.Status400BadRequest);

            if (!agentAssistantUserContext.Conversations.TryGetValue(conversationId, out var conversationMapping))
                throw new ResourceProviderException(
                    $"The existing assistant user context {assistantUserContext.Name} is missing the conversation mapping for the conversation {conversationId} and agent {agentObjectId}.",
                    StatusCodes.Status400BadRequest);

            #region Create the OpenAI assistant and thread

            var newOpenAIAssistantId = default(string);
            var newOpenAIAssistantThreadId = default(string);
            var newOpenAIAssistantVectorStoreId = default(string);

            var gatewayClient = new GatewayServiceClient(
               await _serviceProvider.GetRequiredService<IHttpClientFactoryService>()
                   .CreateClient(HttpClientNames.GatewayAPI, userIdentity),
               _serviceProvider.GetRequiredService<ILogger<GatewayServiceClient>>());

            Dictionary<string, object> parameters = new()
                {
                    { OpenAIAgentCapabilityParameterNames.CreateAssistant, mustCreateAssistant },
                    { OpenAIAgentCapabilityParameterNames.CreateAssistantThread, mustCreateAssistantThread },
                    { OpenAIAgentCapabilityParameterNames.Endpoint, agentAssistantUserContext.Endpoint },
                    { OpenAIAgentCapabilityParameterNames.ModelDeploymentName, agentAssistantUserContext.ModelDeploymentName },
                    { OpenAIAgentCapabilityParameterNames.AssistantPrompt, agentAssistantUserContext.Prompt }
                };

            if (!string.IsNullOrWhiteSpace(agentAssistantUserContext.OpenAIAssistantId))
                parameters.Add(OpenAIAgentCapabilityParameterNames.AssistantId, agentAssistantUserContext.OpenAIAssistantId);

            var agentCapabilityResult = await gatewayClient!.CreateAgentCapability(
                _instanceSettings.Id,
                AgentCapabilityCategoryNames.OpenAIAssistants,
                assistantUserContext.Name,
                parameters);

            var referenceTime = DateTime.UtcNow;

            if (agentCapabilityResult.TryGetValue(OpenAIAgentCapabilityParameterNames.AssistantId, out var newOpenAIAssistantIdObject)
                && newOpenAIAssistantIdObject != null)
                newOpenAIAssistantId = ((JsonElement)newOpenAIAssistantIdObject!).Deserialize<string>();

            if (mustCreateAssistant)
            {
                if(string.IsNullOrWhiteSpace(newOpenAIAssistantId))
                    throw new ResourceProviderException(
                        $"The OpenAI assistant was not created for the agent {agentObjectId}.",
                        StatusCodes.Status500InternalServerError);

                agentAssistantUserContext.OpenAIAssistantId = newOpenAIAssistantId;
                agentAssistantUserContext.OpenAIAssistantCreatedOn = referenceTime;
            }

            if (agentCapabilityResult.TryGetValue(OpenAIAgentCapabilityParameterNames.AssistantThreadId, out var newOpenAIAssistantThreadIdObject)
                && newOpenAIAssistantThreadIdObject != null)
                newOpenAIAssistantThreadId = ((JsonElement)newOpenAIAssistantThreadIdObject!).Deserialize<string>();

            if (agentCapabilityResult.TryGetValue(OpenAIAgentCapabilityParameterNames.AssistantVectorStoreId, out var newOpenAIAssistantVectorStoreIdObject)
                && newOpenAIAssistantVectorStoreIdObject != null)
                newOpenAIAssistantVectorStoreId = ((JsonElement)newOpenAIAssistantVectorStoreIdObject!).Deserialize<string>();

            if (mustCreateAssistantThread)
            {
                if (string.IsNullOrWhiteSpace(newOpenAIAssistantVectorStoreId))
                    throw new ResourceProviderException(
                        $"The OpenAI assistant vector store was not created for the agent {agentObjectId} and conversation {conversationId}.",
                        StatusCodes.Status500InternalServerError);

                if (string.IsNullOrWhiteSpace(newOpenAIAssistantThreadId))
                    throw new ResourceProviderException(
                        $"The OpenAI assistant thread was not created for the agent {agentObjectId} and conversation {conversationId}.",
                        StatusCodes.Status500InternalServerError);

                conversationMapping.OpenAIThreadId = newOpenAIAssistantThreadId;
                conversationMapping.OpenAIThreadCreatedOn = referenceTime;
                conversationMapping.OpenAIVectorStoreId = newOpenAIAssistantVectorStoreId;
            }

            #endregion

            var resourceReference = await _resourceReferenceStore!.GetResourceReference(assistantUserContext.Name);

            if (resourceReference == null)
            {
                var assistantUserContextResourceReference = new AzureOpenAIResourceReference
                {
                    Name = assistantUserContext.Name!,
                    Type = assistantUserContext.Type!,
                    Filename = $"/{_name}/{assistantUserContext.Name}.json",
                    Deleted = false
                };

                assistantUserContext.ObjectId = ResourcePath.GetObjectId(
                    _instanceSettings.Id,
                    _name,
                    AzureOpenAIResourceTypeNames.AssistantUserContexts,
                    assistantUserContext.Name);

                try
                {
                    // Ensure that only one thread can create the resource at a time.
                    await _localLock.WaitAsync();

                    // Ensure the assistant user context was not created by another thread in the resource provider instance or
                    // by another resource provider instance in the meantime (since we last checked before acquiring the lock).
                    var existingResourceReference = await _resourceReferenceStore!.GetResourceReference(assistantUserContext.Name);
                    if (existingResourceReference == null)
                    {
                        #region Persist new assistant user context

                        UpdateBaseProperties(assistantUserContext, userIdentity, isNew: true);

                        // Ensure the file user context is also created if it does not exist already.                        
                        var existingFileUserContextReference = await _resourceReferenceStore!.GetResourceReference(fileUserContextName);
                        if (existingFileUserContextReference == null)
                        {
                            // We need to create the file user context as well.
                            var newFileUserContext = new FileUserContext()
                            {
                                Name = fileUserContextName,
                                AssistantUserContextName = assistantUserContext.Name,
                                UserPrincipalName = assistantUserContext.UserPrincipalName,
                                AgentFiles = new()
                                {
                                    {
                                        agentObjectId!,
                                        new ()
                                        {
                                            Endpoint = agentAssistantUserContext!.Endpoint
                                        }
                                    }
                                }
                                
                            };
                            var newUserFileContextResourceReference = new AzureOpenAIResourceReference
                            {
                                Name = fileUserContextName,
                                Type = AzureOpenAITypes.FileUserContext,
                                Filename = $"/{_name}/{fileUserContextName}.json",
                                Deleted = false
                            };

                            await CreateResources<AssistantUserContext, FileUserContext>(
                                assistantUserContextResourceReference, assistantUserContext,
                                newUserFileContextResourceReference, newFileUserContext);
                        }
                        else
                        {
                            // The file user context already exists, so we only need to create the assistant user context.
                            await CreateResource<AssistantUserContext>(assistantUserContextResourceReference, assistantUserContext);
                        }

                        #endregion

                        return new AssistantUserContextUpsertResult
                        {
                            ObjectId = assistantUserContext.ObjectId,
                            ResourceExists = false,
                            NewOpenAIAssistantId = newOpenAIAssistantId!,
                            NewOpenAIAssistantThreadId = newOpenAIAssistantThreadId!,
                            NewOpenAIAssistantVectorStoreId = newOpenAIAssistantVectorStoreId
                        };
                    }

                    return new AssistantUserContextUpsertResult
                    {
                        ObjectId = assistantUserContext.ObjectId,
                        ResourceExists = true
                    };
                }
                finally
                {
                    _localLock.Release();
                }
            }
            else
            {
                try
                {
                    // Ensure that only one thread can update the assistant user context at a time.
                    await _localLock.WaitAsync();

                    #region Update the assistant user context

                    UpdateBaseProperties(assistantUserContext, userIdentity, isNew: false);
                    await SaveResource<AssistantUserContext>(resourceReference, assistantUserContext);

                    #endregion
                }
                finally
                {
                    _localLock.Release();
                }

                // Ensure the agent has a key in the AgentFiles property of the file user context.
                // Safely check the existence of the file user context.
                var fileUserContextReference = await _resourceReferenceStore!.GetResourceReference(fileUserContextName);
                if (fileUserContextReference != null)
                {
                    var fileUserContext = await LoadResource<FileUserContext>(fileUserContextName);
                    if (!fileUserContext!.AgentFiles.ContainsKey(agentObjectId))
                    {
                        fileUserContext.AgentFiles.Add(agentObjectId, new ()
                        {
                            Endpoint = agentAssistantUserContext.Endpoint
                        });
                        await UpdateFileUserContext(fileUserContext, userIdentity);
                    }                    
                }
                else
                {
                    // Create the file user context with the current agent entry in AgentFiles.
                    var newFileUserContext = new FileUserContext()
                    {
                        Name = fileUserContextName,
                        AssistantUserContextName = assistantUserContext.Name,
                        UserPrincipalName = assistantUserContext.UserPrincipalName,
                        AgentFiles = new()
                            {
                                {
                                    agentObjectId!,
                                    new ()
                                    {
                                        Endpoint = agentAssistantUserContext!.Endpoint
                                    }
                                }
                            }

                    };
                    await UpdateFileUserContext(newFileUserContext, userIdentity);
                }
                  
                return new AssistantUserContextUpsertResult
                {
                    ObjectId = assistantUserContext.ObjectId!,
                    ResourceExists = true,
                    NewOpenAIAssistantId = agentAssistantUserContext.OpenAIAssistantId,
                    NewOpenAIAssistantThreadId = newOpenAIAssistantThreadId,
                    NewOpenAIAssistantVectorStoreId = newOpenAIAssistantVectorStoreId
                };
               
            }
        }

        private async Task<FileUserContextUpsertResult> UpdateFileUserContext(
            FileUserContext fileUserContext,
            UnifiedUserIdentity userIdentity,
            ResourceProviderUpsertOptions? options = null)
        {
            var mustCreateAssistantFile = false;
            var newOpenAIFileId = default(string);
            AgentFileUserContext agentFileUserContext = null;

            if (options != null)
            {
                var optionResult =
                    options.Parameters.GetValueOrDefault(AzureOpenAIResourceProviderUpsertParameterNames.MustCreateAssistantFile) as bool?;
                if (optionResult.HasValue)
                    mustCreateAssistantFile = optionResult.Value;
            }

            if (mustCreateAssistantFile)
            {
                #region Load and validate upsert options

                var agentObjectId = options?.Parameters.GetValueOrDefault(AzureOpenAIResourceProviderUpsertParameterNames.AgentObjectId) as string
                    ?? throw new ResourceProviderException(
                        $"The {_name} resource provider requires the {AzureOpenAIResourceProviderUpsertParameterNames.AgentObjectId} parameter to update the {fileUserContext.Name} file user context.",
                        StatusCodes.Status400BadRequest);

                var conversationId = options?.Parameters.GetValueOrDefault(AzureOpenAIResourceProviderUpsertParameterNames.ConversationId) as string
                    ?? throw new ResourceProviderException(
                        $"The {_name} resource provider requires the {AzureOpenAIResourceProviderUpsertParameterNames.ConversationId} parameter to update the {fileUserContext.Name} file user context.",
                        StatusCodes.Status400BadRequest);

                var attachmentObjectId = options?.Parameters.GetValueOrDefault(AzureOpenAIResourceProviderUpsertParameterNames.AttachmentObjectId) as string
                    ?? throw new ResourceProviderException(
                        $"The {_name} resource provider requires the {AzureOpenAIResourceProviderUpsertParameterNames.AttachmentObjectId} parameter to update the {fileUserContext.Name} file user context.",
                        StatusCodes.Status400BadRequest);

                #endregion

                // The file user context must always contain:
                // - An agent file user context for the agent.
                // - A file mapping for the attachment.

                if (!fileUserContext.AgentFiles.TryGetValue(agentObjectId, out agentFileUserContext))
                    throw new ResourceProviderException(
                        $"The file user context {fileUserContext.Name} is missing the agent file user context for the agent {agentObjectId}.",
                        StatusCodes.Status400BadRequest);

                if (!agentFileUserContext.Files.TryGetValue(attachmentObjectId, out var fileMapping))
                    throw new ResourceProviderException(
                        $"The existing file user context {fileUserContext.Name} is missing the file mapping for the attachment {attachmentObjectId} and agent {agentObjectId}.",
                        StatusCodes.Status400BadRequest);

                #region Create the OpenAI Assistant file

                var gatewayClient = new GatewayServiceClient(
                   await _serviceProvider.GetRequiredService<IHttpClientFactoryService>()
                       .CreateClient(HttpClientNames.GatewayAPI, userIdentity),
                   _serviceProvider.GetRequiredService<ILogger<GatewayServiceClient>>());

                Dictionary<string, object> parameters = new()
                    {
                        { OpenAIAgentCapabilityParameterNames.CreateAssistantFile, true },
                        { OpenAIAgentCapabilityParameterNames.Endpoint, agentFileUserContext.Endpoint },
                        { OpenAIAgentCapabilityParameterNames.AttachmentObjectId,  attachmentObjectId }
                    };

                var agentCapabilityResult = await gatewayClient!.CreateAgentCapability(
                        _instanceSettings.Id,
                        AgentCapabilityCategoryNames.OpenAIAssistants,
                        fileUserContext.AssistantUserContextName,
                        parameters);

                var referenceTime = DateTime.UtcNow;

                if (agentCapabilityResult.TryGetValue(OpenAIAgentCapabilityParameterNames.AssistantFileId, out var newOpenAIFileIdObject)
                    && newOpenAIFileIdObject != null)
                        newOpenAIFileId = ((JsonElement)newOpenAIFileIdObject!).Deserialize<string>();

                if (mustCreateAssistantFile)
                {
                    if (string.IsNullOrWhiteSpace(newOpenAIFileId))
                        throw new ResourceProviderException(
                            $"The OpenAI assistant file was not created for the agent {agentObjectId}.",
                            StatusCodes.Status500InternalServerError);

                    fileMapping.OpenAIFileId = newOpenAIFileId;
                    fileMapping.OpenAIFileUploadedOn = referenceTime;
                }

                #endregion
            }

            var resourceReference = await _resourceReferenceStore!.GetResourceReference(fileUserContext.Name);
            
            if (resourceReference == null)
            {
                var fileUserContextResourceReference = new AzureOpenAIResourceReference
                {
                    Name = fileUserContext.Name!,
                    Type = fileUserContext.Type!,
                    Filename = $"/{_name}/{fileUserContext.Name}.json",
                    Deleted = false
                };

                fileUserContext.ObjectId = ResourcePath.GetObjectId(
                    _instanceSettings.Id,
                    _name,
                    AzureOpenAIResourceTypeNames.FileUserContexts,
                    fileUserContext.Name);

                try
                {
                    // Ensure that only one thread can create the resource at a time.
                    await _localLock.WaitAsync();

                    // Ensure the file user context was not created by another thread in the resource provider instance or
                    // by another resource provider instance in the meantime (since we last checked before acquiring the lock).
                    var existingResourceReference = await _resourceReferenceStore!.GetResourceReference(fileUserContext.Name);
                    if (existingResourceReference == null)
                    {
                        #region Persist new file user context

                        UpdateBaseProperties(fileUserContext, userIdentity, isNew: true);

                        await CreateResource<FileUserContext>(fileUserContextResourceReference, fileUserContext);

                        #endregion

                        return new FileUserContextUpsertResult
                        {
                            ObjectId = fileUserContext.ObjectId,
                            ResourceExists = false,
                            NewOpenAIFileId = newOpenAIFileId
                        };
                    }

                    return new FileUserContextUpsertResult
                    {
                        ObjectId = fileUserContext.ObjectId,
                        ResourceExists = true,
                    };
                }
                finally
                {
                    _localLock.Release();
                }
            }
            else
            {
                #region 

                try
                {
                    // Ensure that only one thread can update the Files collection at a time.
                    await _localLock.WaitAsync();
                    var existingUserContext = await LoadResource<FileUserContext>(fileUserContext.Name);
                    if (agentFileUserContext != null)
                    {
                        
                        foreach (var agentFile in fileUserContext.AgentFiles)
                        {
                            if (!existingUserContext!.AgentFiles.ContainsKey(agentFile.Key))
                            {
                                existingUserContext.AgentFiles.Add(agentFile.Key, agentFile.Value);
                            }
                            else
                            {
                                //merge the Files property of existing agent files
                                foreach (var file in agentFile.Value.Files)
                                {
                                    if (!existingUserContext.AgentFiles[agentFile.Key].Files.ContainsKey(file.Key))
                                        existingUserContext.AgentFiles[agentFile.Key].Files.Add(file.Key, file.Value);
                                }
                            }
                        }
                    }                   

                    UpdateBaseProperties(fileUserContext, userIdentity, isNew: false);
                    await SaveResource<FileUserContext>(resourceReference, existingUserContext!);

                    return new FileUserContextUpsertResult
                    {
                        ObjectId = fileUserContext.ObjectId!,
                        ResourceExists = true,
                        NewOpenAIFileId = newOpenAIFileId!
                    };
                }
                finally
                {
                    _localLock.Release();
                }

                #endregion
            }
        }

        #endregion
    }
}

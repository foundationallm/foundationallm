using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProvider;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.Prompt.Constants;
using FoundationaLLM.Prompt.Models.Metadata;
using FoundationaLLM.Prompt.Models.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Prompt.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Prompt resource provider.
    /// </summary>
    public class PromptResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Prompt)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        ILogger<PromptResourceProviderService> logger)
        : ResourceProviderServiceBase(
            instanceOptions.Value,
            storageService,
            eventService,
            resourceValidatorFactory,
            logger)
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() => new()
        {
            {
                PromptResourceTypeNames.Prompts,
                new ResourceTypeDescriptor(
                        PromptResourceTypeNames.Prompts)
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [typeof(MultipartPrompt)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(MultipartPrompt)], [typeof(ResourceProviderUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, [], [], []),
                    ],
                    Actions = [
                            new ResourceTypeAction("checkname", false, true, [
                                new ResourceTypeAllowedTypes(HttpMethod.Post.Method, [], [typeof(ResourceName)], [typeof(ResourceNameCheckResult)])
                            ])
                        ]
                }
            },
            {
                PromptResourceTypeNames.PromptReferences,
                new ResourceTypeDescriptor(PromptResourceTypeNames.PromptReferences)
            }
        };

        private ConcurrentDictionary<string, PromptReference> _promptReferences = [];

        private const string PROMPT_REFERENCES_FILE_NAME = "_prompt-references.json";
        private const string PROMPT_REFERENCES_FILE_PATH = $"/{ResourceProviderNames.FoundationaLLM_Prompt}/_prompt-references.json";

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Prompt;

        /// <inheritdoc/>
        protected override async Task InitializeInternal()
        {
            _logger.LogInformation("Starting to initialize the {ResourceProvider} resource provider...", _name);

            if (await _storageService.FileExistsAsync(_storageContainerName, PROMPT_REFERENCES_FILE_PATH, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, PROMPT_REFERENCES_FILE_PATH, default);
                var promptReferenceStore = JsonSerializer.Deserialize<PromptReferenceStore>(
                    Encoding.UTF8.GetString(fileContent.ToArray()));

                _promptReferences = new ConcurrentDictionary<string, PromptReference>(
                    promptReferenceStore!.ToDictionary());
            }
            else
            {
                await _storageService.WriteFileAsync(
                    _storageContainerName,
                    PROMPT_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(new PromptReferenceStore { PromptReferences = [] }),
                    default,
                    default);
            }

            _logger.LogInformation("The {ResourceProvider} resource provider was successfully initialized.", _name);
        }

        #region Support for Management API

        /// <inheritdoc/>
        protected override async Task<object> GetResourcesAsyncInternal(List<ResourceTypeInstance> instances) =>
            instances[0].ResourceType switch
            {
                PromptResourceTypeNames.Prompts => await LoadPrompts(instances[0]),
                _ => throw new ResourceProviderException($"The resource type {instances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for GetResourcesAsyncInternal

        private async Task<List<PromptBase>> LoadPrompts(ResourceTypeInstance instance)
        {
            if (instance.ResourceId == null)
            {
                return
                [
                    .. (await Task.WhenAll(
                        _promptReferences.Values
                            .Where(pr => !pr.Deleted)
                            .Select(pr => LoadPrompt(pr))))
                ];
            }
            else
            {
                if (!_promptReferences.TryGetValue(instance.ResourceId, out var promptReference)
                    || promptReference.Deleted)
                    throw new ResourceProviderException($"Could not locate the {instance.ResourceId} prompt resource.",
                        StatusCodes.Status404NotFound);

                var prompt = await LoadPrompt(promptReference!);

                return [prompt];
            }
        }

        private async Task<MultipartPrompt> LoadPrompt(PromptReference promptReference)
        {
            if (await _storageService.FileExistsAsync(_storageContainerName, promptReference.Filename, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, promptReference.Filename, default);
                return JsonSerializer.Deserialize(
                    Encoding.UTF8.GetString(fileContent.ToArray()),
                    promptReference.PromptType,
                    _serializerSettings) as Models.Metadata.MultipartPrompt
                    ?? throw new ResourceProviderException($"Failed to load the prompt {promptReference.Name}.");
            }

            throw new ResourceProviderException($"Could not locate the {promptReference.Name} prompt resource.",
                StatusCodes.Status404NotFound);
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task<object> UpsertResourceAsync(List<ResourceTypeInstance> instances, string serializedResource) =>
            instances[0].ResourceType switch
            {
                PromptResourceTypeNames.Prompts => await UpdatePrompt(instances, serializedResource),
                _ => throw new ResourceProviderException($"The resource type {instances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest),
            };

        #region Helpers for UpsertResourceAsync

        private async Task<ResourceProviderUpsertResult> UpdatePrompt(List<ResourceTypeInstance> instances, string serializedPrompt)
        {
            var promptBase = JsonSerializer.Deserialize<PromptBase>(serializedPrompt)
                ?? throw new ResourceProviderException("The object definition is invalid.");

            if (_promptReferences.TryGetValue(promptBase.Name!, out var existingPromptReference)
                && existingPromptReference!.Deleted)
                throw new ResourceProviderException($"The prompt resource {existingPromptReference.Name} cannot be added or updated.",
                        StatusCodes.Status400BadRequest);

            if (instances[0].ResourceId != promptBase.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            var promptReference = new PromptReference
            {
                Name = promptBase.Name!,
                Type = promptBase.Type!,
                Filename = $"/{_name}/{promptBase.Name}.json",
                Deleted = false
            };

            var prompt = JsonSerializer.Deserialize(serializedPrompt, promptReference.PromptType, _serializerSettings);
            (prompt as PromptBase)!.ObjectId = GetObjectId(instances);

            await _storageService.WriteFileAsync(
                _storageContainerName,
                promptReference.Filename,
                JsonSerializer.Serialize(prompt, promptReference.PromptType, _serializerSettings),
                default,
                default);

            _promptReferences.AddOrUpdate(promptReference.Name, promptReference, (k, v) => v);

            await _storageService.WriteFileAsync(
                    _storageContainerName,
                    PROMPT_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(PromptReferenceStore.FromDictionary(_promptReferences.ToDictionary())),
                    default,
                    default);

            return new ResourceProviderUpsertResult
            {
                ObjectId = (prompt as PromptBase)!.ObjectId
            };
        }

        #endregion

        /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task<object> ExecuteActionAsync(List<ResourceTypeInstance> instances, string serializedAction) =>
            instances.Last().ResourceType switch
            {
                PromptResourceTypeNames.Prompts => instances.Last().Action switch
                {
                    PromptResourceProviderActions.CheckName => CheckPromptName(serializedAction),
                    _ => throw new ResourceProviderException($"The action {instances.Last().Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                _ => throw new ResourceProviderException()
            };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        #region Helpers for ExecuteActionAsync

        private ResourceNameCheckResult CheckPromptName(string serializedAction)
        {
            var resourceName = JsonSerializer.Deserialize<ResourceName>(serializedAction);
            return _promptReferences.Values.Any(ar => ar.Name == resourceName!.Name)
                ? new ResourceNameCheckResult
                {
                    Name = resourceName!.Name,
                    Type = resourceName.Type,
                    Status = NameCheckResultType.Denied,
                    Message = "A resource with the specified name already exists or was previously deleted and not purged."
                }
                : new ResourceNameCheckResult
                {
                    Name = resourceName!.Name,
                    Type = resourceName.Type,
                    Status = NameCheckResultType.Allowed
                };
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(List<ResourceTypeInstance> instances)
        {
            switch (instances.Last().ResourceType)
            {
                case PromptResourceTypeNames.Prompts:
                    await DeletePrompt(instances);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {instances.Last().ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
        }

        #region Helpers for DeleteResourceAsync

        private async Task DeletePrompt(List<ResourceTypeInstance> instances)
        {
            if (_promptReferences.TryGetValue(instances.Last().ResourceId!, out var promptReference)
                || promptReference!.Deleted)
            {
                promptReference.Deleted = true;

                await _storageService.WriteFileAsync(
                    _storageContainerName,
                    PROMPT_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(PromptReferenceStore.FromDictionary(_promptReferences.ToDictionary())),
                    default,
                    default);
            }
            else
                throw new ResourceProviderException($"Could not locate the {instances.Last().ResourceId} agent resource.",
                            StatusCodes.Status404NotFound);
        }

        #endregion

        #endregion

        #region Obsolete

        /// <inheritdoc/>
        protected override async Task<T> GetResourceAsyncInternal<T>(List<ResourceTypeInstance> instances) where T: class =>
            instances[0].ResourceType switch
            {
                PromptResourceTypeNames.PromptReferences => await GetPromptAsync<T>(instances),
                _ => throw new ResourceProviderException($"The resource type {instances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        /// <inheritdoc/>
        protected override async Task<IList<T>> GetResourcesAsyncInternal<T>(List<ResourceTypeInstance> instances) where T : class =>
            instances[0].ResourceType switch
            {
                PromptResourceTypeNames.PromptReferences => await GetPromptsAsync<T>(instances),
                _ => throw new ResourceProviderException($"The resource type {instances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };


        private async Task<List<T>> GetPromptsAsync<T>(List<ResourceTypeInstance> instances) where T : class
        {
            if (typeof(T) != typeof(PromptReference))
                throw new ResourceProviderException($"The type of requested resource ({typeof(T)}) does not match the resource type specified in the path ({instances[0].ResourceType}).",
                    StatusCodes.Status400BadRequest);

            var promptReferences = _promptReferences.Values.Cast<PromptReference>().ToList();
            foreach (var promptReference in promptReferences)
            {
                var prompt = await LoadPrompt(promptReference);
            }

            return promptReferences.Cast<T>().ToList();
        }

        private async Task<T> GetPromptAsync<T>(List<ResourceTypeInstance> instances) where T : class
        {
            if (instances.Count != 1)
                throw new ResourceProviderException($"Invalid resource path",
                    StatusCodes.Status400BadRequest);

            if (typeof(T) != typeof(PromptReference))
                throw new ResourceProviderException($"The type of requested resource ({typeof(T)}) does not match the resource type specified in the path ({instances[0].ResourceType}).",
                    StatusCodes.Status400BadRequest);

            _promptReferences.TryGetValue(instances[0].ResourceId!, out var promptReference);
            if (promptReference != null)
            {
                return promptReference as T ?? throw new ResourceProviderException(
                    $"The resource {instances[0].ResourceId!} of type {instances[0].ResourceType} was not found.",
                    StatusCodes.Status404NotFound);
            }
            throw new ResourceProviderException(
                $"The resource {instances[0].ResourceId!} of type {instances[0].ResourceType} was not found.",
                StatusCodes.Status404NotFound);
        }

        #endregion
    }
}

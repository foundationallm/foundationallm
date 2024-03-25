using Azure.Messaging;
using FluentValidation;
using FoundationaLLM.Agent.Constants;
using FoundationaLLM.Agent.Models.Resources;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Events;
using FoundationaLLM.Common.Models.ResourceProvider;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Services.ResourceProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Agent.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Agent resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationService"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to provide loggers for logging.</param>
    public class AgentResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IAuthorizationService authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Agent)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        ILoggerFactory loggerFactory)
        : ResourceProviderServiceBase(
            instanceOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            loggerFactory.CreateLogger<AgentResourceProviderService>(),
            [
                EventSetEventNamespaces.FoundationaLLM_ResourceProvider_Agent
            ])
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            AgentResourceProviderMetadata.AllowedResourceTypes;

        private ConcurrentDictionary<string, AgentReference> _agentReferences = [];

        private const string AGENT_REFERENCES_FILE_NAME = "_agent-references.json";
        private const string AGENT_REFERENCES_FILE_PATH = $"/{ResourceProviderNames.FoundationaLLM_Agent}/_agent-references.json";

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Agent;

        /// <inheritdoc/>
        protected override async Task InitializeInternal()
        {
            _logger.LogInformation("Starting to initialize the {ResourceProvider} resource provider...", _name);

            if (await _storageService.FileExistsAsync(_storageContainerName, AGENT_REFERENCES_FILE_PATH, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, AGENT_REFERENCES_FILE_PATH, default);
                var agentReferenceStore = JsonSerializer.Deserialize<AgentReferenceStore>(
                    Encoding.UTF8.GetString(fileContent.ToArray()));

                _agentReferences = new ConcurrentDictionary<string, AgentReference>(
                    agentReferenceStore!.ToDictionary());
            }
            else
            {
                await _storageService.WriteFileAsync(
                    _storageContainerName,
                    AGENT_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(new AgentReferenceStore { AgentReferences = [] }),
                    default,
                    default);
            }

            _logger.LogInformation("The {ResourceProvider} resource provider was successfully initialized.", _name);
        }

        #region Support for Management API

        /// <inheritdoc/>
        protected override async Task<object> GetResourcesAsyncInternal(ResourcePath resourcePath) =>
            resourcePath.ResourceTypeInstances[0].ResourceType switch
            {
                AgentResourceTypeNames.Agents => await LoadAgents(resourcePath.ResourceTypeInstances[0]),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for GetResourcesAsyncInternal

        private async Task<List<AgentBase>> LoadAgents(ResourceTypeInstance instance)
        {
            if (instance.ResourceId == null)
            {
                return
                [
                    .. (await Task.WhenAll(
                        _agentReferences.Values
                            .Where(ar => !ar.Deleted)
                            .Select(ar => LoadAgent(ar))))
                ];
            }
            else
            {
                if (!_agentReferences.TryGetValue(instance.ResourceId, out var agentReference)
                    || agentReference.Deleted)
                    throw new ResourceProviderException($"Could not locate the {instance.ResourceId} agent resource.",
                        StatusCodes.Status404NotFound);

                var agent = await LoadAgent(agentReference!);

                return [agent];
            }
        }

        private async Task<AgentBase> LoadAgent(AgentReference agentReference)
        {
            // agentReference is null for legacy agents
            if (agentReference != null)
            {
                if (await _storageService.FileExistsAsync(_storageContainerName, agentReference.Filename, default))
                {
                    var fileContent = await _storageService.ReadFileAsync(_storageContainerName, agentReference.Filename, default);
                    return JsonSerializer.Deserialize(
                        Encoding.UTF8.GetString(fileContent.ToArray()),
                        agentReference.AgentType,
                        _serializerSettings) as AgentBase
                        ?? throw new ResourceProviderException($"Failed to load the agent {agentReference.Name}.",
                            StatusCodes.Status400BadRequest);
                }
            }

            var agentName = "legacy";
            if (agentReference!= null)
            {
                agentName = agentReference.Name;
            }
            throw new ResourceProviderException($"Could not locate the {agentName} agent resource.",
                StatusCodes.Status404NotFound);
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task<object> UpsertResourceAsync(ResourcePath resourcePath, string serializedResource) =>
            resourcePath.ResourceTypeInstances[0].ResourceType switch
            {
                AgentResourceTypeNames.Agents => await UpdateAgent(resourcePath, serializedResource),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for UpsertResourceAsync

        private async Task<ResourceProviderUpsertResult> UpdateAgent(ResourcePath resourcePath, string serializedAgent)
        {
            var agentBase = JsonSerializer.Deserialize<AgentBase>(serializedAgent)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            if (_agentReferences.TryGetValue(agentBase.Name!, out var existingAgentReference)
                && existingAgentReference!.Deleted)
                throw new ResourceProviderException($"The agent resource {existingAgentReference.Name} cannot be added or updated.",
                        StatusCodes.Status400BadRequest);

            if (resourcePath.ResourceTypeInstances[0].ResourceId != agentBase.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            var agentReference = new AgentReference
            {
                Name = agentBase.Name!,
                Type = agentBase.Type!,
                Filename = $"/{_name}/{agentBase.Name}.json",
                Deleted = false
            };

            var agent = JsonSerializer.Deserialize(serializedAgent, agentReference.AgentType, _serializerSettings);
            (agent as AgentBase)!.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);

            var validator = _resourceValidatorFactory.GetValidator(agentReference.AgentType);
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

            await _storageService.WriteFileAsync(
                _storageContainerName,
                agentReference.Filename,
                JsonSerializer.Serialize(agent, agentReference.AgentType, _serializerSettings),
                default,
                default);

            _agentReferences.AddOrUpdate(agentReference.Name, agentReference, (k, v) => agentReference);

            await _storageService.WriteFileAsync(
                    _storageContainerName,
                    AGENT_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(AgentReferenceStore.FromDictionary(_agentReferences.ToDictionary())),
                    default,
                    default);

            return new ResourceProviderUpsertResult
            {
                ObjectId = (agent as AgentBase)!.ObjectId
            };
        }

        #endregion

        /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task<object> ExecuteActionAsync(ResourcePath resourcePath, string serializedAction) =>
            resourcePath.ResourceTypeInstances.Last().ResourceType switch
            {
                AgentResourceTypeNames.Agents => resourcePath.ResourceTypeInstances.Last().Action switch
                {
                    AgentResourceProviderActions.CheckName => CheckAgentName(serializedAction),
                    _ => throw new ResourceProviderException($"The action {resourcePath.ResourceTypeInstances.Last().Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                _ => throw new ResourceProviderException()
            };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        #region Helpers for ExecuteActionAsync

        private ResourceNameCheckResult CheckAgentName(string serializedAction)
        {
            var resourceName = JsonSerializer.Deserialize<ResourceName>(serializedAction);
            return _agentReferences.Values.Any(ar => ar.Name == resourceName!.Name)
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
        protected override async Task DeleteResourceAsync(ResourcePath resourcePath)
        {
            switch (resourcePath.ResourceTypeInstances.Last().ResourceType)
            {
                case AgentResourceTypeNames.Agents:
                    await DeleteAgent(resourcePath.ResourceTypeInstances);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances.Last().ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
        }

        #region Helpers for DeleteResourceAsync

        private async Task DeleteAgent(List<ResourceTypeInstance> instances)
        {
            if (_agentReferences.TryGetValue(instances.Last().ResourceId!, out var agentReference))
            {
                if (!agentReference.Deleted)
                {
                    agentReference.Deleted = true;

                    await _storageService.WriteFileAsync(
                        _storageContainerName,
                        AGENT_REFERENCES_FILE_PATH,
                        JsonSerializer.Serialize(AgentReferenceStore.FromDictionary(_agentReferences.ToDictionary())),
                        default,
                        default);
                }
            }
            else
            {
                throw new ResourceProviderException($"Could not locate the {instances.Last().ResourceId} agent resource.",
                    StatusCodes.Status404NotFound);
            }
        }

        #endregion

        #endregion

        #region Event handling

        /// <inheritdoc/>
        protected override async Task HandleEvents(EventSetEventArgs e)
        {
            _logger.LogInformation("{EventsCount} events received in the {EventsNamespace} events namespace.",
                e.Events.Count, e.Namespace);

            switch (e.Namespace)
            {
                case EventSetEventNamespaces.FoundationaLLM_ResourceProvider_Agent:
                    foreach (var @event in e.Events)
                        await HandleAgentResourceProviderEvent(@event);
                    break;
                default:
                    // Ignore sliently any event namespace that's of no interest.
                    break;
            }

            await Task.CompletedTask;
        }

        private async Task HandleAgentResourceProviderEvent(CloudEvent e)
        {
            if (string.IsNullOrWhiteSpace(e.Subject))
                return;

            var fileName = e.Subject.Split("/").Last();

            _logger.LogInformation("The file [{FileName}] managed by the [{ResourceProvider}] resource provider has changed and will be reloaded.",
                fileName, _name);

            var agentReference = new AgentReference
            {
                Name = Path.GetFileNameWithoutExtension(fileName),
                Filename = $"/{_name}/{fileName}",
                Type = AgentTypes.Basic,
                Deleted = false
            };

            var agent = await LoadAgent(agentReference);
            agentReference.Name = agent.Name;
            agentReference.Type = agent.Type;

            _agentReferences.AddOrUpdate(
                agentReference.Name,
                agentReference,
                (k, v) => v);

            _logger.LogInformation("The agent reference for the [{AgentName}] agent or type [{AgentType}] was loaded.",
                agentReference.Name, agentReference.Type);
        }

        #endregion
    }
}

using Azure.Messaging;
using FluentValidation;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Events;
using FoundationaLLM.Common.Models.ResourceProvider;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.DataSource.Constants;
using FoundationaLLM.DataSource.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.DataSource.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.DataSource resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationService"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to provide loggers for logging.</param>
    public class DataSourceResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IAuthorizationService authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProvider_DataSource)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        ILoggerFactory loggerFactory)
        : ResourceProviderServiceBase(
            instanceOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            loggerFactory.CreateLogger<DataSourceResourceProviderService>(),
            [
                EventSetEventNamespaces.FoundationaLLM_ResourceProvider_DataSource
            ])
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            DataSourceResourceProviderMetadata.AllowedResourceTypes;

        private ConcurrentDictionary<string, DataSourceReference> _dataSourceReferences = [];

        private const string DATA_SOURCE_REFERENCES_FILE_NAME = "_data-source-references.json";
        private const string DATA_SOURCE_REFERENCES_FILE_PATH = $"/{ResourceProviderNames.FoundationaLLM_DataSource}/_data-source-references.json";

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_DataSource;

        /// <inheritdoc/>
        protected override async Task InitializeInternal()
        {
            _logger.LogInformation("Starting to initialize the {ResourceProvider} resource provider...", _name);

            if (await _storageService.FileExistsAsync(_storageContainerName, DATA_SOURCE_REFERENCES_FILE_PATH, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, DATA_SOURCE_REFERENCES_FILE_PATH, default);
                var dataSourceReferenceStore = JsonSerializer.Deserialize<DataSourceReferenceStore>(
                    Encoding.UTF8.GetString(fileContent.ToArray()));

                _dataSourceReferences = new ConcurrentDictionary<string, DataSourceReference>(
                    dataSourceReferenceStore!.ToDictionary());
            }
            else
            {
                await _storageService.WriteFileAsync(
                    _storageContainerName,
                    DATA_SOURCE_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(new DataSourceReferenceStore { DataSourceReferences = [] }),
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
                DataSourceResourceTypeNames.DataSources => await LoadDataSources(resourcePath.ResourceTypeInstances[0]),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for GetResourcesAsyncInternal

        private async Task<List<DataSourceBase>> LoadDataSources(ResourceTypeInstance instance)
        {
            if (instance.ResourceId == null)
            {
                return
                [
                    .. (await Task.WhenAll(
                        _dataSourceReferences.Values
                            .Where(dsr => !dsr.Deleted)
                            .Select(dsr => LoadDataSource(dsr))))
                ];
            }
            else
            {
                if (!_dataSourceReferences.TryGetValue(instance.ResourceId, out var dataSourceReference)
                    || dataSourceReference.Deleted)
                    throw new ResourceProviderException($"Could not locate the {instance.ResourceId} data source resource.",
                        StatusCodes.Status404NotFound);

                var dataSource = await LoadDataSource(dataSourceReference!);

                return [dataSource];
            }
        }

        private async Task<DataSourceBase> LoadDataSource(DataSourceReference dataSourceReference)
        {
            if (await _storageService.FileExistsAsync(_storageContainerName, dataSourceReference.Filename, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, dataSourceReference.Filename, default);
                return JsonSerializer.Deserialize(
                    Encoding.UTF8.GetString(fileContent.ToArray()),
                    dataSourceReference.DataSourceType,
                    _serializerSettings) as DataSourceBase
                    ?? throw new ResourceProviderException($"Failed to load the data source {dataSourceReference.Name}.",
                        StatusCodes.Status400BadRequest);
            }
            else
                throw new ResourceProviderException($"Could not locate the {dataSourceReference.Name} data source resource.",
                    StatusCodes.Status404NotFound);
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task<object> UpsertResourceAsync(ResourcePath resourcePath, string serializedResource) =>
            resourcePath.ResourceTypeInstances[0].ResourceType switch
            {
                DataSourceResourceTypeNames.DataSources => await UpdateDataSource(resourcePath, serializedResource),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for UpsertResourceAsync

        private async Task<ResourceProviderUpsertResult> UpdateDataSource(ResourcePath resourcePath, string serializedDataSource)
        {
            var dataSourceBase = JsonSerializer.Deserialize<DataSourceBase>(serializedDataSource)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            if (_dataSourceReferences.TryGetValue(dataSourceBase.Name!, out var existingDataSourceReference)
                && existingDataSourceReference!.Deleted)
                throw new ResourceProviderException($"The data source resource {existingDataSourceReference.Name} cannot be added or updated.",
                        StatusCodes.Status400BadRequest);

            if (resourcePath.ResourceTypeInstances[0].ResourceId != dataSourceBase.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            var dataSourceReference = new DataSourceReference
            {
                Name = dataSourceBase.Name!,
                Type = dataSourceBase.Type!,
                Filename = $"/{_name}/{dataSourceBase.Name}.json",
                Deleted = false
            };

            var dataSource = JsonSerializer.Deserialize(serializedDataSource, dataSourceReference.DataSourceType, _serializerSettings);
            (dataSource as DataSourceBase)!.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);

            var validator = _resourceValidatorFactory.GetValidator(dataSourceReference.DataSourceType);
            if (validator is IValidator dataSourceValidator)
            {
                var context = new ValidationContext<object>(dataSource);
                var validationResult = await dataSourceValidator.ValidateAsync(context);
                if (!validationResult.IsValid)
                {
                    throw new ResourceProviderException($"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}",
                        StatusCodes.Status400BadRequest);
                }
            }

            await _storageService.WriteFileAsync(
                _storageContainerName,
                dataSourceReference.Filename,
                JsonSerializer.Serialize(dataSource, dataSourceReference.DataSourceType, _serializerSettings),
                default,
                default);

            _dataSourceReferences.AddOrUpdate(dataSourceReference.Name, dataSourceReference, (k, v) => dataSourceReference);

            await _storageService.WriteFileAsync(
                    _storageContainerName,
                    DATA_SOURCE_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(DataSourceReferenceStore.FromDictionary(_dataSourceReferences.ToDictionary())),
                    default,
                    default);

            return new ResourceProviderUpsertResult
            {
                ObjectId = (dataSource as DataSourceBase)!.ObjectId
            };
        }

        #endregion

        /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task<object> ExecuteActionAsync(ResourcePath resourcePath, string serializedAction) =>
            resourcePath.ResourceTypeInstances.Last().ResourceType switch
            {
                DataSourceResourceTypeNames.DataSources => resourcePath.ResourceTypeInstances.Last().Action switch
                {
                    DataSourceResourceProviderActions.CheckName => CheckDataSourceName(serializedAction),
                    _ => throw new ResourceProviderException($"The action {resourcePath.ResourceTypeInstances.Last().Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                _ => throw new ResourceProviderException()
            };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        #region Helpers for ExecuteActionAsync

        private ResourceNameCheckResult CheckDataSourceName(string serializedAction)
        {
            var resourceName = JsonSerializer.Deserialize<ResourceName>(serializedAction);
            return _dataSourceReferences.Values.Any(ar => ar.Name == resourceName!.Name)
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
                case DataSourceResourceTypeNames.DataSources:
                    await DeleteDataSource(resourcePath.ResourceTypeInstances);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances.Last().ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
        }

        #region Helpers for DeleteResourceAsync

        private async Task DeleteDataSource(List<ResourceTypeInstance> instances)
        {
            if (_dataSourceReferences.TryGetValue(instances.Last().ResourceId!, out var dataSourceReference))
            {
                if (!dataSourceReference.Deleted)
                {
                    dataSourceReference.Deleted = true;

                    await _storageService.WriteFileAsync(
                        _storageContainerName,
                        DATA_SOURCE_REFERENCES_FILE_PATH,
                        JsonSerializer.Serialize(DataSourceReferenceStore.FromDictionary(_dataSourceReferences.ToDictionary())),
                        default,
                        default);
                }
            }
            else
            {
                throw new ResourceProviderException($"Could not locate the {instances.Last().ResourceId} data source resource.",
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
                case EventSetEventNamespaces.FoundationaLLM_ResourceProvider_DataSource:
                    foreach (var @event in e.Events)
                        await HandleDataSourceResourceProviderEvent(@event);
                    break;
                default:
                    // Ignore sliently any event namespace that's of no interest.
                    break;
            }

            await Task.CompletedTask;
        }

        private async Task HandleDataSourceResourceProviderEvent(CloudEvent e)
        {
            if (string.IsNullOrWhiteSpace(e.Subject))
                return;

            var fileName = e.Subject.Split("/").Last();

            _logger.LogInformation("The file [{FileName}] managed by the [{ResourceProvider}] resource provider has changed and will be reloaded.",
                fileName, _name);

            var dataSourceReference = new DataSourceReference
            {
                Name = Path.GetFileNameWithoutExtension(fileName),
                Filename = $"/{_name}/{fileName}",
                Type = DataSourceTypes.Basic,
                Deleted = false
            };

            var dataSource = await LoadDataSource(dataSourceReference);
            dataSourceReference.Name = dataSource.Name;
            dataSourceReference.Type = dataSource.Type!;

            _dataSourceReferences.AddOrUpdate(
                dataSourceReference.Name,
                dataSourceReference,
                (k, v) => v);

            _logger.LogInformation("The data source reference for the [{DataSourceName}] agent or type [{DataSourceType}] was loaded.",
                dataSourceReference.Name, dataSourceReference.Type);
        }

        #endregion
    }
}

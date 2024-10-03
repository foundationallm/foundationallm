﻿using Azure.Messaging;
using FluentValidation;
using FoundationaLLM.Attachment.Models;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.Events;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using FoundationaLLM.Common.Services.ResourceProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FoundationaLLM.Attachment.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Attachment resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationService"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to provide loggers for logging.</param>
    public class AttachmentResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IAuthorizationService authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Attachment)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory)
        : ResourceProviderServiceBase<AttachmentReference>(
            instanceOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            loggerFactory.CreateLogger<AttachmentResourceProviderService>(),
            [
                EventSetEventNamespaces.FoundationaLLM_ResourceProvider_Attachment
            ],
            useInternalReferencesStore: true)
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            AttachmentResourceProviderMetadata.AllowedResourceTypes;

        protected override string _name => ResourceProviderNames.FoundationaLLM_Attachment;

        protected override async Task InitializeInternal() =>
            await Task.CompletedTask;

        #region Resource provider support for Management API

        /// <inheritdoc/>
        protected override async Task<object> GetResourcesAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderLoadOptions? options = null) =>
            resourcePath.MainResourceTypeName switch
            {
                AttachmentResourceTypeNames.Attachments =>
                    // Attachments have a custom implementation for loading resources.
                    await LoadResources<AttachmentFile>(
                        resourcePath.ResourceTypeInstances[0],
                        authorizationResult,
                        options ?? new ResourceProviderLoadOptions
                        {
                            IncludeRoles = resourcePath.IsResourceTypePath,
                            LoadContent = false
                        },
                        LoadAttachment),
                _ =>
                    throw new ResourceProviderException($"The resource type {resourcePath.MainResourceTypeName} is not supported by the {_name} resource provider.",
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
                AttachmentResourceTypeNames.Attachments => resourcePath.Action switch
                {
                    ResourceProviderActions.Filter =>
                        // Attachments have a custom implementation for loading resources.
                        await FilterResources<AttachmentFile>(
                            resourcePath,
                            JsonSerializer.Deserialize<ResourceFilter>(serializedAction)!,
                            authorizationResult,
                            new ResourceProviderLoadOptions
                            {
                                LoadContent = false,
                                IncludeRoles = false
                            },
                            LoadAttachment),
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
                case AttachmentResourceTypeNames.Attachments:
                    await DeleteResource<AttachmentFile>(resourcePath);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
        }

        #endregion

        #region Resource provider strongly typed operations

        /// <inheritdoc/>
        protected override async Task<T> GetResourceAsyncInternal<T>(ResourcePath resourcePath, UnifiedUserIdentity userIdentity, ResourceProviderLoadOptions? options = null) where T : class
        {
            var attachmentReference = await _resourceReferenceStore!.GetResourceReference(resourcePath.ResourceTypeInstances[0].ResourceId!)
                ?? throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.MainResourceTypeName} was not found.");

            return (await LoadAttachment(attachmentReference, loadContent: options?.LoadContent ?? false)) as T
                ?? throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.MainResourceTypeName} could not be loaded.");
        }

        /// <inheritdoc/>
        protected override async Task<TResult> UpsertResourceAsyncInternal<T, TResult>(ResourcePath resourcePath, T resource, UnifiedUserIdentity userIdentity) =>
            resource switch
            {
                AttachmentFile attachment => (TResult) await UpdateAttachment(resourcePath, attachment),
                _ => throw new ResourceProviderException(
                    $"The type {nameof(T)} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #endregion

        #region Event handling

        /// <inheritdoc/>
        protected override async Task HandleEvents(EventSetEventArgs e)
        {
            _logger.LogInformation("{EventsCount} events received in the {EventsNamespace} events namespace.",
                e.Events.Count, e.Namespace);

            switch (e.Namespace)
            {
                case EventSetEventNamespaces.FoundationaLLM_ResourceProvider_Attachment:
                    foreach (var @event in e.Events)
                        await HandleAttachmentResourceProviderEvent(@event);
                    break;
                default:
                    // Ignore sliently any event namespace that's of no interest.
                    break;
            }

            await Task.CompletedTask;
        }

        private async Task HandleAttachmentResourceProviderEvent(CloudEvent e) => await Task.CompletedTask;

        #endregion

        #region Resource management

        private async Task<AttachmentFile> LoadAttachment(AttachmentReference attachmentReference, bool loadContent = false)
        {
            var attachmentFile = new AttachmentFile
            {
                ObjectId = attachmentReference.ObjectId,
                Name = attachmentReference.Name,
                OriginalFileName = attachmentReference.OriginalFilename,
                Type = attachmentReference.Type,
                Path = $"{_storageContainerName}{attachmentReference.Filename}",
                ContentType = attachmentReference.ContentType,
                SecondaryProvider = attachmentReference.SecondaryProvider
            };

            if (loadContent)
            {
                var fileContent = await _storageService.ReadFileAsync(
                    _storageContainerName,
                    attachmentReference.Filename,
                    default);
                attachmentFile.Content = fileContent.ToArray();
            }

            return attachmentFile;
        }

        private async Task<ResourceProviderUpsertResult> UpdateAttachment(ResourcePath resourcePath, AttachmentFile attachment)
        {
            if (resourcePath.ResourceTypeInstances[0].ResourceId != attachment.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            var extension = GetFileExtension(attachment.DisplayName!);
            var fullName = $"{attachment.Name}{extension}";

            attachment.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);
            var attachmentReference = new AttachmentReference
            {
                ObjectId = attachment.ObjectId,
                OriginalFilename = attachment.DisplayName!,
                ContentType = attachment.ContentType!,
                Name = attachment.Name,
                Type = AttachmentTypes.File,
                Filename = $"/{_name}/{fullName}",
                Size = attachment.Content!.Length,
                SecondaryProvider = attachment.SecondaryProvider,
                Deleted = false
            };

            var validator = _resourceValidatorFactory.GetValidator(typeof(AttachmentFile));
            if (validator is IValidator attachmentValidator)
            {
                var context = new ValidationContext<object>(attachment);
                var validationResult = await attachmentValidator.ValidateAsync(context);
                if (!validationResult.IsValid)
                {
                    throw new ResourceProviderException($"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}",
                        StatusCodes.Status400BadRequest);
                }
            }

            await CreateResource(
                attachmentReference,
                new MemoryStream(attachment.Content!),
                attachment.ContentType ?? default);

            return new ResourceProviderUpsertResult
            {
                ObjectId = (attachment as AttachmentFile)!.ObjectId,
                ResourceExists = false
            };
        }

        private string GetFileExtension(string fileName) =>
            Path.GetExtension(fileName);

        #endregion
    }
}

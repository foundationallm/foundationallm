﻿using FluentValidation;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.Events;
using FoundationaLLM.Common.Constants.Plugins;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Common.Models.Plugins.Metadata;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Plugin;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.Plugin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using NuGet.Packaging;
using NuGet.Versioning;
using System.Runtime.Loader;
using System.Text.Json;

namespace FoundationaLLM.Plugin.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Plugin resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>    
    /// <param name="cacheOptions">The options providing the <see cref="ResourceProviderCacheSettings"/> with settings for the resource provider cache.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationServiceClient"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="loggerFactory">The factory responsible for creating loggers.</param>    
    public class PluginResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IOptions<ResourceProviderCacheSettings> cacheOptions,
        IAuthorizationServiceClient authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Plugin_Storage)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory)
        : ResourceProviderServiceBase<PluginReference>(
            instanceOptions.Value,
            cacheOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            loggerFactory.CreateLogger<PluginResourceProviderService>(),
            [
                EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand
            ],
            useInternalReferencesStore: true)
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            PluginResourceProviderMetadata.AllowedResourceTypes;

        protected override string _name => ResourceProviderNames.FoundationaLLM_Plugin;

        protected override async Task InitializeInternal() =>
            await Task.CompletedTask;

        #region Resource provider support for Management API

        /// <inheritdoc/>
        protected override async Task<object> GetResourcesAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options = null) =>
            resourcePath.ResourceTypeName switch
            {
                PluginResourceTypeNames.PluginPackages => await LoadResources<PluginPackageDefinition>(
                    resourcePath.ResourceTypeInstances[0],
                    authorizationResult,
                    options ?? new ResourceProviderGetOptions
                    {
                        IncludeRoles = resourcePath.IsResourceTypePath
                    }),
                PluginResourceTypeNames.Plugins => await LoadResources<PluginDefinition>(
                    resourcePath.ResourceTypeInstances[0],
                    authorizationResult,
                    options ?? new ResourceProviderGetOptions
                    {
                        IncludeRoles = resourcePath.IsResourceTypePath
                    }),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
            };

        /// <inheritdoc/>
        protected override async Task<object> UpsertResourceAsync(
            ResourcePath resourcePath,
            string? serializedResource,
            ResourceProviderFormFile? formFile,
            UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeName switch
            {
                PluginResourceTypeNames.PluginPackages => await UpdatePluginPackage(
                    resourcePath,
                    serializedResource!,
                    formFile,
                    userIdentity),
                _ => throw new ResourceProviderException(
                    $"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        protected override async Task<object> ExecuteActionAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction,
            UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeName switch
            {
                PluginResourceTypeNames.Plugins =>
                    resourcePath.Action switch
                    {
                        ResourceProviderActions.Filter => await FilterPlugins(
                            resourcePath,
                            authorizationResult,
                            serializedAction,
                            userIdentity),
                        _ => throw new ResourceProviderException(
                                $"The resource type {resourcePath.ResourceTypeName} and action {resourcePath.Action!} are not supported by the {_name} resource provider.",
                                StatusCodes.Status400BadRequest)
                    },
                _ => throw new ResourceProviderException(
                        $"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
            };

        #region Helpers for UpsertResourceAsync

        private async Task<ResourceProviderUpsertResult> UpdatePluginPackage(
            ResourcePath resourcePath,
            string serializedPluginPackage,
            ResourceProviderFormFile? formFile,
            UnifiedUserIdentity userIdentity)
        {
            var pluginPackageBase = JsonSerializer.Deserialize<ResourceBase>(serializedPluginPackage)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            if (resourcePath.MainResourceId != pluginPackageBase.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            var pluginPackage = await GetPluginPackage(
                resourcePath,
                pluginPackageBase,
                formFile,
                userIdentity);
            pluginPackage.ObjectId = resourcePath.ObjectId
                ?? throw new ResourceProviderException("The resource path cannot be converted to a valid resource object identifier.",
                    StatusCodes.Status400BadRequest);

            var pluginPackageReference = new PluginReference
            {
                Name = pluginPackage.Name!,
                Type = pluginPackage.Type!,
                Filename = $"/{_name}/{pluginPackage.Name}.json",
                Deleted = false
            };
            var existingPluginPackageReference = await _resourceReferenceStore!.GetResourceReference(pluginPackageBase.Name);

            var validator = _resourceValidatorFactory.GetValidator(pluginPackageReference.ResourceType);
            if (validator is IValidator pluginPackageValidator)
            {
                var context = new ValidationContext<object>(pluginPackage);
                var validationResult = await pluginPackageValidator.ValidateAsync(context);
                if (!validationResult.IsValid)
                    throw new ResourceProviderException($"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}",
                        StatusCodes.Status400BadRequest);
            }

            UpdateBaseProperties(pluginPackage, userIdentity, isNew: existingPluginPackageReference is null);
            if (existingPluginPackageReference is null)
                await CreateResource<PluginPackageDefinition>(pluginPackageReference, pluginPackage);
            else
                await SaveResource<PluginPackageDefinition>(pluginPackageReference, pluginPackage);

            await _storageService.WriteFileAsync(
                _storageContainerName,
                pluginPackage.PackageFilePath,
                new MemoryStream(formFile!.BinaryContent.ToArray()),
                formFile.ContentType ?? default,
                default);

            return new ResourceProviderUpsertResult
            {
                ObjectId = pluginPackage!.ObjectId,
                ResourceExists = existingPluginPackageReference is not null
            };
        }

        private async Task<PluginPackageDefinition> GetPluginPackage(
            ResourcePath resourcePath,
            ResourceBase pluginPackageBase,
            ResourceProviderFormFile? formFile,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                // Enforce the platform-based naming convention
                var platformName = pluginPackageBase.Name!.Split('-')[0];
                if (!Enum.TryParse<PluginPackagePlatform>(platformName, out var packagePlatform))
                    throw new ResourceProviderException("The plugin package name does not follow the platform-based naming convention or the platform is not supported.",
                        StatusCodes.Status400BadRequest);

                if (formFile == null || formFile.BinaryContent.Length == 0)
                    throw new ResourceProviderException("The attached plugin package is not valid.",
                        StatusCodes.Status400BadRequest);

                var packageVersion = new SemanticVersion(0, 0, 0);
                var packageMetadata = default(PluginPackageMetadata);

                if (packagePlatform == PluginPackagePlatform.Dotnet)
                {
                    using var packageReader = new PackageArchiveReader(new MemoryStream(formFile.BinaryContent.ToArray()));
                    var nuspecReader = await packageReader.GetNuspecReaderAsync(default);
                    packageVersion = nuspecReader.GetVersion();

                    foreach (var packagePath in await packageReader.GetFilesAsync("lib", default))
                    {
                        var assemblyStream = await packageReader.GetStreamAsync(packagePath, default);
                        var assemblyMemoryStream = new MemoryStream();
                        await assemblyStream.CopyToAsync(assemblyMemoryStream);
                        assemblyMemoryStream.Seek(0, SeekOrigin.Begin);

                        var assemblyLoadContext = new AssemblyLoadContext(packagePath, isCollectible: true);
                        var assembly = assemblyLoadContext.LoadFromStream(assemblyMemoryStream);

                        var pluginPackageManagerType = assembly.GetTypes()
                            .FirstOrDefault(t => (typeof(IPluginPackageManager)).IsAssignableFrom(t));

                        if (pluginPackageManagerType is not null)
                        {
                            var pluginPackageManager = assembly.CreateInstance(pluginPackageManagerType.FullName!) as IPluginPackageManager;

                            if (pluginPackageManager is not null)
                            {
                                packageMetadata = pluginPackageManager.GetMetadata(
                                    resourcePath.InstanceId!);

                                assemblyLoadContext.Unload();
                                break;
                            }
                        }

                        assemblyLoadContext.Unload();
                    }
                }

                if (packageMetadata is null)
                    throw new ResourceProviderException("The plugin package does not have a valid package manager and/or package configuration.",
                        StatusCodes.Status400BadRequest);

                if (packageMetadata.Name != pluginPackageBase.Name)
                    throw new ResourceProviderException("The plugin package name does not match the name from the package configuration.",
                        StatusCodes.Status400BadRequest);

                if (packageMetadata.Platform != packagePlatform)
                    throw new ResourceProviderException("The plugin package platform does not match the platform from the package configuration.",
                        StatusCodes.Status400BadRequest);

                foreach (var pluginMetadata in packageMetadata.Plugins)
                {
                    var existingPluginReference = await _resourceReferenceStore!.GetResourceReference(pluginMetadata.Name);
                    var newPlugin = existingPluginReference is null;
                    var pluginReference = newPlugin
                        ? new PluginReference
                        {
                            Name = pluginMetadata.Name,
                            Type = PluginTypes.Plugin,
                            Filename = $"/{_name}/{pluginMetadata.Name}.json",
                            Deleted = false
                        }
                        : existingPluginReference!;

                    var plugin = new PluginDefinition
                    {
                        Type = PluginTypes.Plugin,
                        ObjectId = pluginMetadata.ObjectId,
                        Name = pluginMetadata.Name,
                        DisplayName = pluginMetadata.DisplayName,
                        Description = pluginMetadata.Description,
                        Category = pluginMetadata.Category,
                        Parameters = pluginMetadata.Parameters,
                        ParameterSelectionHints = pluginMetadata.ParameterSelectionHints,
                        Dependencies = pluginMetadata.Dependencies
                    };

                    UpdateBaseProperties(plugin, userIdentity, isNew: newPlugin);
                    if (newPlugin)
                        await CreateResource<PluginDefinition>(pluginReference, plugin);
                    else
                        await SaveResource<PluginDefinition>(pluginReference, plugin);
                }

                return new PluginPackageDefinition
                {
                    Type = pluginPackageBase.Type,
                    Name = packageMetadata.Name,
                    DisplayName = packageMetadata.DisplayName,
                    Description = packageMetadata.Description,
                    PackagePlatform = packagePlatform,
                    PackageVersion = packageVersion,
                    PackageFilePath = $"/{_name}/{resourcePath.InstanceId!}/{pluginPackageBase.Name}/{formFile.FileName}",
                    PackageFileSize = formFile.BinaryContent.Length
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse the plugin package definition.");
                throw new ResourceProviderException(
                    "The plugin package definition is invalid.",
                    StatusCodes.Status400BadRequest);
            }
        }

        private async Task<List<PluginDefinition>> FilterPlugins(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction,
            UnifiedUserIdentity userIdentity)
        {
            var pluginFilter = JsonSerializer.Deserialize<PluginFilter>(serializedAction)
                ?? throw new ResourceProviderException($"The serialized action cannot be deserialized.",
                    StatusCodes.Status400BadRequest);

            var allPlugins = await LoadResources<PluginDefinition>(
                resourcePath.ResourceTypeInstances.First(),
                authorizationResult);

            return allPlugins
                .Select(p => p.Resource)
                .Where(p => pluginFilter.Categories.Contains(p.Category, StringComparer.Ordinal))
                .ToList();
        }

        #endregion

        #endregion
    }
}

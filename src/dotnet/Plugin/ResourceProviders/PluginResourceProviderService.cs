using FluentValidation;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.Events;
using FoundationaLLM.Common.Constants.Plugins;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
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
using NuGet.Packaging;
using NuGet.Versioning;
using System.IO.Compression;
using System.Text;
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
    /// <param name="proxyMode">Indicates whether the resource provider is running in proxy mode.</param>
    public class PluginResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IOptions<ResourceProviderCacheSettings> cacheOptions,
        IAuthorizationServiceClient authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Plugin_Storage)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory,
        bool proxyMode = false)
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
            useInternalReferencesStore: true,
            proxyMode: proxyMode)
    {
        private readonly Dictionary<string, PluginPackageManagerInstance> _pluginPackageManagerInstances = [];

        private readonly Dictionary<string, Dictionary<string, string>> _pluginDependencies = [];

        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            PluginResourceProviderMetadata.AllowedResourceTypes;

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Plugin;

        /// <inheritdoc/>
        protected override async Task InitializeInternal()
        {
            var files = await _storageService.GetFilePathsAsync(
                _storageContainerName,
                $"/{_name}/{_instanceSettings.Id}",
                recursive: true,
                default);

            var dependencyFiles = files
                .Where(f => f.EndsWith(".nupkg.json", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            foreach (var dependencyFile in dependencyFiles)
            {
                var pluginPackageBinaryDependencies = await _storageService.ReadFileAsync(
                    _storageContainerName,
                    dependencyFile,
                    default);

                var pluginPackageDependencies = JsonSerializer.Deserialize<Dictionary<string, string>>(
                    pluginPackageBinaryDependencies.ToArray());

                var dependencyFileTokens = dependencyFile.Split('/');
                _pluginDependencies[dependencyFileTokens[2]] = pluginPackageDependencies ?? [];
            }
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
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            Func<object, bool>? requestPayloadValidator = null) =>

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
            UnifiedUserIdentity userIdentity,
            Func<object, bool>? requestPayloadValidator = null) =>
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

            var pluginPackage = await GetPluginPackageDefinition(
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

        private async Task<PluginPackageDefinition> GetPluginPackageDefinition(
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

                if (formFile == null || formFile.BinaryContent.ToMemory().Length == 0)
                    throw new ResourceProviderException("The attached plugin package is not valid.",
                        StatusCodes.Status400BadRequest);

                var packageVersion = new SemanticVersion(0, 0, 0);
                var packageMetadata = default(PluginPackageMetadata);

                if (packagePlatform == PluginPackagePlatform.Dotnet)
                {
                    var packageManagerInstance = await LoadDotnetPackage(
                        resourcePath.ResourceId!,
                        new MemoryStream(formFile.BinaryContent.ToArray()));
                    packageVersion = packageManagerInstance.PackageVersion;
                    packageMetadata = packageManagerInstance.Instance.GetMetadata(resourcePath.InstanceId!);

                    packageManagerInstance.AssemblyLoadContext.Unload();
                } else if (packagePlatform == PluginPackagePlatform.Python)
                {
                    (packageVersion, packageMetadata) = await LoadPythonPackage(
                        resourcePath.ResourceId!,
                        new MemoryStream(formFile.BinaryContent.ToArray()));
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
                        PluginPackageObjectId = resourcePath.ObjectId!,
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
                    PackageFileSize = formFile.BinaryContent.ToMemory().Length,
                    Properties = packageMetadata.Properties
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

            return [.. allPlugins
                .Select(p => p.Resource)
                .Where(p => pluginFilter.Categories.Contains(p.Category, StringComparer.Ordinal))];
        }

        #endregion

        #endregion

        #region Resource provider strongly typed operations

        /// <inheritdoc/>
        protected override async Task<T> GetResourceAsyncInternal<T>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options = null,
            ResourceBase? parentResourceInstance = null) =>
            typeof(T) switch
            {
                Type t when t == typeof(PluginDefinition) =>
                    ((await LoadResource<PluginDefinition>(resourcePath.MainResourceId!)) as T)!,
                _ => throw new ResourceProviderException(
                        $"The resource type {typeof(T).Name} is not supported by the {_name} resource provider.",
                            StatusCodes.Status400BadRequest)
            };

        /// <inheritdoc/>
        protected override async Task<TResult> ExecuteResourceActionAsyncInternal<T, TAction, TResult>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            TAction actionPayload,
            UnifiedUserIdentity userIdentity) =>
            typeof(T) switch
            {
                Type t when t == typeof(PluginPackageDefinition) =>
                    resourcePath.Action! switch
                    {
                        ResourceProviderActions.LoadPluginPackage => (
                            (await CreatePluginPackageManagerInstanceResult(resourcePath)) as TResult)!,
                        _ => throw new ResourceProviderException(
                                $"The resource type {typeof(T).Name} and action {resourcePath.Action!} are not supported by the {_name} resource provider.",
                                    StatusCodes.Status400BadRequest)
                    },
                _ => throw new ResourceProviderException(
                        $"The resource type {typeof(T).Name} is not supported by the {_name} resource provider.",
                            StatusCodes.Status400BadRequest)
            };

        #region Helpers for strongly typed operations

        private async Task<ResourceProviderActionResult<PluginPackageManagerInstance>> CreatePluginPackageManagerInstanceResult(
            ResourcePath resourcePath)
        {
            var resource = await CreatePluginPackageManagerInstance(resourcePath.MainResourceId!);

            return new ResourceProviderActionResult<PluginPackageManagerInstance>(
                resourcePath.ObjectIdWithoutAction!,
                true)
            {
                Resource = resource
            };
        }

        private async Task<PluginPackageManagerInstance> CreatePluginPackageManagerInstance(
            string pluginPackageName)
        {
            // Check if the package manager instance is already cached and return it if so.
            if (_pluginPackageManagerInstances.TryGetValue(pluginPackageName, out var cachedPackageManagerInstance))
                return cachedPackageManagerInstance;

            var existingPluginPackageReference =
                await _resourceReferenceStore!.GetResourceReference(pluginPackageName);

            var pluginPackageDefinition =
                await LoadResource<PluginPackageDefinition>(existingPluginPackageReference!);

            if (pluginPackageDefinition!.PackagePlatform != PluginPackagePlatform.Dotnet)
                throw new ResourceProviderException("The plugin package platform is not supported.",
                    StatusCodes.Status400BadRequest);

            var pluginPackageBinaryContent = await _storageService.ReadFileAsync(
                _storageContainerName,
                pluginPackageDefinition!.PackageFilePath,
                default);

            var packageManagerInstance = await LoadDotnetPackage(
                pluginPackageName,
                new MemoryStream(pluginPackageBinaryContent.ToArray()));

            // Add the newly created package manager instance to the cache.
            _pluginPackageManagerInstances[pluginPackageName] = packageManagerInstance;

            return packageManagerInstance;
        }

        private async Task<PluginPackageManagerInstance> LoadDotnetPackage(
            string packageName,
            MemoryStream packageBinaryContent)
        {
            using var packageReader = new PackageArchiveReader(packageBinaryContent);
            var nuspecReader = await packageReader.GetNuspecReaderAsync(default);
            var packageVersion = nuspecReader.GetVersion();
            //var packageId = nuspecReader.GetId();

            foreach (var packagePath in await packageReader.GetFilesAsync("lib", default))
            {
                if (packagePath.EndsWith(".dll"))
                {
                    var assemblyStream = await packageReader.GetStreamAsync(packagePath, default);
                    var assemblyMemoryStream = new MemoryStream();
                    await assemblyStream.CopyToAsync(assemblyMemoryStream);
                    assemblyMemoryStream.Seek(0, SeekOrigin.Begin);

                    var assemblyLoadContext = new PluginLoadContext(
                        packageName,
                        true,
                        _pluginDependencies,
                        _serviceProvider.GetRequiredService<ILogger<PluginLoadContext>>(),
                        $"/{_name}/{_instanceSettings.Id}",
                        _storageService,
                        _storageContainerName);
                    var assembly = assemblyLoadContext.LoadFromStream(assemblyMemoryStream);

                    var pluginPackageManagerType = assembly.GetTypes()
                        .FirstOrDefault(t => (typeof(IPluginPackageManager)).IsAssignableFrom(t));

                    if (pluginPackageManagerType is not null)
                    {
                        var pluginPackageManager = assembly.CreateInstance(pluginPackageManagerType.FullName!) as IPluginPackageManager;

                        if (pluginPackageManager is not null)
                        {
                            return new PluginPackageManagerInstance
                            {
                                PackageVersion = packageVersion,
                                Instance = pluginPackageManager,
                                AssemblyLoadContext = assemblyLoadContext,
                                Name = packageName
                            };
                        }
                    }
                }
            }

            throw new ResourceProviderException("The plugin package does not have a valid package manager that can be instantiated.",
                StatusCodes.Status400BadRequest);
        }

        private async Task<(SemanticVersion Vestion, PluginPackageMetadata Metadata)> LoadPythonPackage(
            string packageName,
            MemoryStream packageBinaryContent)
        {
            using var archive = new ZipArchive(packageBinaryContent, ZipArchiveMode.Read, leaveOpen: true);

            var foundationallmManifestText = await GetTextFileContent(archive, "foundationallm_manifest.json");
            var topLevelText = await GetTextFileContent(archive, "top_level.txt");
            var metadata = await GetTextFileContent(archive, "METADATA");
            
            var moduleName = topLevelText
                .Split(Environment.NewLine)
                .First()
                .Trim();

            var metadataVersionLine = metadata
                .Split(Environment.NewLine)
                .Single(s => s.StartsWith("Version:"));
            var versionString = metadataVersionLine
                .Split(':', StringSplitOptions.TrimEntries)[1];    

            using var foundationallmManifestDoc = JsonDocument.Parse(foundationallmManifestText);

            if (!foundationallmManifestDoc.RootElement.TryGetProperty("display_name", out var displayNameProperty)
                || displayNameProperty.ValueKind != JsonValueKind.String)
                throw new ResourceProviderException("The python package manifest does not contain a valid 'display_name' property.",
                    StatusCodes.Status400BadRequest);

            if (!foundationallmManifestDoc.RootElement.TryGetProperty("description", out var descriptionPropery)
                || descriptionPropery.ValueKind != JsonValueKind.String)
                throw new ResourceProviderException("The python package manifest does not contain a valid 'description' property.",
                    StatusCodes.Status400BadRequest);

            if (!foundationallmManifestDoc.RootElement.TryGetProperty("plugin_managers", out var pluginManagersProperty)
                || pluginManagersProperty.ValueKind != JsonValueKind.Array)
                throw new ResourceProviderException("The python package manifest does not contain a valid 'plugin_managers' property.",
                    StatusCodes.Status400BadRequest);

            var version = GetVersion(versionString);
            var pluginManagers = pluginManagersProperty.EnumerateArray()
                .Select(pm => pm.GetString())
                .ToList();

            var packageMetadata = new PluginPackageMetadata
            {
                Name = packageName,
                DisplayName = displayNameProperty.ToString(),
                Description = descriptionPropery.ToString(),
                Platform = PluginPackagePlatform.Python,
                Plugins = [],
                Properties = new Dictionary<string, string>
                {
                    { "module_name", moduleName },
                    { "plugin_managers", string.Join(",", pluginManagers) }
                }
            };

            return (version, packageMetadata);
        }

        private async Task<string> GetTextFileContent(
            ZipArchive archive,
            string fileName)
        {
            // Find candidate entries with the manifest filename, prefer entries with smallest path depth (topmost).
            var candidates = archive.Entries
                .Select(e => new
                {
                    Entry = e,
                    Depth = e.FullName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Length
                })
                .Where(x => string.Equals(Path.GetFileName(x.Entry.FullName), fileName, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Depth)
                .ToArray();

            if (candidates.Length == 0)
                throw new ResourceProviderException($"The python package does not contain the {fileName} file.",
                    StatusCodes.Status400BadRequest);

            var fileEntry = candidates.First().Entry;

            using var entryStream = fileEntry.Open();
            using var ms = new MemoryStream();
            await entryStream.CopyToAsync(ms);
            ms.Seek(0, SeekOrigin.Begin);

            var fileContent = Encoding.UTF8.GetString(ms.ToArray());
            return fileContent;
        }

        private SemanticVersion GetVersion(string pythonVersion) =>
            // Supports only specific Python formats:
            // 1. X.Y.Z
            // 2. X.Y.ZrcN
            // 3. X.Y.ZaN
            // 4. X.Y.ZbN
            // 5/ X.Y.Z.postN
            SemanticVersion.Parse(pythonVersion
                .Replace("rc", "-rc")
                .Replace("a", "-alpha")
                .Replace("b", "-beta")
                .Replace(".post", "-post"));

        #endregion

        #endregion
    }
}

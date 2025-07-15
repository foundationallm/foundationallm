using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using Microsoft.Extensions.Logging;
using NuGet.Packaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace FoundationaLLM.Plugin.ResourceProviders
{
    /// <summary>
    /// Represents a custom assembly load context for loading plugins.
    /// </summary>
    /// <remarks>This class extends <see cref="AssemblyLoadContext"/> to provide a mechanism for loading
    /// assemblies in isolation, which is useful for plugin architectures where different versions of the same assembly
    /// might need to be loaded simultaneously.</remarks>
    public class PluginLoadContext : AssemblyLoadContext
    {
        private readonly Dictionary<string, Dictionary<string, string>> _pluginDependencies;
        private readonly ILogger<PluginLoadContext> _logger;
        private readonly string _rootDependencyPackagePath;
        private readonly IStorageService _storageService;
        private readonly string _storageContainerName;

        /// <summary>
        /// 
        /// </summary>
        public PluginLoadContext(
            string name,
            bool isCollectible,
            Dictionary<string, Dictionary<string, string>> pluginDependencies,
            ILogger<PluginLoadContext> logger,
            string rootDependencyPackagePath,
            IStorageService storageService,
            string storageContainerName
            ) : base(name, isCollectible)
        {
            _pluginDependencies = pluginDependencies;
            _logger = logger;
            _rootDependencyPackagePath = rootDependencyPackagePath;
            _storageService = storageService;
            _storageContainerName = storageContainerName;

            this.Resolving += LoadDependencyAssembly;
            this.ResolvingUnmanagedDll += LoadDependencyDLL;
        }

        private IntPtr LoadDependencyDLL(Assembly assembly, string dllName)
        {
            try
            {
                var dllNameWithoutExtension = Path.GetFileNameWithoutExtension(dllName);

                if (!_pluginDependencies.TryGetValue(Name!, out var dependencies))
                {
                    _logger.LogError("No dependencies found for assembly load context {AssemblyLoadContextName} when trying to load {AssemblyName}",
                        Name!, assembly.FullName);
                    return IntPtr.Zero;
                }

                if (!dependencies.TryGetValue(dllNameWithoutExtension, out var dependency))
                {
                    _logger.LogError("No dependencies found for DLL {DLLName} in load context {AssemblyLoadContextName}.",
                        dllName, Name!);
                    return IntPtr.Zero;
                }

                var dependencyTokens = dependency.Split('|');
                var dependencyPackagePath = $"{_rootDependencyPackagePath}/{dependencyTokens[0]}";
                var dependencyFilePath = $"{dependencyTokens[1]}/{dllName}";

                var dependencyBinaryContent = _storageService.ReadFile(
                    _storageContainerName,
                    dependencyPackagePath);

                using var packageReader = new PackageArchiveReader(
                    new MemoryStream(dependencyBinaryContent.ToArray()));

                var dllStream = packageReader.GetStream(dependencyFilePath);
                string tempPath = Path.Combine(Path.GetTempPath(), dllName);

                // Write stream to file
                using (var file = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
                {
                    dllStream.CopyTo(file);
                }

                // Load the native library from file
                IntPtr handle = NativeLibrary.Load(tempPath);

                // Optionally delete file after load (risky if OS needs it resident)
                // File.Delete(tempPath);

                return handle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load dependency DLL {DLLName}", dllName);
                return IntPtr.Zero;
            }
        }

        private Assembly? LoadDependencyAssembly(
            AssemblyLoadContext assemblyLoadContext,
            AssemblyName assemblyName)
        {
            try
            {
                if (!_pluginDependencies.TryGetValue(Name!, out var dependencies))
                {
                    _logger.LogError("No dependencies found for assembly load context: {AssemblyLoadContextName} when trying to load {AssemblyName}",
                        assemblyLoadContext.Name, assemblyName.FullName);
                    return null;
                }

                if (!dependencies.TryGetValue(assemblyName.Name!, out var dependency))
                {
                    _logger.LogError("No dependency found for assembly: {AssemblyName} in load context: {AssemblyLoadContextName}",
                        assemblyName.Name, assemblyLoadContext.Name);
                    return null;
                }

                var dependencyTokens = dependency.Split('|');
                var dependencyPackagePath = $"{_rootDependencyPackagePath}/{dependencyTokens[0]}";
                var dependencyFilePath = $"{dependencyTokens[1]}/{assemblyName.Name!}.dll";

                var dependencyBinaryContent = _storageService.ReadFile(
                    _storageContainerName,
                    dependencyPackagePath);

                using var packageReader = new PackageArchiveReader(
                    new MemoryStream(dependencyBinaryContent.ToArray()));

                var assemblyStream = packageReader.GetStream(dependencyFilePath);
                var assemblyMemoryStream = new MemoryStream();
                assemblyStream.CopyTo(assemblyMemoryStream);
                assemblyMemoryStream.Seek(0, SeekOrigin.Begin);

                var assembly = assemblyLoadContext.LoadFromStream(assemblyMemoryStream);
                return assembly;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load dependency assembly: {AssemblyName}", assemblyName.Name);
                return null;
            }
        }
    }
}

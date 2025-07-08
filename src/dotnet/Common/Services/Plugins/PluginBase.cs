using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.Plugins.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Common.Services.Plugins
{
    /// <summary>
    /// Implements the basic functionality of a plugin.
    /// </summary>
    public class PluginBase
    {
        protected virtual string Name => string.Empty;
        protected readonly Dictionary<string, object> _pluginParameters;
        protected readonly IPluginPackageManager _packageManager;
        protected readonly PluginMetadata _pluginMetadata;
        protected readonly IServiceProvider _serviceProvider;

        protected readonly ILogger<PluginBase> _logger;

        /// <summary>
        /// Initializes the plugin with the specified parameters and package manager.
        /// </summary>
        /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
        /// <param name="packageManager">The package manager for the plugin.</param>
        /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
        public PluginBase(
            Dictionary<string, object> pluginParameters,
            IPluginPackageManager packageManager,
            IServiceProvider serviceProvider)
        {
            _pluginParameters = pluginParameters
                ?? throw new PluginException("The plugin parameters dictionary cannot be null.");
            _packageManager = packageManager
                ?? throw new PluginException("The plugin package manager cannot be null.");
            _pluginMetadata = _packageManager.GetMetadata(string.Empty).Plugins.SingleOrDefault(p => p.Name == Name)
                ?? throw new PluginException($"The plugin metadata for '{Name}' cannot be found.");
            _serviceProvider = serviceProvider
                ?? throw new PluginException("The service provider cannot be null.");

            if (_pluginMetadata.Parameters
                .Select(p => p.Name)
                .Intersect(_pluginParameters.Keys)
                .Count()
                    != _pluginMetadata.Parameters.Count)
                throw new PluginException($"The plugin is missing values for the following parameters: {(
                    string.Join(",", _pluginMetadata.Parameters
                                        .Where(p => !_pluginParameters.ContainsKey(p.Name))
                                        .Select(p => p.Name)))}");

            _logger = serviceProvider.GetRequiredService<ILogger<PluginBase>>();
        }
    }
}

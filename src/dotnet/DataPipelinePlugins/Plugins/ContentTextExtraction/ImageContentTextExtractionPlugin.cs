using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Services.Plugins;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.ContentTextExtraction
{
    /// <summary>
    /// Implements the Image Content Text Extraction Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    public class ImageContentTextExtractionPlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IServiceProvider serviceProvider)
        : PluginBase(pluginParameters, packageManager, serviceProvider), IContentTextExtractionPlugin
    {
        protected override string Name => PluginNames.IMAGE_CONTENTTEXTEXTRACTION;

        /// <inheritdoc/>
        public async Task<PluginResult<string>> ExtractText(BinaryData rawContent) =>
            throw new NotImplementedException();
    }
}

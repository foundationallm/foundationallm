using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Plugin;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Plugin.Models
{
    /// <summary>
    /// Provides a reference to a resource managed by the FoundationaLLM.Plugin resource provider.
    /// </summary>
    public class PluginReference: ResourceReference
    {
        /// <summary>
        /// The object type of the plugin.
        /// </summary>
        [JsonIgnore]
        public override Type ResourceType =>
            Type switch
            {
                PluginTypes.PluginPackage => typeof(PluginPackageDefinition),
                PluginTypes.Plugin => typeof(PluginDefinition),
                _ => throw new ResourceProviderException($"The plugin type {Type} is not supported.")
            };
    }
}

using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Vector;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Plugin.Models
{
    /// <summary>
    /// Provides a reference to a resource managed by the FoundationaLLM.Plugin resource provider.
    /// </summary>
    public class VectorReference: ResourceReference
    {
        /// <summary>
        /// The object type of the plugin.
        /// </summary>
        [JsonIgnore]
        public override Type ResourceType =>
            Type switch
            {
                VectorTypes.VectorDatabase => typeof(VectorDatabase),
                _ => throw new ResourceProviderException($"The vector type {Type} is not supported.")
            };
    }
}

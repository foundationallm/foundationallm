using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Configuration.Models
{
    /// <summary>
    /// Holds a reference to an api endpoint reference.
    /// </summary>
    public class APIEndpointReference : ResourceReference
    {
        /// <summary>
        /// The object type of the agent.
        /// </summary>
        [JsonIgnore]
        public override Type ResourceType =>
            Type switch
            {
                ConfigurationTypes.APIEndpointConfiguration => typeof(APIEndpointConfiguration),
                _ => throw new ResourceProviderException($"The resource type {Type} is not supported.")
            };
    }
}

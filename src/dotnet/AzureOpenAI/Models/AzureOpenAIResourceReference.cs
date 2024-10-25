using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI;
using System.Text.Json.Serialization;

namespace FoundationaLLM.AzureOpenAI.Models
{
    /// <summary>
    /// References an <see cref="AzureOpenAIResourceBase"/> resource managed by the FoundationaLLM.AzureOpenAI resource provider.
    /// </summary>
    public class AzureOpenAIResourceReference : ResourceReference
    {
        /// <summary>
        /// The object type of the resource.
        /// </summary>
        [JsonIgnore]
        public override Type ResourceType =>
            Type switch
            {
                AzureOpenAITypes.AssistantUserContext => typeof(AssistantUserContext),
                AzureOpenAITypes.FileUserContext => typeof(FileUserContext),
                _ => throw new ResourceProviderException($"The resource type {Type} is not supported.")
            };
    }
}

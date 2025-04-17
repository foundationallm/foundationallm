using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.AzureAI;
using System.Text.Json.Serialization;

namespace AzureAI.Models.Resources
{
    public class ProjectReference : ResourceReference
    {
        /// <summary>
        /// The object type of the agent.
        /// </summary>
        [JsonIgnore]
        public override Type ResourceType =>
            Type switch
            {              
                AzureAITypes.Project => typeof(AzureAIProject),
                _ => throw new ResourceProviderException($"The type {Type} is not supported.")
            };
    }
}

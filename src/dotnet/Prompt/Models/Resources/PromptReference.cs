using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Prompt.Models.Resources
{
    /// <summary>
    /// Provides details about a prompt.
    /// </summary>
    public class PromptReference : ResourceReference
    {
        /// <summary>
        /// The object type of the agent.
        /// </summary>
        [JsonIgnore]
        public Type PromptType =>
            Type switch
            {
                PromptTypes.Basic => typeof(PromptBase),
                PromptTypes.Multipart => typeof(MultipartPrompt),
                _ => throw new ResourceProviderException($"The prompt type {Type} is not supported.")
            };
    }
}

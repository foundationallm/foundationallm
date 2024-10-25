using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI
{
    /// <summary>
    /// Basic model for resources managed by the FoundationaLLM.AzureOpenAI resource manager.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(AssistantUserContext), AzureOpenAITypes.AssistantUserContext)]
    [JsonDerivedType(typeof(FileUserContext), AzureOpenAITypes.FileUserContext)]
    public class AzureOpenAIResourceBase : ResourceBase
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string? Type { get; set; }
    }
}

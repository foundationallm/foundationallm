using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Prompt
{
    /// <summary>
    /// Basic prompt.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(MultipartPrompt), "multipart")]
    public class PromptBase : ResourceBase
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string? Type { get; set; }
    }

}

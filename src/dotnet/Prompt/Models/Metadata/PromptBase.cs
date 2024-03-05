using System.Text.Json.Serialization;
using FoundationaLLM.Common.Models.ResourceProvider;

namespace FoundationaLLM.Prompt.Models.Metadata
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

using System.Text.Json.Serialization;

namespace FoundationaLLM.Context.Models
{
    /// <summary>
    /// Represents the files associated with a code session.
    /// </summary>
    public class CodeSessionFileStore
    {
        /// <summary>
        /// Gets or sets the list of items associated with the code session.
        /// </summary>
        [JsonPropertyName("value")]
        public List<CodeSessionFileStoreItem> Items { get; set; } = [];
    }
}

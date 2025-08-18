using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Represents the result of an upsert operation.
    /// </summary>
    public class ResourceProviderUpsertResult
    {
        /// <summary>
        /// The id of the object that was created or updated.
        /// </summary>
        [JsonPropertyName("object_id")]
        public required string ObjectId { get; set; }

        /// <summary>
        /// A flag denoting whether the upserted resource already exists.
        /// </summary>
        [JsonPropertyName("resource_exists")]
        public required bool ResourceExists { get; set; }

        /// <summary>
        /// Gets or sets the resource resulting from the upsert operation.
        /// </summary>
        /// <remarks>
        /// Each resource provider will decide whether to return the resource in the upsert result or not.
        /// </remarks>
        [JsonPropertyName("resource")]
        public object? Resource { get; set; }
    }
}

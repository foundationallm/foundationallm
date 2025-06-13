using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Represents the typed result of an upsert operation.
    /// </summary>
    public class ResourceProviderUpsertResult<T> : ResourceProviderUpsertResult
        where T : ResourceBase
    {
        /// <summary>
        /// Gets or sets the resource resulting from the upsert operation.
        /// </summary>
        /// <remarks>
        /// Each resource provider will decide whether to return the resource in the upsert result or not.
        /// </remarks>
        [JsonPropertyName("resource")]
        public new T? Resource { get; set; }

        /// <summary>
        /// Converts the upsert result to an action result.
        /// </summary>
        /// <returns>The action result created from the upsert result.</returns>
        public ResourceProviderActionResult<T> ToActionResult() =>
            new(ObjectId, true)
            {
                Resource = Resource
            };
    }
}

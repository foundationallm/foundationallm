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

        /// <summary>
        /// Converts the current instance to a <see cref="ResourceProviderUpsertResult{T}"/> object.
        /// </summary>
        /// <typeparam name="T">The type of the resource, which must derive from <see cref="ResourceBase"/>.</typeparam>
        /// <returns>A <see cref="ResourceProviderUpsertResult{T}"/> containing the object ID, resource existence status,  and
        /// the resource cast to the specified type <typeparamref name="T"/>.</returns>
        public ResourceProviderUpsertResult<T> ToResourceProviderUpsertResult<T>()
            where T : ResourceBase =>
            new()
            {
                ObjectId = this.ObjectId,
                ResourceExists = this.ResourceExists,
                Resource = this.Resource as T
            };
    }
}

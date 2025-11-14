using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Represents a collection of resources of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of resource in the collection.</typeparam>
    public class ResourceCollection<T> : ResourceBase
        where T : ResourceBase
    {
        /// <summary>
        /// Gets or sets the total number of pages
        /// </summary>
        [JsonPropertyName("total_pages_count")]
        public int TotalPagesCount { get; set; }

        /// <summary>
        /// Gets or sets the collection of resources of type <typeparamref name="T"/>.
        /// </summary>
        public IEnumerable<T> Resources { get; set; } = [];
    }
}

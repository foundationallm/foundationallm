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
        /// Gets or sets the collection of resources of type <typeparamref name="T"/>.
        /// </summary>
        public IEnumerable<T> Resources { get; set; } = [];
    }
}

using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Vectorization.Models.Resources
{
    /// <summary>
    /// Models the content of a resource store managed by the FoundationaLLM.Vectorization resource provider.
    /// </summary>
    public class ResourceStore<T> where T : ResourceBase
    {
        /// <summary>
        /// The list of all resources that are registered in the resource store.
        /// </summary>
        public required List<T> Resources { get; set; }

        /// <summary>
        /// The name of the default resource (if any).
        /// </summary>
        public string? DefaultResourceName { get; set; }

        /// <summary>
        /// Creates a new resource store from a dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary containing the resources.</param>
        /// <returns>The newly created resource store.</returns>
        public static ResourceStore<T> FromDictionary(Dictionary<string, T> dictionary) =>
            new ResourceStore<T>
            {
                Resources = [.. dictionary.Values]
            };

        /// <summary>
        /// Creates a dictionary of resources from the resource store.
        /// </summary>
        /// <returns>The newly created dictionary.</returns>
        public Dictionary<string, T> ToDictionary() =>
            Resources.ToDictionary<T, string>(p => p.Name);
    }
}

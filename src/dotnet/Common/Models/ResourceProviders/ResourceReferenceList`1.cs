namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Maintains a collection of resource references.
    /// </summary>
    /// <typeparam name="T">The type of resource reference kept in the store.</typeparam>
    public class ResourceReferenceList<T> where T : ResourceReference
    {
        /// <summary>
        /// Gets or sets the schema version of the resource reference list.
        /// </summary>
        public int SchemaVersion { get; set; } = 1;

        /// <summary>
        /// The dictionary of resource references indexed by their unique names.
        /// </summary>
        public required List<T> ResourceReferences { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource that should be used as the default resource.
        /// </summary>
        public string? DefaultResourceName { get; set; }
    }
}

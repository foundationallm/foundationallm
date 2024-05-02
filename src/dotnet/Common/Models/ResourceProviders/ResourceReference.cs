namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Resource reference used by resource providers to index the resources they manage.
    /// </summary>
    public class ResourceReference
    {
        /// <summary>
        /// The name of the resource.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// The filename of the resource.
        /// </summary>
        public required string Filename { get; set; }

        /// <summary>
        /// The type of the resource.
        /// </summary>
        public required string Type { get; set; }

        /// <summary>
        /// Indicates whether the resource has been logically deleted.
        /// </summary>
        public bool Deleted { get; set; } = false;
    }
}

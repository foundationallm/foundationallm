namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Represents the result of a delete operation.
    /// </summary>
    public class ResourceProviderDeleteResult
    {
        /// <summary>
        /// A flag denoting whether the resource was deleted or not.
        /// </summary>
        public required bool Deleted { get; set; }

        /// <summary>
        /// The reason why the resource was not deleted.
        /// </summary>
        public string? Reason { get; set; }
    }
}

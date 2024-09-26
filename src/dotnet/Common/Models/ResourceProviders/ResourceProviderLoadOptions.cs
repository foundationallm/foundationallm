namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Options for the resource provider requests.
    /// </summary>
    public class ResourceProviderLoadOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to load resource content (applicable only to resources that have content).
        /// </summary>
        public bool LoadContent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include roles in the response.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the value is set to <c>true</c>, for each resource, the response will include
        /// the roles assigned directly or indirectly to the resource.
        /// </para>
        /// </remarks>
        public bool IncludeRoles { get; set; }
    }
}

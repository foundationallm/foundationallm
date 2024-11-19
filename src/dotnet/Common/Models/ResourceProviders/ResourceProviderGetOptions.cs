namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Provides options for resource provider get operations.
    /// </summary>
    public class ResourceProviderGetOptions : ResourceProviderOperationOptionsBase
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

        /// <summary>
        /// Gets or sets a value indicating whether to include actions in the response.
        /// </summary>
        /// <remarks>
        /// If the value is set to <c>true</c>, for each resource, the response will include
        /// the authorizable actions assigned directly or indirectly to the resource.
        /// </remarks>
        public bool IncludeActions { get; set; }
    }
}

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Provides options common to all resource provider operations.
    /// </summary>
    public class ResourceProviderOperationOptionsBase
    {
        /// <summary>
        /// Gets or sets a dictionary of parameters to be used in the resource provider operation request.
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = [];
    }
}

namespace FoundationaLLM.Common.Models.Plugins.Metadata
{
    /// <summary>
    /// Provides the model for selection hints required by plugin parameters of type resource-object-id.
    /// </summary>
    public class PluginParameterSelectionHint
    {
        /// <summary>
        /// Gets or sets the resource path used to filter the available resources.
        /// </summary>
        /// <remarks>
        /// The resource path does not include the FoundationaLLM instance identifier.
        /// The resource path has the following form: "providers/{resourceProvider}/{mainResourceType}".
        /// </remarks>
        public required string ResourcePath { get; set; }

        /// <summary>
        /// Gets or sets the filter action payload used to filter the available resources.
        /// </summary>
        public object? FilterActionPayload { get; set; }
    }
}

namespace FoundationaLLM.Common.Constants.Agents
{
    /// <summary>
    /// Provides well-known source names for agent tool property values.
    /// </summary>
    public static class AgentToolPropertyValueSources
    {
        /// <summary>
        /// The value must be sourced from the completion request's metadata property.
        /// If not present an error will be thrown.
        /// </summary>
        public const string CompletionRequestMetadata_Required = "__COMPLETION_REQUEST_METADATA_!__";

        /// <summary>
        /// The value may be sourced from the completion request's metadata property.
        /// If not present, the metadata property will be ignored.
        /// </summary>
        public const string CompletionRequestMetadata_Optional = "__COMPLETION_REQUEST_METADATA__";
    }
}

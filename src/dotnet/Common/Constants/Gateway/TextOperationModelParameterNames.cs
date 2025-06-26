namespace FoundationaLLM.Common.Constants.Gateway
{
    /// <summary>
    /// Defines the model parameter names used in text operations executed by the FoundationaLLM Gateway API.
    /// </summary>
    public static class TextOperationModelParameterNames
    {
        /// <summary>
        /// The name of the property that holds the embedding dimensions.
        /// </summary>
        public const string EmbeddingDimensions = "embedding_dimensions";

        /// <summary>
        /// The name of the ToP completion model parameter.
        /// </summary>
        public const string TopP = "top_p";

        /// <summary>
        /// The name of the temperature completion model parameter.
        /// </summary>
        public const string Temperature = "temperature";

        /// <summary>
        /// The name of the model parameter that holds the maximum output token count.
        /// </summary>
        public const string MaxOutputTokenCount = "max_output_token_count";
    }
}

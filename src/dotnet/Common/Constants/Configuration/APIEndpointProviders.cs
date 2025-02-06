namespace FoundationaLLM.Common.Constants.Configuration
{
    /// <summary>
    /// Language Model provider constants.
    /// </summary>
    public static class APIEndpointProviders
    {
        /// <summary>
        /// Microsoft
        /// </summary>
        public const string MICROSOFT = "microsoft";

        /// <summary>
        /// OpenAI
        /// </summary>
        public const string OPENAI = "openai";

        /// <summary>
        /// Amazon Bedrock
        /// </summary>
        public const string BEDROCK = "bedrock";

        /// <summary>
        /// Google VertexAI
        /// </summary>
        public const string VERTEXAI = "vertexai";

        /// <summary>
        /// All providers.
        /// </summary>
        public readonly static string[] All = [MICROSOFT, OPENAI, BEDROCK, VERTEXAI];
    }
}

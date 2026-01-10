namespace FoundationaLLM.Common.Constants.Configuration
{
    /// <summary>
    /// Language Model provider constants.
    /// </summary>
    public static class APIEndpointProviders
    {
        /// <summary>
        /// Azure AI Inferencing API
        /// </summary>
        public const string AZUREAI = "azureai";
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
        /// Google Gemini Developer API
        /// </summary>
        public const string GOOGLE_GENAI = "google_genai";

        /// <summary>
        /// All providers.
        /// </summary>
        public readonly static string[] All = [AZUREAI, MICROSOFT, OPENAI, BEDROCK, VERTEXAI, GOOGLE_GENAI];
    }
}

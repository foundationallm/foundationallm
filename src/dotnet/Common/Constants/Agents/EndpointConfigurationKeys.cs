namespace FoundationaLLM.Common.Constants.Agents
{
    /// <summary>
    /// Contains constants of the keys for endpoint configuration settings.
    /// </summary>
    public static class EndpointConfigurationKeys
    {
        /// <summary>
        /// The LLM provider.
        /// Can be microsoft or openai.
        /// </summary>
        public const string Provider = "provider";

        /// <summary>
        /// The API Endpoint configuration setting.
        /// This value should be a URI representing the endpoint of the API.
        /// </summary>
        public const string Endpoint = "endpoint";

        /// <summary>
        /// The API Key configuration setting.
        /// </summary>
        public const string APIKey = "api_key";

        /// <summary>
        /// The API Version configuration setting.
        /// </summary>
        public const string APIVersion = "api_version";

        /// <summary>
        /// The type of authentication to use for connecting to the endpoint.
        /// This value with be either key or token.
        /// </summary>
        public const string AuthenticationType = "authentication_type";

        /// <summary>
        /// The type of operation the endpoint is performing.
        /// This value should be completion or chat.
        /// </summary>
        public const string OperationType = "operation_type";
    }
}

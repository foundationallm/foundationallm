using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Configuration
{
    /// <summary>
    /// Provides the configuration for an API endpoint resource.
    /// </summary>
    public class APIEndpointConfiguration : ResourceBase
    {
        /// <summary>
        /// Creates a new instance of <see cref="APIEndpointConfiguration"/>.
        /// </summary>
        public APIEndpointConfiguration() =>
            Type = ConfigurationTypes.APIEndpointConfiguration;

        /// <summary>
        /// The api endpoint category.
        /// </summary>
        [JsonPropertyName("category")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required APIEndpointCategory Category { get; set; }

        /// <summary>
        /// The type of authentication required for accessing the API.
        /// </summary>
        [JsonPropertyName("authentication_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required AuthenticationTypes AuthenticationType { get; set; }

        /// <summary>
        /// The base URL of the API endpoint.
        /// </summary>
        [JsonPropertyName("url")]
        public required string Url { get; set; }

        /// <summary>
        /// The status URL of the API endpoint, if available.
        /// </summary>
        [JsonPropertyName("status_url")]
        public string? StatusUrl { get; set; }

        /// <summary>
        /// A list of URL exceptions.
        /// </summary>
        [JsonPropertyName("url_exceptions")]
        public List<UrlException> UrlExceptions { get; set; } = [];

        /// <summary>
        /// Dictionary with values used for authentication.
        /// <para>
        /// For the list of supported keys, see <see cref="AuthenticationParametersKeys"/>.
        /// </para>
        /// </summary>
        [JsonPropertyName("authentication_parameters")]
        public Dictionary<string, object> AuthenticationParameters { get; set; } = [];

        /// <summary>
        /// The timeout duration in seconds for API calls.
        /// </summary>
        [JsonPropertyName("timeout_seconds")]
        public required int TimeoutSeconds { get; set; }

        /// <summary>
        /// The name of the retry strategy.
        /// </summary>
        [JsonPropertyName("retry_strategy_name")]
        public required string RetryStrategyName { get; set; }

        /// <summary>
        /// The API endpoint provider.
        /// <para>
        /// For a list of available API endpoint providers, see <see cref="APIEndpointProviders"/>.
        /// </para>
        /// </summary>
        [JsonPropertyName("provider")]
        public string? Provider { get; set; }

        /// <summary>
        /// The version to use when calling the API represented by the endpoint.
        /// </summary>
        [JsonPropertyName("api_version")]
        public string? APIVersion { get; set; }

        /// <summary>
        /// Type of operation the endpoint is performing.
        /// This value should be completions or chat.
        /// Default value is chat.
        /// </summary>
        [JsonPropertyName("operation_type")]
        public string? OperationType { get; set; }

    }

    /// <summary>
    /// Represents an exception to the base URL.
    /// </summary>
    public class UrlException
    {
        /// <summary>
        /// The user principal name.
        /// </summary>
        [JsonPropertyName("user_principal_name")]
        public required string UserPrincipalName { get; set; }

        /// <summary>
        /// The alternative URL.
        /// </summary>
        [JsonPropertyName("url")]
        public required string Url { get; set; }

        /// <summary>
        /// Indicates whether the exception is enabled.
        /// </summary>
        [JsonPropertyName("enabled")]
        public required bool Enabled { get; set; }
    }
}

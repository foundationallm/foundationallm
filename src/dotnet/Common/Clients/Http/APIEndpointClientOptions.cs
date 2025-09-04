using FoundationaLLM.Common.Constants;
using System.ClientModel.Primitives;

namespace FoundationaLLM.Common.Clients.Http
{
    /// <summary>
    /// Exposes client options for FoundationaLLM API endpoints.
    /// </summary>
    public class APIEndpointClientOptions : ClientPipelineOptions
    {
        /// <summary>
        /// Gets or sets the name of the API key header to send.
        /// </summary>
        public string? APIKeyHeaderName { get; set; }

        /// <summary>
        /// Gets or sets an optional prefix to add before the API key.
        /// </summary>
        public string? APIKeyPrefix { get; set; }

        /// <summary>
        /// Gets the API version of the API endpoint.
        /// </summary>
        public string? APIVersion { get; init; }

        /// <summary>
        /// Creates an instance of <see cref="APIEndpointClientOptions"/> using the specified client builder parameters.
        /// </summary>
        /// <remarks>The following keys are expected in the <paramref name="clientBuilderParameters"/>
        /// dictionary: <list type="bullet"> <item> <description> <see
        /// cref="HttpClientFactoryServiceKeyNames.APIKeyHeaderName"/>: The name of the header used for the API key.
        /// </description> </item> <item> <description> <see cref="HttpClientFactoryServiceKeyNames.APIKeyPrefix"/>: The
        /// prefix to prepend to the API key in the header. </description> </item> <item> <description> <see
        /// cref="HttpClientFactoryServiceKeyNames.APIVersion"/>: The version of the API to use. </description> </item>
        /// <item> <description> <see cref="HttpClientFactoryServiceKeyNames.TimeoutSeconds"/>: The network timeout in
        /// seconds. Defaults to 120 seconds if not specified. </description> </item> </list></remarks>
        /// <param name="clientBuilderParameters">A dictionary containing configuration parameters for the client. The dictionary keys should match the
        /// constants defined in <see cref="HttpClientFactoryServiceKeyNames"/>.</param>
        /// <returns>An <see cref="APIEndpointClientOptions"/> instance populated with values from the provided <paramref
        /// name="clientBuilderParameters"/>. If a required parameter is missing, default values are used.</returns>
        public static APIEndpointClientOptions FromClientBuilderParameters(
            Dictionary<string, object> clientBuilderParameters) =>
            new()
            {
                APIKeyHeaderName = clientBuilderParameters.TryGetValue(
                    HttpClientFactoryServiceKeyNames.APIKeyHeaderName, out var apiKeyHeaderNameObj)
                    ? apiKeyHeaderNameObj.ToString()!
                    : null,

                APIKeyPrefix = clientBuilderParameters.TryGetValue(
                    HttpClientFactoryServiceKeyNames.APIKeyPrefix, out var apiKeyPrefixObj)
                    ? apiKeyPrefixObj.ToString()!
                    : null,

                APIVersion = clientBuilderParameters.TryGetValue(
                    HttpClientFactoryServiceKeyNames.APIVersion, out var apiVersionObj)
                    ? apiVersionObj.ToString()!
                    : null,

                NetworkTimeout = TimeSpan.FromSeconds(
                    clientBuilderParameters.TryGetValue(
                        HttpClientFactoryServiceKeyNames.TimeoutSeconds,
                        out var timeoutSecondsObject)
                    && timeoutSecondsObject is not null
                    ? (int) timeoutSecondsObject
                    : 120)
            };
    }
}

using System.Net.Http.Headers;

namespace FoundationaLLM.Common.Extensions
{
    /// <summary>
    /// Adds extension methods to the <see cref="HttpClient"/> class.
    /// </summary>
    public static class HttpClientExtensions
    {
        private const string StatusEndpointKey = "StatusEndpoint";

        /// <summary>
        /// Sets the bearer token for the <see cref="HttpClient"/> if the
        /// passed in token is not null or empty.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> to extend.</param>
        /// <param name="token">An auth token.</param>
        public static void SetBearerToken(this HttpClient httpClient, string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        /// <summary>
        /// Sets the status endpoint for the <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> to extend.</param>
        /// <param name="statusEndpoint">The endpoint's status endpoint.</param>
        public static void SetStatusEndpoint(this HttpClient client, string statusEndpoint)
        {
            client.DefaultRequestHeaders.Remove(StatusEndpointKey);
            client.DefaultRequestHeaders.Add(StatusEndpointKey, statusEndpoint);
        }

        /// <summary>
        /// Gets the status endpoint for the <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> to extend.</param>
        /// <returns></returns>
        public static string? GetStatusEndpoint(this HttpClient client) =>
            client.DefaultRequestHeaders.TryGetValues(StatusEndpointKey, out var values) ? values.FirstOrDefault() : null;
    }
}

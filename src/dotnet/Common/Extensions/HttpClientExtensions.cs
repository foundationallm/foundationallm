using System.Net.Http.Headers;

namespace FoundationaLLM.Common.Extensions
{
    /// <summary>
    /// Adds extension methods to the <see cref="HttpClient"/> class.
    /// </summary>
    public static class HttpClientExtensions
    {
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
        /// Sets the agent access token for the <see cref="HttpClient"/> if the
        /// passed in token is not null or empty.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> to extend.</param>
        /// <param name="token">An agent access token.</param>
        public static void SetAgentAccessToken(this HttpClient httpClient, string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Add("X-AGENT-ACCESS-TOKEN", token);
            }
        }
    }
}

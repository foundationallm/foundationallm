using Azure.Identity;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Graph;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace FoundationaLLM.Common.Services.Graph
{
    /// <summary>
    /// Service for interacting with Microsoft Graph API.
    /// </summary>
    public class GraphService: IGraphService
    {
        readonly GraphServiceSettings _settings;
        readonly GraphServiceClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphService"/> class.
        /// </summary>
        /// <param name="settings">The settings for configuring the Graph service.</param>
        public GraphService(IOptions<GraphServiceSettings> settings)
        {
            _settings = settings.Value;
            _client = GetGraphServiceClient();
        }

        /// <inheritdoc/>
        public async Task<List<string>> GetMemberships(string upn)
        {
            var result = await _client.Users[upn].TransitiveMemberOf.GraphGroup.GetAsync();

            return result == null || result.Value == null ? [] : result.Value!.Where(x=>x.Id != null).Select(x=>x.Id!).ToList();
        }

        /// <summary>
        /// Retrieves an instance of the GraphServiceClient for interacting with Microsoft Graph API.
        /// </summary>
        /// <returns>An instance of GraphServiceClient.</returns>
        private GraphServiceClient GetGraphServiceClient()
        {
            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };

            ClientSecretCredential clientSecretCredential = new(
                        _settings.TenantId,
                        _settings.ClientId,
                        _settings.ClientSecret,
            new ClientSecretCredentialOptions() { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud });

            return new GraphServiceClient(clientSecretCredential, scopes);
        }
    }
}

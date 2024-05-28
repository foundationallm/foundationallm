using FoundationaLLM.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace FoundationaLLM.Common.Services.Security
{
    /// <summary>
    /// Implements group membership services using the Microsoft Graph API.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="MicrosoftGraphGroupMembershipService"/> class.
    /// </remarks>
    /// <param name="graphServiceClient">The GraphServiceClient to be used for API interactions.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class MicrosoftGraphGroupMembershipService(
        GraphServiceClient graphServiceClient,
        ILogger<MicrosoftGraphGroupMembershipService> logger) : IGroupMembershipService
    {
        private readonly GraphServiceClient _graphServiceClient = graphServiceClient;
        private readonly ILogger<MicrosoftGraphGroupMembershipService> _logger = logger;

        /// <inheritdoc/>
        public async Task<List<string>> GetGroupsForPrincipal(string userIdentifier)
        {
            var groups = await _graphServiceClient.Users[userIdentifier].TransitiveMemberOf.GraphGroup.GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Top = 500;
            }).ConfigureAwait(false);

            var groupMembership = new List<Group>();

            while (groups?.Value != null)
            {
                groupMembership.AddRange(groups.Value);

                // Invoke paging if required.
                if (!string.IsNullOrEmpty(groups.OdataNextLink))
                {
                    groups = await _graphServiceClient.Users[userIdentifier].TransitiveMemberOf.GraphGroup
                        .WithUrl(groups.OdataNextLink)
                        .GetAsync();
                }
                else
                {
                    break;
                }
            }

            return groupMembership.Count == 0
                ? []
                : groupMembership.Where(x => x.Id != null).Select(x => x.Id!).ToList();
        }
    }
}

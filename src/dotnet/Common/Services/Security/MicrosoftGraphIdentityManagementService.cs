using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Collections;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.DirectoryObjects.GetByIds;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;

namespace FoundationaLLM.Common.Services.Security
{
    /// <summary>
    /// Implements group membership services using the Microsoft Graph API.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="MicrosoftGraphIdentityManagementService"/> class.
    /// </remarks>
    /// <param name="graphServiceClient">The GraphServiceClient to be used for API interactions.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class MicrosoftGraphIdentityManagementService(
        GraphServiceClient graphServiceClient,
        ILogger<MicrosoftGraphIdentityManagementService> logger) : IIdentityManagementService
    {
        private readonly ILogger<MicrosoftGraphIdentityManagementService> _logger = logger;
        private readonly IMemoryCache _groupMembershipCache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = 5000, // Limit cache size to 5000 lists of security group identifiers.
            ExpirationScanFrequency = TimeSpan.FromMinutes(1), // Scan for expired items every minute.
        });
        private readonly MemoryCacheEntryOptions _cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10)) // Cache entries are valid for 10 minutes.
            .SetSlidingExpiration(TimeSpan.FromMinutes(5)) // Reset expiration time if accessed within 5 minutes.
            .SetSize(1); // Each cache entry is a single list of security group identifiers.

        private readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);

        /// <inheritdoc/>
        public async Task<List<string>> GetGroupsForPrincipal(string userIdentifier)
        {
            if (_groupMembershipCache.TryGetValue(userIdentifier, out List<string>? groupIdsList))
            {
                return groupIdsList!;
            }

            // Retrieve group membership from the Graph API outside of the synchronized block.
            // This is to shorten the time the lock is held (since there is a low probability that multiple simultaneous requests will be made for the same user).
            // We are prioritizing shorter lock times over the possibility of making multiple Graph API requests.
            var groupIds = await GetGroupMembershipFromGraph(userIdentifier);

            try
            {
                await _cacheLock.WaitAsync();

                // Was the list of group identifiers already added to the cache by another thread?
                // If yes, just return the cached list.
                if (_groupMembershipCache.TryGetValue(userIdentifier, out List<string>? newGroupIdsList))
                {
                    return newGroupIdsList!;
                }

                // Add the list of group identifiers to the cache.
                _groupMembershipCache.Set(userIdentifier, groupIds, _cacheEntryOptions);
                return groupIds;
            }
            finally
            {
                _cacheLock.Release();
            }

            return [];
        }

        /// <inheritdoc/>
        public async Task<List<ObjectQueryResult>> GetObjectsByIds(ObjectQueryParameters parameters)
        {
            if (parameters.Ids == null || parameters.Ids.Length == 0)
                throw new Exception("The list of object ids is invalid.");

            var requestBody = new GetByIdsPostRequestBody
            {
                Ids = new List<string>(parameters.Ids),
                Types =
                [
                    "user",
                    "group",
                    "servicePrincipal",
                ],
            };

            // The maximum number of IDs that can be passed in a single request is 1000.
            var objects =
                await graphServiceClient.DirectoryObjects.GetByIds.PostAsGetByIdsPostResponseAsync(requestBody);

            if (objects?.Value == null || objects.Value.Count == 0)
            {
                return [];
            }

            var results = new List<ObjectQueryResult>();

            foreach (var directoryObject in objects.Value)
            {
                string? email = null;
                string? displayName = null;
                var objectType = ObjectTypes.Other;

                if (directoryObject is User user)
                {
                    email = user.Mail;
                    displayName = user.DisplayName;
                    objectType = ObjectTypes.User;
                }
                else if (directoryObject is Group group)
                {
                    email = group.Mail;
                    displayName = group.DisplayName;
                    objectType = ObjectTypes.Group;
                }
                else if (directoryObject is ServicePrincipal servicePrincipal)
                {
                    displayName = servicePrincipal.DisplayName;
                    objectType = ObjectTypes.Other;
                }

                results.Add(new ObjectQueryResult
                {
                    Id = directoryObject.Id,
                    Email = email,
                    DisplayName = displayName,
                    ObjectType = objectType
                });
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<ObjectQueryResult> GetUserGroupById(string groupId)
        {
            var group = await graphServiceClient.Groups[groupId].GetAsync();

            return new ObjectQueryResult
            {
                Id = group?.Id,
                Email = group?.Mail,
                DisplayName = group?.DisplayName,
                ObjectType = ObjectTypes.Group,
            };
        }

        /// <inheritdoc/>
        public async Task<PagedResponse<ObjectQueryResult>> GetUserGroups(ObjectQueryParameters queryParams)
        {
            var pageSize = queryParams.PageSize ?? 100;
            var userGroups = new List<ObjectQueryResult>();

            var currentPage = 1;

            // Retrieve group accounts with filtering and paging options.
            var groupsPage = await graphServiceClient.Groups
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = ["id", "displayName", "mail"];
                    requestConfiguration.QueryParameters.Filter = "securityEnabled eq true";
                    if (!string.IsNullOrEmpty(queryParams.Name))
                    {
                        requestConfiguration.QueryParameters.Search = $"\"displayName:{queryParams.Name}\"";
                    }
                    requestConfiguration.QueryParameters.Orderby = ["displayName"];
                    requestConfiguration.QueryParameters.Top = pageSize;
                    requestConfiguration.QueryParameters.Count = true;
                    requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                });

            // Skip pages until we reach the desired page.
            while (groupsPage?.OdataNextLink != null && currentPage < queryParams.PageNumber)
            {
                groupsPage = await graphServiceClient.Groups
                    .WithUrl(groupsPage.OdataNextLink)
                    .GetAsync();
                currentPage++;
            }

            // Process the desired page.
            if (groupsPage?.Value != null)
            {
                userGroups.AddRange(groupsPage.Value.Select(x => new ObjectQueryResult
                {
                    Id = x?.Id,
                    Email = x?.Mail,
                    DisplayName = x?.DisplayName,
                    ObjectType = ObjectTypes.Group,
                }));
            }

            return new PagedResponse<ObjectQueryResult>
            {
                Items = userGroups,
                TotalItems = groupsPage?.OdataCount,
                HasNextPage = groupsPage?.OdataNextLink != null
            };
        }

        /// <inheritdoc/>
        public async Task<ObjectQueryResult> GetUserById(string userId)
        {
            var user = await graphServiceClient.Users[userId].GetAsync();

            return new ObjectQueryResult
            {
                Id = user?.Id,
                Email = user?.Mail,
                DisplayName = user?.DisplayName,
                ObjectType = ObjectTypes.User,
            };
        }

        /// <inheritdoc/>
        public async Task<PagedResponse<ObjectQueryResult>> GetUsers(ObjectQueryParameters queryParams)
        {
            var pageSize = queryParams.PageSize ?? 100;
            var users = new List<ObjectQueryResult>();

            var currentPage = 1;

            // Retrieve users with filtering and paging options.
            var usersPage = await graphServiceClient.Users
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = ["id", "displayName", "mail"];
                    requestConfiguration.QueryParameters.Filter = "accountEnabled eq true";
                    if (!string.IsNullOrEmpty(queryParams.Name))
                    {
                        requestConfiguration.QueryParameters.Search = $"\"displayName:{queryParams.Name}\"";
                    }
                    requestConfiguration.QueryParameters.Orderby = ["displayName"];
                    requestConfiguration.QueryParameters.Top = pageSize;
                    requestConfiguration.QueryParameters.Count = true;
                    requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                });

            // Skip pages until we reach the desired page.
            while (usersPage?.OdataNextLink != null && currentPage < queryParams.PageNumber)
            {
                usersPage = await graphServiceClient.Users
                    .WithUrl(usersPage.OdataNextLink)
                    .GetAsync();
                currentPage++;
            }

            // Process the desired page.
            if (usersPage?.Value != null)
            {
                users.AddRange(usersPage.Value.Select(x => new ObjectQueryResult
                {
                    Id = x?.Id,
                    Email = x?.Mail,
                    DisplayName = x?.DisplayName,
                    ObjectType = ObjectTypes.User,
                }));
            }

            return new PagedResponse<ObjectQueryResult>
            {
                Items = users,
                TotalItems = usersPage?.OdataCount,
                HasNextPage = usersPage?.OdataNextLink != null
            };
        }

        private async Task<List<string>> GetGroupMembershipFromGraph(string userIdentifier)
        {
            try
            {
                var groups = await graphServiceClient.Users[userIdentifier].TransitiveMemberOf.GraphGroup.GetAsync(requestConfiguration =>
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
                        groups = await graphServiceClient.Users[userIdentifier].TransitiveMemberOf.GraphGroup
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
            catch (ODataError ex)
            {
                _logger.LogError(ex, "Failed to retrieve group membership from the Graph API for user {UserIdentifier}.", userIdentifier);
                return [];
            }
        }
    }
}

using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Collections;
using FoundationaLLM.Common.Models.Configuration.Security;
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
    /// <param name="settings">The settings for the Microsoft Graph Identity Management Service.</param>
    /// <param name="graphServiceClient">The GraphServiceClient to be used for API interactions.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class MicrosoftGraphIdentityManagementService(
        MicrosoftGraphIdentityManagementServiceSettings settings,
        GraphServiceClient graphServiceClient,
        ILogger<MicrosoftGraphIdentityManagementService> logger) : IIdentityManagementService
    {
        private readonly MicrosoftGraphIdentityManagementServiceSettings _settings = settings;
        private readonly GraphServiceClient _graphServiceClient = graphServiceClient;   
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

            _logger.LogDebug("Group membership: Retrieved {GroupCount} groups for user {UserIdentifier} from Microsoft Graph API.", groupIds.Count, userIdentifier);
            _logger.LogDebug("Group membership: {GroupIds}", string.Join(",", groupIds));

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
        }

        /// <inheritdoc/>
        public async Task<List<ObjectQueryResult>> GetObjectsByIds(ObjectQueryParameters parameters)
        {
            if (parameters.Ids == null || parameters.Ids.Length == 0)
                throw new Exception("The list of object ids is invalid.");

            var requestBody = new GetByIdsPostRequestBody
            {
                Ids = [.. parameters.Ids],
                Types =
                [
                    "user",
                    "group",
                    "servicePrincipal",
                ],
            };

            GetByIdsPostResponse? response = null;

            if (_settings.RetrieveOnPremisesAccountName)
            {
                var requestInfo = _graphServiceClient.DirectoryObjects.GetByIds.ToPostRequestInformation(requestBody);
                requestInfo.QueryParameters["%24select"] = new[] { "id", "displayName", "mail", "onPremisesSamAccountName" };
                requestInfo.UrlTemplate += "{?%24select}";

                response = await _graphServiceClient.RequestAdapter.SendAsync<GetByIdsPostResponse>(
                    requestInfo,
                    GetByIdsPostResponse.CreateFromDiscriminatorValue);
            }
            else
                response = await _graphServiceClient.DirectoryObjects.GetByIds.PostAsGetByIdsPostResponseAsync(requestBody);

            if (response?.Value == null || response.Value.Count == 0)
            {
                return [];
            }

            var results = new List<ObjectQueryResult>();

            foreach (var directoryObject in response.Value)
            {
                string? email = null;
                string? displayName = null;
                string? onPremisesAccountName = null;
                var objectType = SecurityPrincipalTypes.Other;

                if (directoryObject is User user)
                {
                    email = user.Mail;
                    displayName = user.DisplayName;
                    onPremisesAccountName = user.OnPremisesSamAccountName;
                    objectType = SecurityPrincipalTypes.User;
                }
                else if (directoryObject is Group group)
                {
                    email = group.Mail;
                    displayName = group.DisplayName;
                    onPremisesAccountName = group.OnPremisesSamAccountName;
                    objectType = SecurityPrincipalTypes.Group;
                }
                else if (directoryObject is ServicePrincipal servicePrincipal)
                {
                    displayName = servicePrincipal.DisplayName;
                    objectType = SecurityPrincipalTypes.ServicePrincipal;
                }

                results.Add(new ObjectQueryResult
                {
                    Id = directoryObject.Id,
                    Email = email,
                    DisplayName = displayName,
                    OnPremisesAccountName = onPremisesAccountName,
                    ObjectType = objectType
                });
            }

            // Add to the results an `ObjectQueryResult` object for any `parameters.Ids` that are not in the results.
            foreach (var id in parameters.Ids)
            {
                if (results.All(x => x.Id != id))
                {
                    results.Add(new ObjectQueryResult
                    {
                        Id = id,
                        DisplayName = id,
                        ObjectType = SecurityPrincipalTypes.Other
                    });
                }
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<ObjectQueryResult> GetUserGroupById(string groupId)
        {
            var group = await _graphServiceClient.Groups[groupId].GetAsync();

            return new ObjectQueryResult
            {
                Id = group?.Id,
                Email = group?.Mail,
                DisplayName = group?.DisplayName,
                OnPremisesAccountName = group?.OnPremisesSamAccountName,
                ObjectType = SecurityPrincipalTypes.Group,
            };
        }

        /// <inheritdoc/>
        public async Task<PagedResponse<ObjectQueryResult>> GetUserGroups(ObjectQueryParameters queryParams)
        {
            var pageSize = queryParams.PageSize ?? 100;
            var userGroups = new List<ObjectQueryResult>();

            //var currentPage = 1;

            // Retrieve group accounts with filtering and paging options.
            var groupsResponse = await _graphServiceClient.Groups
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = ["id", "displayName", "mail", "onPremisesSamAccountName"];
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
            //while (groupsResponse?.OdataNextLink != null && currentPage < queryParams.PageNumber)
            //{
            //    groupsResponse = await _graphServiceClient.Groups
            //        .WithUrl(groupsResponse.OdataNextLink)
            //        .GetAsync();
            //    currentPage++;
            //}

            // Process the desired page.
            if (groupsResponse?.Value != null)
            {
                userGroups.AddRange(groupsResponse.Value.Select(x => new ObjectQueryResult
                {
                    Id = x?.Id,
                    Email = x?.Mail,
                    DisplayName = x?.DisplayName,
                    OnPremisesAccountName = x?.OnPremisesSamAccountName,
                    ObjectType = SecurityPrincipalTypes.Group,
                }));
            }

            if (_settings.RetrieveOnPremisesAccountName
                && !string.IsNullOrWhiteSpace(queryParams.Name))
            {
                var groupsResponse2 = await _graphServiceClient.Groups
                    .GetAsync(requestConfiguration =>
                    {
                        requestConfiguration.QueryParameters.Select = ["id", "displayName", "mail", "onPremisesSamAccountName"];
                        requestConfiguration.QueryParameters.Filter = $"startsWith(onPremisesSamAccountName, '{queryParams.Name}') and (securityEnabled eq true)";
                        requestConfiguration.QueryParameters.Orderby = ["displayName"];
                        requestConfiguration.QueryParameters.Top = pageSize;
                        requestConfiguration.QueryParameters.Count = true;
                        requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                    });

                if (groupsResponse2?.Value != null)
                {
                    var existingGroupIds = userGroups.Select(u => u.Id).ToHashSet();
                    userGroups.AddRange(groupsResponse2.Value
                        .Where(x => x?.Id != null && !existingGroupIds.Contains(x.Id))
                        .Select(x => new ObjectQueryResult
                            {
                                Id = x?.Id,
                                Email = x?.Mail,
                                DisplayName = x?.DisplayName,
                                OnPremisesAccountName = x?.OnPremisesSamAccountName,
                                ObjectType = SecurityPrincipalTypes.Group,
                            }));
                }
            }

            return new PagedResponse<ObjectQueryResult>
            {
                Items = userGroups,
                TotalItems = userGroups.Count,
                HasNextPage = false
            };
        }

        /// <inheritdoc/>
        public async Task<ObjectQueryResult> GetUserById(string userId)
        {
            var user = await _graphServiceClient.Users[userId].GetAsync();

            return new ObjectQueryResult
            {
                Id = user?.Id,
                Email = user?.Mail,
                DisplayName = user?.DisplayName,
                OnPremisesAccountName = user?.OnPremisesSamAccountName,
                ObjectType = SecurityPrincipalTypes.User,
            };
        }

        /// <inheritdoc/>
        public async Task<PagedResponse<ObjectQueryResult>> GetUsers(ObjectQueryParameters queryParams)
        {
            var pageSize = queryParams.PageSize ?? 100;
            var users = new List<ObjectQueryResult>();

            //var currentPage = 1;

            // Retrieve users with filtering and paging options.
            var usersResponse = await _graphServiceClient.Users
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = ["id", "displayName", "mail", "onPremisesSamAccountName"];
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
            //while (usersPage?.OdataNextLink != null && currentPage < queryParams.PageNumber)
            //{
            //    usersPage = await _graphServiceClient.Users
            //        .WithUrl(usersPage.OdataNextLink)
            //        .GetAsync();
            //    currentPage++;
            //}

            // Process the desired page.
            if (usersResponse?.Value != null)
            {
                users.AddRange(usersResponse.Value.Select(x => new ObjectQueryResult
                {
                    Id = x?.Id,
                    Email = x?.Mail,
                    DisplayName = x?.DisplayName,
                    OnPremisesAccountName = x?.OnPremisesSamAccountName,
                    ObjectType = SecurityPrincipalTypes.User,
                }));
            }

            if (_settings.RetrieveOnPremisesAccountName
                && !string.IsNullOrWhiteSpace(queryParams.Name))
            {
                var usersResponse2 = await _graphServiceClient.Users
                    .GetAsync(requestConfiguration =>
                    {
                        requestConfiguration.QueryParameters.Select = ["id", "displayName", "mail", "onPremisesSamAccountName"];
                        requestConfiguration.QueryParameters.Filter = $"startsWith(onPremisesSamAccountName, '{queryParams.Name}') and (accountEnabled eq true)";
                        requestConfiguration.QueryParameters.Orderby = ["displayName"];
                        requestConfiguration.QueryParameters.Top = pageSize;
                        requestConfiguration.QueryParameters.Count = true;
                        requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                    });

                if (usersResponse2?.Value != null)
                {
                    // Ensure no duplicates are added.
                    var existingUserIds = users.Select(u => u.Id).ToHashSet();
                    users.AddRange(usersResponse2.Value
                        .Where(x => x?.Id != null && !existingUserIds.Contains(x.Id))
                        .Select(x => new ObjectQueryResult
                            {
                                Id = x?.Id,
                                Email = x?.Mail,
                                DisplayName = x?.DisplayName,
                                OnPremisesAccountName = x?.OnPremisesSamAccountName,
                                ObjectType = SecurityPrincipalTypes.User,
                            }));

                    users = [.. users
                        .OrderBy(x => x.DisplayName)
                        .Take(pageSize)
                    ];
                }
            }

            return new PagedResponse<ObjectQueryResult>
            {
                Items = users,
                TotalItems = users.Count,
                HasNextPage = false
            };
        }

        /// <inheritdoc/>
        public async Task<ObjectQueryResult> GetServicePrincipalById(string servicePrincipalId)
        {
            var servicePrincipal = await _graphServiceClient.ServicePrincipals[servicePrincipalId].GetAsync();

            return new ObjectQueryResult
            {
                Id = servicePrincipal?.Id,
                Email = null,
                DisplayName = servicePrincipal?.DisplayName,
                ObjectType = SecurityPrincipalTypes.ServicePrincipal,
            };
        }

        /// <inheritdoc/>
        public async Task<PagedResponse<ObjectQueryResult>> GetServicePrincipals(ObjectQueryParameters queryParams)
        {
            var pageSize = queryParams.PageSize ?? 100;
            var servicePrincipals = new List<ObjectQueryResult>();

            //var currentPage = 1;

            // Retrieve users with filtering and paging options.
            var servicePrincipalsResponse = await _graphServiceClient.ServicePrincipals
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = ["id", "displayName"];
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
            //while (servicePrincipalsPage?.OdataNextLink != null && currentPage < queryParams.PageNumber)
            //{
            //    servicePrincipalsPage = await _graphServiceClient.ServicePrincipals
            //        .WithUrl(servicePrincipalsPage.OdataNextLink)
            //        .GetAsync();
            //    currentPage++;
            //}

            // Process the desired page.
            if (servicePrincipalsResponse?.Value != null)
            {
                servicePrincipals.AddRange(servicePrincipalsResponse.Value.Select(x => new ObjectQueryResult
                {
                    Id = x?.Id,
                    Email = null,
                    DisplayName = x?.DisplayName,
                    ObjectType = SecurityPrincipalTypes.ServicePrincipal,
                }));
            }

            return new PagedResponse<ObjectQueryResult>
            {
                Items = servicePrincipals,
                TotalItems = servicePrincipals.Count,
                HasNextPage = servicePrincipalsResponse?.OdataNextLink != null
            };
        }

        private async Task<List<string>> GetGroupMembershipFromGraph(string userIdentifier)
        {
            try
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
            catch (ODataError ex)
            {
                _logger.LogError(ex, "Failed to retrieve group membership from the Graph API for user {UserIdentifier}.", userIdentifier);
                return [];
            }
        }
    }
}

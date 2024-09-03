using FoundationaLLM.Authorization.Models.Configuration;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FoundationaLLM.Authorization.Services
{
    /// <summary>
    /// Provides methods for interacting with the Authorization API.
    /// </summary>
    public class AuthorizationService : IAuthorizationService
    {
        private readonly AuthorizationServiceSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(
            IHttpClientFactory httpClientFactory,
            IOptions<AuthorizationServiceSettings> options,
            ILogger<AuthorizationService> logger)
        {
            _settings = options.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ActionAuthorizationResult> ProcessAuthorizationRequest(
            string instanceId,
            string action,
            List<string> resourcePaths,
            UnifiedUserIdentity userIdentity)
        {
            var defaultResults = resourcePaths.Distinct().ToDictionary(rp => rp, auth => false);

            try
            {
                var authorizationRequest = new ActionAuthorizationRequest
                {
                    Action = action,
                    ResourcePaths = resourcePaths,
                    UserContext = new UserAuthorizationContext
                    {
                        SecurityPrincipalId = userIdentity.UserId!,
                        UserPrincipalName = userIdentity.UPN!,
                        SecurityGroupIds = userIdentity.GroupIds
                    }
                };

                var httpClient = await CreateHttpClient();
                var response = await httpClient.PostAsync(
                    $"/instances/{instanceId}/authorize",
                    JsonContent.Create(authorizationRequest));

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ActionAuthorizationResult>(responseContent)!;
                }

                _logger.LogError("The call to the Authorization API returned an error: {StatusCode} - {ReasonPhrase}.", response.StatusCode, response.ReasonPhrase);
                return new ActionAuthorizationResult { AuthorizationResults = defaultResults };
            }
            catch (Exception ex)
            { _logger.LogError(ex, "There was an error calling the Authorization API");
                return new ActionAuthorizationResult { AuthorizationResults = defaultResults };
            }
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, RoleAssignmentsWithActionsResult>> ProcessRoleAssignmentsWithActionsRequest(
            string instanceId,
            RoleAssignmentsWithActionsRequest request,
            UnifiedUserIdentity userIdentity)
        {
            var defaultResults = request.Scopes.Distinct().ToDictionary(scp => scp, res => new RoleAssignmentsWithActionsResult() { Actions = [], Roles = [] });

            try
            {
                var httpClient = await CreateHttpClient();
                var response = await httpClient.PostAsync(
                    $"/instances/{instanceId}/roleassignments/querywithactions",
                    JsonContent.Create(request));

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Dictionary<string, RoleAssignmentsWithActionsResult>>(responseContent)!;
                }

                _logger.LogError("The call to the Authorization API returned an error: {StatusCode} - {ReasonPhrase}.", response.StatusCode, response.ReasonPhrase);
                return defaultResults;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error calling the Authorization API");
                return defaultResults;
            }
        }

        /// <inheritdoc/>
        public async Task<RoleAssignmentOperationResult> CreateRoleAssignment(
            string instanceId,
            RoleAssignmentRequest roleAssignmentRequest,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                var httpClient = await CreateHttpClient();
                var response = await httpClient.PostAsync(
                    $"/instances/{instanceId}/roleassignments",
                    JsonContent.Create(roleAssignmentRequest));

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<RoleAssignmentOperationResult>(responseContent);

                    if (result == null)
                        return new RoleAssignmentOperationResult() { Success = false };

                    return result;
                }

                _logger.LogError("The call to the Authorization API returned an error: {StatusCode} - {ReasonPhrase}.", response.StatusCode, response.ReasonPhrase);
                return new RoleAssignmentOperationResult() { Success = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error calling the Authorization API");
                return new RoleAssignmentOperationResult() { Success = false };
            }
        }

        /// <inheritdoc/>
        public async Task<List<RoleAssignment>> GetRoleAssignments(
            string instanceId,
            RoleAssignmentQueryParameters queryParameters,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                var httpClient = await CreateHttpClient();
                var response = await httpClient.PostAsync(
                    $"/instances/{instanceId}/roleassignments/query",
                    JsonContent.Create(queryParameters));

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<RoleAssignment>>(responseContent)!;
                }

                _logger.LogError("The call to the Authorization API returned an error: {StatusCode} - {ReasonPhrase}.", response.StatusCode, response.ReasonPhrase);
                return [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error calling the Authorization API");
                return [];
            }
        }

        /// <inheritdoc/>
        public async Task<RoleAssignmentOperationResult> DeleteRoleAssignment(
            string instanceId,
            string roleAssignment,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                var httpClient = await CreateHttpClient();
                var response = await httpClient.DeleteAsync(
                    $"/instances/{instanceId}/roleassignments/{roleAssignment}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<RoleAssignmentOperationResult>(responseContent);

                    if (result == null)
                        return new RoleAssignmentOperationResult() { Success = false };

                    return result;
                }

                _logger.LogError("The call to the Authorization API returned an error: {StatusCode} - {ReasonPhrase}.", response.StatusCode, response.ReasonPhrase);
                return new RoleAssignmentOperationResult() { Success = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error calling the Authorization API");
                return new RoleAssignmentOperationResult() { Success = false };
            }
        }

        /// <summary>
        /// Exception to the unified HTTP client factory when consuming the Authorization API.
        /// </summary>
        /// <returns></returns>
        private async Task<HttpClient> CreateHttpClient()
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_settings.APIUrl);

            var credentials = DefaultAuthentication.AzureCredential;
            var tokenResult = await credentials.GetTokenAsync(
                new([_settings.APIScope]),
                default);

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResult.Token);

            return httpClient;
        }
    }
}

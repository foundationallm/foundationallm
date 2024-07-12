using FoundationaLLM.Authorization.Models.Configuration;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly IHttpClientFactoryService _httpClientFactoryService;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(
            IHttpClientFactoryService httpClientFactoryService,
            IOptions<AuthorizationServiceSettings> options,
            ILogger<AuthorizationService> logger)
        {
            _settings = options.Value;
            _httpClientFactoryService = httpClientFactoryService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ActionAuthorizationResult> ProcessAuthorizationRequest(
            string instanceId,
            ActionAuthorizationRequest authorizationRequest,
            UnifiedUserIdentity userIdentity)
        {
            var defaultResults = authorizationRequest.ResourcePaths.Distinct().ToDictionary(rp => rp, auth => false);

            try
            {
                var httpClient = await _httpClientFactoryService.CreateClient(HttpClients.AuthorizationAPI, userIdentity);
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
            {
                _logger.LogError(ex, "There was an error calling the Authorization API");
                return new ActionAuthorizationResult { AuthorizationResults = defaultResults };
            }
        }

        /// <inheritdoc/>
        public async Task<RoleAssignmentResult> ProcessRoleAssignmentRequest(
            string instanceId,
            RoleAssignmentRequest roleAssignmentRequest,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                var httpClient = await _httpClientFactoryService.CreateClient(HttpClients.AuthorizationAPI, userIdentity);
                var response = await httpClient.PostAsync(
                    $"/instances/{instanceId}/roleassignments",
                    JsonContent.Create(roleAssignmentRequest));

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<RoleAssignmentResult>(responseContent);

                    if (result == null)
                        return new RoleAssignmentResult() { Success = false };

                    return result;
                }

                _logger.LogError("The call to the Authorization API returned an error: {StatusCode} - {ReasonPhrase}.", response.StatusCode, response.ReasonPhrase);
                return new RoleAssignmentResult() { Success = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error calling the Authorization API");
                return new RoleAssignmentResult() { Success = false };
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
                var httpClient = await _httpClientFactoryService.CreateClient(HttpClients.AuthorizationAPI, userIdentity);
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
        public async Task<List<object>> GetRoleAssignments(
            string instanceId,
            RoleAssignmentQueryParameters queryParameters,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                var httpClient = await _httpClientFactoryService.CreateClient(HttpClients.AuthorizationAPI, userIdentity);
                var response = await httpClient.PostAsync(
                    $"/instances/{instanceId}/roleassignments/query",
                    JsonContent.Create(queryParameters));

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<object>>(responseContent)!;
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
        public async Task<RoleAssignmentResult> RevokeRoleAssignment(
            string instanceId,
            string roleAssignment,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                var httpClient = await _httpClientFactoryService.CreateClient(HttpClients.AuthorizationAPI, userIdentity);
                var response = await httpClient.DeleteAsync(
                    $"/instances/{instanceId}/roleassignments/{roleAssignment}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<RoleAssignmentResult>(responseContent);

                    if (result == null)
                        return new RoleAssignmentResult() { Success = false };

                    return result;
                }

                _logger.LogError("The call to the Authorization API returned an error: {StatusCode} - {ReasonPhrase}.", response.StatusCode, response.ReasonPhrase);
                return new RoleAssignmentResult() { Success = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error calling the Authorization API");
                return new RoleAssignmentResult() { Success = false };
            }
        }
    }
}

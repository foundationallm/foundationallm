using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentAccessTokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace FoundationaLLM.Common.Authentication
{
    /// <summary>
    /// Handles authentication for agent access tokens.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AgentAccessTokenAuthenticationHandler"/> class.
    /// </remarks>
    /// <param name="resourceProviderServices">The <see cref="IResourceProviderService"/> resource provider services.</param>
    /// <param name="options">The monitor for the options instance.</param>
    /// <param name="logger">The <see cref="ILoggerFactory"/> used to create loggers.</param>
    /// <param name="encoder">The <see cref="UrlEncoder"/> used to perform URL character encoding.</param>
    public class AgentAccessTokenAuthenticationHandler(
        IEnumerable<IResourceProviderService> resourceProviderServices,
        IOptionsMonitor<AgentAccessTokenOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : AuthenticationHandler<AgentAccessTokenOptions>(options, logger, encoder)
    {
        private readonly IResourceProviderService _agentResourceProvider =
#pragma warning disable CS8601 // Possible null reference assignment.
            resourceProviderServices
                .SingleOrDefault(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Agent);
#pragma warning restore CS8601 // Possible null reference assignment.
        private readonly ILogger<AgentAccessTokenAuthenticationHandler> _logger =
            logger.CreateLogger<AgentAccessTokenAuthenticationHandler>();

        /// <summary>
        /// Handles authentication for agent access tokens.
        /// Supports both X-AGENT-ACCESS-TOKEN header and Authorization: Bearer header for OpenAI compatibility.
        /// </summary>
        /// <returns></returns>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string? agentAccessToken = null;

            // Check existing X-AGENT-ACCESS-TOKEN header (existing behavior - backward compatible)
            if (Request.Headers.TryGetValue(
                Constants.HttpHeaders.AgentAccessToken,
                out var headerToken))
            {
                agentAccessToken = headerToken.ToString();
            }
            // Check Authorization header for Bearer token (new behavior for OpenAI compatibility)
            else if (Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var headerValue = authHeader.ToString();
                if (headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    agentAccessToken = headerValue.Substring(7).Trim();
                }
            }

            if (string.IsNullOrWhiteSpace(agentAccessToken))
            {
                return AuthenticateResult.NoResult();
            }

            try
            {
                // The agent access token is used to handle requests to authenticate FoundationaLLM agents.
                var validationResult = await ValidateAgentAccessToken(agentAccessToken);

                if (validationResult?.Valid ?? false)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimConstants.Name, validationResult.VirtualIdentity!.Name!),
                        new Claim(ClaimConstants.Oid, validationResult.VirtualIdentity!.UserId!),
                        new Claim(ClaimConstants.ObjectId, validationResult.VirtualIdentity!.UserId!),
                        new Claim(ClaimConstants.PreferredUserName, validationResult.VirtualIdentity!.UPN!),
                        new Claim(ClaimConstants.Scope, "Data.Read")
                    };

                    claims.AddRange(
                        validationResult.VirtualIdentity!.GroupIds
                            .Select(groupId => new Claim(EntraUserClaimConstants.Groups, groupId))
                            .ToArray());

                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    var identities = new List<ClaimsIdentity> { identity };
                    var principal = new ClaimsPrincipal(identities);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);

                    return AuthenticateResult.Success(ticket);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Agent access token authentication failed.");
            }

            return AuthenticateResult.Fail("The agent access token is invalid.");
        }

        private async Task<AgentAccessTokenValidationResult> ValidateAgentAccessToken(
            string agentAccessToken)
        {
            var result = new AgentAccessTokenValidationResult
            {
                Valid = false,
                VirtualIdentity = null
            };

            try
            {
                if (_agentResourceProvider == null)
                    return result;

                if (!ClientSecretKey.TryParse(agentAccessToken, out var clientSecretKey)
                    || clientSecretKey == null)
                    return result;

                var agentClientSecretKey = AgentClientSecretKey.FromClientSecretKey(clientSecretKey);

                var serializedResult = await _agentResourceProvider.HandlePostAsync(
                    $"instances/{agentClientSecretKey.InstanceId}/providers/{ResourceProviderNames.FoundationaLLM_Agent}/{AgentResourceTypeNames.Agents}/{agentClientSecretKey.AgentName}/{AgentResourceTypeNames.AgentAccessTokens}/{ResourceProviderActions.Validate}",
                    JsonSerializer.Serialize(
                        new AgentAccessTokenValidationRequest
                        {
                            AccessToken = agentAccessToken
                        }),
                    null,
                    ServiceContext.ServiceIdentity!);

                return serializedResult as AgentAccessTokenValidationResult ?? result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Agent access token validation failed.");
                return result;
            }
        }
    }
}

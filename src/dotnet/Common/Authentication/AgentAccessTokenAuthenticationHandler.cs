using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentAccessTokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using System.Diagnostics.CodeAnalysis;
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

        /// <summary>
        /// Handles authentication for agent access tokens.
        /// </summary>
        /// <returns></returns>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(
                Constants.HttpHeaders.AgentAccessToken,
                out var agentAccessToken))
            {
                return AuthenticateResult.NoResult();
            }

            if (!string.IsNullOrWhiteSpace(agentAccessToken.ToString()))
            {
                var agentAccessTokenValue = agentAccessToken.ToString();
                // The agent access token is used to handle requests to authenticate FoundationaLLM agents.

                var validationResult = await ValidateAgentAccessToken(agentAccessTokenValue);

                if (validationResult?.Valid ?? false)
                {
                    var claims = new[]
                    {
                        new Claim(ClaimConstants.Name, validationResult.VirtualIdentity!.Name!),
                        new Claim(ClaimConstants.Oid, validationResult.VirtualIdentity!.UserId!),
                        new Claim(ClaimConstants.ObjectId, validationResult.VirtualIdentity!.UserId!),
                        new Claim(ClaimConstants.PreferredUserName, validationResult.VirtualIdentity!.UPN!)
                        // TODO: Add group claims here.
                    };
                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    var identities = new List<ClaimsIdentity> { identity };
                    var principal = new ClaimsPrincipal(identities);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);

                    return AuthenticateResult.Success(ticket);
                }
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

            if (_agentResourceProvider == null)
                return result;

            if (!ClientSecretKey.TryParse<AgentClientSecretKey>(agentAccessToken, out AgentClientSecretKey? agentClientSecretKey)
                || agentClientSecretKey == null)
                return result;

            var serializedResult = await _agentResourceProvider.HandlePostAsync(
                $"instances/{agentClientSecretKey.InstanceId}/providers/{ResourceProviderNames.FoundationaLLM_Agent}/{AgentResourceTypeNames.Agents}/{agentClientSecretKey.AgentName}/{AgentResourceTypeNames.AgentAccessTokens}/{ResourceProviderActions.Validate}",
                JsonSerializer.Serialize(
                    new AgentAccessTokenValidationRequest
                    {
                        AccessToken = agentAccessToken
                    }),
                null,
                DefaultAuthentication.ServiceIdentity!);

            return serializedResult as AgentAccessTokenValidationResult ?? result;
        }
    }
}

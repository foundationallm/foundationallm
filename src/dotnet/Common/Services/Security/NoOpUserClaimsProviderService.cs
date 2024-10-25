using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using System.Security.Claims;

namespace FoundationaLLM.Common.Services.Security
{
    /// <summary>
    /// No-op implementation of <see cref="IUserClaimsProviderService"/> in cases
    /// where the user identity is not needed.
    /// </summary>
    public class NoOpUserClaimsProviderService : IUserClaimsProviderService
    {
        /// <inheritdoc/>
        public UnifiedUserIdentity? GetUserIdentity(ClaimsPrincipal? userPrincipal) => null;

        /// <inheritdoc/>
        public List<string>? GetSecurityGroupIds(ClaimsPrincipal? userPrincipal) => null;

        /// <inheritdoc/>
        public bool IsServicePrincipal(ClaimsPrincipal userPrincipal) => false;
    }
}

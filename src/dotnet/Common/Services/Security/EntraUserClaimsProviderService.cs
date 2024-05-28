using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace FoundationaLLM.Common.Services.Security
{
    /// <summary>
    /// Provides a common interface for retrieving and resolving user claims
    /// from Microsoft Entra ID to a <see cref="UnifiedUserIdentity"/> object.
    /// </summary>
    public class EntraUserClaimsProviderService : IUserClaimsProviderService
    {
        /// <inheritdoc/>
        public UnifiedUserIdentity? GetUserIdentity(ClaimsPrincipal? userPrincipal)
        {
            if (userPrincipal == null)
            {
                return null;
            }
            return new UnifiedUserIdentity
            {
                Name = userPrincipal.FindFirstValue(ClaimConstants.Name) ?? string.Empty,
                Username = ResolveUsername(userPrincipal),
                UPN = ResolveUsername(userPrincipal),
                UserId = userPrincipal.FindFirstValue(ClaimConstants.Oid) ?? userPrincipal.FindFirstValue(ClaimConstants.ObjectId)
            };
        }

        /// <inheritdoc/>
        public bool IsServicePrincipal(ClaimsPrincipal userPrincipal) =>
            // Service Principal tokens do not have a "scp" claim
            userPrincipal.FindFirstValue(ClaimConstants.Scp) == null &&
            userPrincipal.FindFirstValue(ClaimConstants.Scope) == null;

        /// <summary>
        /// Resolves the username from the provided <see cref="ClaimsPrincipal"/> object.
        /// </summary>
        /// <param name="userPrincipal">The claims principal object that contains the authenticated identity.</param>
        /// <returns></returns>
        private string ResolveUsername(ClaimsPrincipal? userPrincipal) =>
            // Depending on which Microsoft Entra ID license the user has, the username may be extracted from the Identity.Name value or the preferred_username claim.
            (!string.IsNullOrWhiteSpace(userPrincipal?.Identity?.Name) ? userPrincipal.Identity.Name : userPrincipal?.FindFirstValue(ClaimConstants.PreferredUserName)) ?? string.Empty;
    }
}

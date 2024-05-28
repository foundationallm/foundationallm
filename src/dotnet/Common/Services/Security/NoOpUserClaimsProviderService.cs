using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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
        public bool IsServicePrincipal(ClaimsPrincipal userPrincipal) => false;
    }
}

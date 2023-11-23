﻿using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Services
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
                Name = userPrincipal.FindFirstValue("name"),
                Username = ResolveUsername(userPrincipal),
                UPN = ResolveUsername(userPrincipal)
            };
        }

        /// <summary>
        /// Resolves the username from the provided <see cref="ClaimsPrincipal"/> object.
        /// </summary>
        /// <param name="userPrincipal">The claims principal object that contains the authenticated identity.</param>
        /// <returns></returns>
        private string ResolveUsername(ClaimsPrincipal? userPrincipal)
        {
            // Depending on which Microsoft Entra ID license the user has, the username may be extracted from the Identity.Name value or the preferred_username claim.
            return (!string.IsNullOrWhiteSpace(userPrincipal?.Identity?.Name) ? userPrincipal.Identity.Name : userPrincipal?.FindFirstValue("preferred_username")) ?? string.Empty;
        }
    }
}

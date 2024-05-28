﻿using FoundationaLLM.Common.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Provides a common interface for retrieving and resolving user claims
    /// to a <see cref="UnifiedUserIdentity"/> object.
    /// </summary>
    public interface IUserClaimsProviderService
    {
        /// <summary>
        /// Returns a <see cref="UnifiedUserIdentity"/> object from the provided
        /// <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="userPrincipal">The principal that provides multiple
        /// claims-based identities.</param>
        /// <returns></returns>
        UnifiedUserIdentity? GetUserIdentity(ClaimsPrincipal? userPrincipal);

        /// <summary>
        /// Inidicates whether the specified principal is a service principal or not.
        /// </summary>
        /// <param name="userPrincipal">The <see cref="ClaimsPrincipal"/> object providing details about the security principal.</param>
        bool IsServicePrincipal(ClaimsPrincipal userPrincipal);
    }
}

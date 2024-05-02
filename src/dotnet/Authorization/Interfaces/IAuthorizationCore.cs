﻿using FoundationaLLM.Common.Models.Authorization;

namespace FoundationaLLM.Authorization.Interfaces
{
    /// <summary>
    /// Defines the methods for authorization core.
    /// </summary>
    public interface IAuthorizationCore
    {
        /// <summary>
        /// Processes an authorization request.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="authorizationRequest">The <see cref="ActionAuthorizationRequest"/> containing the details of the authorization request.</param>
        /// <returns>An <see cref="ActionAuthorizationResult"/> indicating whether the requested authorization was successfull or not for each resource path.</returns>
        ActionAuthorizationResult ProcessAuthorizationRequest(string instanceId, ActionAuthorizationRequest authorizationRequest);

        /// <summary>
        /// Checks if a specified security principal is allowed to process authorization requests. 
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="securityPrincipalId">The id of the security principal whose authorization is checked.</param>
        /// <returns>True if the security principal is allowed to process authorization requests.</returns>
        bool AllowAuthorizationRequestsProcessing(string instanceId, string securityPrincipalId);
    }
}

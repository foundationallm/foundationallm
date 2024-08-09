﻿using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;

namespace FoundationaLLM.Vectorization.Interfaces
{
    /// <summary>
    /// Interface for the vectorization request processor.
    /// </summary>
    public interface IVectorizationRequestProcessor
    {
        /// <summary>
        /// Processes an incoming vectorization request.
        /// </summary>
        /// <param name="vectorizationRequest">The <see cref="VectorizationRequest"/> object containing the details of the vectorization request.</param>
        /// <param name="userIdentity">The user identity.</param>
        /// <returns></returns>
        Task<VectorizationResult> ProcessRequest(VectorizationRequest vectorizationRequest, UnifiedUserIdentity? userIdentity);
    }
}

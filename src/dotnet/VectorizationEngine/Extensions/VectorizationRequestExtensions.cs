using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Vectorization.ResourceProviders;

namespace FoundationaLLM.Vectorization.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="VectorizationRequest"/>.
    /// </summary>
    public static class VectorizationRequestExtensions
    {
        /// <summary>
        /// Updates the vectorization request resource using the vectorization resource provider.
        /// Also updates the vectorization pipeline state if request is part of a pipeline.
        /// </summary>
        /// <param name="request">The vectorization request</param>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="vectorizationResourceProvider">The vectorization resource provider</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        public static async Task UpdateVectorizationRequestResource(
            this VectorizationRequest request,
            string instanceId,
            IResourceProviderService vectorizationResourceProvider,
            UnifiedUserIdentity userIdentity
        )
        {  
            // in the case of a new request, this updates the object id with the fully qualified object id, otherwise it remains the same.
            var result = await vectorizationResourceProvider.UpsertResourceAsync<VectorizationRequest, ResourceProviderUpsertResult>(
                instanceId,
                request,
                userIdentity);
            request.ObjectId = result.ObjectId;
        }

        /// <summary>
        /// Issues the "process" action on the vectorization request resource using the vectorization resource provider.        
        /// </summary>
        /// <param name="request">The vectorization request</param>
        /// <param name="vectorizationResourceProvider">The vectorization resource provider</param>              
        public static async Task<VectorizationResult> ProcessVectorizationRequest(
            this VectorizationRequest request,
            VectorizationResourceProviderService vectorizationResourceProvider
        ) => (VectorizationResult)await vectorizationResourceProvider.ExecuteActionAsync($"{request.ObjectId!}/process");
    }
}

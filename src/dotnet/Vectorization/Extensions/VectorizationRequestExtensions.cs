using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
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
        /// <param name="vectorizationResourceProvider">The vectorization resource provider</param>        
        public static async Task UpdateVectorizationRequestResource(
            this VectorizationRequest request,
            IResourceProviderService vectorizationResourceProvider            
        )
        {  
            if (request.ObjectId == null)
            {               
                //build the minimal object id for new requests
                request.ObjectId = $"/{VectorizationResourceTypeNames.VectorizationRequests}/{request.Name}";
            }
            // in the case of a new request, this updates the object id with the fully qualified object id, otherwise it remains the same.
            request.ObjectId = await vectorizationResourceProvider.UpsertResourceAsync(request.ObjectId, request);                      
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

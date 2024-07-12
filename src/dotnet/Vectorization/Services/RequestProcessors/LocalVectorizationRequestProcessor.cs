using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Services.VectorizationServices;

namespace FoundationaLLM.Vectorization.Services.RequestProcessors
{
    /// <summary>
    /// Processes the vectorization request internally.
    /// </summary>
    /// <param name="vectorizationServiceFactory"></param>
    public class LocalVectorizationRequestProcessor (VectorizationServiceFactory vectorizationServiceFactory) : IVectorizationRequestProcessor
    {        
        /// <inheritdoc/>
        public async Task<VectorizationResult> ProcessRequest(VectorizationRequest vectorizationRequest, UnifiedUserIdentity? userIdentity)
        {            
            var vectorizationService = vectorizationServiceFactory!.GetService(vectorizationRequest);
            var response = await vectorizationService.ProcessRequest(vectorizationRequest, userIdentity);
            return response;
        }
    }
}

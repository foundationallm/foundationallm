using FoundationaLLM.Common.Constants;
using FoundationaLLM.Vectorization.Models;

namespace FoundationaLLM.Vectorization.Handlers
{
    public class EmbeddingHandler : VectorizationStepHandlerBase
    {
        public EmbeddingHandler() : base(VectorizationSteps.Embed)
        {
        }

        protected override async Task ProcessRequest(VectorizationRequest request, VectorizationState state, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }
}

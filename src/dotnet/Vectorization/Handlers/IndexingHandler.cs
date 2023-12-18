using FoundationaLLM.Common.Constants;
using FoundationaLLM.Vectorization.Models;

namespace FoundationaLLM.Vectorization.Handlers
{
    public class IndexingHandler : VectorizationStepHandlerBase
    {
        public IndexingHandler() : base(VectorizationSteps.Index)
        {
        }

        protected override async Task ProcessRequest(VectorizationRequest request, VectorizationState state, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }
}

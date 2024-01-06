using FoundationaLLM.Common.Constants;
using FoundationaLLM.Vectorization.Models;

namespace FoundationaLLM.Vectorization.Handlers
{
    public class PartitionHandler : VectorizationStepHandlerBase
    {
        public PartitionHandler(
            Dictionary<string, string> parameters) : base(VectorizationSteps.Partition, parameters)
        {
        }

        protected override async Task ProcessRequest(VectorizationRequest request, VectorizationState state, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }
}

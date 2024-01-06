using FoundationaLLM.Vectorization.Exceptions;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;

namespace FoundationaLLM.Vectorization.Handlers
{
    public class VectorizationStepHandlerBase : IVectorizationStepHandler
    {
        protected readonly string _stepId = string.Empty;
        protected readonly Dictionary<string, string> _parameters;

        /// <inheritdoc/>
        public string StepId => _stepId;

        public VectorizationStepHandlerBase(
            string stepId, Dictionary<string, string> parameters)
        {
            _stepId = stepId;
            _parameters = parameters;
        }

        /// <inheritdoc/>
        public async Task Invoke(VectorizationRequest request, VectorizationState state, CancellationToken cancellationToken)
        {
            try
            {
                state.LogHandlerStart(this, request.Id);

                ValidateRequest(request);
                await ProcessRequest(request, state, cancellationToken);

                state.LogHandlerEnd(this, request.Id);
            }
            catch (Exception ex)
            {
                state.LogHandlerError(this, request.Id, ex);
            }
        }

        private void ValidateRequest(VectorizationRequest request)
        {
            if (request[_stepId] == null)
                throw new VectorizationException($"The request with id {request.Id} does not contain a step with id {_stepId}.");
        }

        protected virtual async Task ProcessRequest(VectorizationRequest request, VectorizationState state, CancellationToken cancellationToken) =>
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
    }
}

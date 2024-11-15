using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Vectorization;

namespace FoundationaLLM.Gateway.Interfaces
{
    /// <summary>
    /// Defines the interface of the FoundationaLLM Gateway core.
    /// </summary>
    public interface IGatewayCore
    {
        /// <summary>
        /// Starts the Gateway service, allowing it to initialize.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> signaling cancellation.</param>
        /// <returns></returns>
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Stops the Gateway service, allowing it to cleanup.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> signaling cancellation.</param>
        /// <returns></returns>
        Task StopAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Executes the Gateway service until cancellation is signaled.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> signaling cancellation.</param>
        /// <returns></returns>
        Task ExecuteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Starts an embedding operation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="embeddingRequest">The <see cref="TextEmbeddingRequest"/> object containing the details of the embedding operation.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <returns>A <see cref="TextEmbeddingResult"/> object with the outcome of the operation.</returns>
        Task<TextEmbeddingResult> StartEmbeddingOperation(string instanceId, TextEmbeddingRequest embeddingRequest, UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Retrieves the outcome of an embedding operation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="operationId">The unique identifier of the text embedding operation.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <returns>A <see cref="TextEmbeddingResult"/> object with the outcome of the operation.</returns>
        Task<TextEmbeddingResult> GetEmbeddingOperationResult(string instanceId, string operationId, UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Creates a new LLM-based agent capability.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="capabilityCategory">The category of the capability.</param>
        /// <param name="capabilityName">The name of the capability to be created.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <param name="parameters">A dictionary of parameter values used to create the capability.</param>
        /// <returns>A dictionary of output values.</returns>
        /// <remarks>
        /// The supported categories are:
        /// <list type="bullet">
        /// <item>
        /// OpenAI.Assistants (the names of the keys for the parameters and output dictionaries are defined in <see cref="OpenAIAgentCapabilityParameterNames"/>)
        /// </item>
        /// </list>
        /// </remarks>
        Task<Dictionary<string, object>> CreateAgentCapability(string instanceId, string capabilityCategory, string capabilityName, UnifiedUserIdentity userIdentity, Dictionary<string, object>? parameters = null);
    }
}

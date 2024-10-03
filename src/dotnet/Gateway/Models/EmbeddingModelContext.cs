using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FoundationaLLM.Gateway.Models
{
    /// <summary>
    /// Provides context associated with an embedding model.
    /// </summary>
    /// <param name="embeddingOperations">The global dictionary of <see cref="EmbeddingOperationContext"/> objects indexed by operation identifier.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class EmbeddingModelContext(
        ConcurrentDictionary<string, EmbeddingOperationContext> embeddingOperations,
        ILogger<EmbeddingModelContext> logger)
    {
        private readonly ConcurrentDictionary<string, EmbeddingOperationContext> _embeddingOperations = embeddingOperations;
        private readonly ILogger<EmbeddingModelContext> _logger = logger;
        private readonly object _syncRoot = new();

        /// <summary>
        /// The name of the embedding model.
        /// </summary>
        public required string ModelName { get; set; }

        /// <summary>
        /// A list of <see cref="EmbeddingModelDeploymentContext"/> objects providing contexts for the available deployments for the model.
        /// </summary>
        public List<EmbeddingModelDeploymentContext> DeploymentContexts { get; set; } = [];

        /// <summary>
        /// The list of active embedding operation identifiers.
        /// </summary>
        private readonly List<string> _embeddingOperationIds = [];

        public void AddEmbeddingOperationContext(EmbeddingOperationContext embeddingOperationContext)
        {
            _embeddingOperations.AddOrUpdate(
                embeddingOperationContext.Result.OperationId!,
                embeddingOperationContext,
            (k, v) => v);

            lock (_syncRoot)
            {
                if (embeddingOperationContext.Prioritized)
                {
                    // Prioritized contexts get added to the front of the queue.
                    _embeddingOperationIds.Insert(0, embeddingOperationContext.Result.OperationId!);
                }
                else
                {
                    // Queue normally.
                    _embeddingOperationIds.Add(embeddingOperationContext.Result.OperationId!);
                }
            }
        }

        /// <summary>
        /// Processes embedding operations in a continuous loop.
        /// </summary>
        /// <param name="cancellationToken">Notifies that the operation must be cancelled.</param>
        /// <returns></returns>
        public async Task ProcessOperations(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Embedding operations processing started for the {ModelName} model.", ModelName);

            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return;

                try
                {
                    lock (_syncRoot)
                    {
                        var currentDeploymentContextIndex = 0;
                        var capacityReached = false;

                        foreach (var operationId in _embeddingOperationIds)
                        {
                            if (_embeddingOperations.TryGetValue(operationId, out var operationContext)
                                && operationContext.Result.InProgress)
                                foreach (var inputTextChunk in operationContext.Result.TextChunks
                                    .Where(tc => tc.Embedding == null)
                                    .Select(tc => operationContext.InputTextChunks[tc.Position - 1]))
                                {
                                    if (!DeploymentContexts[currentDeploymentContextIndex].TryAddInputTextChunk(inputTextChunk))
                                    {
                                        currentDeploymentContextIndex++;
                                        if (currentDeploymentContextIndex == DeploymentContexts.Count)
                                        {
                                            // We're at capacity.
                                            capacityReached = true;
                                            break;
                                        }
                                    }
                                }
                            if (capacityReached) break;
                        }
                    }

                    // Use all available deployments to get embeddings for the input text chunks.
                    var results = await Task.WhenAll(DeploymentContexts
                        .Where(dc => dc.HasInput)
                        .Select(dc => dc.GetEmbeddingsForInputTextChunks()));

                    // Record all failed operations
                    foreach (var failedOperation in results
                        .Where(r => r.Failed)
                        .SelectMany(r => r.TextChunks)
                        .GroupBy(x => x.OperationId)
                        .Select(g => new
                        {
                            OperationId = g.Key,
                            Positions = g.Select(tc => tc.Position).ToList()
                        }))
                    {
                        _logger.LogError($"An error occured in text embedding operation {failedOperation.OperationId}. "
                            + $"The following text chunk positions were not embeded: {string.Join(", ", failedOperation.Positions)}.");
                    }

                    // Set the embeddings for all successful operations.
                    foreach (var successfulOperation in results
                        .Where(r => !r.InProgress)
                        .SelectMany(r => r.TextChunks)
                        .GroupBy(x => x.OperationId)
                        .Select(g => new
                        {
                            OperationId = g.Key,
                            TextChunks = g.ToList()
                        }))
                    {
                        _embeddingOperations[successfulOperation.OperationId!].SetEmbeddings(successfulOperation.TextChunks);

                        lock (_syncRoot)
                        {
                            if (!_embeddingOperations[successfulOperation.OperationId!].Result.InProgress)
                            _embeddingOperationIds.Remove(successfulOperation.OperationId!);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while attempting to execute a processing cycle.");
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}

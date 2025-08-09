using FoundationaLLM.Gateway.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FoundationaLLM.Gateway.Services
{
    /// <summary>
    /// Provides context associated with an embedding model.
    /// </summary>
    /// <param name="textOperationContexts">The global dictionary of <see cref="TextOperationContext"/> objects indexed by operation identifier.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class ModelContext(
        ConcurrentDictionary<string, TextOperationContext> textOperationContexts,
        ILogger<ModelContext> logger)
    {
        private readonly ConcurrentDictionary<string, TextOperationContext> _textOperationContexts = textOperationContexts;
        private readonly ILogger<ModelContext> _logger = logger;
        private readonly object _syncRoot = new();

        /// <summary>
        /// The name of the embedding model.
        /// </summary>
        public required string ModelName { get; set; }

        /// <summary>
        /// A list of <see cref="ModelDeploymentContext"/> objects providing contexts for the available deployments for the model.
        /// </summary>
        public List<ModelDeploymentContext> DeploymentContexts { get; set; } = [];

        /// <summary>
        /// The list of active text operation identifiers.
        /// </summary>
        private readonly List<string> _textOperationIds = [];

        public void AddTextOperationContext(TextOperationContext textOperationContext)
        {
            _textOperationContexts.AddOrUpdate(
                textOperationContext.Result.OperationId!,
                textOperationContext,
                (k, v) => v);

            lock (_syncRoot)
            {
                if (textOperationContext.Prioritized)
                {
                    // Prioritized contexts get added to the front of the queue.
                    _textOperationIds.Insert(0, textOperationContext.Result.OperationId!);
                }
                else
                {
                    // Queue normally.
                    _textOperationIds.Add(textOperationContext.Result.OperationId!);
                }
            }
        }

        /// <summary>
        /// Processes text operations in a continuous loop.
        /// </summary>
        /// <param name="cancellationToken">Notifies that the processing loop must be cancelled.</param>
        /// <returns></returns>
        public async Task ProcessTextOperations(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Text operations processing started for the {ModelName} model.", ModelName);

            long cycleCount = 0;
            var lastActiveWorkTimestamp = DateTimeOffset.UtcNow;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return;

                // Wait for longer if the last active work was more than 60 seconds ago.
                if ((DateTimeOffset.UtcNow - lastActiveWorkTimestamp).TotalSeconds >= 60)
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                else
                    await Task.Delay(TimeSpan.FromSeconds(0.1), cancellationToken);

                cycleCount++;
                var cycleStartTime = DateTimeOffset.UtcNow;
                if (cycleCount % 100 == 0)
                    _logger.LogInformation("[{ModelName}]: Processing cycle {CycleCount} started at {CycleStartTime}.",
                        ModelName,
                        cycleCount,
                        cycleStartTime);

                try
                {
                    lock (_syncRoot)
                    {
                        if(_textOperationIds.Count == 0
                            || !_textOperationContexts.Values.Any(eoc => eoc.Result.InProgress))
                            continue; // Nothing to process.

                        var currentDeploymentContextIndex = 0;
                        var capacityReached = false;

                        foreach (var operationId in _textOperationIds)
                        {
                            if (_textOperationContexts.TryGetValue(operationId, out var textOperationContext)
                                && textOperationContext.Result.InProgress)
                                foreach (var inputTextChunk in textOperationContext.Result.TextChunks
                                    .Where(resultTextChunk => !textOperationContext.TextChunkCompletenessChecker(resultTextChunk))
                                    .Select(resultTextChunk => textOperationContext.InputTextChunks[resultTextChunk.Position]))
                                {
                                    var modelDeploymentContext = DeploymentContexts[currentDeploymentContextIndex];
                                    if (!modelDeploymentContext.TryAddInputTextChunk(
                                        inputTextChunk,
                                        textOperationContext.ModelParameters))
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

                    // Use all available deployments to process the text operations for the input text chunks.
                    var results = (await Task.WhenAll(DeploymentContexts
                        .Where(dc => dc.HasInput)
                        .Select(async dc => await dc.ProcessTextOperationRequests())))
                            .SelectMany(r => r);

                    // Record all failed operations
                    var failedResults = results
                        .Where(r => r.Failed)
                        .ToList();
                    if (failedResults.Count > 0)
                    {
                        var failedOperations = failedResults
                            .Select(fr => fr.FailedOperationIds.Select(frid => new
                            {
                                OperationId = frid,
                                fr.ErrorMessage
                            }))
                            .SelectMany(x => x)
                            .GroupBy(x => x.OperationId)
                            .Select(g => new
                            {
                                OperationId = g.Key,
                                ErrorMessages = string.Join(
                                    Environment.NewLine,
                                    [.. g.ToList().Select(x => x.ErrorMessage)
                                        .Prepend($"Operation id {g.Key}:")
                                        .Append(string.Empty)])
                            });

                        _logger.LogError("The following text operations had failures: {NewLine}{FailedOperations}",
                            Environment.NewLine,
                            string.Join(
                                Environment.NewLine,
                                [.. failedOperations.Select(fo => fo.ErrorMessages)]));

                        lock (_syncRoot)
                        {
                            foreach (var failedOperation in failedOperations)
                            {
                                _textOperationContexts[failedOperation.OperationId].SetIntermediateError(failedOperation.ErrorMessages);
                                if (!_textOperationContexts[failedOperation.OperationId].Result.InProgress)
                                    _textOperationIds.Remove(failedOperation.OperationId);
                            }
                        }
                    }

                    // Update the text chunks for all successful operations.
                    foreach (var successfulOperation in results
                        .Where(r => !r.Failed)
                        .SelectMany(r => r.TextChunks)
                        .GroupBy(x => x.OperationId)
                        .Select(g => new
                        {
                            OperationId = g.Key,
                            TextChunks = g.ToList()
                        }))
                    {
                        _textOperationContexts[successfulOperation.OperationId!].UpdateTextChunks(successfulOperation.TextChunks);

                        lock (_syncRoot)
                        {
                            if (!_textOperationContexts[successfulOperation.OperationId!].Result.InProgress)
                                _textOperationIds.Remove(successfulOperation.OperationId!);
                        }
                    }

                    // Better to set the last active work timestamp after the cycle's processing is complete,
                    // to avoid considering time spent in processing as idle time.
                    lastActiveWorkTimestamp = DateTimeOffset.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while attempting to execute a processing cycle.");
                }

                _logger.LogInformation("[{ModelName}]: Completed cycle {CycleCount} in {CycleDurationSeconds} seconds.",
                    ModelName,
                    cycleCount,
                    $"{(DateTimeOffset.UtcNow - cycleStartTime).TotalSeconds:F2}");
            }
        }
    }
}

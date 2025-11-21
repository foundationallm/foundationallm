using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.Models.Context.Knowledge;
using FoundationaLLM.Common.Models.Services;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Context.Services
{
    /// <summary>
    /// Provides methods for querying knowledge sources.
    /// </summary>
    /// <param name="knowledgeSourceId">The identifier of the knowldege source.</param>
    /// <param name="knowledgeUnitQueryEngines"> The collection of knowledge unit query engines used to execute queries.</param>
    /// <param name="logger"> The logger used for logging.</param>
    public class KnowledgeSourceQueryEngine(
        string knowledgeSourceId,
        IEnumerable<KnowledgeUnitQueryEngine> knowledgeUnitQueryEngines,
        ILogger<KnowledgeSourceQueryEngine> logger)
    {
        private readonly string _knowledgeSourceId = knowledgeSourceId;
        private readonly IEnumerable<KnowledgeUnitQueryEngine> _knowledgeUnitQueryEngines = knowledgeUnitQueryEngines;
        private readonly ILogger<KnowledgeSourceQueryEngine> _logger = logger;

        /// <summary>
        /// Executes an asynchronous query against the context knowledge source.
        /// </summary>
        /// <remarks>Use this method to retrieve data from the context knowledge source based on the 
        /// specified query parameters. Ensure that the <paramref name="queryRequest"/> object  is properly populated
        /// before calling this method.</remarks>
        /// <param name="queryRequest">The request object containing the parameters and criteria for the query.  This cannot be <see
        /// langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains  a <see
        /// cref="ContextKnowledgeSourceQueryResponse"/> object with the results of the query.</returns>
        public async Task<Result<ContextKnowledgeSourceQueryResponse>> QueryAsync(
            ContextKnowledgeSourceQueryRequest queryRequest)
        {
            var queryResultTasks = _knowledgeUnitQueryEngines
                .Select(engine => engine.QueryAsync(queryRequest))
                .ToList();
            var queryResults = await Task.WhenAll(queryResultTasks);

            var queryResult = ConsolidateResponses(queryResults);

            return
                queryResult.TryGetValue(out var queryResponse)
                && (queryRequest.FormatResponse ?? false)
                    ? Result<ContextKnowledgeSourceQueryResponse>.Success(
                        FormatQueryResponse(queryResponse))
                    : queryResult;
        }

        private Result<ContextKnowledgeSourceQueryResponse> ConsolidateResponses(
            IEnumerable<Result<ContextKnowledgeSourceQueryResponse>> queryResults)
        {
            if (queryResults.Count() == 1)
                // If there's only one response, return it directly
                return queryResults.First();

            foreach (var result in queryResults.Where(qr => !qr.IsSuccess))
                _logger.LogWarning(
                    "Knowledge unit {KnowledgeUnitId} failed to process the query with error: {ErrorMessage}",
                    result.Error?.Instance ?? "N/A",
                    result.Error?.Detail ?? "N/A");

            if (!queryResults.Any(qr => qr.IsSuccess))
            {
                _logger.LogError(
                    "All knowledge unit queries failed for knowledge source {KnowledgeSourceId}.",
                    _knowledgeSourceId);

                // Not getting any responses is an error condition.
                return Result<ContextKnowledgeSourceQueryResponse>.FailureFromErrorMessage(
                    "None of the knowledge unit queries returned a successful response.",
                    instance: _knowledgeSourceId);
            }

            var consolidatedResponse = new ContextKnowledgeSourceQueryResponse
            {
                Source = _knowledgeSourceId,
                VectorStoreResponse = new ContextVectorStoreResponse(),
                KnowledgeGraphResponse = new ContextKnowledgeGraphResponse()
            };
            foreach (var result in queryResults.Where(qr => qr.IsSuccess))
            {
                if (result.Value!.VectorStoreResponse is not null)
                {
                    consolidatedResponse.VectorStoreResponse.TextChunks
                        .AddRange(result.Value!.VectorStoreResponse.TextChunks);
                }

                if (result.Value!.KnowledgeGraphResponse is not null)
                {
                    consolidatedResponse.KnowledgeGraphResponse.Entities
                        .AddRange(result.Value!.KnowledgeGraphResponse.Entities);
                    consolidatedResponse.KnowledgeGraphResponse.RelatedEntities
                        .AddRange(result.Value!.KnowledgeGraphResponse.RelatedEntities);
                    consolidatedResponse.KnowledgeGraphResponse.Relationships
                        .AddRange(result.Value!.KnowledgeGraphResponse.Relationships);
                    consolidatedResponse.KnowledgeGraphResponse.TextChunks
                        .AddRange(result.Value!.KnowledgeGraphResponse.TextChunks);
                }
                
            }
            return Result<ContextKnowledgeSourceQueryResponse>.Success(consolidatedResponse);
        }

        private ContextKnowledgeSourceQueryResponse FormatQueryResponse(
            ContextKnowledgeSourceQueryResponse queryResponse)
        {
            var formattedResponseChunks = new List<string>();

            var textChunks = new List<ContextTextChunk>();

            if (queryResponse.KnowledgeGraphResponse is not null)
            {
                if (queryResponse.KnowledgeGraphResponse.Entities.Count > 0)
                {
                    formattedResponseChunks.AddRange(
                        [
                            "The following entities are relevant for the answer:",
                            Environment.NewLine
                        ]);

                    formattedResponseChunks.AddRange(
                        queryResponse.KnowledgeGraphResponse.Entities
                            .Select(e => $"- {e.Name} (of type {e.Type}): {e.SummaryDescription}"));
                    formattedResponseChunks.Add(Environment.NewLine);
                }

                if (queryResponse.KnowledgeGraphResponse.RelatedEntities.Count > 0)
                {
                    formattedResponseChunks.AddRange(
                        [
                            "The following related entities are also relevant for the answer:",
                            Environment.NewLine
                        ]);

                    formattedResponseChunks.AddRange(
                        queryResponse.KnowledgeGraphResponse.RelatedEntities
                            .Select(e => $"- {e.Name} (of type {e.Type}): {e.SummaryDescription}"));
                    formattedResponseChunks.Add(Environment.NewLine);
                }

                if (queryResponse.KnowledgeGraphResponse.Relationships.Count > 0)
                {
                    formattedResponseChunks.AddRange(
                        [
                            "The following relationship between entities are relevant for the answer:",
                            Environment.NewLine
                        ]);

                    formattedResponseChunks.AddRange(
                        queryResponse.KnowledgeGraphResponse.Relationships
                            .Select(r => $"- {r.Source} (of type {r.SourceType}) and {r.Target} (of type {r.TargetType}): {r.SummaryDescription}"));
                    formattedResponseChunks.Add(Environment.NewLine);
                }

                if (queryResponse.KnowledgeGraphResponse.TextChunks.Count > 0)
                    textChunks.AddRange(queryResponse.KnowledgeGraphResponse.TextChunks);
            }

            if (queryResponse.VectorStoreResponse is not null
                && queryResponse.VectorStoreResponse.TextChunks.Count > 0)
                textChunks.AddRange(queryResponse.VectorStoreResponse.TextChunks);

            if (textChunks.Count > 0)
            {
                formattedResponseChunks.AddRange(
                    [
                        "The following information is relevant for the answer:",
                        Environment.NewLine
                    ]);

                formattedResponseChunks.AddRange(
                    textChunks
                        .Select(tc => $"- {tc.Content}"));
            }

            var formattedResponse = string.Join(Environment.NewLine, formattedResponseChunks);

            queryResponse.TextResponse = formattedResponse;
            queryResponse.VectorStoreResponse = null;
            queryResponse.KnowledgeGraphResponse = null;
            queryResponse.ContentReferences =
                [.. textChunks.Select(tc => tc.Metadata)];
            return queryResponse;
        }
    }
}

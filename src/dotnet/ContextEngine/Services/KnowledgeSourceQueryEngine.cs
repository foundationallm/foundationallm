using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.Models.Context.Knowledge;
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
        public async Task<ContextKnowledgeSourceQueryResponse> QueryAsync(
            ContextKnowledgeSourceQueryRequest queryRequest)
        {
            var queryResultTasks = _knowledgeUnitQueryEngines
                .Select(engine => engine.QueryAsync(queryRequest))
                .ToList();
            var queryResponses = await Task.WhenAll(queryResultTasks);

            var queryResponse = ConsolidateResponses(queryResponses);

            if (!queryResponse.IsSuccess)
                return queryResponse;

            return (queryRequest.FormatResponse ?? false)
                ? FormatQueryResponse(queryResponse)
                : queryResponse;
        }

        private ContextKnowledgeSourceQueryResponse ConsolidateResponses(
            IEnumerable<ContextKnowledgeSourceQueryResponse> queryResponses)
        {
            if (queryResponses.Count() == 1)
                // If there's only one response, return it directly
                return queryResponses.First();

            foreach (var response in queryResponses.Where(qr => !qr.IsSuccess))
                _logger.LogWarning(
                    "Knowledge unit {KnowledgeUnitId} failed to process the query with error: {ErrorMessage}",
                    response.Source,
                    response.ErrorMessage);

            if (!queryResponses.Any(qr => qr.IsSuccess))
            {
                _logger.LogError(
                    "All knowledge unit queries failed for knowledge source {KnowledgeSourceId}.",
                    _knowledgeSourceId);

                // Not getting any responses is an error condition.
                return new ContextKnowledgeSourceQueryResponse
                {
                    Source = _knowledgeSourceId,
                    IsSuccess = false,
                    ErrorMessage = "None of the knowledge unit queries returned a successful response."
                };
            }

            var consolidatedResponse = new ContextKnowledgeSourceQueryResponse
            {
                Source = _knowledgeSourceId,
                IsSuccess = true,
                VectorStoreResponse = new ContextVectorStoreResponse(),
                KnowledgeGraphResponse = new ContextKnowledgeGraphResponse()
            };
            foreach (var response in queryResponses.Where(qr => qr.IsSuccess))
            {
                if (response.VectorStoreResponse is not null)
                {
                    consolidatedResponse.VectorStoreResponse.TextChunks
                        .AddRange(response.VectorStoreResponse.TextChunks);
                }

                if (response.KnowledgeGraphResponse is not null)
                {
                    consolidatedResponse.KnowledgeGraphResponse.Entities
                        .AddRange(response.KnowledgeGraphResponse.Entities);
                    consolidatedResponse.KnowledgeGraphResponse.RelatedEntities
                        .AddRange(response.KnowledgeGraphResponse.RelatedEntities);
                    consolidatedResponse.KnowledgeGraphResponse.Relationships
                        .AddRange(response.KnowledgeGraphResponse.Relationships);
                    consolidatedResponse.KnowledgeGraphResponse.TextChunks
                        .AddRange(response.KnowledgeGraphResponse.TextChunks);
                }
                
            }
            return consolidatedResponse;
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

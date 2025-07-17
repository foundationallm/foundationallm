using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.Models.Context.Knowledge;
using FoundationaLLM.Common.Models.Knowledge;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Common.Models.ResourceProviders.Vector;
using FoundationaLLM.Context.Models;
using MathNet.Numerics;
using Microsoft.Extensions.Logging;
using OpenAI.Embeddings;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace FoundationaLLM.Context.Services
{
    /// <summary>
    /// Provides methods for querying knowledge sources.
    /// </summary>
    public class KnowledgeSourceQueryEngine
    {
        private readonly KnowledgeSource _knowledgeSource;
        private readonly CachedKnowledgeSource _cachedKnowledgeSource;
        private readonly VectorDatabase _vectorDatabase;
        private readonly ILogger<KnowledgeSourceQueryEngine> _logger;

        private const string KEY_FIELD_NAME = "Id";

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeSourceQueryEngine"/> class.
        /// </summary>
        /// <param name="knowledgeSource">The knowledge source definition to be queried by the engine.</param>
        /// <param name="cachedKnowledgeSource">The cached version of the knowledge source to be queried.</param>
        /// <param name="vectorDatabase">The vector database associated with the knowledge source.</param>
        /// <param name="logger">The logger used for logging operations within the query engine.</param>
        public KnowledgeSourceQueryEngine(
            KnowledgeSource knowledgeSource,
            CachedKnowledgeSource cachedKnowledgeSource,
            VectorDatabase vectorDatabase,
            ILogger<KnowledgeSourceQueryEngine> logger)
        {
            _knowledgeSource = knowledgeSource;
            _cachedKnowledgeSource = cachedKnowledgeSource;
            _vectorDatabase = vectorDatabase;
            _logger = logger;
        }

        public async Task<ContextKnowledgeSourceQueryResponse> QueryAsync(
            ContextKnowledgeSourceQueryRequest queryRequest)
        {

            try
            {
                ValidateQueryRequest(queryRequest);

                var vectorStoreId = string.IsNullOrWhiteSpace(_knowledgeSource.VectorStoreId)
                    ? queryRequest.VectorStoreId
                    : _knowledgeSource.VectorStoreId;
                if (string.IsNullOrWhiteSpace(vectorStoreId))
                    return new ContextKnowledgeSourceQueryResponse
                    {
                        Success = false,
                        ErrorMessage = $"The knowledge source {_knowledgeSource.Name} does not have a vector store identifier specified and none was provided in the query request."
                    };

                var userPromptEmbedding = await _cachedKnowledgeSource.EmbeddingClient.GenerateEmbeddingAsync(
                    queryRequest.UserPrompt,
                    new EmbeddingGenerationOptions
                    {
                        Dimensions = _knowledgeSource.EmbeddingDimensions
                    });

                var queryResponse = new ContextKnowledgeSourceQueryResponse
                {
                    Success = true
                };

                if (queryRequest.VectorStoreQuery is not null)
                {
                    #region Direct filtering on the vector store

                    var matchingDocumentsFilter = GetFilter(
                        vectorStoreId,
                        [],
                        queryRequest.VectorStoreMetadataFilter);

                    var matchingDocuments = await _cachedKnowledgeSource.SearchService.SearchDocuments(
                        _vectorDatabase.DatabaseName,
                        [
                            KEY_FIELD_NAME,
                            _vectorDatabase.ContentPropertyName,
                            _vectorDatabase.MetadataPropertyName
                        ],
                        matchingDocumentsFilter,
                        queryRequest.VectorStoreQuery.UseHybridSearch
                            ? queryRequest.UserPrompt!
                            : null,
                        userPromptEmbedding.Value.ToFloats(),
                        _vectorDatabase.EmbeddingPropertyName,
                        queryRequest.VectorStoreQuery.TextChunksSimilarityThreshold,
                        queryRequest.VectorStoreQuery.TextChunksMaxCount,
                        queryRequest.VectorStoreQuery.UseSemanticRanking);

                    queryResponse.TextChunks = [.. matchingDocuments
                        .Select(md => new ContextTextChunk
                        {
                            Content = md.GetString(_vectorDatabase.ContentPropertyName),
                            Metadata = md.GetObject(_vectorDatabase.MetadataPropertyName).ToDictionary()
                        })
                    ];

                    #endregion
                }

                if (queryRequest.KnowledgeGraphQuery is not null)
                {
                    var matchingEntities = GetMatchingKnowledgeGraphEntities(
                        _cachedKnowledgeSource.KnowledgeGraph!,
                        userPromptEmbedding.Value.ToFloats(),
                        queryRequest.KnowledgeGraphQuery.MappedEntitiesSimilarityThreshold,
                        queryRequest.KnowledgeGraphQuery.MappedEntitiesMaxCount,
                        queryRequest.KnowledgeGraphQuery.RelationshipsMaxDepth,
                        queryRequest.KnowledgeGraphQuery.AllEntitiesMaxCount);

                    queryResponse.KnowledgeGraphResponse = new ContextKnowledgeGraphResponse
                    {
                        Entities = [.. matchingEntities.Entities
                            .Select(e => new KnowledgeEntity
                            {
                                Type = e.Type,
                                Name = e.Name,
                                SummaryDescription = e.SummaryDescription
                            })
                        ],
                        RelatedEntities = [.. matchingEntities.RelatedEntities
                            .Select(re => new KnowledgeEntity
                            {
                                Type = re.Type,
                                Name = re.Name,
                                SummaryDescription = re.SummaryDescription
                            })
                        ],
                        Relationships = [.. matchingEntities.Relationships
                            .Select(r => new KnowledgeRelationship
                            {
                                SourceType = r.SourceType,
                                Source = r.Source,
                                TargetType = r.TargetType,
                                Target = r.Target,
                                SummaryDescription = r.SummaryDescription
                            })
                        ]
                    };

                    if (queryRequest.KnowledgeGraphQuery.VectorStoreQuery is not null)
                    {
                        var matchingDocumentsFilter = GetFilter(
                            vectorStoreId,
                            [.. matchingEntities.Entities.SelectMany(e => e.ChunkIds).Distinct()],
                            queryRequest.VectorStoreMetadataFilter);


                        var matchingDocuments = await _cachedKnowledgeSource.SearchService.SearchDocuments(
                            _vectorDatabase.DatabaseName,
                            [
                                KEY_FIELD_NAME,
                                _vectorDatabase.ContentPropertyName,
                                _vectorDatabase.MetadataPropertyName
                            ],
                            matchingDocumentsFilter,
                            queryRequest.KnowledgeGraphQuery.VectorStoreQuery.UseHybridSearch
                                ? queryRequest.UserPrompt!
                                : null,
                            userPromptEmbedding.Value.ToFloats(),
                            _vectorDatabase.EmbeddingPropertyName,
                            queryRequest.KnowledgeGraphQuery.VectorStoreQuery.TextChunksSimilarityThreshold,
                            queryRequest.KnowledgeGraphQuery.VectorStoreQuery.TextChunksMaxCount,
                            queryRequest.KnowledgeGraphQuery.VectorStoreQuery.UseSemanticRanking);

                        queryResponse.KnowledgeGraphResponse.TextChunks = [.. matchingDocuments
                            .Select(md => new ContextTextChunk
                            {
                                Content = md.GetString(_vectorDatabase.ContentPropertyName),
                                Metadata = md.GetObject(_vectorDatabase.MetadataPropertyName).ToDictionary()
                            })
                        ];
                    }
                }

                return queryResponse;
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, "Invalid query request for the knowledge source {KnowledgeSourceId}.",
                    _knowledgeSource.Name);
                return new ContextKnowledgeSourceQueryResponse
                {
                    Success = false,
                    ErrorMessage = $"Invalid query request: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while querying the knowledge source {KnowledgeSourceId}.",
                    _knowledgeSource.Name);
                return new ContextKnowledgeSourceQueryResponse
                {
                    Success = false,
                    ErrorMessage = $"An error occurred while querying the knowledge source {_knowledgeSource.Name}."
                };
            }
        }

        private void ValidateQueryRequest(
            ContextKnowledgeSourceQueryRequest queryRequest)
        {
            if (string.IsNullOrWhiteSpace(queryRequest.UserPrompt))
                throw new ValidationException("User prompt cannot be null or empty.");

            if (queryRequest.VectorStoreQuery == null && queryRequest.KnowledgeGraphQuery == null)
                throw new ValidationException("At least one of VectorStoreQuery or KnowledgeGraphQuery must be provided.");
        }

        private (List<KnowledgeEntity> Entities, List<KnowledgeEntity> RelatedEntities, List<KnowledgeRelationship> Relationships)
            GetMatchingKnowledgeGraphEntities(
                CachedKnowledgeGraph knowledgeGraph,
                ReadOnlyMemory<float> userPromptEmbedding,
                float similarityThreshold,
                int matchedEntitiesMaxCount,
                int relationshipsMaxDepth,
                int allEntitiesMaxCount)
        {
            var similarEntities = new List<(KnowledgeEntity Entity, float SimilarityScore)>();
            var maxSimilarity = 0f;
            var minSimilarity = 1f;
            foreach (var entity in knowledgeGraph.Entities)
            {
                var similarity = 1.0f - Distance.Cosine(
                    userPromptEmbedding.ToArray(),
                    entity.SummaryDescriptionEmbedding);
                if (similarity >= similarityThreshold)
                {
                    similarEntities.Add((entity, similarity));
                }

                if (similarity > maxSimilarity)
                    maxSimilarity = similarity;
                if (similarity < minSimilarity)
                    minSimilarity = similarity;
            }

            _logger.LogInformation("The minimum and maximum similarity values are {MinSimilarity} and {MaxSimilarity}", minSimilarity, maxSimilarity);

            List<KnowledgeEntity> entities = [.. similarEntities
                .OrderByDescending(se => se.SimilarityScore)
                .Take(matchedEntitiesMaxCount)
                .Select(se => se.Entity)];

            var remainingEntitiesCount = allEntitiesMaxCount >= matchedEntitiesMaxCount
                ? allEntitiesMaxCount - matchedEntitiesMaxCount
                : 0;

            if (remainingEntitiesCount == 0
                || relationshipsMaxDepth == 0)
                return (entities, [], []);

            var entitiesIds = entities
                .Select(e => e.UniqueId)
                .ToHashSet();

            var relationshipEntities = new List<KnowledgeEntity>();
            var relationships = new List<KnowledgeRelationship>();

            // Get the first layer of related entities for the most similar entities.
            // This is required to kick off the search for the most relevant relationships.
            var currentIndexRelatedNodes = entities
                .Select(e => knowledgeGraph.Index.Nodes[e.UniqueId])
                .SelectMany(n => n.RelatedNodes)
                .Where(rn => !entitiesIds.Contains(rn.RelatedEntity.UniqueId))
                .Distinct(new KnowledgeGraphIndexRelatedNodeComparer())
                .ToList();
            var currentSimilarityScores = currentIndexRelatedNodes
                .Select(rn => 1.0f - Distance.Cosine(
                    userPromptEmbedding.ToArray(),
                    rn.RelatedEntity.SummaryDescriptionEmbedding))
                .ToList();
            var currentDepthLevels = currentIndexRelatedNodes
                .Select(n => 1)
                .ToList();

            // Repeat the process until the specified number of entities is reached or there are no more
            // related nodes to process.
            while (remainingEntitiesCount > 0
                && currentIndexRelatedNodes.Count > 0)
            {
                // Identify the most similar node.
                var currentMaxSimilarity = currentSimilarityScores.Max();
                var mostSimilarIndex = currentSimilarityScores
                    .FindIndex(s => s == currentMaxSimilarity);
                var mostSimilarRelatedNode = currentIndexRelatedNodes[mostSimilarIndex];
                var mostSimilarRelatedNodeDepth = currentDepthLevels[mostSimilarIndex];

                // Add the node at the index to the results.
                entitiesIds.Add(mostSimilarRelatedNode.RelatedEntity.UniqueId);
                relationshipEntities.Add(mostSimilarRelatedNode.RelatedEntity);
                relationships.Add(mostSimilarRelatedNode.Relationship);

                //Remove the node from the current state.
                currentIndexRelatedNodes.RemoveAt(mostSimilarIndex);
                currentSimilarityScores.RemoveAt(mostSimilarIndex);
                currentDepthLevels.RemoveAt(mostSimilarIndex);

                // From the nodes directly related to the most similar node (if any),
                // determine the ones that are neither in the current results nor in the current state
                // and add them to the current state.
                // If the most similar node is not at the maximum allowed depth, do not continue.
                var mostSimilarIndexNode = knowledgeGraph.Index.Nodes[mostSimilarRelatedNode.RelatedEntity.UniqueId];
                if (mostSimilarIndexNode.RelatedNodes.Count > 0
                    && mostSimilarRelatedNodeDepth < relationshipsMaxDepth)
                {
                    var indexRelatedNodesToAdd = mostSimilarIndexNode
                        .RelatedNodes
                        .Where(rn =>
                            !entitiesIds.Contains(rn.RelatedEntity.UniqueId)
                            && !currentIndexRelatedNodes.Contains(rn, new KnowledgeGraphIndexRelatedNodeComparer()))
                        .ToList();
                    var similarityScoresToAdd = indexRelatedNodesToAdd
                        .Select(rn => 1.0f - Distance.Cosine(
                            userPromptEmbedding.ToArray(),
                            rn.RelatedEntity.SummaryDescriptionEmbedding))
                        .ToList();
                    var depthLevelsToAdd = indexRelatedNodesToAdd
                        .Select(rn => mostSimilarRelatedNodeDepth + 1)
                        .ToList();

                    currentIndexRelatedNodes.AddRange(indexRelatedNodesToAdd);
                    currentSimilarityScores.AddRange(similarityScoresToAdd);
                    currentDepthLevels.AddRange(depthLevelsToAdd);
                }

                remainingEntitiesCount--;
            }

            return (entities, relationshipEntities, relationships);
        }

        private string GetFilter(
            string vectorStoreId,
            List<string> indexIds,
            Dictionary<string, object>? metadataFilter)
        {
            var filter = $"{_vectorDatabase.VectorStoreIdPropertyName} eq '{vectorStoreId}'";

            if (indexIds.Count > 0)
                filter += $" and search.in({KEY_FIELD_NAME}, '{string.Join(',', indexIds)}')";

            if (metadataFilter is not null
                && metadataFilter.Count > 0)
                filter += " and " + string.Join(" and ", metadataFilter
                        .Select(kvp => $"{_vectorDatabase.MetadataPropertyName}/{kvp.Key} eq {GetFilterValue(kvp.Value)}"));

            return filter;
        }

        private string GetFilterValue(object value)
        {
            var jsonValue = (JsonElement)value;

            return jsonValue.ValueKind switch
            {
                JsonValueKind.String => $"'{jsonValue.GetString()!.Replace("'", "''")}'",
                JsonValueKind.Number => jsonValue.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => throw new InvalidOperationException($"Unsupported JSON value kind: {jsonValue.ValueKind}")
            };
        }
    }
}

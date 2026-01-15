using FoundationaLLM.Common.Constants.Context;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.Models.Context.Knowledge;
using FoundationaLLM.Common.Models.Knowledge;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Common.Models.ResourceProviders.Vector;
using FoundationaLLM.Common.Models.Services;
using FoundationaLLM.Context.Models;
using Microsoft.Extensions.Logging;
using OpenAI.Embeddings;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace FoundationaLLM.Context.Services
{
    /// <summary>
    /// Provides methods for querying knowledge units.
    /// </summary>
    public class KnowledgeUnitQueryEngine
    {
        private readonly KnowledgeUnit _knowledgeUnit;
        private readonly CachedKnowledgeUnit _cachedKnowledgeUnit;
        private readonly VectorDatabase _vectorDatabase;
        private readonly VectorDatabase? _knowledgeGraphVectorDatabase;
        private readonly ILogger<KnowledgeUnitQueryEngine> _logger;

        private const string KEY_FIELD_NAME = "Id";
        private const string UNIQUE_ID_METADATA_PROPERTY_NAME = "UniqueId";
        private const string ITEM_TYPE_METADATA_PROPERTY_NAME = "ItemType";
        private const string KNOWLEDGE_GRAPH_ENTITY = "Entity";
        private const string KNOWLEDGE_GRAPH_RELATIONSHIP = "Relationship";

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeUnitQueryEngine"/> class.
        /// </summary>
        /// <param name="knowledgeUnit">The knowledge unit definition to be queried by the engine.</param>
        /// <param name="cachedKnowledgeUnit">The cached version of the knowledge unit to be queried.</param>
        /// <param name="vectorDatabase">The vector database associated with the knowledge unit.</param>
        /// <param name="logger">The logger used for logging operations within the query engine.</param>
        public KnowledgeUnitQueryEngine(
            KnowledgeUnit knowledgeUnit,
            CachedKnowledgeUnit cachedKnowledgeUnit,
            VectorDatabase vectorDatabase,
            VectorDatabase? knowledgeGraphDatabase,
            ILogger<KnowledgeUnitQueryEngine> logger)
        {
            _knowledgeUnit = knowledgeUnit;
            _cachedKnowledgeUnit = cachedKnowledgeUnit;
            _vectorDatabase = vectorDatabase;
            _knowledgeGraphVectorDatabase = knowledgeGraphDatabase;
            _logger = logger;
        }

        public async Task<Result<ContextKnowledgeSourceQueryResponse>> QueryAsync(
            ContextKnowledgeSourceQueryRequest queryRequest)
        {
            try
            {
                ValidateQueryRequest(queryRequest);

                var vectorStoreFilter = queryRequest.KnowledgeUnitVectorStoreFilters
                    .SingleOrDefault(f => f.KnowledgeUnitId == _knowledgeUnit.Name);

                var vectorStoreId = string.IsNullOrWhiteSpace(_knowledgeUnit.VectorStoreId)
                    ? vectorStoreFilter?.VectorStoreId
                    : _knowledgeUnit.VectorStoreId;
                if (string.IsNullOrWhiteSpace(vectorStoreId))
                    return Result<ContextKnowledgeSourceQueryResponse>.FailureFromErrorMessage(
                        $"The knowledge unit {_knowledgeUnit.Name} does not have a vector store identifier specified and none was provided in the query request.",
                        instance: _knowledgeUnit.Name);

                var userPromptEmbedding = await _cachedKnowledgeUnit.EmbeddingClient.GenerateEmbeddingAsync(
                    queryRequest.UserPrompt,
                    new EmbeddingGenerationOptions
                    {
                        Dimensions = _vectorDatabase.EmbeddingDimensions
                    });

                var queryResponse = new ContextKnowledgeSourceQueryResponse
                {
                    Source = _knowledgeUnit.Name
                };

                if (queryRequest.VectorStoreQuery is not null)
                {
                    #region Direct filtering on the vector store

                    var matchingDocumentsFilter = GetFilter(
                        _vectorDatabase,
                        vectorStoreId,
                        [],
                        vectorStoreFilter?.VectorStoreMetadataFilter);

                    _logger.LogInformation(
                        "Vector store query - filter for vector store {VectorDatabase}/{VectorStore} in knowledge unit {KnowledgeUnit}: {Filter}",
                        _vectorDatabase.DatabaseName,
                        vectorStoreId,
                        _knowledgeUnit.Name,
                        matchingDocumentsFilter);

                    var matchingDocuments = await _cachedKnowledgeUnit.SearchService.SearchDocuments(
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

                    _logger.LogInformation(
                        "Vector store query - found {MatchingDocumentsCount} matching documents for vector store {VectorDatabase}/{VectorStore} in knowledge unit {KnowledgeUnit}",
                        matchingDocuments.Count(),
                        _vectorDatabase.DatabaseName,
                        vectorStoreId,
                        _knowledgeUnit.Name);

                    if (!matchingDocuments.Any()
                        && queryRequest.KnowledgeTask == ContextKnowledgeTasks.Summary
                        && vectorStoreFilter is not null
                        && vectorStoreFilter.VectorStoreMetadataFilter is not null
                        && vectorStoreFilter.VectorStoreMetadataFilter.ContainsKey("FileName"))
                    {
                        _logger.LogInformation(
                            "Vector store query - attempting non-vector query for vector store {VectorDatabase}/{VectorStore} in knowledge unit {KnowledgeUnit}",
                            _vectorDatabase.DatabaseName,
                            vectorStoreId,
                            _knowledgeUnit.Name);

                        matchingDocuments = await _cachedKnowledgeUnit.SearchService.SearchDocuments(
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
                            null,
                            null,
                            null,
                            queryRequest.VectorStoreQuery.TextChunksMaxCount,
                            queryRequest.VectorStoreQuery.UseSemanticRanking);

                        _logger.LogInformation(
                            "Vector store query - non-vector query found {MatchingDocumentsCount} matching documents for vector store {VectorDatabase}/{VectorStore} in knowledge unit {KnowledgeUnit}",
                            matchingDocuments.Count(),
                            _vectorDatabase.DatabaseName,
                            vectorStoreId,
                            _knowledgeUnit.Name);
                    }

                    queryResponse.VectorStoreResponse = new ContextVectorStoreResponse
                    {
                        TextChunks = [.. matchingDocuments
                            .Select(md => new ContextTextChunk
                            {
                                Score = md.GetDouble("Score"),
                                Content = md.GetString(_vectorDatabase.ContentPropertyName),
                                Metadata = md.GetObject(_vectorDatabase.MetadataPropertyName).ToDictionary()
                            })
                        ]
                    };

                    #endregion
                }

                if (queryRequest.KnowledgeGraphQuery is not null)
                {
                    if (_knowledgeGraphVectorDatabase is null)
                        return Result<ContextKnowledgeSourceQueryResponse>.FailureFromErrorMessage(
                            $"The knowledge unit {_knowledgeUnit.Name} does not have a knowledge graph vector database.",
                            instance: _knowledgeUnit.Name);

                    var knowledgeGraphUserPromptEmbedding = await _cachedKnowledgeUnit.KnowledgeGraphEmbeddingClient!.GenerateEmbeddingAsync(
                        queryRequest.UserPrompt,
                        new EmbeddingGenerationOptions
                        {
                            Dimensions = _knowledgeGraphVectorDatabase.EmbeddingDimensions
                        });

                    var matchingEntities = await GetMatchingKnowledgeGraphEntities(
                        _cachedKnowledgeUnit,
                        _knowledgeGraphVectorDatabase,
                        vectorStoreId,
                        vectorStoreFilter,
                        knowledgeGraphUserPromptEmbedding.Value.ToFloats(),
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
                            _vectorDatabase,
                            vectorStoreId,
                            [.. matchingEntities.Entities.SelectMany(e => e.ChunkIds).Distinct()],
                            vectorStoreFilter?.VectorStoreMetadataFilter);

                        _logger.LogInformation(
                            "Vector store query (via KG) - filter for vector store {VectorDatabase}/{VectorStore} in knowledge unit {KnowledgeUnit}: {Filter}",
                            _vectorDatabase.DatabaseName,
                            vectorStoreId,
                            _knowledgeUnit.Name,
                            matchingDocumentsFilter);

                        var matchingDocuments = await _cachedKnowledgeUnit.SearchService.SearchDocuments(
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

                        _logger.LogInformation(
                            "Vector store query (via KG) - found {MatchingDocumentsCount} matching documents for vector store {VectorDatabase}/{VectorStore} in knowledge unit {KnowledgeUnit}",
                            matchingDocuments.Count(),
                            _vectorDatabase.DatabaseName,
                            vectorStoreId,
                            _knowledgeUnit.Name);

                        queryResponse.KnowledgeGraphResponse.TextChunks = [.. matchingDocuments
                            .Select(md => new ContextTextChunk
                            {
                                Score = md.GetDouble("Score"),
                                Content = md.GetString(_vectorDatabase.ContentPropertyName),
                                Metadata = md.GetObject(_vectorDatabase.MetadataPropertyName).ToDictionary()
                            })
                        ];
                    }
                }

                return Result<ContextKnowledgeSourceQueryResponse>.Success(queryResponse);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, "Invalid query request for the knowledge source {KnowledgeSourceId}.",
                    _knowledgeUnit.Name);
                return Result<ContextKnowledgeSourceQueryResponse>.FailureFromException(ex, instance: _knowledgeUnit.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while querying the knowledge source {KnowledgeSourceId}.",
                    _knowledgeUnit.Name);
                return Result<ContextKnowledgeSourceQueryResponse>.FailureFromException(ex, instance: _knowledgeUnit.Name);
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

        private async Task<(List<KnowledgeEntity> Entities, List<KnowledgeEntity> RelatedEntities, List<KnowledgeRelationship> Relationships)>
            GetMatchingKnowledgeGraphEntities(
                CachedKnowledgeUnit cachedKnowledgeUnit,
                VectorDatabase vectorDatabase,
                string vectorStoreId,
                ContextKnowledgeUnitVectorStoreFilter? vectorStoreFilter,
                ReadOnlyMemory<float> userPromptEmbedding,
                float similarityThreshold,
                int matchedEntitiesMaxCount,
                int relationshipsMaxDepth,
                int allEntitiesMaxCount)
        {
            var knowledgeGraph = cachedKnowledgeUnit.KnowledgeGraph!;

            var matchingDocumentsFilter = GetFilter(
                        _vectorDatabase,
                        vectorStoreId,
                        [],
                        null);

            _logger.LogInformation(
                "Match knowledge graph items - filter for vector store {VectorDatabase}/{VectorStore} in knowledge unit {KnowledgeUnit}: {Filter}",
                vectorDatabase.DatabaseName,
                vectorStoreId,
                _knowledgeUnit.Name,
                matchingDocumentsFilter);

            var matchingDocuments = await cachedKnowledgeUnit.KnowledgeGraphSearchService!.SearchDocuments(
                vectorDatabase.DatabaseName,
                [
                    KEY_FIELD_NAME,
                    vectorDatabase.ContentPropertyName,
                    vectorDatabase.MetadataPropertyName
                ],
                matchingDocumentsFilter,
                null, // No hybrid search for knowledge graph entities
                userPromptEmbedding,
                vectorDatabase.EmbeddingPropertyName,
                similarityThreshold,
                matchedEntitiesMaxCount,
                false);

            _logger.LogInformation(
                "Match knowledge graph items - found {MatchingDocumentsCount} matching documents for vector store {VectorDatabase}/{VectorStore} in knowledge unit {KnowledgeUnit}",
                matchingDocuments.Count(),
                vectorDatabase.DatabaseName,
                vectorStoreId,
                _knowledgeUnit.Name);

            List <ContextTextChunk> matchingTextChunks = [.. matchingDocuments
                .Select(md => new ContextTextChunk
                {
                    Score = md.GetDouble("Score"),
                    Content = md.GetString(_vectorDatabase.ContentPropertyName),
                    Metadata = md.GetObject(_vectorDatabase.MetadataPropertyName).ToDictionary()
                })
            ];

            var minScore = matchingTextChunks.Min(tc => tc.Score);
            var maxScore = matchingTextChunks.Max(tc => tc.Score);

            _logger.LogInformation(
                "The minimum and maximum score values are {MinScore} and {MaxScore}",
                minScore, maxScore);

            var similarEntitiesIds = new HashSet<string>();
            var similarEntities = new List<(KnowledgeEntity Entity, double Score)>();

            foreach (var matchingTextChunk in matchingTextChunks)
                if (matchingTextChunk.Metadata.TryGetValue(UNIQUE_ID_METADATA_PROPERTY_NAME, out var uniqueIdObj)
                    && uniqueIdObj is not null
                    && uniqueIdObj is string uniqueId
                    && matchingTextChunk.Metadata.TryGetValue(ITEM_TYPE_METADATA_PROPERTY_NAME, out var itemTypeObj)
                    && itemTypeObj is not null
                    && itemTypeObj is string itemType)
                {
                    switch (itemType)
                    {
                        case KNOWLEDGE_GRAPH_ENTITY:
                            if (!similarEntitiesIds.Contains(uniqueId)
                                && knowledgeGraph.Index.Nodes.TryGetValue(uniqueId, out var node))
                            {
                                similarEntitiesIds.Add(node.Entity.UniqueId);
                                similarEntities.Add((node.Entity, matchingTextChunk.Score ?? 0));
                            }
                            break;
                        case KNOWLEDGE_GRAPH_RELATIONSHIP:
                            // If a relationship is matched, include both the source and target entities.
                            if (knowledgeGraph.Index.Relationships.TryGetValue(uniqueId, out var relationship))
                            {
                                if (!similarEntitiesIds.Contains(relationship.SourceUniqueId)
                                    && knowledgeGraph.Index.Nodes.TryGetValue(relationship.SourceUniqueId, out var sourceNode))
                                {
                                    similarEntitiesIds.Add(sourceNode.Entity.UniqueId);
                                    similarEntities.Add((sourceNode.Entity, matchingTextChunk.Score ?? 0));
                                }

                                if (!similarEntitiesIds.Contains(relationship.TargetUniqueId)
                                    && knowledgeGraph.Index.Nodes.TryGetValue(relationship.TargetUniqueId, out var targetNode))
                                {
                                    similarEntitiesIds.Add(targetNode.Entity.UniqueId);
                                    similarEntities.Add((targetNode.Entity, matchingTextChunk.Score ?? 0));
                                }
                            }
                            break;
                        default:
                            _logger.LogWarning(
                                "The item type {ItemType} is not supported when querying the knowledge graph.",
                                itemType);
                            break;
                    }
                }

            List<KnowledgeEntity> entities = [.. similarEntities
                .OrderByDescending(se => se.Score)
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
            var currentRelationshipStrengths = currentIndexRelatedNodes
                .Select(rn => rn.RelationshipStrength)
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
                var currentMaxStrength = currentRelationshipStrengths.Max();
                var mostStrengthfulIndex = currentRelationshipStrengths
                    .FindIndex(s => s == currentMaxStrength);
                var mostStrengthfulRelatedNode = currentIndexRelatedNodes[mostStrengthfulIndex];
                var mostStrengthfulRelatedNodeDepth = currentDepthLevels[mostStrengthfulIndex];

                // Add the node at the index to the results.
                entitiesIds.Add(mostStrengthfulRelatedNode.RelatedEntity.UniqueId);
                relationshipEntities.Add(mostStrengthfulRelatedNode.RelatedEntity);
                relationships.Add(mostStrengthfulRelatedNode.Relationship);

                //Remove the node from the current state.
                currentIndexRelatedNodes.RemoveAt(mostStrengthfulIndex);
                currentRelationshipStrengths.RemoveAt(mostStrengthfulIndex);
                currentDepthLevels.RemoveAt(mostStrengthfulIndex);

                // From the nodes directly related to the most strengthful node (if any),
                // determine the ones that are neither in the current results nor in the current state
                // and add them to the current state.
                // If the most strengthful node is at the maximum allowed depth, do not continue.
                var mostStrengthfulIndexNode = knowledgeGraph.Index.Nodes[mostStrengthfulRelatedNode.RelatedEntity.UniqueId];
                if (mostStrengthfulIndexNode.RelatedNodes.Count > 0
                    && mostStrengthfulRelatedNodeDepth < relationshipsMaxDepth)
                {
                    var indexRelatedNodesToAdd = mostStrengthfulIndexNode
                        .RelatedNodes
                        .Where(rn =>
                            !entitiesIds.Contains(rn.RelatedEntity.UniqueId)
                            && !currentIndexRelatedNodes.Contains(rn, new KnowledgeGraphIndexRelatedNodeComparer()))
                        .ToList();
                    var strengthScoresToAdd = indexRelatedNodesToAdd
                        .Select(rn => rn.RelationshipStrength)
                        .ToList();
                    var depthLevelsToAdd = indexRelatedNodesToAdd
                        .Select(rn => mostStrengthfulRelatedNodeDepth + 1)
                        .ToList();

                    currentIndexRelatedNodes.AddRange(indexRelatedNodesToAdd);
                    currentRelationshipStrengths.AddRange(strengthScoresToAdd);
                    currentDepthLevels.AddRange(depthLevelsToAdd);
                }

                remainingEntitiesCount--;
            }

            return (entities, relationshipEntities, relationships);
        }

        private string GetFilter(
            VectorDatabase vectorDatabase,
            string vectorStoreId,
            List<string> indexIds,
            Dictionary<string, object>? metadataFilter)
        {
            var filter = $"{vectorDatabase.VectorStoreIdPropertyName} eq '{vectorStoreId}'";

            if (indexIds.Count > 0)
                filter += $" and search.in({KEY_FIELD_NAME}, '{string.Join(',', indexIds)}')";

            if (metadataFilter is not null
                && metadataFilter.Count > 0)
                filter += " and " + string.Join(" and ", metadataFilter
                        .Select(kvp => GetFilterValue(
                            vectorDatabase.MetadataPropertyName,
                            kvp.Key,
                            kvp.Value)));

            return filter;
        }

        private string GetFilterValue(
            string metadataPropertyName,
            string propertyName,
            object value)
        {
            var jsonValue = (JsonElement)value;

            if (jsonValue.ValueKind == JsonValueKind.Array)
            {
                var arrayItems = jsonValue.EnumerateArray();
                if (!arrayItems.Any())
                    throw new InvalidOperationException("Empty array values are not supported in metadata filters.");

                var firstItemKind = arrayItems.First().ValueKind;
                switch (firstItemKind)
                {
                    case JsonValueKind.String:
                        var valuesList = string.Join(',',
                            jsonValue.EnumerateArray()
                                .Select(v => v.GetString()!.Replace("'", "''")));
                        return $"search.in({metadataPropertyName}/{propertyName},'{valuesList}', ',')";
                    case JsonValueKind.Number:
                        var valuesFilter = string.Join(" or ",
                            jsonValue.EnumerateArray()
                                .Select(v => $"{metadataPropertyName}/{propertyName} eq {v.GetRawText()}"));
                        return $"({valuesFilter})";
                    default:
                        throw new InvalidOperationException($"The JSON value kind {firstItemKind} is not supported in metadata filter arrays.");
                }                
            }

            var filter = jsonValue.ValueKind switch
            {
                JsonValueKind.String => $"'{jsonValue.GetString()!.Replace("'", "''")}'",
                JsonValueKind.Number => jsonValue.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => throw new InvalidOperationException($"Unsupported JSON value kind: {jsonValue.ValueKind}")
            };

            return $"{metadataPropertyName}/{propertyName} eq {filter}";
        }
    }
}

using FoundationaLLM.Client.Management;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Knowledge;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Services.Storage;
using FoundationaLLM.DataPipelineEngine.Services;
using FoundationaLLM.DataPipelineEngine.Services.CosmosDB;
using FoundationaLLM.Tests.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples.Concepts.Storage
{
    /// <summary>
    /// Example class for testing embedding via the Gateway API.
    /// </summary>
    public class Example_Storage_DataPipelineStateStorage
    {
        private readonly ITestOutputHelper _output;
        private readonly TestEnvironment _testEnvironment;
        private readonly ILogger<Example_Storage_DataPipelineStateStorage> _logger;

        private IDataPipelineStateService _dataPipelineStateService = null!;

        private readonly KnowledgeEntityRelationshipCollection<KnowledgeEntity, KnowledgeRelationship> _entityRelationships = new();

        private const string KNOWLEDGE_PARTS_FILE_NAME = "knowledge-parts.parquet";

        public Example_Storage_DataPipelineStateStorage(
            ITestOutputHelper output)
        {
            _output = output;
            _testEnvironment = new TestEnvironment();
            _testEnvironment.InitializeServices(output);
            _logger = _testEnvironment.GetRequiredService<ILogger<Example_Storage_DataPipelineStateStorage>>();
        }

        private void InitializeServices()
        {
            var storageService = new BlobStorageService(
                storageOptions: Options.Create<BlobStorageServiceSettings>(new BlobStorageServiceSettings
                {
                    AuthenticationType = AuthenticationTypes.AzureIdentity,
                    AccountName = _testEnvironment.Configuration["DataPipelineStateService:StorageAccountName"]
                }),
                logger: _testEnvironment.GetRequiredService<ILogger<BlobStorageService>>());

            _dataPipelineStateService = new DataPipelineStateService(
                null!,
                storageService,
                _testEnvironment.GetRequiredService<ILogger<DataPipelineStateService>>());
        }

        [Fact]
        public async Task Storage_DataPipelineStateService_Load()
        {
            InitializeServices();

            _output.WriteLine("============ FoundationaLLM Storage - Data Pipeline State Service Tests ============");

            var instanceId = _testEnvironment.Configuration["ManagementClient:InstanceId"];
            var dataPipelineDefinitionName = _testEnvironment.Configuration["ManagementClient:DataPipelineDefinitionName"];
            var dataPipelineRunId = _testEnvironment.Configuration["ManagementClient:DataPipelineRunId"];
            var dataPipelineRunWorkItemId = _testEnvironment.Configuration["ManagementClient:DataPipelineRunWorkItemId"];

            var managementClient = new ManagementClient(
                _testEnvironment.Configuration["ManagementClient:ManagementAPIUrl"]!,
                ServiceContext.AzureCredential!,
                instanceId!);

            var dataPipelineDefinition = await managementClient.GetResourceByObjectId<DataPipelineDefinition>(
                $"instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines/{dataPipelineDefinitionName}");

            var dataPipelineRun = await managementClient.GetResourceByObjectId<DataPipelineRun>(
                $"instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines/{dataPipelineDefinitionName}/dataPipelineRuns/{dataPipelineRunId}");

            var dataPipelineRunWorkItem = new DataPipelineRunWorkItem
            {
                Id = dataPipelineRunWorkItemId!,
                RunId = dataPipelineRunId!,
                Stage = string.Empty,
                ContentItemCanonicalId = string.Empty
            };

            var contentArtifactsLoadResult = await _dataPipelineStateService.TryLoadDataPipelineRunArtifacts(
                dataPipelineDefinition,
                dataPipelineRun,
                "content-items/content-items.json");

            var contentItemCanonicalIds = JsonSerializer.Deserialize<List<string>>(
                contentArtifactsLoadResult.Artifacts[0].Content)
                ?? throw new PluginException("The content items artifact is not valid.");

            var knowledgeParts = await LoadExtractedKnowledge(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                contentItemCanonicalIds);

            foreach (var knowledgePart in knowledgeParts)
                AddExtractedKnowledgePart(knowledgePart);
        }

        private async Task<List<DataPipelineContentItemKnowledgePart>> LoadExtractedKnowledge(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem,
            List<string> contentItemCanonicalIds)
        {
            using var semaphore = new SemaphoreSlim(10);

            var loadTasks = contentItemCanonicalIds
                .Select(async contentItemCanonicalId =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        return await LoadExtractedKnowledge(
                            dataPipelineDefinition,
                            dataPipelineRun,
                            dataPipelineRunWorkItem,
                            contentItemCanonicalId);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                })
                .ToList();

            var knowledgePartsLists = await Task.WhenAll(loadTasks);

            if (knowledgePartsLists.Length < contentItemCanonicalIds.Count)
            {
                _logger.LogWarning("Not all content items have extracted knowledge parts in data pipeline run work item {DataPipelineRunWorkItemId}. Expected {ExpectedCount}, but got {ActualCount}.",
                    dataPipelineRunWorkItem.Id, contentItemCanonicalIds.Count, knowledgePartsLists.Length);
                dataPipelineRunWorkItem.Warnings.Add(
                    $"Not all content items have extracted knowledge parts. Expected {contentItemCanonicalIds.Count}, but got {knowledgePartsLists.Length}.");
            }

            return [.. knowledgePartsLists.SelectMany(x => x)];
        }

        private async Task<List<DataPipelineContentItemKnowledgePart>> LoadExtractedKnowledge(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem,
            string contentItemCanonicalId)
        {
            try
            {
                var contentItemKnowledgeParts = await _dataPipelineStateService.LoadDataPipelineRunWorkItemParts<DataPipelineContentItemKnowledgePart>(
                    dataPipelineDefinition,
                    dataPipelineRun,
                    contentItemCanonicalId,
                    KNOWLEDGE_PARTS_FILE_NAME);

                var knowledgeParts = contentItemKnowledgeParts?
                    .Where(p => p.EntitiesAndRelationships is not null)
                    .Select(p => p)
                    .ToList();

                return knowledgeParts ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load extracted knowledge for content item {ContentItemCanonicalId} in data pipeline run work item {DataPipelineRunWorkItemId}.",
                    contentItemCanonicalId, dataPipelineRunWorkItem.Id);
                return [];
            }
        }

        private void AddExtractedKnowledgePart(
            DataPipelineContentItemKnowledgePart knowledgePart)
        {
            foreach (var entity in knowledgePart.EntitiesAndRelationships!.Entities)
            {
                var existingEntity = _entityRelationships.Entities
                    .FirstOrDefault(
                        x => x.Type.Equals(entity.Type, StringComparison.OrdinalIgnoreCase)
                        && x.Name.Equals(entity.Name, StringComparison.OrdinalIgnoreCase));
                if (existingEntity != null)
                {
                    existingEntity.Descriptions ??= [];
                    existingEntity.ChunkIds ??= [];

                    existingEntity.Descriptions.Add(entity.Description);
                    existingEntity.ChunkIds.Add(knowledgePart.IndexEntryId!);
                }
                else
                {
                    _entityRelationships.Entities.Add(new KnowledgeEntity
                    {
                        Position = _entityRelationships.Entities.Count + 1,
                        Type = entity.Type,
                        Name = entity.Name,
                        Descriptions = [entity.Description],
                        ChunkIds = [knowledgePart.IndexEntryId]
                    });
                }
            }

            foreach (var relationship in knowledgePart.EntitiesAndRelationships.Relationships)
            {
                var existingRelationship = _entityRelationships.Relationships
                    .FirstOrDefault(
                        x => x.Source.Equals(relationship.Source, StringComparison.OrdinalIgnoreCase)
                        && x.Target.Equals(relationship.Target, StringComparison.OrdinalIgnoreCase));
                if (existingRelationship != null)
                {
                    existingRelationship.ShortDescriptions ??= [];
                    existingRelationship.Descriptions ??= [];
                    existingRelationship.ChunkIds ??= [];
                    existingRelationship.Strengths ??= [];

                    existingRelationship.ShortDescriptions.Add(relationship.ShortDescription);
                    existingRelationship.Descriptions.Add(relationship.Description);
                    existingRelationship.ChunkIds.Add(knowledgePart.IndexEntryId!);
                    existingRelationship.Strengths.Add(relationship.Strength);
                }
                else
                {
                    _entityRelationships.Relationships.Add(new KnowledgeRelationship
                    {
                        Position = _entityRelationships.Relationships.Count + 1,
                        Source = relationship.Source,
                        SourceType = knowledgePart
                            .EntitiesAndRelationships
                            .Entities
                            .FirstOrDefault(e => e.Name == relationship.Source)?
                            .Type
                            ?? "N/A",
                        Target = relationship.Target,
                        TargetType = knowledgePart
                            .EntitiesAndRelationships
                            .Entities
                            .FirstOrDefault(e => e.Name == relationship.Target)?
                            .Type
                            ?? "N/A",
                        Strengths = [relationship.Strength],
                        ShortDescriptions = [relationship.ShortDescription],
                        Descriptions = [relationship.Description],
                        ChunkIds = [knowledgePart.IndexEntryId!]
                    });
                }
            }
        }
    }
}
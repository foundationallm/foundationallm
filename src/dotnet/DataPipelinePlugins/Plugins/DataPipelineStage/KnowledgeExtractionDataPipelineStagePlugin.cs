using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Knowledge Extraction Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class KnowledgeExtractionDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IServiceProvider serviceProvider)
        : DataPipelineStagePluginBase(pluginParameters, packageManager, serviceProvider)
    {
        protected override string Name => PluginNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE;

        private const string CONTENT_PARTS_FILE_NAME = "content-parts.parquet";
        private const string KNOWLEDGE_PARTS_FILE_NAME = "knowledge-parts.parquet";

        private readonly IResourceProviderService _promptResourceProvider =
            serviceProvider.GetRequiredService<IEnumerable<IResourceProviderService>>()
                .SingleOrDefault(rp => rp.Name == ResourceProviderNames.FoundationaLLM_Prompt)
            ?? throw new PluginException($"The resource provider {ResourceProviderNames.FoundationaLLM_Prompt} is not available in the dependency injection container.");

        /// <inheritdoc/>
        public override async Task<PluginResult> ProcessWorkItem(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONPROMPTOBJECTID,
                out var entityExtractionPromptId))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONPROMPTOBJECTID} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONCOMPLETIONMODEL,
                out var entityExtractionCompletionModel))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONCOMPLETIONMODEL} parameter.");

            var entityExtractionPrompt = await _promptResourceProvider.GetResourceAsync<PromptBase>(
                dataPipelineRun.InstanceId,
                entityExtractionPromptId.ToString()!,
                ServiceContext.ServiceIdentity!);

            using var scope = _serviceProvider.CreateScope();

            var clientFactoryService = scope.ServiceProvider
                .GetRequiredService<IHttpClientFactoryService>()
                ?? throw new PluginException("The HTTP client factory service is not available in the dependency injection container.");

            var embeddingServiceClient = new GatewayServiceClient(
                await clientFactoryService.CreateClient(
                    HttpClientNames.GatewayAPI, ServiceContext.ServiceIdentity!),
                _serviceProvider.GetRequiredService<ILogger<GatewayServiceClient>>());

            var contentItemContentParts = await _dataPipelineStateService.LoadDataPipelineRunWorkItemParts<DataPipelineContentItemContentPart>(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                CONTENT_PARTS_FILE_NAME);

            var contentItemKnowledgeParts = contentItemContentParts
                .Select(p =>
                {
                    var knowledgePart = DataPipelineContentItemKnowledgePart.FromContentItemPart(p);
                    knowledgePart.Entities = "N/A";
                    return knowledgePart;
                })
                .ToList();

            await _dataPipelineStateService.SaveDataPipelineRunWorkItemParts<DataPipelineContentItemKnowledgePart>(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                contentItemKnowledgeParts,
                KNOWLEDGE_PARTS_FILE_NAME);

            return
                new PluginResult(true, false);
        }
    }
}

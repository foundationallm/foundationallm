using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Services.Plugins;
using FoundationaLLM.Core.Examples.Setup;
using FoundationaLLM.Plugins.DataPipeline;
using FoundationaLLM.Tests;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples.Concepts.Plugins
{
    /// <summary>
    /// Example class for testing the SharePoint Online Data Source Plugin in Data Pipelines.
    /// </summary>
    public class Example_DataPipelinePlugins_KnowledgeGraphDataPipelineStage : TestBase, IClassFixture<TestFixture>
    {
        public Example_DataPipelinePlugins_KnowledgeGraphDataPipelineStage(ITestOutputHelper output, TestFixture fixture)
            : base(1, output, fixture, new DependencyInjectionContainerInitializer())
        {
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task DataPipelinePlugins_KnowledgeGraphDataPipelineStage_ProcessWorkItem(
            string dataPipelineRunWorkItemId,
            string dataPipelineRunId,
            Dictionary<string, object> pluginParameters)
        {
            // Wait for all required initialization tasks to complete.
            await Task.WhenAll([
                StartEventsWorkers()
            ]);

            var packageManager = new PluginPackageManager();

            var pluginResourceProviderService = GetService<IEnumerable<IResourceProviderService>>()
                .Single(rp => rp.Name == ResourceProviderNames.FoundationaLLM_Plugin);
            var packageManagerResolver = new PluginPackageManagerResolver(
                pluginResourceProviderService);

            var dataPipelineStateService = GetService<IDataPipelineStateService>();
            var dataPipelineResourceProviderService = GetService<IEnumerable<IResourceProviderService>>()
                .Single(rp => rp.Name == ResourceProviderNames.FoundationaLLM_DataPipeline);
            var dataPipelineRunWorkItem = await dataPipelineStateService.GetDataPipelineRunWorkItem(
                dataPipelineRunWorkItemId,
                dataPipelineRunId);
            var dataPipelineRun = await dataPipelineStateService.GetDataPipelineRun(dataPipelineRunId);
            var dataPipelineDefinitionSnapshot = await dataPipelineResourceProviderService.GetResourceAsync<DataPipelineDefinitionSnapshot>(
                dataPipelineRun!.DataPipelineObjectId,
                ServiceContext.ServiceIdentity!);

            WriteLine("============ FoundationaLLM Data Pipeline Plugins - Knowledge Graph Data Pipeline Stage Tests ============");

            var dataSourcePlugin = packageManager.GetDataPipelineStagePlugin(
                PluginNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE,
                pluginParameters,
                packageManagerResolver,
                MainServiceContainer.ServiceProvider);

            var pluginResult = await dataSourcePlugin.ProcessWorkItem(
                dataPipelineDefinitionSnapshot.DataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem!);

            Assert.True(pluginResult.Success, "The plugin should have succeeded.");
        }

        public static TheoryData<string, string, Dictionary<string, object>> TestData =>
        new()
        {
            {
                "work-item-FwB5zJHt3k6PmGF5rofb7g",
                "run-20250630-165745-JmaV2TETeU2G4S6EwUDjDQ-TAfGit69y0OhQOwAAtltKw",
                new Dictionary<string, object>
                {
                    {
                        PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONPROMPTOBJECTID,
                        "instances/8ac6074c-bdde-43cb-a140-ec0002d96d2b/providers/FoundationaLLM.Prompt/prompts/ProcessSPOFiles-EntitySummarization"
                    },
                    {
                        PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONCOMPLETIONMODEL,
                        "gpt-4o-mini"
                    },
                    {
                        PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONCOMPLETIONMAXOUTPUTTOKENCOUNT,
                        1000
                    },
                    {
                        PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONCOMPLETIONMODELTEMPERATURE,
                        0.7
                    },
                    {
                        PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_KNOWLEDGEUNITOBJECTID,
                        "instances/8ac6074c-bdde-43cb-a140-ec0002d96d2b/providers/FoundationaLLM.Context/knowledgeUnits/ProcessSPOFiles-KG-01"
                    }
                }
            }
        };
    }
}

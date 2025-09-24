using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
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
    public class Example_DataPipelinePlugins_AzureAISearchIndexingDataPipelineStage : TestBase, IClassFixture<TestFixture>
    {
        public Example_DataPipelinePlugins_AzureAISearchIndexingDataPipelineStage(ITestOutputHelper output, TestFixture fixture)
            : base(1, output, fixture, new DependencyInjectionContainerInitializer())
        {
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task DataPipelinePlugins_AzureAISearchIndexingDataPipelineStage_ProcessWorkItem(
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

            WriteLine("============ FoundationaLLM Data Pipeline Plugins - Azure AI Indexing Data Pipeline Stage Tests ============");

            var dataSourcePlugin = packageManager.GetDataPipelineStagePlugin(
                PluginNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE,
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
                "work-item-ptNkaLhZOkqujDU2W5fjhA",
                "run-20250924-114010-koNWof6FX0e7ry5x-8FQ4w-TAfGit69y0OhQOwAAtltKw",
                new Dictionary<string, object>
                {
                    {
                        PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_KNOWLEDGEUNITOBJECTID,
                        "instances/8ac6074c-bdde-43cb-a140-ec0002d96d2b/providers/FoundationaLLM.Context/knowledgeUnits/AgentPrivateStores"
                    },
                    {
                        PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_VECTORSTOREID,
                        "MAA-02"
                    }
                }
            }
        };
    }
}

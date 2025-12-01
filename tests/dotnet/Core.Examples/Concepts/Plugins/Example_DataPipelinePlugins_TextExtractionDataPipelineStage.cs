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
    public class Example_DataPipelinePlugins_TextExtractionDataPipelineStage : TestBase, IClassFixture<TestFixture>
    {
        public Example_DataPipelinePlugins_TextExtractionDataPipelineStage(ITestOutputHelper output, TestFixture fixture)
            : base(1, output, fixture, new DependencyInjectionContainerInitializer())
        {
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task DataPipelinePlugins_TextExtractionDataPipelineStage_ProcessWorkItem(
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

            WriteLine("============ FoundationaLLM Data Pipeline Plugins - Text Extraction Data Pipeline Stage Tests ============");

            var dataSourcePlugin = packageManager.GetDataPipelineStagePlugin(
                PluginNames.TEXTEXTRACTION_DATAPIPELINESTAGE,
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
                "work-item-7mf2sJXGN02tMPemT1VNlA",
                "run-20251201-195436-WjZmcmmi30-e1WVaFUiK2g-TAfGit69y0OhQOwAAtltKw",
                new Dictionary<string, object>
                {
                    {
                        PluginParameterNames.TEXTEXTRACTION_DATAPIPELINESTAGE_MAXCONTENTSIZECHARACTERS,
                        10_000_000
                    }
                }
            }
        };
    }
}

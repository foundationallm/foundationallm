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
    public class Example_DataPipelinePlugins_TextPartitioningDataPipelineStage : TestBase, IClassFixture<TestFixture>
    {
        public Example_DataPipelinePlugins_TextPartitioningDataPipelineStage(ITestOutputHelper output, TestFixture fixture)
            : base(1, output, fixture, new DependencyInjectionContainerInitializer())
        {
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task DataPipelinePlugins_TextPartitioningDataPipelineStage_ProcessWorkItem(
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

            WriteLine("============ FoundationaLLM Data Pipeline Plugins - Knowledge Extraction Data Pipeline Stage Tests ============");

            var dataSourcePlugin = packageManager.GetDataPipelineStagePlugin(
                PluginNames.TEXTPARTITIONING_DATAPIPELINESTAGE,
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
                "work-item-Zs6koWtwk0aJokqRl4OOWQ", // empty.pdf
                "run-20250716-121054-8-LLCpnE6EGUvXt95mqSfA-TAfGit69y0OhQOwAAtltKw",
                new Dictionary<string, object>
                {
                    {
                        PluginParameterNames.TEXTPARTITIONING_DATAPIPELINESTAGE_PARITTIONINGSTRATETGY,
                        "Token"
                    },
                    {
                        PluginParameterNames.TOKEN_CONTENTTEXTPARTITIONING_PARTITIONSIZETOKENS,
                        800
                    },
                    {
                        PluginParameterNames.TOKEN_CONTENTTEXTPARTITIONING_PARTITIONOVERLAPTOKENS,
                        400
                    }
                }
            }
        };
    }
}

using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Core.Examples.Setup;
using FoundationaLLM.Plugins.DataPipeline;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples.Concepts.Plugins
{
    /// <summary>
    /// Example class for testing the SharePoint Online Data Source Plugin in Data Pipelines.
    /// </summary>
    public class Example_DataPipelinePlugins_KnowledgeExtractionDataPipelineStage : TestBase, IClassFixture<TestFixture>
    {
        public Example_DataPipelinePlugins_KnowledgeExtractionDataPipelineStage(ITestOutputHelper output, TestFixture fixture)
            : base(1, output, fixture)
        {
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task DataPipelinePlugins_SharePointOnlineDataSource_GetContentItems(
            string dataPipelineRunWorkItemId,
            string dataPipelineRunId,
            Dictionary<string, object> pluginParameters)
        {
            // Wait for all required initialization tasks to complete.
            await Task.WhenAll([
                StartEventsWorkers()
            ]);

            var packageManager = new PluginPackageManager();
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
                PluginNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE,
                pluginParameters,
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
                //"work-item-AtzJMo4iV06EdZ9348DuRg", // Fannie Mae
                //"work-item-v9Z2ktVYC0aRjqs2VGbxQA", // Curious cat
                "work-item-OeT7azA-Jk2VkUxk3taERw", // web-rule
                "run-20250626-120304-m3sUVTY41E6vJY0SK2BQFQ-TAfGit69y0OhQOwAAtltKw",
                new Dictionary<string, object>
                {
                    {
                        PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONPROMPTOBJECTID,
                        "instances/8ac6074c-bdde-43cb-a140-ec0002d96d2b/providers/FoundationaLLM.Prompt/prompts/ProcessSPOFiles-EntityExtraction"
                    },
                    {
                        PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONCOMPLETIONMODEL,
                        "gpt-4o-mini"
                    },
                    {
                        PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONENTITYTYPES,
                        "PERSON,ORGANIZATION,LOCATION,DATE,TIME,MONEY,PERCENT,PRODUCT,EVENT,CONCEPT"
                    },
                    {
                        PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONMAXOUTPUTTOKENCOUNT,
                        1000
                    },
                    {
                        PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONMODELTEMPERATURE,
                        0.0
                    }
                }
            }
        };
    }
}

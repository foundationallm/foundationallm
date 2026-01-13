using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Services.Plugins;
using FoundationaLLM.Core.Examples.Setup;
using FoundationaLLM.Plugins.DataPipeline;
using FoundationaLLM.Tests;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples.Concepts.Plugins
{
    /// <summary>
    /// Example class for testing the Azure Data Lake Data Source Plugin in Data Pipelines.
    /// </summary>
    public class Example_DataPipelinePlugins_AzureDataLakeDataSource : TestBase, IClassFixture<TestFixture>
    {
        public Example_DataPipelinePlugins_AzureDataLakeDataSource(ITestOutputHelper output, TestFixture fixture)
            : base(1, output, fixture, new DependencyInjectionContainerInitializer())
        {
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task DataPipelinePlugins_AzureDataLakeDataSource_GetContentItems(
            string dataSourceObjectId,
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

            WriteLine("============ FoundationaLLM Data Pipeline Plugins - Azure Data Lake Data Source Tests ============");

            var dataSourcePlugin = packageManager.GetDataSourcePlugin(
                PluginNames.AZUREDATALAKE_DATASOURCE,
                dataSourceObjectId,
                pluginParameters,
                packageManagerResolver,
                MainServiceContainer.ServiceProvider);

            var contentItems = await dataSourcePlugin.GetContentItems();

            Assert.NotEmpty(contentItems);
            Assert.True(contentItems.Count > 0);

            var contentItemContent = await dataSourcePlugin.GetContentItemRawContent(
                contentItems[0].ContentIdentifier);

            Assert.NotNull(contentItemContent);
            Assert.NotNull(contentItemContent.Value);
            Assert.True(contentItemContent.Value.RawContent.ToArray().Length > 0);
        }

        public static TheoryData<string, Dictionary<string, object>> TestData =>
        new()
        {
            {
                "/instances/8ac6074c-bdde-43cb-a140-ec0002d96d2b/providers/FoundationaLLM.DataSource/dataSources/test-azure-data-lake",
                new Dictionary<string, object>
                {
                    {
                        PluginParameterNames.AZUREDATALAKE_DATASOURCE_FOLDERS,
                        "testcontainer/documents"
                    }
                }
            }
        };
    }
}

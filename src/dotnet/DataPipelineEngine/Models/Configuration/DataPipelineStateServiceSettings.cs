using FoundationaLLM.Common.Models.Configuration.CosmosDB;

namespace FoundationaLLM.DataPipelineEngine.Models.Configuration
{
    /// <summary>
    /// Provides the settings for the Data Pipeline State service.
    /// </summary>
    public class DataPipelineStateServiceSettings : DataPipelineServiceSettingsBase
    {
        /// <summary>
        /// Gets or sets the Azure Cosmos DB settings.
        /// </summary>
        public required AzureCosmosDBSettings CosmosDB { get; set; }
    }
}

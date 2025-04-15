using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;

namespace FoundationaLLM.Common.Models.ResourceProviders.DataPipeline
{
    /// <summary>
    /// Provider the model for a data pipeline run.
    /// </summary>
    public class DataPipelineRun : AzureCosmosDBResource, IRunnableResource
    {
        /// <summary>
        /// Gets or sets the object identifier of the data pipeline.
        /// </summary>
        public required string DataPipelineObjectId { get; set; }

        /// <summary>
        /// Gets or sets the name of the manual trigger used to start the pipeline.
        /// </summary>
        public required string TriggerName { get; set; }

        /// <summary>
        /// Gets or sets a dictionary that contains the parameter values required to trigger the pipeline.
        /// </summary>
        public required Dictionary<string, object> TriggerParameterValues { get; set; } = [];

        /// <summary>
        /// Gets or sets the user principal name (UPN) of the user that triggered the creation of the data pipeline run.
        /// </summary>
        public required string TriggeringUPN { get; set; }

        /// <inheritdoc/>
        public bool Completed { get; set; }

        /// <inheritdoc/>
        public bool Successful { get; set; }

        /// <summary>
        /// The logical partition key for the data pipeline run.
        /// </summary>
        /// <remarks>
        /// This property is used by storage providers that support partitioning of data (e.g. Azure Cosmos DB).
        /// </remarks>
        public string PartitionKey =>
            $"{Id}-{InstanceId}";

        /// <summary>
        /// Set default property values.
        /// </summary>
        public DataPipelineRun() =>
            Type = DataPipelineTypes.DataPipelineRun;
    }
}

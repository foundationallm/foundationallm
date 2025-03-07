using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using System.Text.Json.Serialization;

namespace FoundationaLLM.DataPipeline.Models
{
    public class DataPipelineReference : ResourceReference
    {
        /// <summary>
        /// The object type of the data pipeline.
        /// </summary>
        [JsonIgnore]
        public override Type ResourceType =>
            Type switch
            {
                DataPipelineTypes.DataPipeline => typeof(DataPipelineDefinition),
                _ => throw new ResourceProviderException($"The data pipeline type {Type} is not supported.")
            };
    }
}

using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;

namespace FoundationaLLM.Common.Clients
{
    public class DataPipelineServiceClient : IDataPipelineServiceClient
    {
        public Task<DataPipelineRun> CreateDataPipelineRunAsync(DataPipelineRun dataPipelineRun) => throw new NotImplementedException();
    }
}

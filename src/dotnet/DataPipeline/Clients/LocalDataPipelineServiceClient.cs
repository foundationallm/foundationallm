using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.DataPipeline.Interfaces;

namespace FoundationaLLM.DataPipeline.Clients
{
    public class LocalDataPipelineServiceClient : IDataPipelineServiceClient
    {
        public Task<DataPipelineRun> CreateDataPipelineRunAsync(DataPipelineRun dataPipelineRun) => throw new NotImplementedException();
        public Task<DataPipelineRun> GetDataPipelineRunAsync(string dataPipelineRunId) => throw new NotImplementedException();
    }
}

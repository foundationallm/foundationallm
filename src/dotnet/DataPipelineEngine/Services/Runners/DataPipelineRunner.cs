using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.DataPipelineEngine.Exceptions;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.DataPipelineEngine.Services.Runners
{
    /// <summary>
    /// Provides capabilities for running data pipelines.
    /// </summary>
    /// <param name="stateService">The Data Pipeline State service.</param>
    /// <param name="serviceProvider">The service collection provided by the dependency injection container.</param>
    public class DataPipelineRunner(
        IDataPipelineStateService stateService,
        IServiceProvider serviceProvider)
    {
        private readonly IDataPipelineStateService _stateService = stateService;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<DataPipelineRunner> _logger =
            serviceProvider.GetRequiredService<ILogger<DataPipelineRunner>>();

        public async Task Initialize(
            DataPipelineRun dataPipelineRun,
            List<DataPipelineContentItem> contentItems,
            DataPipelineDefinition dataPipelineDefinition,
            UnifiedUserIdentity userIdentity)
        {
            foreach (var contentItem in contentItems)
                contentItem.RunId = dataPipelineRun.RunId;

            var initializationSuccessful = await _stateService.InitializeDataPipelineRunState(
               dataPipelineRun,
               contentItems);

            if (!initializationSuccessful)
                throw new DataPipelineServiceException($"Failed to initialize state for data pipeline run {dataPipelineRun.RunId}.");
        }
    }
}

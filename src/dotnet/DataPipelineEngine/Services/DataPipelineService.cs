using FluentValidation;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.DataPipelineEngine.Exceptions;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using FoundationaLLM.DataPipelineEngine.Models.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.DataPipelineEngine.Services
{
    /// <summary>
    /// Provides services for managing data pipelines.
    /// </summary>
    /// <param name="cosmosDBService">The Azure Cosmos DB service providing database services.</param>
    /// <param name="settings">The settings for the service.</param>
    /// <param name="resourceValidatorFactory">The factory used to create resource validators.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class DataPipelineService(
        IAzureCosmosDBDataPipelineService cosmosDBService,
        DataPipelineServiceSettings settings,
        IResourceValidatorFactory resourceValidatorFactory,
        ILogger<DataPipelineService> logger) : IDataPipelineService
    {
        private readonly IAzureCosmosDBDataPipelineService _cosmosDBService = cosmosDBService;
        private readonly DataPipelineServiceSettings _settings = settings;
        private readonly IResourceValidatorFactory _resourceValidatorFactory = resourceValidatorFactory;
        private readonly ILogger<DataPipelineService> _logger = logger;

        public async Task<DataPipelineRun> CreateDataPipelineRun(string instanceId, DataPipelineRun dataPipelineRun)
        {
            var validator = _resourceValidatorFactory.GetValidator(typeof(DataPipelineRun));
            if (validator is IValidator agentValidator)
            {
                var context = new ValidationContext<object>(dataPipelineRun);
                var validationResult = await agentValidator.ValidateAsync(context);
                if (!validationResult.IsValid)
                {
                    throw new DataPipelineServiceException(
                        $"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}",
                        StatusCodes.Status400BadRequest);
                }
            }

            var newDataPipelineRunId = $"run-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToBase64String()}";
            dataPipelineRun.Id = newDataPipelineRunId;
            dataPipelineRun.Name = newDataPipelineRunId;

            var updatedDataPipelineRun = await _cosmosDBService.UpsertItemAsync<DataPipelineRun>(
                dataPipelineRun.PartitionKey,
                dataPipelineRun);

            return updatedDataPipelineRun;
        }

        public async Task<DataPipelineRun> GetDataPipelineRun(string instanceId, string dataPipelineRunName) =>
            throw new NotImplementedException();
    }
}

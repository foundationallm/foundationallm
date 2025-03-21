using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Text;
using FoundationaLLM.Context.Interfaces;
using FoundationaLLM.Context.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Context.Services
{
    /// <summary>
    /// Provides the implementation for the Azure Cosmos DB file service.
    /// </summary>
    public class AzureCosmosDBFileService: IAzureCosmosDBFileService
    {
        private readonly AzureCosmosDBSettings _settings;
        private readonly ILogger<AzureCosmosDBFileService> _logger;

        private readonly CosmosClient _cosmosClient;
        private readonly Container _contextContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureCosmosDBFileService"/> class.
        /// </summary>
        /// <param name="options">The <see cref="IOptions"/> providing the <see cref="AzureCosmosDBSettings"/>) with the Azure Cosmos DB settings.</param>
        /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
        public AzureCosmosDBFileService(
            IOptions<AzureCosmosDBSettings> options,
            ILogger<AzureCosmosDBFileService> logger)
        {
            _settings = options.Value;
            _logger = logger;

            _logger.LogInformation("Initializing Azure Cosmos DB file service...");
            _cosmosClient = GetCosmosDBClient();
            _contextContainer = GetCosmosDBContextContainer();
            _logger.LogInformation("Successfully initialized Azure Cosmos DB file service.");
        }

        #region Initialization

        private CosmosClient GetCosmosDBClient()
        {
            var opt = new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            // Configure CosmosSystemTextJsonSerializer
            var serializer
                = new CosmosSystemTextJsonSerializer(opt);
            var result = new CosmosClientBuilder(
                    _settings.Endpoint,
                    ServiceContext.AzureCredential)
                .WithCustomSerializer(serializer)
                .WithConnectionModeGateway()
                .Build();

            if (!_settings.EnableTracing)
            {
                var defaultTrace =
                    Type.GetType("Microsoft.Azure.Cosmos.Core.Trace.DefaultTrace,Microsoft.Azure.Cosmos.Direct");
                var traceSource = (TraceSource)defaultTrace?.GetProperty("TraceSource")?.GetValue(null)!;
                traceSource.Switch.Level = SourceLevels.All;
                traceSource.Listeners.Clear();
            }

            return result;
        }

        private Container GetCosmosDBContextContainer()
        {
            var database = _cosmosClient.GetDatabase(_settings.Database)
                ?? throw new ConfigurationValueException($"The Azure Cosmos DB database '{_settings.Database}' was not found.");

            // The Context container name is the first container in the list.
            var contextContainerName = _settings.Containers.Split(',')[0];
            var result = database.GetContainer(contextContainerName)
                ?? throw new ConfigurationValueException($"The Azure Cosmos DB container [{contextContainerName}] was not found.");

            return result;
        }

        #endregion

        /// <inheritdoc/>
        public async Task<T> UpsertItemAsync<T>(
            string partitionKey,
            T item,
            CancellationToken cancellationToken = default)
        {
            var response = await _contextContainer.UpsertItemAsync(
                item: item,
                partitionKey: new PartitionKey(partitionKey),
                cancellationToken: cancellationToken
            );

            return response.Resource;
        }
    }
}

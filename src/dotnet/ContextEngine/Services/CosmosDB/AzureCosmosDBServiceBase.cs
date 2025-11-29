using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Text;
using FoundationaLLM.Context.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Context.Services.CosmosDB
{
    /// <summary>
    /// Provides the base implementation for the Azure Cosmos DB services.
    /// </summary>
    public class AzureCosmosDBServiceBase : IAzureCosmosDBServiceBase
    {
        protected readonly AzureCosmosDBSettings _settings;
        protected readonly ILogger<AzureCosmosDBServiceBase> _logger;

        protected readonly CosmosClient _cosmosClient;
        protected readonly Container _contextContainer;

        /// <inheritdoc/>
        public Container ContextContainer => _contextContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureCosmosDBServiceBase"/> class.
        /// </summary>
        /// <param name="options">The <see cref="IOptions"/> providing the <see cref="AzureCosmosDBSettings"/>) with the Azure Cosmos DB settings.</param>
        /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
        public AzureCosmosDBServiceBase(
            IOptions<AzureCosmosDBSettings> options,
            ILogger<AzureCosmosDBServiceBase> logger)
        {
            _settings = options.Value;
            _logger = logger;

            _logger.LogInformation("Initializing the Azure Cosmos DB service base...");
            _cosmosClient = GetCosmosDBClient();
            _contextContainer = GetCosmosDBContextContainer();
            _logger.LogInformation("Successfully initialized the Azure Cosmos DB service base.");
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

        public async Task<List<T>> RetrieveItems<T>(
            QueryDefinition query)
        {
            var results = _contextContainer.GetItemQueryIterator<T>(query);

            List<T> output = [];
            while (results.HasMoreResults)
            {
                var response = await results.ReadNextAsync();
                output.AddRange(response);
            }

            return output;
        }

        public async Task<bool> ItemExists<T>(
            string partitionKey,
            string id,
            Func<T,bool>? existencePredicate)
        {
            try
            {
                var response = await _contextContainer.ReadItemAsync<T>(
                    id,
                    new PartitionKey(partitionKey));

                return
                    response.StatusCode == HttpStatusCode.OK
                    && (existencePredicate is null || existencePredicate(response.Resource));
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Item does not exist
                return false;
            }
        }
    }
}

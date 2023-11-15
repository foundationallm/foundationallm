using FoundationaLLM.Common.Models;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Search;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models.Configuration;
using FoundationaLLM.Core.Utils;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace FoundationaLLM.Core.Services
{
    /// <summary>
    /// Service to access Azure Cosmos DB for NoSQL.
    /// </summary>
    public class CosmosDbService : ICosmosDbService
    {
        private readonly Container _sessions;
        private readonly Container _userSessions;
        private readonly Database _database;
        private readonly Dictionary<string, Container> _containers;
        private readonly CosmosDbSettings _settings;
        private readonly ILogger _logger;

        private const string SoftDeleteQueryRestriction = " (not IS_DEFINED(c.deleted) OR c.deleted = false)";

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbService"/> class.
        /// </summary>
        /// <param name="ragService">The service used to make calls to the Gatekeeper
        /// API to add the entity to the orchestrator's memory used by the RAG service.</param>
        /// <param name="settings">The <see cref="CosmosDbSettings"/> settings retrieved
        /// by the injected <see cref="IOptions{TOptions}"/>.</param>
        /// <param name="logger">The logging interface used to log under the
        /// <see cref="CosmosDbService"></see> type name.</param>
        /// <exception cref="ArgumentException">Thrown if any of the required settings
        /// are null or empty.</exception>
        public CosmosDbService(
            IOptions<CosmosDbSettings> settings,
            ILogger<CosmosDbService> logger)
        {
            _settings = settings.Value;
            ArgumentException.ThrowIfNullOrEmpty(_settings.Endpoint);
            ArgumentException.ThrowIfNullOrEmpty(_settings.Key);
            ArgumentException.ThrowIfNullOrEmpty(_settings.Database);
            ArgumentException.ThrowIfNullOrEmpty(_settings.Containers);

            _logger = logger;
            _logger.LogInformation("Initializing Cosmos DB service.");

            if (!_settings.EnableTracing)
            {
                var defaultTrace = Type.GetType("Microsoft.Azure.Cosmos.Core.Trace.DefaultTrace,Microsoft.Azure.Cosmos.Direct");
                var traceSource = (TraceSource)defaultTrace?.GetProperty("TraceSource")?.GetValue(null)!;
                traceSource.Switch.Level = SourceLevels.All;
                traceSource.Listeners.Clear();
            }

            CosmosSerializationOptions options = new()
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            };

            var client = new CosmosClientBuilder(_settings.Endpoint, _settings.Key)
                .WithSerializerOptions(options)
                .WithConnectionModeGateway()
                .Build();

            var database = client?.GetDatabase(_settings.Database);

            _database = database ??
                        throw new ArgumentException("Unable to connect to existing Azure Cosmos DB database.");


            // Dictionary of container references for all containers listed in config.
            _containers = new Dictionary<string, Container>();

            var containers = _settings.Containers.Split(',').ToList();

            foreach (var containerName in containers)
            {
                var container = database?.GetContainer(containerName.Trim()) ??
                                throw new ArgumentException("Unable to connect to existing Azure Cosmos DB container or database.");

                _containers.Add(containerName.Trim(), container);
            }

            _sessions = database?.GetContainer(CosmosDbContainers.Sessions) ??
                        throw new ArgumentException($"Unable to connect to existing Azure Cosmos DB container ({CosmosDbContainers.Sessions}).");
            _userSessions = database?.GetContainer(CosmosDbContainers.UserSessions) ??
                            throw new ArgumentException($"Unable to connect to existing Azure Cosmos DB container ({CosmosDbContainers.UserSessions}).");
            _logger.LogInformation("Cosmos DB service initialized.");
        }

        /// <summary>
        /// Gets a list of all current chat sessions.
        /// </summary>
        /// <param name="type">The session type to return.</param>
        /// <param name="upn">The user principal name used for retrieving
        /// sessions for the signed in user.</param>
        /// <returns>List of distinct chat session items.</returns>
        public async Task<List<Session>> GetSessionsAsync(string type, string upn)
        {
            var query = new QueryDefinition($"SELECT DISTINCT * FROM c WHERE c.type = @type AND c.upn = @upn AND {SoftDeleteQueryRestriction} ORDER BY c._ts DESC")
                .WithParameter("@type", type)
                .WithParameter("@upn", upn);

            var response = _userSessions.GetItemQueryIterator<Session>(query);

            List<Session> output = new();
            while (response.HasMoreResults)
            {
                var results = await response.ReadNextAsync();
                output.AddRange(results);
            }

            return output;
        }

        /// <summary>
        /// Performs a point read to retrieve a single chat session item.
        /// </summary>
        /// <returns>The chat session item.</returns>
        public async Task<Session> GetSessionAsync(string id)
        {
            var session = await _sessions.ReadItemAsync<Session>(
                id: id,
                partitionKey: new PartitionKey(id));
            
            return session;
        }

        /// <summary>
        /// Gets a list of all current chat messages for a specified session identifier.
        /// </summary>
        /// <param name="sessionId">Chat session identifier used to filter messages.</param>
        /// <param name="upn">The user principal name used for retrieving the messages for
        /// the signed in user.</param>
        /// <returns>List of chat message items for the specified session.</returns>
        public async Task<List<Message>> GetSessionMessagesAsync(string sessionId, string upn)
        {
            var query =
                new QueryDefinition($"SELECT * FROM c WHERE c.sessionId = @sessionId AND c.type = @type AND c.upn = @upn AND {SoftDeleteQueryRestriction}")
                    .WithParameter("@sessionId", sessionId)
                    .WithParameter("@type", nameof(Message))
                    .WithParameter("@upn", upn);

            var results = _sessions.GetItemQueryIterator<Message>(query);

            List<Message> output = new();
            while (results.HasMoreResults)
            {
                var response = await results.ReadNextAsync();
                output.AddRange(response);
            }

            return output;
        }

        /// <summary>
        /// Creates a new chat session.
        /// </summary>
        /// <param name="session">Chat session item to create.</param>
        /// <returns>Newly created chat session item.</returns>
        public async Task<Session> InsertSessionAsync(Session session)
        {
            PartitionKey partitionKey = new(session.SessionId);
            return await _sessions.CreateItemAsync(
                item: session,
                partitionKey: partitionKey
            );
        }

        /// <summary>
        /// Creates a new chat message.
        /// </summary>
        /// <param name="message">Chat message item to create.</param>
        /// <returns>Newly created chat message item.</returns>
        public async Task<Message> InsertMessageAsync(Message message)
        {
            PartitionKey partitionKey = new(message.SessionId);
            return await _sessions.CreateItemAsync(
                item: message,
                partitionKey: partitionKey
            );
        }

        /// <summary>
        /// Updates an existing chat message.
        /// </summary>
        /// <param name="message">Chat message item to update.</param>
        /// <returns>Revised chat message item.</returns>
        public async Task<Message> UpdateMessageAsync(Message message)
        {
            PartitionKey partitionKey = new(message.SessionId);
            return await _sessions.ReplaceItemAsync(
                item: message,
                id: message.Id,
                partitionKey: partitionKey
            );
        }

        /// <summary>
        /// Updates a message's rating through a patch operation.
        /// </summary>
        /// <param name="id">The message id.</param>
        /// <param name="sessionId">The message's partition key (session id).</param>
        /// <param name="rating">The rating to replace.</param>
        /// <returns>Revised chat message item.</returns>
        public async Task<Message> UpdateMessageRatingAsync(string id, string sessionId, bool? rating)
        {
            var response = await _sessions.PatchItemAsync<Message>(
                id: id,
                partitionKey: new PartitionKey(sessionId),
                patchOperations: new[]
                {
                    PatchOperation.Set("/rating", rating),
                }
            );
            return response.Resource;
        }

        /// <summary>
        /// Updates an existing chat session.
        /// </summary>
        /// <param name="session">Chat session item to update.</param>
        /// <returns>Revised created chat session item.</returns>
        public async Task<Session> UpdateSessionAsync(Session session)
        {
            PartitionKey partitionKey = new(session.SessionId);
            return await _sessions.ReplaceItemAsync(
                item: session,
                id: session.Id,
                partitionKey: partitionKey
            );
        }

        /// <summary>
        /// Updates a session's name through a patch operation.
        /// </summary>
        /// <param name="id">The session id.</param>
        /// <param name="name">The session's new name.</param>
        /// <returns>Revised chat session item.</returns>
        public async Task<Session> UpdateSessionNameAsync(string id, string name)
        {
            var response = await _sessions.PatchItemAsync<Session>(
                id: id,
                partitionKey: new PartitionKey(id),
                patchOperations: new[]
                {
                    PatchOperation.Set("/name", name),
                }
            );
            return response.Resource;
        }

        /// <summary>
        /// Batch create or update chat messages and session.
        /// </summary>
        /// <param name="messages">Chat message and session items to create or replace.</param>
        public async Task UpsertSessionBatchAsync(params dynamic[] messages)
        {
            if (messages.Select(m => m.SessionId).Distinct().Count() > 1)
            {
                throw new ArgumentException("All items must have the same partition key.");
            }

            PartitionKey partitionKey = new(messages.First().SessionId);
            var batch = _sessions.CreateTransactionalBatch(partitionKey);
            foreach (var message in messages)
            {
                batch.UpsertItem(
                    item: message
                );
            }

            await batch.ExecuteAsync();
        }

        /// <summary>
        /// Create or update a user session from the passed in Session object.
        /// </summary>
        /// <param name="session">The chat session item to create or replace.</param>
        /// <returns></returns>
        public async Task UpsertUserSessionAsync(Session session)
        {
            PartitionKey partitionKey = new(session.UPN);
            await _userSessions.UpsertItemAsync(
               item: session,
               partitionKey: partitionKey);
        }

        /// <summary>
        /// Batch deletes an existing chat session and all related messages.
        /// </summary>
        /// <param name="sessionId">Chat session identifier used to flag messages and sessions for deletion.</param>
        public async Task DeleteSessionAndMessagesAsync(string sessionId)
        {
            PartitionKey partitionKey = new(sessionId);

            // TODO: await container.DeleteAllItemsByPartitionKeyStreamAsync(partitionKey);

            var query = new QueryDefinition($"SELECT * FROM c WHERE c.sessionId = @sessionId AND {SoftDeleteQueryRestriction}")
                .WithParameter("@sessionId", sessionId);

            var response = _sessions.GetItemQueryIterator<dynamic>(query);

            Console.WriteLine($"Deleting {sessionId} session and related messages.");

            var batch = _sessions.CreateTransactionalBatch(partitionKey);
            var count = 0;

            // Local function to execute and reset the batch.
            async Task ExecuteBatchAsync()
            {
                if (count > 0) // Execute the batch only if it has any items.
                {
                    await batch.ExecuteAsync();
                    count = 0;
                    batch = _sessions.CreateTransactionalBatch(partitionKey);
                }
            }

            while (response.HasMoreResults)
            {
                var results = await response.ReadNextAsync();
                foreach (var item in results)
                {
                    item.deleted = true;
                    batch.UpsertItem(item);
                    count++;
                    if (count >= 100) // Execute the batch after adding 100 items (100 actions per batch execution is the limit).
                    {
                        await ExecuteBatchAsync();
                    }
                }
            }

            await ExecuteBatchAsync();
        }

        /// <summary>
        /// Reads all documents retrieved by Vector Search.
        /// </summary>
        /// <param name="vectorDocuments">List string of JSON documents from vector search results</param>
        public async Task<string> GetVectorSearchDocumentsAsync(List<DocumentVector> vectorDocuments)
        {

            var searchDocuments = new List<string>();

            foreach (var document in vectorDocuments)
            {

                try
                {
                    var response = await _containers[document.containerName].ReadItemStreamAsync(
                        document.itemId, new PartitionKey(document.partitionKey));


                    if ((int)response.StatusCode < 200 || (int)response.StatusCode >= 400)
                        _logger.LogError(
                            $"Failed to retrieve an item for id '{document.itemId}' - status code '{response.StatusCode}");

                    if (response.Content == null)
                    {
                        _logger.LogInformation(
                            $"Null content received for document '{document.itemId}' - status code '{response.StatusCode}");
                        continue;
                    }

                    string item;
                    using (var sr = new StreamReader(response.Content))
                        item = await sr.ReadToEndAsync();

                    searchDocuments.Add(item);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);

                }
            }

            var resultDocuments = string.Join(Environment.NewLine + "-", searchDocuments);

            return resultDocuments;

        }

        /// <summary>
        /// Returns the completion prompt for a given session and completion prompt id.
        /// </summary>
        /// <param name="sessionId">The session id from which to retrieve the completion prompt.</param>
        /// <param name="completionPromptId">The id of the completion prompt to retrieve.</param>
        /// <returns></returns>
        public async Task<CompletionPrompt> GetCompletionPrompt(string sessionId, string completionPromptId)
        {
            return await _sessions.ReadItemAsync<CompletionPrompt>(
                id: completionPromptId,
                partitionKey: new PartitionKey(sessionId));
        }

    }
}

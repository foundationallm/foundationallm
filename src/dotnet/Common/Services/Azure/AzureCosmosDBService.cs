using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Azure.CosmosDB;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Models.Configuration.Users;
using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentFiles;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using FoundationaLLM.Common.Models.ResourceProviders.Skill;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace FoundationaLLM.Common.Services.Azure
{
    /// <summary>
    /// Service to access Azure Cosmos DB for NoSQL.
    /// </summary>
    public class AzureCosmosDBService : IAzureCosmosDBService
    {
        private Container _sessions;
        private Container _userSessions;
        private Container _operations;
        private Container _agents;
        private Container _attachments;
        private Container _externalResources;
        private Container _completionsCache;
        private Container _dataPipelines;
        private readonly Lazy<Task<Container>> _userProfiles;
        private Task<Container> _userProfilesTask => _userProfiles.Value;
        private readonly Lazy<Task<Container>> _skills;
        private Task<Container> _skillsTask => _skills.Value;
        private readonly Database _database;
        private readonly AzureCosmosDBSettings _settings;
        private readonly ResiliencePipeline _resiliencePipeline;
        private readonly ILogger _logger;

        private readonly Dictionary<string, Container> _containers = [];

        private const string SoftDeleteQueryRestriction = " (not IS_DEFINED(c.deleted) OR c.deleted = false)";

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureCosmosDBService"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="AzureCosmosDBSettings"/> settings retrieved
        /// by the injected <see cref="IOptions{TOptions}"/>.</param>
        /// <param name="client">The Cosmos DB client.</param>
        /// <param name="logger">The logging interface used to log under the
        /// <see cref="AzureCosmosDBService"></see> type name.</param>
        /// <exception cref="ArgumentException">Thrown if any of the required settings
        /// are null or empty.</exception>
        public AzureCosmosDBService(
            IOptions<AzureCosmosDBSettings> settings,
            CosmosClient client,
            ILogger<AzureCosmosDBService> logger)
        {
            _settings = settings.Value;
            ArgumentException.ThrowIfNullOrEmpty(_settings.Endpoint);
            ArgumentException.ThrowIfNullOrEmpty(_settings.Database);
            ArgumentException.ThrowIfNullOrEmpty(_settings.Containers);

            _logger = logger;
            _logger.LogInformation("Initializing Cosmos DB service.");

            if (!_settings.EnableTracing)
            {
                var defaultTrace =
                    Type.GetType("Microsoft.Azure.Cosmos.Core.Trace.DefaultTrace,Microsoft.Azure.Cosmos.Direct");
                var traceSource = (TraceSource)defaultTrace?.GetProperty("TraceSource")?.GetValue(null)!;
                traceSource.Switch.Level = SourceLevels.All;
                traceSource.Listeners.Clear();
            }

            _resiliencePipeline = new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                MaxRetryAttempts = 6,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    var exception = args.Outcome.Exception!;
                    _logger.LogWarning($"Cosmos DB resilience strategy handling: {exception.Message}");
                    _logger.LogWarning($" ... automatically delaying for {args.RetryDelay.TotalMilliseconds}ms.");
                    return default;
                }
            }).Build();

            var database = client?.GetDatabase(_settings.Database);

            _database = database
                ?? throw new ArgumentException("Unable to connect to existing Azure Cosmos DB database.");

            _sessions = database?.GetContainer(AzureCosmosDBContainers.Sessions)
                ?? throw new ArgumentException(
                    $"Unable to connect to existing Azure Cosmos DB container ({AzureCosmosDBContainers.Sessions}).");

            _userSessions = database?.GetContainer(AzureCosmosDBContainers.UserSessions)
                ?? throw new ArgumentException(
                    $"Unable to connect to existing Azure Cosmos DB container ({AzureCosmosDBContainers.UserSessions}).");

            _operations = database?.GetContainer(AzureCosmosDBContainers.Operations)
                ?? throw new ArgumentException(
                    $"Unable to connect to existing Azure Cosmos DB container ({AzureCosmosDBContainers.Operations}).");

            _userProfiles = new Lazy<Task<Container>>(InitializeUserProfilesContainer);
            _skills = new Lazy<Task<Container>>(InitializeSkillsContainer);

            _agents = database?.GetContainer(AzureCosmosDBContainers.Agents)
                ?? throw new ArgumentException(
                    $"Unable to connect to existing Azure Cosmos DB container ({AzureCosmosDBContainers.Agents}).");

            _attachments = database?.GetContainer(AzureCosmosDBContainers.Attachments)
                ?? throw new ArgumentException(
                    $"Unable to connect to existing Azure Cosmos DB container ({AzureCosmosDBContainers.Attachments}).");

            _externalResources = database?.GetContainer(AzureCosmosDBContainers.ExternalResources)
                ?? throw new ArgumentException(
                    $"Unable to connect to existing Azure Cosmos DB container ({AzureCosmosDBContainers.ExternalResources}).");

            _completionsCache = database?.GetContainer(AzureCosmosDBContainers.CompletionsCache)
                ?? throw new ArgumentException(
                    $"Unable to connect to existing Azure Cosmos DB container ({AzureCosmosDBContainers.CompletionsCache}).");

            _dataPipelines = database?.GetContainer(AzureCosmosDBContainers.DataPipelines)
                ?? throw new ArgumentException(
                    $"Unable to connect to existing Azure Cosmos DB container ({AzureCosmosDBContainers.DataPipelines}).");

            _containers[AzureCosmosDBContainers.Sessions] = _sessions;
            _containers[AzureCosmosDBContainers.UserSessions] = _userSessions;
            _containers[AzureCosmosDBContainers.Operations] = _operations;
            _containers[AzureCosmosDBContainers.Attachments] = _attachments;
            _containers[AzureCosmosDBContainers.ExternalResources] = _externalResources;
            _containers[AzureCosmosDBContainers.CompletionsCache] = _completionsCache;
            _containers[AzureCosmosDBContainers.DataPipelines] = _dataPipelines;

            _logger.LogInformation("Cosmos DB service initialized.");
        }

        private async Task<Container> InitializeUserProfilesContainer() =>
            await _resiliencePipeline.ExecuteAsync<Container>(async token => await _database?.CreateContainerIfNotExistsAsync(new ContainerProperties(AzureCosmosDBContainers.UserProfiles,
                "/upn"), ThroughputProperties.CreateAutoscaleThroughput(1000), cancellationToken: token)!);

        private async Task<Container> InitializeSkillsContainer() =>
            await _resiliencePipeline.ExecuteAsync<Container>(async token => await _database?.CreateContainerIfNotExistsAsync(new ContainerProperties(AzureCosmosDBContainers.Skills,
                "/upn"), ThroughputProperties.CreateAutoscaleThroughput(1000), cancellationToken: token)!);

        #region Methods reserved for the FoundationaLLM.Conversation resource provider

        /// <inheritdoc/>
        public async Task<List<Conversation>> GetConversationsAsync(string type, string upn, CancellationToken cancellationToken = default)
        {
            var query = new QueryDefinition($"SELECT DISTINCT * FROM c WHERE c.type = @type AND c.upn = @upn AND {SoftDeleteQueryRestriction} ORDER BY c._ts DESC")
                .WithParameter("@type", type)
                .WithParameter("@upn", upn);

            var response = _userSessions.GetItemQueryIterator<Conversation>(query);

            List<Conversation> output = [];
            while (response.HasMoreResults)
            {
                var results = await response.ReadNextAsync(cancellationToken);
                output.AddRange(results);
            }

            return output;
        }

        /// <inheritdoc/>
        public async Task<Conversation?> GetConversationAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _sessions.ReadItemAsync<Conversation>(
                    id: id,
                    partitionKey: new PartitionKey(id),
                    cancellationToken: cancellationToken);

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<Conversation> CreateOrUpdateConversationAsync(Conversation session, CancellationToken cancellationToken = default)
        {
            PartitionKey partitionKey = new(session.SessionId);
            return await _sessions.UpsertItemAsync(
                item: session,
                partitionKey: partitionKey,
                cancellationToken: cancellationToken
            );
        }

        /// <inheritdoc/>
        public async Task<Conversation> PatchConversationPropertiesAsync(string id, string upn, Dictionary<string, object?> propertyValues, CancellationToken cancellationToken = default)
        {
            // Append audit patch operations.
            const string updatedBy = "/updatedBy";
            const string updatedOn = "/updatedOn";
            if (!propertyValues.ContainsKey(updatedBy))
            {
                propertyValues.Add(updatedBy, !string.IsNullOrWhiteSpace(upn) ? upn : "N/A");
            }
            if (!propertyValues.ContainsKey(updatedOn))
            {
                propertyValues.Add(updatedOn, DateTimeOffset.UtcNow);
            }

            var response = await _sessions.PatchItemAsync<Conversation>(
                id: id,
                partitionKey: new PartitionKey(id),
                patchOperations: propertyValues.Keys
                    .Select(key => PatchOperation.Set(key, propertyValues[key])).ToArray(),
                requestOptions: new PatchItemRequestOptions
                {
                    FilterPredicate = $"FROM c WHERE c.upn = '{upn}'"
                },
                cancellationToken: cancellationToken
            );
            return response.Resource;
        }

        /// <inheritdoc/>
        public async Task DeleteConversationAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            PartitionKey partitionKey = new(sessionId);

            // TODO: await container.DeleteAllItemsByPartitionKeyStreamAsync(partitionKey);

            var query = new QueryDefinition($"SELECT * FROM c WHERE c.sessionId = @sessionId AND {SoftDeleteQueryRestriction}")
                .WithParameter("@sessionId", sessionId);

            var response = _sessions.GetItemQueryIterator<dynamic>(query);

            var batch = _sessions.CreateTransactionalBatch(partitionKey);
            var count = 0;

            // Local function to execute and reset the batch.
            async Task ExecuteBatchAsync()
            {
                if (count > 0) // Execute the batch only if it has any items.
                {
                    await batch.ExecuteAsync(cancellationToken);
                    count = 0;
                    batch = _sessions.CreateTransactionalBatch(partitionKey);
                }
            }

            while (response.HasMoreResults)
            {
                var results = await response.ReadNextAsync(cancellationToken);
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

        #endregion

        /// <inheritdoc/>
        public async Task<T?> GetItemAsync<T>(string containerName, string id, string partitionKey, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _containers[containerName].ReadItemAsync<T>(
                    id: id,
                    partitionKey: new PartitionKey(partitionKey),
                    cancellationToken: cancellationToken);

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }
        }

        /// <inheritdoc/>
        public async Task<T?> UpsertItemAsync<T>(string containerName, string partitionKey, T item, CancellationToken cancellationToken = default)
        {
            var response = await _containers[containerName].UpsertItemAsync(
                item: item,
                partitionKey: new PartitionKey(partitionKey),
                cancellationToken: cancellationToken
            );

            return response.Resource;
        }

        /// <inheritdoc/>
        public async Task<T> PatchItemPropertiesAsync<T>(string containerName, string partitionKey, string id,
            string upn, Dictionary<string, object?> propertyValues, CancellationToken cancellationToken = default)
        {
            await EnsureContainerAvailability(containerName);

            var response = await _containers[containerName].PatchItemAsync<T>(
                id: id,
                partitionKey: new PartitionKey(partitionKey),
                patchOperations: propertyValues.Keys
                    .Select(key => PatchOperation.Set(key, propertyValues[key])).ToArray(),
                requestOptions: new PatchItemRequestOptions
                {
                    FilterPredicate = $"FROM c WHERE c.upn = '{upn}'"
                },
                cancellationToken: cancellationToken
            );
            return response.Resource;
        }

        /// <inheritdoc/>
        public async Task<List<Message>> GetConversationMessagesAsync(string sessionId, string upn, int? max = null, CancellationToken cancellationToken = default)
        {
            var select = max.HasValue
                ? $"SELECT TOP {max} * FROM c WHERE c.sessionId = @sessionId AND c.type = @type AND c.upn = @upn AND {SoftDeleteQueryRestriction} ORDER BY c.timeStamp DESC"
                : $"SELECT * FROM c WHERE c.sessionId = @sessionId AND c.type = @type AND c.upn = @upn AND {SoftDeleteQueryRestriction} ORDER BY c.timeStamp";
            var query = new QueryDefinition(select)
                    .WithParameter("@sessionId", sessionId)
                    .WithParameter("@type", nameof(Message))
                    .WithParameter("@upn", upn);

            var results = _sessions.GetItemQueryIterator<Message>(query);

            List<Message> output = new();
            while (results.HasMoreResults)
            {
                var response = await results.ReadNextAsync(cancellationToken);
                output.AddRange(response);
            }

            output = output.OrderBy(m => m.TimeStamp).ToList();

            return output;
        }

        /// <inheritdoc/>
        public async Task<int> GetConversationMessagesCountAsync(string sessionId, string upn, CancellationToken cancellationToken = default)
        {
            var query = new QueryDefinition($"""
                SELECT VALUE COUNT(1)
                FROM c
                WHERE c.sessionId = @sessionId
                  AND c.type = @type
                  AND c.upn = @upn
                  AND {SoftDeleteQueryRestriction}
                """)
                .WithParameter("@sessionId", sessionId)
                .WithParameter("@type", nameof(Message))
                .WithParameter("@upn", upn);

            var iterator = _sessions.GetItemQueryIterator<int>(query);

            if (!iterator.HasMoreResults)
                return 0;

            var response = await iterator.ReadNextAsync(cancellationToken);
            var count = response.Resource.FirstOrDefault();

            return count;
        }

        /// <inheritdoc/>
        public async Task<Message> GetMessageAsync(string id, string sessionId, CancellationToken cancellationToken = default) =>
            await _sessions.ReadItemAsync<Message>(
                id: id,
                partitionKey: new PartitionKey(sessionId),
                cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public async Task<Message> InsertMessageAsync(Message message, CancellationToken cancellationToken = default)
        {
            PartitionKey partitionKey = new(message.SessionId);
            return await _sessions.CreateItemAsync(
                item: message,
                partitionKey: partitionKey,
                cancellationToken: cancellationToken
            );
        }

        /// <inheritdoc/>
        public async Task<Message> UpdateMessageAsync(Message message, CancellationToken cancellationToken = default)
        {
            PartitionKey partitionKey = new(message.SessionId);
            return await _sessions.ReplaceItemAsync(
                item: message,
                id: message.Id,
                partitionKey: partitionKey,
                cancellationToken: cancellationToken
            );
        }

        /// <inheritdoc/>
        public async Task<T> PatchSessionsItemPropertiesAsync<T>(
            string itemId,
            string partitionKey,
            Dictionary<string, object?> propertyValues,
            CancellationToken cancellationToken = default)
        {
            var result = await _sessions.PatchItemAsync<T>(
                id: itemId,
                partitionKey: new PartitionKey(partitionKey),
                patchOperations: propertyValues.Keys
                    .Select(key => PatchOperation.Set(key, propertyValues[key])).ToArray(),
                cancellationToken: cancellationToken
            );

            return result.Resource;
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, object>> PatchMultipleSessionsItemsInTransactionAsync(
            string partitionKey,
            List<IPatchOperationItem> patchOperations,
            CancellationToken cancellationToken = default)
        {
            var container = _sessions;
            var batch = container.CreateTransactionalBatch(new PartitionKey(partitionKey));

            foreach (var patchOperation in patchOperations)
            {
                batch.PatchItem(
                    id: patchOperation.ItemId,
                    patchOperations: patchOperation.PropertyValues.Keys
                        .Select(key => PatchOperation.Set(key, patchOperation.PropertyValues[key])).ToArray()
                );
            }

            var response = await batch.ExecuteAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Transactional batch failed with status code: {response.StatusCode}");
            }

            var resultDictionary = new Dictionary<string, object>();

            for (var i = 0; i < response.Count; i++)
            {
                var patchOperation = patchOperations[i];

                var method = typeof(TransactionalBatchResponse)
                    .GetMethod(nameof(response.GetOperationResultAtIndex))!
                    .MakeGenericMethod(patchOperation.ItemType);

                var result = method.Invoke(response, new object[] { i });
                var resourceProperty = result.GetType().GetProperty("Resource");
                var deserializedObject = resourceProperty?.GetValue(result);

                resultDictionary[patchOperation.ItemId] = deserializedObject;
            }

            return resultDictionary;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public async Task UpsertUserSessionAsync(Conversation session, CancellationToken cancellationToken = default)
        {
            PartitionKey partitionKey = new(session.UPN);
            await _userSessions.UpsertItemAsync(
               item: session,
               partitionKey: partitionKey,
               cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<CompletionPrompt> GetCompletionPromptAsync(string sessionId, string completionPromptId) =>
            await _sessions.ReadItemAsync<CompletionPrompt>(
                id: completionPromptId,
                partitionKey: new PartitionKey(sessionId));

        /// <inheritdoc/>
        public async Task<UserProfile> GetUserProfileAsync(string upn, CancellationToken cancellationToken = default)
        {
            var userProfiles = await _userProfilesTask;

            try
            {
                var userProfile = await userProfiles.ReadItemAsync<UserProfile>(
                    id: upn,
                    partitionKey: new PartitionKey(upn),
                    cancellationToken: cancellationToken);

                return userProfile;
            }
            catch (CosmosException ce) when (ce.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var userProfile = new UserProfile(upn);

                await UpsertUserProfileAsync(userProfile, cancellationToken);

                return userProfile;
            }
        }

        /// <inheritdoc/>
        public async Task<UserData?> GetUserDataAsync(string upn, CancellationToken cancellationToken = default)
        {
            var userProfiles = await _userProfilesTask;

            try
            {
                var userData = await userProfiles.ReadItemAsync<UserData>(
                    id: UserData.GetId(upn),
                    partitionKey: new PartitionKey(upn),
                    cancellationToken: cancellationToken);

                return userData;
            }
            catch (CosmosException ce) when (ce.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }
        }

        /// <inheritdoc/>
        public async Task UpsertUserProfileAsync(UserProfile userProfile, CancellationToken cancellationToken = default)
        {
            var userProfiles = await _userProfilesTask;
            PartitionKey partitionKey = new(userProfile.UPN);
            await userProfiles.UpsertItemAsync(
                item: userProfile,
                partitionKey: partitionKey,
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public async Task UpsertUserDataAsync(UserData userData, CancellationToken cancellationToken = default)
        {
            var userProfiles = await _userProfilesTask;
            PartitionKey partitionKey = new(userData.UPN);
            await userProfiles.UpsertItemAsync(
                item: userData,
                partitionKey: partitionKey,
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<LongRunningOperationContext> GetLongRunningOperationContextAsync(string operationId, CancellationToken cancellationToken = default)
        {
            var longRunningOperationContext = await _operations.ReadItemAsync<LongRunningOperationContext>(
                id: operationId,
                partitionKey: new PartitionKey(operationId),
                cancellationToken: cancellationToken);

            return longRunningOperationContext;
        }

        /// <inheritdoc/>
        public async Task UpsertLongRunningOperationContextAsync(LongRunningOperationContext longRunningOperationContext, CancellationToken cancellationToken = default)
        {
            PartitionKey partitionKey = new(longRunningOperationContext.OperationId);
            await _operations.UpsertItemAsync(
                item: longRunningOperationContext,
                partitionKey: partitionKey,
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<T> PatchOperationsItemPropertiesAsync<T>(string itemId, string partitionKey, Dictionary<string, object?> propertyValues, CancellationToken cancellationToken = default)
        {
            var result = await _operations.PatchItemAsync<T>(
                id: itemId,
                partitionKey: new PartitionKey(partitionKey),
                patchOperations: propertyValues.Keys
                    .Select(key => PatchOperation.Set(key, propertyValues[key])).ToArray(),
                cancellationToken: cancellationToken
            );

            return result.Resource;
        }

        /// <inheritdoc/>
        public async Task<AttachmentReference?> GetAttachment(string upn, string resourceName, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _attachments.ReadItemAsync<AttachmentReference>(
                    id: resourceName,
                    partitionKey: new PartitionKey(upn),
                    cancellationToken: cancellationToken);

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<List<AttachmentReference>> FilterAttachments(string upn, ResourceFilter resourceFilter, CancellationToken cancellationToken = default)
        {
            var objectIds = string.Join(',', resourceFilter.ObjectIDs!.Select(x => $"'{x}'"));

            var query =
               new QueryDefinition($"SELECT DISTINCT * FROM c WHERE c.objectId IN ({@objectIds}) AND c.upn = @upn AND {SoftDeleteQueryRestriction}")
                   .WithParameter("@upn", upn);

            var results = _attachments.GetItemQueryIterator<AttachmentReference>(query);

            List<AttachmentReference> output = new();
            while (results.HasMoreResults)
            {
                var response = await results.ReadNextAsync(cancellationToken);
                output.AddRange(response);
            }

            return output;
        }

        /// <inheritdoc/>
        public async Task<List<AttachmentReference>> GetAttachments(string upn, CancellationToken cancellationToken = default)
        {
            var query = new QueryDefinition($"SELECT DISTINCT * FROM c WHERE c.upn = @upn AND {SoftDeleteQueryRestriction} ORDER BY c._ts DESC")
                .WithParameter("@upn", upn);

            var response = _attachments.GetItemQueryIterator<AttachmentReference>(query);

            List<AttachmentReference> output = [];
            while (response.HasMoreResults)
            {
                var results = await response.ReadNextAsync(cancellationToken);
                output.AddRange(results);
            }

            return output;
        }

        /// <inheritdoc/>
        public async Task CreateAttachment(AttachmentReference attachmentReference, CancellationToken cancellationToken = default) =>
            await _attachments.CreateItemAsync(item: attachmentReference, partitionKey: new PartitionKey(attachmentReference.UPN), cancellationToken: cancellationToken);

        /// <inheritdoc/>
        public async Task DeleteAttachment(AttachmentReference attachmentReference, CancellationToken cancellationToken = default)
        {
            PartitionKey partitionKey = new(attachmentReference.UPN);

            attachmentReference.Deleted = true;

            await _attachments.UpsertItemAsync(
               item: attachmentReference,
               partitionKey: partitionKey,
               cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<AgentFileReference?> GetAgentFile(string instanceId, string agentName, string id, CancellationToken cancellationToken = default)
        {
            try
            {
                PartitionKey partitionKey = new PartitionKeyBuilder()
                    .Add(instanceId)
                    .Add(agentName)
                    .Build();

                var response = await _agents.ReadItemAsync<AgentFileReference>(
                    id: id,
                    partitionKey: partitionKey,
                    cancellationToken: cancellationToken);

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<List<AgentFileReference>> GetAgentFiles(string instanceId, string agentName, CancellationToken cancellationToken = default)
        {
            var query = new QueryDefinition($"SELECT DISTINCT * FROM c WHERE c.instanceId = @instanceId AND c.agentName = @agentName AND {SoftDeleteQueryRestriction} ORDER BY c._ts DESC")
                .WithParameter("@instanceId", instanceId)
                .WithParameter("@agentName", agentName);

            var response = _agents.GetItemQueryIterator<AgentFileReference>(query);

            List<AgentFileReference> output = [];
            while (response.HasMoreResults)
            {
                var results = await response.ReadNextAsync(cancellationToken);
                output.AddRange(results);
            }

            return output;
        }

        /// <inheritdoc/>
        public async Task CreateAgentFile(AgentFileReference agentFile, CancellationToken cancellationToken = default)
        {
            PartitionKey partitionKey = new PartitionKeyBuilder()
                .Add(agentFile.InstanceId)
                .Add(agentFile.AgentName)
                .Build();

            await _agents.CreateItemAsync(item: agentFile, partitionKey: partitionKey, cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public async Task DeleteAgentFile(AgentFileReference agentFile, CancellationToken cancellationToken = default)
        {
            PartitionKey partitionKey = new PartitionKeyBuilder()
                .Add(agentFile.InstanceId)
                .Add(agentFile.AgentName)
                .Build();

            agentFile.Deleted = true;

            await _agents.UpsertItemAsync(
               item: agentFile,
               partitionKey: partitionKey,
               cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public async Task CreateVectorSearchContainerAsync(
            string containerName,
            string partitionKeyPath,
            string vectorPropertyPath,
            int vectorDimensions,
            CancellationToken cancellationToken = default)
        {
            var containerProperties = new ContainerProperties(containerName, partitionKeyPath)
            {
                VectorEmbeddingPolicy = new(new Collection<Embedding>(
                    [
                        new Embedding()
                        {
                            Path = vectorPropertyPath,
                            DataType = VectorDataType.Float32,
                            DistanceFunction = DistanceFunction.Cosine,
                            Dimensions = vectorDimensions
                        }
                    ])),

                IndexingPolicy = new IndexingPolicy
                {
                    VectorIndexes =
                        [
                            new VectorIndexPath()
                            {
                                Path = vectorPropertyPath,
                                Type = VectorIndexType.DiskANN
                            }
                        ]
                }
            };
            containerProperties.IndexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/*" });
            containerProperties.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath { Path = $"{vectorPropertyPath}/*" });

            var containerResponse = await _database.CreateContainerIfNotExistsAsync(
                containerProperties,
                cancellationToken: cancellationToken);
            if (containerResponse.Container != null)
                _containers[containerName] = containerResponse.Container;
        }

        /// <inheritdoc/>
        public async Task<CompletionResponse?> GetCompletionResponseAsync(
            string containerName,
            ReadOnlyMemory<float> userPromptEmbedding,
            decimal minimumSimilarityScore)
        {
            var query = new QueryDefinition("""
                SELECT TOP 1
                    x.serializedItem, x.similarityScore
                FROM
                    (
                        SELECT c.serializedItem, VectorDistance(c.userPromptEmbedding, @userPromptEmbedding) AS similarityScore FROM c
                    ) x
                WHERE
                    x.similarityScore >= @minimumSimilarityScore
                ORDER BY
                    x.similarityScore DESC
                """);
            query.WithParameter("@userPromptEmbedding", userPromptEmbedding.ToArray());
            query.WithParameter("@minimumSimilarityScore", (float)minimumSimilarityScore);

            using var feedIterator = _completionsCache.GetItemQueryIterator<Object>(query);
            if (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync();
                var result = response.Resource.FirstOrDefault();

                if (result == null)
                    return null;

                var serializedCompletionResponse = (result as Newtonsoft.Json.Linq.JObject)!["serializedItem"]!.ToString();
                var completionResponse = JsonSerializer.Deserialize<CompletionResponse>(serializedCompletionResponse);

                return completionResponse;
            }

            return null;
        }

        #region Skill Methods (Procedural Memory)

        /// <inheritdoc/>
        public async Task<SkillReference?> GetSkillAsync(string upn, string skillId, CancellationToken cancellationToken = default)
        {
            var skills = await _skillsTask;

            try
            {
                var response = await skills.ReadItemAsync<SkillReference>(
                    id: skillId,
                    partitionKey: new PartitionKey(upn),
                    cancellationToken: cancellationToken);

                // Check if soft-deleted
                if (response.Resource?.Deleted == true)
                    return null;

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<List<SkillReference>> GetSkillsAsync(string upn, string? agentObjectId = null, CancellationToken cancellationToken = default)
        {
            var skills = await _skillsTask;

            var queryText = agentObjectId != null
                ? $"SELECT * FROM c WHERE c.upn = @upn AND c.agent_object_id = @agentObjectId AND c.type = 'skill' AND {SoftDeleteQueryRestriction} ORDER BY c._ts DESC"
                : $"SELECT * FROM c WHERE c.upn = @upn AND c.type = 'skill' AND {SoftDeleteQueryRestriction} ORDER BY c._ts DESC";

            var query = new QueryDefinition(queryText)
                .WithParameter("@upn", upn);

            if (agentObjectId != null)
            {
                query.WithParameter("@agentObjectId", agentObjectId);
            }

            var response = skills.GetItemQueryIterator<SkillReference>(query);

            List<SkillReference> output = [];
            while (response.HasMoreResults)
            {
                var results = await response.ReadNextAsync(cancellationToken);
                output.AddRange(results);
            }

            return output;
        }

        /// <inheritdoc/>
        public async Task<SkillReference> UpsertSkillAsync(SkillReference skill, CancellationToken cancellationToken = default)
        {
            var skills = await _skillsTask;

            // Ensure required fields are set
            skill.Type = "skill";
            if (skill.CreatedOn == default)
            {
                skill.CreatedOn = DateTimeOffset.UtcNow;
            }
            skill.UpdatedOn = DateTimeOffset.UtcNow;

            var response = await skills.UpsertItemAsync(
                item: skill,
                partitionKey: new PartitionKey(skill.UPN),
                cancellationToken: cancellationToken);

            return response.Resource;
        }

        /// <inheritdoc/>
        public async Task DeleteSkillAsync(SkillReference skill, CancellationToken cancellationToken = default)
        {
            var skills = await _skillsTask;

            // Soft delete
            skill.Deleted = true;
            skill.UpdatedOn = DateTimeOffset.UtcNow;

            await skills.UpsertItemAsync(
                item: skill,
                partitionKey: new PartitionKey(skill.UPN),
                cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<SkillReference> UpdateSkillExecutionAsync(string upn, string skillId, bool success, CancellationToken cancellationToken = default)
        {
            var skill = await GetSkillAsync(upn, skillId, cancellationToken);

            if (skill == null)
            {
                throw new InvalidOperationException($"Skill '{skillId}' not found for user '{upn}'.");
            }

            // Update execution statistics
            skill.ExecutionCount++;

            // Update success rate with exponential moving average
            var alpha = 0.1; // Smoothing factor
            var successValue = success ? 1.0 : 0.0;
            skill.SuccessRate = (alpha * successValue) + ((1 - alpha) * skill.SuccessRate);

            skill.UpdatedOn = DateTimeOffset.UtcNow;

            return await UpsertSkillAsync(skill, cancellationToken);
        }

        #endregion

        private async Task EnsureContainerAvailability(string containerName)
        {
            if (_containers.ContainsKey(containerName))
                return;

            switch (containerName)
            {
                case AzureCosmosDBContainers.UserProfiles:
                    _containers[containerName] = await _userProfilesTask;
                    return;
                case AzureCosmosDBContainers.Skills:
                    _containers[containerName] = await _skillsTask;
                    return;
                default:
                    return;
            }
        }
    }
}

using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.DataSourceConfigurations;
using FoundationaLLM.Common.Models.Orchestration.DataSources;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Cache;
using FoundationaLLM.Common.Models.Hubs;
using FoundationaLLM.Common.Models.Metadata;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.AgentFactory.Core.Orchestration
{
    /// <summary>
    /// Default (legacy) orchestration.
    /// </summary>
    /// <remarks>
    /// Constructor for a legacy orchestration.
    /// </remarks>
    /// <param name="agentMetadata"></param>
    /// <param name="cacheService">The <see cref="ICacheService"/> used to cache agent-related artifacts.</param>
    /// <param name="callContext">The call context of the request being handled.</param>
    /// <param name="orchestrationService"></param>
    /// <param name="promptHubService"></param>
    /// <param name="dataSourceHubService"></param>
    /// <param name="logger">The logger used for logging.</param>
    public class LegacyOrchestration(
        AgentMetadata agentMetadata,
        ICacheService cacheService,
        ICallContext callContext,
        ILLMOrchestrationService orchestrationService,
        IPromptHubAPIService promptHubService,
        IDataSourceHubAPIService dataSourceHubService,
        ILogger<LegacyOrchestration> logger) : OrchestrationBase(agentMetadata, orchestrationService, promptHubService, dataSourceHubService)
    {
        private LegacyCompletionRequest _completionRequestTemplate = null!;
        private readonly ICacheService _cacheService = cacheService;
        private readonly ICallContext _callContext = callContext;
        private readonly ILogger<LegacyOrchestration> _logger = logger;

        /// <inheritdoc/>
        public override async Task Configure(CompletionRequest completionRequest)
        {
            // Get prompts for the agent from the prompt hub.
            var promptResponse = _callContext.AgentHint != null
                ? await _cacheService.Get<PromptHubResponse>(
                    new CacheKey(_callContext.AgentHint.Name!, CacheCategories.Prompt),
                    async () => {
                        return await _promptHubService.ResolveRequest(
                            _agentMetadata!.PromptContainer ?? _agentMetadata.Name!,
                            completionRequest.SessionId ?? string.Empty
                        );
                    },
                    false,
                    TimeSpan.FromHours(1))
                : await _promptHubService.ResolveRequest(
                    _agentMetadata!.PromptContainer ?? _agentMetadata.Name!,
                    completionRequest.SessionId ?? string.Empty
                  );

            if (promptResponse is {Prompt: not null})
            {
                _logger.LogInformation("The legacy orchestration received the following prompt from the Prompt Hub: {PromptName}.",
                    promptResponse!.Prompt!.Name);
            }

            // Get data sources listed for the agent.
            var dataSourceResponse = _callContext.AgentHint != null
                ? await _cacheService.Get<DataSourceHubResponse>(
                    new CacheKey(_callContext.AgentHint.Name!, CacheCategories.DataSource),
                    async () => { return await _dataSourceHubService.ResolveRequest(
                        _agentMetadata!.AllowedDataSourceNames!,
                        completionRequest.SessionId ?? string.Empty); },
                    false,
                    TimeSpan.FromHours(1))
                : await _dataSourceHubService.ResolveRequest(
                    _agentMetadata!.AllowedDataSourceNames!,
                    completionRequest.SessionId ?? string.Empty);
            
            if (dataSourceResponse is {DataSources: not null})
            {
                _logger.LogInformation(
                    "The legacy orchestration received the following data sources from the Data Source Hub: {DataSourceList}.",
                    string.Join(",", dataSourceResponse!.DataSources!.Select(ds => ds.Name)));
            }

            var dataSourceMetadata = new List<DataSourceBase>();

            var dataSources = dataSourceResponse!.DataSources!;
                        
            foreach (var dataSource in dataSources)
            {
                switch (dataSource.UnderlyingImplementation)
                {
                    case "csv":
                    case "generic-resolver":
                    case "blob-storage":
                        dataSourceMetadata.Add(new BlobStorageDataSource
                        {
                            Name = dataSource.Name,
                            Type = dataSource.UnderlyingImplementation,
                            Description = dataSource.Description,
                            Configuration = new BlobStorageConfiguration
                            {
                                ConnectionStringSecretName = dataSource.Authentication!["connection_string_secret"],
                                ContainerName = dataSource.Container,
                                Files = dataSource.Files
                            },
                            DataDescription = dataSource.DataDescription
                        });
                        break;

                    case "search-service":
                        dataSourceMetadata.Add(new SearchServiceDataSource
                        {
                            Name = dataSource.Name,
                            Type = dataSource.UnderlyingImplementation,
                            Description = dataSource.Description,
                            Configuration = new SearchServiceConfiguration
                            {
                                Endpoint = dataSource.Authentication!["endpoint"],
                                KeySecret = dataSource.Authentication["key_secret"],
                                IndexName = dataSource.IndexName,
                                EmbeddingFieldName = dataSource.EmbeddingFieldName,
                                TextFieldName = dataSource.TextFieldName,
                                TopN = dataSource.TopN
                            },
                            DataDescription = dataSource.DataDescription
                        });
                        break;
                    case "anomaly":
                    case "sql":
                        dataSourceMetadata.Add(new SQLDatabaseDataSource
                        {
                            Name = dataSource.Name,
                            Type = dataSource.UnderlyingImplementation,
                            Description = dataSource.Description,
                            Configuration = new SQLDatabaseConfiguration
                            {
                                Dialect = dataSource.Dialect,
                                Host = dataSource.Authentication!["host"],
                                Port = Convert.ToInt32(dataSource.Authentication["port"]),
                                DatabaseName = dataSource.Authentication["database"],
                                Username = dataSource.Authentication["username"],
                                PasswordSecretSettingKeyName = dataSource.Authentication["password_secret"],
                                IncludeTables = dataSource.IncludeTables!,
                                ExcludeTables = dataSource.ExcludeTables!,
                                RowLevelSecurityEnabled = dataSource.RowLevelSecurityEnabled ?? false,
                                FewShotExampleCount = dataSource.FewShotExampleCount ?? 0
                            },
                            DataDescription = dataSource.DataDescription
                        });
                        break;
                    case "cxo":
                        dataSourceMetadata.Add(new CXODataSource
                        {
                            Name = dataSource.Name,
                            Type = _agentMetadata.Type,
                            Description = dataSource.Description,
                            DataDescription = dataSource.DataDescription,
                            Configuration = new CXOConfiguration
                            {
                                Endpoint = dataSource.Authentication!["endpoint"],
                                KeySecret = dataSource.Authentication["key_secret"],
                                IndexName = dataSource.IndexName,
                                EmbeddingFieldName = dataSource.EmbeddingFieldName,
                                TextFieldName = dataSource.TextFieldName,
                                TopN = dataSource.TopN,
                                RetrieverMode = dataSource.RetrieverMode,
                                Company = dataSource.Company,
                                Sources = dataSource.Sources
                            }

                        });
                        break;
                    default:
                        throw new ArgumentException($"The {dataSource.UnderlyingImplementation} data source type is not supported.");
                }
            }

            //create LLMOrchestrationCompletionRequest template
            _completionRequestTemplate = new LegacyCompletionRequest()
            {
                UserPrompt = null, // to be filled in GetCompletion / GetSummary
                Agent = new LegacyAgentMetadata
                {
                    Name = _agentMetadata!.Name,
                    Type = _agentMetadata.Type,
                    Description = _agentMetadata.Description,
                    PromptPrefix = promptResponse!.Prompt?.PromptPrefix,
                    PromptSuffix = promptResponse!.Prompt?.PromptSuffix
                },
                LanguageModel = _agentMetadata.LanguageModel,
                EmbeddingModel = _agentMetadata.EmbeddingModel,
                DataSourceMetadata = dataSourceMetadata,
                MessageHistory = null // to be filled in GetCompletion
            };
        }

        /// <inheritdoc/>
        public override async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
        {
            _completionRequestTemplate.SessionId = completionRequest.SessionId;
            _completionRequestTemplate.UserPrompt = completionRequest.UserPrompt;
            _completionRequestTemplate.MessageHistory = completionRequest.MessageHistory;

            var result = await _orchestrationService.GetCompletion(_completionRequestTemplate);

            return new CompletionResponse
            {
                Completion = result.Completion!,
                UserPrompt = completionRequest.UserPrompt!,
                FullPrompt = result.FullPrompt,
                PromptTemplate = result.PromptTemplate,
                AgentName = result.AgentName,
                PromptTokens = result.PromptTokens,
                CompletionTokens = result.CompletionTokens,
            };
        }
    }
}

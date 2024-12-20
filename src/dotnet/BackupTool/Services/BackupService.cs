using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using BackupTool.Interfaces;
using BackupTool.Models.Configuration;
using DnsClient.Internal;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI;
using FoundationaLLM.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BackupTool.Services
{
    public partial class BackupService(
        IAzureCosmosDBService cosmosDBService,
        ILogger<BackupService> logger,
        IOptions<BackupServiceSettings> settings,
        //ICallContext callContext,
        //IEnumerable<IResourceProviderService> resourceProviderServices,
        IConfiguration configuration) : IBackupService
    {
        private readonly IAzureCosmosDBService _cosmosDBService = cosmosDBService;
        private readonly ILogger<BackupService> _logger = logger;

        //private readonly IResourceProviderService _attachmentResourceProvider =
        //    resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Attachment);
        //private readonly IResourceProviderService _agentResourceProvider =
        //    resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Agent);
        //private readonly IResourceProviderService _azureOpenAIResourceProvider =
        //    resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_AzureOpenAI);
        //private readonly IResourceProviderService _aiModelResourceProvider =
        //    resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_AIModel);
        //private readonly IResourceProviderService _configurationResourceProvider =
        //    resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Configuration);
        //private readonly IResourceProviderService _conversationResourceProvider =
        //    resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Conversation);

        private readonly HashSet<string> _azureOpenAIFileSearchFileExtensions =
            settings.Value.AzureOpenAIAssistantsFileSearchFileExtensions
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.ToLowerInvariant())
                .ToHashSet();

        private readonly IConfiguration _configuration;

        public async Task<List<Conversation>> GetAllConversationsAsync(CancellationToken cancellationToken = default)
        {
            var result = await _cosmosDBService.GetConversationsAsync(ConversationTypes.Session, cancellationToken);

            return result;
        }

        public async Task<List<Message>> GetConversationMessagesAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            var result = await _cosmosDBService.GetSessionMessagesAsync(sessionId, cancellationToken);

            return result;
        }

        public async Task<List<CompletionPrompt>> GetCompletionPromptResponses(string sessionId, CancellationToken cancellationToken = default)
        {
            var result = await _cosmosDBService.GetCompletionPromptsAsync(sessionId, cancellationToken);

            return result;
        }

        public async Task<List<AzureOpenAIConversationMapping>> GetConversationMappingResponses(string sessionId, CancellationToken cancellationToken = default)
        {
            var result = await _cosmosDBService.GetConversationMappingsAsync(sessionId, cancellationToken);

            return result;
        }

        public Task<List<AzureOpenAIFileMapping>> GetFileMappingResponses(string partitionKey, CancellationToken cancellationToken = default)
        {
            var result = _cosmosDBService.GetFileMappingsAsync(partitionKey, cancellationToken);

            return result;
        }

        public async Task<AttachmentReference?> GetAttachment(string partitionKey, string fileObjectId, CancellationToken cancellationToken = default)
        {
            var filter = new ResourceFilter
            {
                ObjectIDs = [fileObjectId]
            };

            var result = await _cosmosDBService.FilterAttachments(partitionKey, filter, cancellationToken);

            return result.FirstOrDefault();
        }
    }
}

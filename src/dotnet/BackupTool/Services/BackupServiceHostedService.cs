using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using BackupTool.Interfaces;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Assistants;

namespace FoundationaLLM.Common.Services
{
    public class BackupServiceHostedService(
        ILogger<BackupServiceHostedService> logger,
        IServiceProvider serviceProvider,
        IOptions<InstanceSettings> settings)
        : IHostedService, IHostedLifecycleService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();

                var conversations = await backupService.GetAllConversationsAsync(cancellationToken);

                foreach (var conversation in conversations)
                {
                    logger.LogInformation($"Backing up conversation {conversation.Id}");

                    var conversationMappings = await backupService.GetConversationMappingResponses(conversation.Id, cancellationToken);
                    var messages = await backupService.GetConversationMessagesAsync(conversation.Id, cancellationToken);
                    var completionPrompts = await backupService.GetCompletionPromptResponses(conversation.Id, cancellationToken);

                    foreach (var message in messages)
                    {
                        logger.LogInformation($"Evaluating message {message.Id} in conversation {conversation.Id}");
                    }

                    foreach (var completionPrompt in completionPrompts)
                    {
                        logger.LogInformation($"Evaluating completion prompt {completionPrompt.Id} in conversation {conversation.Id}");
                    }

                    foreach (var conversationMapping in conversationMappings)
                    {
                        logger.LogInformation($"Evaluating conversation mapping {conversationMapping.Id} in conversation {conversation.Id}");
                        var fileMappings = await backupService.GetFileMappingResponses(conversationMapping.PartitionKey, cancellationToken);

                        foreach (var fileMapping in fileMappings)
                        {
                            logger.LogInformation($"Evaluating file mapping {fileMapping.Id} in conversation {conversation.Id}");
                            var attachment = await backupService.GetAttachment(fileMapping.PartitionKey, fileMapping.FileObjectId, cancellationToken);
                            if (attachment != null)
                            {
                                logger.LogInformation(
                                    $"Retrieved attachment {attachment.Id} in conversation {conversation.Id}");
                            }
                        }
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Backup service stopped");

            return Task.CompletedTask;
        }

        public Task StartedAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Backup service started");

            return Task.CompletedTask;
        }

        public Task StartingAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting backup service");

            return Task.CompletedTask;
        }

        public Task StoppedAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Backup service stopped");

            return Task.CompletedTask;
        }

        public Task StoppingAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping backup service");

            return Task.CompletedTask;
        }
    }
}

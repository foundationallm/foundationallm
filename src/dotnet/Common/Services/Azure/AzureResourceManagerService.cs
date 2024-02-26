using Azure.Core;
using Azure;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.EventGrid.Models;
using Azure.ResourceManager.EventGrid;
using FoundationaLLM.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Runtime;
using System.Xml;
using System.Threading;
using FoundationaLLM.Common.Authentication;
using Microsoft.Extensions.Hosting;

namespace FoundationaLLM.Common.Services.Azure
{
    /// <summary>
    /// Provides services to interact with the Azure Resource Manager (ARM) infrastructure.
    /// </summary>
    /// <param name="environment">The <see cref="IHostEnvironment"/> providing details about the environment.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class AzureResourceManagerService(
        IHostEnvironment environment,
        ILogger<AzureResourceManagerService> logger) : IAzureResourceManagerService
    {
        private readonly ArmClient _armClient = new(DefaultAuthentication.GetAzureCredential(
            environment.IsDevelopment()));
        private readonly ILogger<AzureResourceManagerService> _logger = logger;

        /// <inheritdoc/>
        public async Task<bool> CreateEventGridNamespaceTopicSubscription(string namespaceResourceId, string topicName, string topicSubscriptionName, CancellationToken cancellationToken)
        {
            var namespaceTopicId = ResourceIdentifier.Parse($"{namespaceResourceId}/topics/{topicName}");
            var namespaceTopicResource = _armClient.GetNamespaceTopicResource(namespaceTopicId);
            var namespaceTopic = await namespaceTopicResource.GetAsync(cancellationToken);
            var namespaceTopicSubscriptions = namespaceTopic.Value.GetNamespaceTopicEventSubscriptions();

            var eventSubscription = new NamespaceTopicEventSubscriptionData()
            {
                DeliveryConfiguration = new DeliveryConfiguration()
                {
                    DeliveryMode = DeliveryMode.Queue,
                    Queue = new QueueInfo()
                    {
                        ReceiveLockDurationInSeconds = 60,
                        MaxDeliveryCount = 10,
                        EventTimeToLive = XmlConvert.ToTimeSpan("P1D"),
                    },
                },
                EventDeliverySchema = DeliverySchema.CloudEventSchemaV10,
            };

            var newNamespaceTopicSubscription = await namespaceTopicSubscriptions.CreateOrUpdateAsync(
                WaitUntil.Completed,
                topicSubscriptionName,
                eventSubscription,
                cancellationToken);

            if (!newNamespaceTopicSubscription.HasCompleted)
            {
                _logger.LogError("The Azure resource manager operation returned a result without completing. This is unexpected.");
                return false;
            }
            else if (!newNamespaceTopicSubscription.HasValue)
            {
                _logger.LogError("The Azure resource manager operation completed but did not have a return value. This is unexpected.");
                return false;
            }
            else
                return true;
        }

        /// <inheritdoc/>
        public async Task DeleteEventGridNamespaceTopicSubscription(string namespaceResourceId, string topicName, string topicSubscriptionName, CancellationToken cancellationToken)
        {
            var eventSubscriptionId = ResourceIdentifier.Parse($"{namespaceResourceId}/topics/{topicName}/eventSubscriptions/{topicSubscriptionName}");
            var eventSubscriptionResource = _armClient.GetNamespaceTopicEventSubscriptionResource(eventSubscriptionId);

            await eventSubscriptionResource.DeleteAsync(WaitUntil.Completed, cancellationToken);
        }
    }
}

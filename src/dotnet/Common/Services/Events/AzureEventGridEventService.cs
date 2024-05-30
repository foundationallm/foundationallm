using Azure;
using Azure.Identity;
using Azure.Messaging.EventGrid.Namespaces;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Events;
using FoundationaLLM.Common.Models.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Common.Services.Events
{
    /// <summary>
    /// Provides services to integrate with the Azure Event Grid eventing platform.
    /// </summary>
    public class AzureEventGridEventService : IEventService
    {
        private readonly ILogger<AzureEventGridEventService> _logger;
        private readonly AzureEventGridEventServiceSettings _settings;
        private readonly AzureEventGridEventServiceProfile _profile;
        private readonly IAzureResourceManagerService _azureResourceManager;
        private EventGridClient? _eventGridClient;

        private readonly TimeSpan _eventProcessingCycle;

        private readonly Dictionary<string, EventSetEventDelegate?> _eventSetEventDelegates = new()
        {
            {
                EventSetEventNamespaces.FoundationaLLM_ResourceProvider_Agent,
                null
            },
            {
                EventSetEventNamespaces.FoundationaLLM_ResourceProvider_Vectorization,
                null
            },
            {
                EventSetEventNamespaces.FoundationaLLM_ResourceProvider_Configuration,
                null
            },
            {
                EventSetEventNamespaces.FoundationaLLM_ResourceProvider_DataSource,
                null
            },
            {
                EventSetEventNamespaces.FoundationaLLM_ResourceProvider_Attachment,
                null
            }
        };

        /// <summary>
        /// Creates a new instance of the <see cref="AzureEventGridEventService"/> event service.
        /// </summary>
        /// <param name="settingsOptions">The options providing the settings for the service.</param>
        /// <param name="profileOptions">The options providing the profile for the service.</param>
        /// <param name="azureResourceManager">The <see cref="IAzureResourceManagerService"/> service providing access to Azure ARM services.</param>
        /// <param name="logger">The logger used for logging.</param>
        public AzureEventGridEventService(
            IOptions<AzureEventGridEventServiceSettings> settingsOptions,
            IOptions<AzureEventGridEventServiceProfile> profileOptions,
            IAzureResourceManagerService azureResourceManager,
            ILogger<AzureEventGridEventService> logger)
        {
            _settings = settingsOptions.Value;
            _profile = profileOptions.Value;
            _azureResourceManager = azureResourceManager;
            _logger = logger;

            _eventProcessingCycle = TimeSpan.FromSeconds(_profile.EventProcessingCycleSeconds);
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _eventGridClient = GetClient();
                if (_eventGridClient == null)
                    throw new EventException("Cound not create Azure Event Grid client.");
                else
                {
                    // Create the topic subscriptions according to the service profile.
                    await CreateTopicSubscriptions(cancellationToken);

                    _logger.LogInformation("The Azure Event Grid event service was successfully initialized.");
                }
            }
            catch (Exception ex)
            {
                throw new EventException("The Azure Event Grid event service encountered an error while starting and will not listen for any events.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken) =>
            await DeleteTopicSubscriptions(cancellationToken);

        /// <inheritdoc/>
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (_eventGridClient == null)
            {
                _logger.LogCritical("The Azure Event Grid events service is not properly initialized and will not execute.");
                return;
            }

            if (_profile.Topics.Count == 0)
            {
                _logger.LogInformation("The Azure Event Grid event service stopped running because it is not configured to listen to any events.");
                return;
            }

            _logger.LogInformation("The Azure Event Grid event service is starting to process messages.");

            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return;

                foreach (var topic in _profile.Topics)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    if (!topic.SubscriptionAvailable) continue;

                    try
                    {
                        var response = await _eventGridClient.ReceiveCloudEventsAsync(
                            topic.Name,
                            topic.SubscriptionName,
                            maxWaitTime: TimeSpan.FromSeconds(10),
                            cancellationToken: cancellationToken);

                        if (response != null
                            && response.Value.Value != null
                            && response.Value.Value.Count > 0)
                        await ProcessTopicSubscriptionEvents(
                            topic.Name,
                            topic.SubscriptionName!,
                            topic.EventTypeProfiles,
                            response.Value.Value);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occured while trying to retrieve events for topic {TopicName} and subscription {SubscriptionName}.",
                            topic.Name, topic.SubscriptionName);
                    }
                }

                await Task.Delay(_eventProcessingCycle, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public void SubscribeToEventSetEvent(string eventNamespace, EventSetEventDelegate eventHandler)
        {
            ArgumentNullException.ThrowIfNull(eventHandler);

            if (!_eventSetEventDelegates.TryGetValue(eventNamespace, out var eventDelegate))
                throw new EventException($"The namespace {eventNamespace} is invalid.");

            if (eventDelegate == null)
                _eventSetEventDelegates[eventNamespace] = eventHandler!;
            else
                eventDelegate += eventHandler;
        }

        /// <inheritdoc/>
        public void UnsubscribeFromEventSetEvent(string eventNamespace, EventSetEventDelegate eventHandler)
        {
            if (!_eventSetEventDelegates.TryGetValue(eventNamespace, out var eventDelegate))
                throw new EventException($"The namespace {eventNamespace} is invalid.");

            eventDelegate -= eventHandler;
        }

        private async Task CreateTopicSubscriptions(CancellationToken cancellationToken)
        {
            foreach (var topic in _profile.Topics)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                topic.SubscriptionName = $"{topic.SubscriptionPrefix}-{Guid.NewGuid().ToString().ToLower()}";
                _logger.LogInformation("The Azure Event Grid event service will create a subscription named {SubscriptionName} in topic {TopicName}.",
                    topic.SubscriptionName, topic.Name);

                try
                {
                    topic.SubscriptionAvailable = await _azureResourceManager.CreateEventGridNamespaceTopicSubscription(
                        _settings.NamespaceId,
                        topic.Name,
                        topic.SubscriptionName,
                        cancellationToken);

                    if (topic.SubscriptionAvailable)
                        _logger.LogInformation("The Azure Event Grid event service successfully created a subscription named {SubscriptionName} in topic {TopicName}.",
                            topic.SubscriptionName, topic.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occured while creating a subscription named {SubscriptionName} in topic {TopicName}.",
                        topic.SubscriptionName, topic.Name);
                }
            }
        }

        private async Task DeleteTopicSubscriptions(CancellationToken cancellationToken)
        {
            foreach (var topic in _profile.Topics)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (!topic.SubscriptionAvailable)
                {
                    _logger.LogInformation("The Azure Event Grid event service did not create the subscription named {SubscriptionName} from topic {TopicName}. Delete will not be attempted.",
                        topic.SubscriptionName, topic.Name);
                    continue;
                }

                _logger.LogInformation("The Azure Event Grid event service will delete the subscription named {SubscriptionName} from topic {TopicName}.",
                    topic.SubscriptionName, topic.Name);

                try
                {
                    await _azureResourceManager.DeleteEventGridNamespaceTopicSubscription(
                        _settings.NamespaceId,
                        topic.Name,
                        topic.SubscriptionName!,
                        cancellationToken);

                    topic.SubscriptionAvailable = false;
                    _logger.LogInformation("The Azure Event Grid event service successfully deleted the subscription named {SubscriptionName} from topic {TopicName}.",
                        topic.SubscriptionName, topic.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occured while deleting the subscription named {SubscriptionName} from topic {TopicName}.",
                        topic.SubscriptionName, topic.Name);
                }
            }
        }

        private async Task ProcessTopicSubscriptionEvents(
            string topicName,
            string subscriptionName,
            List<EventGridEventTypeProfile> eventTypeProfiles, IReadOnlyList<ReceiveDetails> eventDetails)
        {

            foreach (var eventTypeProfile in eventTypeProfiles)
            foreach (var eventSet in eventTypeProfile.EventSets)
            {
                var events = eventDetails
                    .Select(ed => ed.Event)
                    .Where(e =>
                        e.Type == eventTypeProfile.EventType
                        && string.Equals(e.Source, eventSet.Source, StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(e.Subject)
                        && (string.IsNullOrWhiteSpace(eventSet.SubjectPrefix) || e.Subject.StartsWith(eventSet.SubjectPrefix)))
                    .ToList();

                if (events.Count > 0
                    && _eventSetEventDelegates.TryGetValue(eventSet.Namespace, out EventSetEventDelegate? eventSetDelegate)
                    && eventSetDelegate != null)
                {
                    try
                    {
                        // We have new events and we have at least one event handler attached to the delegate associated with this namespace.
                        // Fire up the event and call all registered delegates.
                        eventSetDelegate(this, new EventSetEventArgs
                        {
                            Namespace = eventSet.Namespace,
                            Events = events
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error invoking registered delegates.");
                    }
                }
            }

            // We are releasing messages instead of acknowledging them.
            // This will allow other subscribers to process them as well.
            // The typical scenario for this is when the event service is run by multiple, identical instances of the same host (e.g., a FoundationaLLM service like Core API).
            // When this happens, all running instances of the host must get the chance to process the messages.
            await AcknowledgeMessages(topicName, subscriptionName, eventDetails);
        }

        private async Task AcknowledgeMessages(
            string topicName,
            string subscriptionName,
            IReadOnlyList<ReceiveDetails> eventDetails)
        {
            var result = await _eventGridClient!.AcknowledgeCloudEventsAsync(
                topicName,
                subscriptionName,
                new AcknowledgeOptions(
                    eventDetails.Select(ed => ed.BrokerProperties.LockToken)));

            foreach (var ackFailure in result.Value.FailedLockTokens)
            {
                _logger.LogError("Failed to acknowledge Event Grid message. Lock token: {LockToken}. Error code: {ErrorCode}. Error description: {ErrorDescription}.",
                    ackFailure.LockToken, ackFailure.Error, ackFailure.ToString());
            }
        }

        #region Create Event Grid client

        private void ValidateEndpoint(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _logger.LogCritical("The Azure Event Grid namespace endpoint is invalid.");
                throw new ConfigurationValueException("The Azure Event Grid namespace endpoint is invalid.");
            }
        }

        private void ValidateAPIKey(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _logger.LogCritical("The Azure Event Grid API key is invalid.");
                throw new ConfigurationValueException("The Azure Event Grid API key is invalid.");
            }
        }

        private EventGridClient? GetClient() =>
            _settings.AuthenticationType switch
            {
                AzureEventGridAuthenticationTypes.AzureIdentity => GetClientFromIdentity(),
                AzureEventGridAuthenticationTypes.APIKey => GetClientFromAPIKey(),
                _ => throw new ConfigurationValueException($"The {_settings.AuthenticationType} authentication type is not supported by the Azure Event Grid events service.")
            };

        private EventGridClient? GetClientFromIdentity()
        {
            EventGridClient? client = default;
            try
            {
                ValidateEndpoint(_settings.Endpoint);
                client = new EventGridClient(new Uri(_settings.Endpoint!), DefaultAuthentication.AzureCredential);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error creating the Azure Event Grid client.");
            }

            return client;
        }

        private EventGridClient? GetClientFromAPIKey()
        {
            EventGridClient? client = null;
            try
            {
                ValidateEndpoint(_settings.Endpoint);
                ValidateAPIKey(_settings.APIKey);
                client = new EventGridClient(new Uri(_settings.Endpoint!), new AzureKeyCredential(_settings.APIKey!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error creating the Azure Event Grid client.");
            }

            return client;
        }
        #endregion
    }
}

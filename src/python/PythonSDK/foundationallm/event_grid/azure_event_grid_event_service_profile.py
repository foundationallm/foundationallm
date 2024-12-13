from typing import List, Optional
from azure.core.messaging import CloudEvent

class AzureEventGridEventServiceProfile:
    """The profile used to configure event handling in the AzureEventGridEventService event service."""
    
    def __init__(self):
        """The time interval in seconds between successive event processing cycles."""
        self.event_processing_cycle_seconds: int = 60
        """The list of EventGridTopicProfile topic profiles used to configure event handling for an Azure Event Grid namespace topic."""
        self.topics: List[EventGridTopicProfile] = []

class EventGridTopicProfile:
    """The profile used to configure event handling for an Azure Event Grid namespace topic."""
    
    def __init__(self, name: str, subscription_prefix: str):
        """The name of the Azure Event Grid namespace topic."""
        self.name: str = name
        """The prefix used for the topic subscription name."""
        self.subscription_prefix: str = subscription_prefix
        """The topic subscription name."""
        self.subscription_name: Optional[str] = None
        """Indicates whether the subscription is available for use or not."""
        self.subscription_available: bool = False
        """The list of EventGridEventTypeProfile event type profiles used to configure handling for event types."""
        self.event_type_profiles: List[EventGridEventTypeProfile] = []

class EventGridEventTypeProfile:
    """The profile used to configure event handling for a specified Azure Event Grid event type."""
    
    def __init__(self, event_type: str):
        """The name of the Azure Event Grid event type."""
        self.event_type: str = event_type
        """The list of EventGridEventSet event sets used to configure event handling for a specific subset of events of a specified event type."""
        self.event_sets: List[EventGridEventSet] = []

class EventGridEventSet:
    """The event set used to configure event handling for a specific subset of events of a specified event type."""
    
    def __init__(self, namespace: str, source: str, subject_prefix: str):
        """The namespace associated with the event set."""
        self.namespace: str = namespace
        """The event source that defines the set."""
        self.source: str = source
        """The event subject prefix that defines the set."""
        self.subject_prefix: str = subject_prefix

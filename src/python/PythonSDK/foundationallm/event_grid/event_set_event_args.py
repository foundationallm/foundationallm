from typing import List
from azure.core.messaging import CloudEvent

class EventSetEventArgs:
    """Event arguments required for event set event delegates."""

    def __init__(self, namespace: str, events: List[CloudEvent]):
        """The namespace associated with the event set."""
        self.namespace = namespace
        """The list of subjects associated with the event."""
        self.events = events

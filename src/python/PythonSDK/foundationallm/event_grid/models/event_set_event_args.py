from pydantic import Field
from typing import List
from azure.core.messaging import CloudEvent

class EventSetEventArgs:
    """Event arguments required for event set event delegates."""

    namespace: str = Field(default="", description="The namespace associated with the event set.")
    events: List[CloudEvent] = Field(default=[], description="The list of subjects associated with the event.")

from pydantic import Field
from typing import List
from foundationallm.event_grid.models import EventGridTopicProfile

class AzureEventGridEventServiceProfile:
    """The profile used to configure event handling in the AzureEventGridEventService event service."""

    event_processing_cycle_seconds: int = Field(default=60, description="The time interval in seconds between successive event processing cycles.")
    topics: List[EventGridTopicProfile] = Field(default=[], description="The list of EventGridTopicProfile topic profiles used to configure event handling for an Azure Event Grid namespace topic.")


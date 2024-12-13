from pydantic import Field
from typing import Optional, List
from foundationallm.event_grid.models import EventGridEventTypeProfile

class EventGridTopicProfile:
    """The profile used to configure event handling for an Azure Event Grid namespace topic."""
    
    name: str = Field(default="", description="The name of the Azure Event Grid namespace topic.")
    subscription_prefix: str = Field(default="", description="The prefix used for the topic subscription name.")
    subscription_name: Optional[str] = Field(default=None, description="The topic subscription name.")
    subscription_available: bool = Field(default=False, description="Indicates whether the subscription is available for use or not.")
    event_type_profiles: List[EventGridEventTypeProfile] = Field(default=[], description="The list of EventGridEventTypeProfile event type profiles used to configure handling for event types.")

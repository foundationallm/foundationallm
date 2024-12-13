from pydantic import Field
from typing import List
from foundationallm.event_grid.models import EventGridEventSet

class EventGridEventTypeProfile:
    """The profile used to configure event handling for a specified Azure Event Grid event type."""

    event_type: str = Field(default="", description="The name of the Azure Event Grid event type.")
    event_sets: List[EventGridEventSet] = Field(default=[], description="The list of EventGridEventSet event sets used to configure event handling for a specific subset of events of a specified event type.")

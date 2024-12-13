from pydantic import Field

class EventGridEventSet:
    """The event set used to configure event handling for a specific subset of events of a specified event type."""

    namespace: str = Field(default="", description="The namespace associated with the event set.")
    source: str = Field(default="", description="The event source that defines the set.")
    subject_prefix: str = Field(default="", description="The event subject prefix that defines the set.")

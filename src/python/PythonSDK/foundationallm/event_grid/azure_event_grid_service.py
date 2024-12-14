import logging
import os
import sys
import time
from azure.identity import DefaultAzureCredential
from azure.core.credentials import AzureKeyCredential
from azure.core.exceptions import AzureError
from azure.core.messaging import CloudEvent
from azure.eventgrid import EventGridPublisherClient, EventGridConsumerClient
from foundationallm.event_grid.models import AzureEventGridEventServiceProfile, EventGridTopicProfile

logger = logging.getLogger('azure.identity')
logger.setLevel(logging.INFO)

handler = logging.StreamHandler(stream=sys.stdout)
formatter = logging.Formatter('[%(levelname)s %(name)s] %(message)s')
handler.setFormatter(formatter)
logger.addHandler(handler)

class AzureEventGridService:
    """Provides services to integrate with the Azure Event Grid eventing platform."""

    def __init__(self, endpoint: str, namespace_topic: str, subscription: str, profile: AzureEventGridEventServiceProfile = None):
        self.credential = AzureKeyCredential(os.environ["EVENTGRID_KEY"]) #DefaultAzureCredential()
        self.profile = profile
        self.endpoint = endpoint
        self.subscription = subscription
        self.event_set_event_delegates = {
            "python-test": None
        }       

        # send a CloudEvent for debug
        publisher_client = EventGridPublisherClient(endpoint, self.credential, namespace_topic=namespace_topic)
        event = CloudEvent(
            type="finished.loading",
            source="/python/test",
            data={"test": "python"},
            subject="python-test",
            specversion="1.0"
        )
        publisher_client.send(event)

        #self.consumer_client = EventGridConsumerClient(endpoint, credential, namespace_topic=namespace_topic, subscription=subscription)

    async def start_async(self):
        """Starts the event service, allowing it to initialize."""

        raise NotImplementedError

    async def stop_async(self):
        """Stops the event service, allowing it to cleanup."""

        raise NotImplementedError

    def execute(self):
        """Executes the event service in a loop."""

        while True:
            try:
                for topic in self.profile.topics:
                    if not topic.subscription_available:
                        continue

                client = EventGridConsumerClient(self.endpoint, self.credential, namespace_topic=topic.name, subscription=topic.subscription_name)

                if not client:
                    raise ValueError("The Azure Event Grid Service client is not properly initialized and will not execute.")

                event_details = client.receive(max_wait_time=10)

                for event_type_profile in topic.event_type_profiles:
                    for event_set in event_type_profile.event_sets:
                        events = [
                            e.event for e in event_details
                            if (e.event.type == event_type_profile.event_type
                                and e.event.source.lower() == event_set.source.lower()
                                and e.event.subject.strip()
                                and (not event_set.SubjectPrefix or e.event.subject.startswith(event_set.subject_prefix)))
                        ]

                        if (len(events) > 0
                            and event_set.namespace in self.event_set_event_delegates
                            and self.event_set_event_delegates[event_set.namespace] is not None):
                            # set the self.event_set_event_delegates[event_set.namespace]
                            pass

                        for event in events:
                            self.process_event(event)
                
                # Acknowledge
                acknowledge_events = []
                for detail in event_details:
                    acknowledge_events.append(detail.broker_properties.lock_token)
                    
                ack_result = self.consumer_client.acknowledge(
                    lock_tokens=acknowledge_events,
                )

                for ack_failure in ack_result.failed_lock_tokens:
                    print("Failed to acknowledge Event Grid message. Lock token: %s. Error code: %s. Error description: %s.",
                            str(ack_failure.lock_token), str(ack_failure.error), str(ack_failure))

                time.sleep(self.profile.event_processing_cycle_seconds)
            except AzureError as e:
                print("An error occured while trying to receive events: %s", str(e))


    def subscribe_to_event(self):
        """Adds an event set event delgate to the list of event handlers for a specified event set namespace."""

        raise NotImplementedError

    def unsubscribe_from_event(self):
        """Removes an event set event delegate from the list of event handlers for a specified event set namespace."""

        raise NotImplementedError

    def process_event(self, event: CloudEvent):
        """Process a single CloundEvent"""

        try:
            # add logic to handle the event
            if event.type == "finished.loading":
                print("Detected 'finished.loading' event. Triggering reload_data on the agent tool.")
                test_agent_tool = TestAgentTool()
                test_agent_tool.reload_data()

            print(f"Event ID: {event.id}, Type: {event.type}, Source: {event.source}, Data: {event.data}")
        except AzureError as e:
            print("Failed to process event: %s", str(e))

class TestAgentTool:
    """Test helper - to be replaced"""

    def reload_data(self):
        # replace with actual tool data cache refresh
        print("Reloading data triggered by finished.loading event.")
        pass

# Uncomment this lines to execute this file as a script
if __name__ == "__main__":
    topic = EventGridTopicProfile()
    topic.name = "python-test"
    topic.subscription_name = "python-test"
    topic.subscription_available = True

    profile = AzureEventGridEventServiceProfile()
    profile.event_processing_cycle_seconds = 60
    profile.topics = [ topic ]

    azure_event_grid_service = AzureEventGridService(
        endpoint=os.environ["EVENTGRID_ENDPOINT"],
        namespace_topic="python-test",
        subscription="python-test",
        profile=profile)

    azure_event_grid_service.execute()

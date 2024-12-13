import logging
import os
import sys
import time
from azure.identity import DefaultAzureCredential
from azure.core.credentials import AzureKeyCredential
from azure.core.exceptions import AzureError
from azure.core.messaging import CloudEvent
from azure.eventgrid import EventGridPublisherClient, EventGridConsumerClient

logger = logging.getLogger('azure.identity')
logger.setLevel(logging.INFO)

handler = logging.StreamHandler(stream=sys.stdout)
formatter = logging.Formatter('[%(levelname)s %(name)s] %(message)s')
handler.setFormatter(formatter)
logger.addHandler(handler)

class AzureEventGridService:
    """Provides services to integrate with the Azure Event Grid eventing platform."""
    def __init__(self, endpoint=None, topic_name=None, subscription_name=None):
        #credential = DefaultAzureCredential()
        credential = AzureKeyCredential(os.environ["EVENTGRID_KEY"])

        # debug defaults
        endpoint = os.environ["EVENTGRID_ENDPOINT"]
        topic_name = "python-test"
        subscription_name = "python-test"

        # send a CloudEvent
        publisher_client = EventGridPublisherClient(endpoint, credential, namespace_topic=topic_name)
        event = CloudEvent(
            type="finished.loading",
            source="/python/test",
            data={"test": "python"},
            subject="PythonTest",
            specversion="1.0"
        )

        publisher_client.send(event)

        self.consumer_client = EventGridConsumerClient(endpoint, credential, namespace_topic=topic_name, subscription=subscription_name)

    async def start_async(self):
        """Starts the event service, allowing it to initialize."""
        raise NotImplementedError

    async def stop_async(self):
        """Stops the event service, allowing it to cleanup."""
        raise NotImplementedError

    def execute(self):
        """Executes the event service in a loop."""
        if self.consumer_client:
            while True:
                try:
                    received_details = self.consumer_client.receive(max_wait_time=10)

                    acknowledge_events = []

                    for detail in received_details:
                        acknowledge_events.append(detail.broker_properties.lock_token)
                        self.process_event(detail.event)

                    # Acknowledge
                    ack_result = self.consumer_client.acknowledge(
                        lock_tokens=acknowledge_events,
                    )

                    for ack_failure in ack_result.failed_lock_tokens:
                        print("Failed to acknowledge Event Grid message. Lock token: %s. Error code: %s. Error description: %s.",
                              str(ack_failure.lock_token), str(ack_failure.error), str(ack_failure))

                    time.sleep(60) # seconds
                except AzureError as e:
                    print("An error occured while trying to receive events: %s", str(e))

        raise ValueError("The Azure Event Grid events service is not properly initialized and will not execute.")

    def subscribe_to_event(self):
        """Adds an event set event delgate to the list of event handlers for a specified event set namespace."""
        raise NotImplementedError

    def unsubscribe_from_event(self):
        """Removes an event set event delegate from the list of event handlers for a specified event set namespace."""
        raise NotImplementedError

    def process_event(self, event: CloudEvent):
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
    def reload_data(self):
        print("Reloading data triggered by finished.loading event.")
        # replace with actual tool data cache refresh
        pass

# Uncomment this lines to execute this file as a script
# if __name__ == "__main__":
#     azure_event_grid_service = AzureEventGridService()
#     azure_event_grid_service.execute()

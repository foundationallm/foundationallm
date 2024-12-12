import time
import uuid
from azure.identity import DefaultAzureCredential
from azure.core.exceptions import AzureError
from azure.core.messaging import CloudEvent
from azure.eventgrid import EventGridPublisherClient, EventGridConsumerClient
from azure.eventgrid.models import EventGridEvent

class AzureEventGridService:
    def __init__(self, endpoint=None, topic_name=None, subscription_name=None):
        credential = DefaultAzureCredential()

        # debug defaults
        endpoint = "evgd-ftpisrjz2rvjc.eastus2-1.eventgrid.azure.net"
        topic_name = "python-test"
        subscription_name = "python-test"

        # send a CloudEvent
        publisher_client = EventGridPublisherClient(endpoint, credential)
        event = CloudEvent(
            type="finished.loading",
            source="/python/test",
            data={"todo": "acknowledge"},
            subject="PythonTest",
            specversion="1.0",
            id=uuid.uuid4()
        )
        publisher_client.send(event)

        self.consumer_client = EventGridConsumerClient(endpoint, credential, namespace_topic=topic_name, subscription=subscription_name)

    async def start_async(self):
        raise NotImplementedError

    async def stop_async(self):
        raise NotImplementedError

    def execute(self):
        if self.consumer_client:
            while True:
                received_details = self.consumer_client.receive(max_wait_time=10)

                release_events = []
                acknowledge_events = []
                reject_events = []

                for detail in received_details:
                    data = detail.event.data
                    broker_properties = detail.broker_properties
                    if data["todo"] == "release":
                        release_events.append(broker_properties.lock_token)
                    elif data["todo"] == "acknowledge":
                        acknowledge_events.append(broker_properties.lock_token)
                    else:
                        reject_events.append(broker_properties.lock_token)

                    self.process_event(detail.event)

                    # Renew
                    # renew_tokens = broker_properties.lock_token
                    # self.renew_result = self.consumer_client.renew_locks(
                    #     lock_tokens=renew_tokens,
                    # )

                # Release
                # self.release_result = self.consumer_client.release(
                #     lock_tokens=release_events,
                # )

                # Acknowledge
                self.ack_result = self.consumer_client.acknowledge(
                    lock_tokens=acknowledge_events,
                )

                # Reject
                # self.reject_result = self.consumer_client.reject(
                #     lock_tokens=reject_events,
                # )

                time.sleep(60) # seconds

        raise ValueError("Client is not initialized")

    def subscribe_to_event(self):
        raise NotImplementedError

    def unsubscribe_from_event(self):
        raise NotImplementedError

    def process_event(event: CloudEvent):
        try:
            # add logic to handle the event

            if event.type == "finished.loading":
                print("Detected 'finished.loading' event. Triggering reload_data on the agent tool.")
                test_agent_tool = TestAgentTool()
                test_agent_tool.reload_data()

            print(f"Event ID: {event.id}, Type: {event.type}, Source: {event.source}")
        except AzureError as e:
            print("Failed to process event: %s", str(e))

class TestAgentTool:
    def reload_data(self):
        print("Reloading data triggered by finished.loading event.")
        # replace with actual tool data cache refresh
        pass

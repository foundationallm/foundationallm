'''
This sample shows how to retrieve the list of agent workflows.
'''

import os
from dotenv import load_dotenv

from clients import ManagementClient

# Load environment variables from .env file
load_dotenv()

management_client = ManagementClient(
    os.getenv("MANAGEMENT_API_SCOPE"),
    os.getenv("MANAGEMENT_API_ENDPOINT"),
    os.getenv("FOUNDATIONALLM_INSTANCE_ID")
)

result = management_client.get_agent_workflows()

print(result)
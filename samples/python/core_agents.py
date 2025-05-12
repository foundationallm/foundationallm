'''
This samepl shows how to retrieve the list of agents available to the user.
'''

import os
from dotenv import load_dotenv

from clients import CoreClient

# Load environment variables from .env file
load_dotenv()

core_client = CoreClient(
    os.getenv("CORE_API_SCOPE"),
    os.getenv("CORE_API_ENDPOINT"),
    os.getenv("FOUNDATIONALLM_INSTANCE_ID")
)

result = core_client.get_agents()

print(result)

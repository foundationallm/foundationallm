'''
This sample shows how to configure a vectorization pipeline using the FoundationaLLM Management API.
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

DATA_SOURCE_NAME = "default-storage"
VECTOR_STORE_NAME = "default-index"

result = management_client.create_vectorization_pipeline(
    "Test04",
    "Sample vectorization pipeline.",
    DATA_SOURCE_NAME,
    VECTOR_STORE_NAME
)

print(result)

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
VECTORIZATION_PIPELINE_NAME = "Test04"
VECTORIZATION_PIPELINE_DESCRIPTION = "Sample vectorization pipeline."

result = management_client.create_vectorization_pipeline(
    VECTORIZATION_PIPELINE_NAME,
    VECTORIZATION_PIPELINE_DESCRIPTION,
    DATA_SOURCE_NAME,
    VECTOR_STORE_NAME
)

print(result)

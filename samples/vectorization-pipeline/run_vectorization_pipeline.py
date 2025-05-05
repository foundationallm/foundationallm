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

VECTORIZATION_PIPELINE_NAME = "Test02"

result = management_client.activate_vectorization_pipeline(
    VECTORIZATION_PIPELINE_NAME
)
print(result)

result = management_client.get_vectorization_pipeline(
    VECTORIZATION_PIPELINE_NAME
)
print(result)

result = management_client.get_vectorization_pipeline_execution(
    VECTORIZATION_PIPELINE_NAME,
    result["resource"]["latest_execution_id"]
)
print(result)

'''
This sample shows how to run and monitor a vectorization pipeline using the FoundationaLLM Management API.
'''

import os
import time
from datetime import datetime, timezone
from dotenv import load_dotenv

from clients import ManagementClient

def format_timedelta(td):
    days = td.days
    hours, remainder = divmod(td.seconds, 3600)
    minutes, seconds = divmod(remainder, 60)
    return f"{days} days, {hours} hours, {minutes} minutes, {seconds} seconds"

# Load environment variables from .env file
load_dotenv()

management_client = ManagementClient(
    os.getenv("MANAGEMENT_API_SCOPE"),
    os.getenv("MANAGEMENT_API_ENDPOINT"),
    os.getenv("FOUNDATIONALLM_INSTANCE_ID")
)

VECTORIZATION_PIPELINE_NAME = "Test04"

result = management_client.get_vectorization_pipeline(
    VECTORIZATION_PIPELINE_NAME
)
pipeline = result[0]["resource"]

#----------------------------------------------------------------------------------------------
#
# Check if the pipeline is already active or if the previous execution is still running.
#
#-----------------------------------------------------------------------------------------------

if (pipeline["active"]):
    print("WARNING: Pipeline is already active.")
    exit(0)

print("Checking if the previous pipeline execution is still running...")
previous_latest_execution_id = pipeline["latest_execution_id"]

if (previous_latest_execution_id is not None):
    result = management_client.get_vectorization_pipeline_execution(
        VECTORIZATION_PIPELINE_NAME,
        previous_latest_execution_id
    )
    pipeline_execution = result[0]["resource"]
    vectorization_request_count = pipeline_execution["vectorization_request_count"]
    vectorization_request_failures_count = pipeline_execution["vectorization_request_failures_count"]
    vectorization_request_successes_count = pipeline_execution["vectorization_request_successes_count"]
    if (vectorization_request_successes_count + vectorization_request_failures_count < vectorization_request_count):
        print("WARNING: Previous pipeline execution is still running.")
        exit(0)

#----------------------------------------------------------------------------------------------
#
# Activate the pipeline and wait for a change in the last execution id.
# This indicates that the pipeline has started executing.
#
#-----------------------------------------------------------------------------------------------

print("Preparing to activate pipeline...")
# record start time
start_time = datetime.now(timezone.utc)
activation_result = management_client.activate_vectorization_pipeline(
    VECTORIZATION_PIPELINE_NAME
)

if (not activation_result["is_success"]):
    print("ERROR: Could not activate the pipeline.")
    exit(0)

print("Pipeline activated successfully.")

print("Waiting for the pipeline execution to start...")

# NOTE: A change in the latest execution id indicates that the pipeline has started executing.
latest_execution_id = previous_latest_execution_id
number_of_checks = 0

while (latest_execution_id == previous_latest_execution_id):

    if (number_of_checks >= 30):
        print("ERROR: Pipeline execution did not start.")
        exit(0)
    
    print("Checking for a change in the last execution id...")
    number_of_checks += 1
    
    result = management_client.get_vectorization_pipeline(
        VECTORIZATION_PIPELINE_NAME
    )
    pipeline = result[0]["resource"]
    latest_execution_id = pipeline["latest_execution_id"]

    time.sleep(10)  

#----------------------------------------------------------------------------------------------
#
# Wait for the pipeline execution to finish.
#
#-----------------------------------------------------------------------------------------------

print("Waiting for the pipeline execution to finish...")

result = management_client.get_vectorization_pipeline_execution(
    VECTORIZATION_PIPELINE_NAME,
    latest_execution_id
)
pipeline_execution = result[0]["resource"]

vectorization_request_count = pipeline_execution["vectorization_request_count"]
vectorization_request_failures_count = 0
vectorization_request_successes_count = 0

print(f"The pipeline will attempt to process {vectorization_request_count} vectorization requests.")

while (vectorization_request_successes_count + vectorization_request_failures_count < vectorization_request_count):

    time.sleep(10)

    result = management_client.get_vectorization_pipeline_execution(
        VECTORIZATION_PIPELINE_NAME,
        latest_execution_id
    )
    pipeline_execution = result[0]["resource"]

    vectorization_request_failures_count = pipeline_execution["vectorization_request_failures_count"]
    vectorization_request_successes_count = pipeline_execution["vectorization_request_successes_count"]

    print(f"Pipeline execution status: {vectorization_request_count} vectorization requests ({vectorization_request_successes_count} successes and {vectorization_request_failures_count} failures, elapsed time {format_timedelta(datetime.now(timezone.utc) - start_time)}).")

if (vectorization_request_failures_count > 0):
    print(f"ERROR: Pipeline execution completed with {vectorization_request_failures_count} vectorization request failures.")

print("Pipeline execution completed successfully.")
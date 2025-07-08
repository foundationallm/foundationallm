import requests
import json
import os
import uuid
import pandas as pd
import time
import concurrent.futures
from tqdm import tqdm 
import mimetypes
from dotenv import load_dotenv

TEST_FILE = "TestQuestions.csv"
AGENT_NAME = "MAA-01"  

# Load environment variables from .env file
load_dotenv()

def create_session():
    fllm_endpoint = os.getenv("FLLM_ENDPOINT")
    if not fllm_endpoint:
        print("Error: FLLM_ENDPOINT environment variable is not set")
        print("Make sure you have a .env file with FLLM_ENDPOINT defined")
        return None

    url = f"{fllm_endpoint}sessions"
    
    # Get access token from environment variable
    access_token = os.getenv("FLLM_ACCESS_TOKEN")
    if not access_token:
        print("Error: FLLM_ACCESS_TOKEN environment variable is not set")
        print("Make sure you have a .env file with FLLM_ACCESS_TOKEN defined")
        return None
    
    # Headers
    headers = {
        "X-AGENT-ACCESS-TOKEN": access_token,
        "Content-Type": "application/json"
    }
    
    # Generate a new GUID for the session name
    session_guid = str(uuid.uuid4())
    
    # Request body
    payload = {
        "name": session_guid
    }
    
    try:

        # Make the POST request
        response = requests.post(url, headers=headers, json=payload)
        
        # Check if the request was successful
        response.raise_for_status()
        
        # Get the response JSON
        response_json = response.json()
        
        # Print the response
        print(f"Session Creation Status Code: {response.status_code}")
        
        # Return the sessionId
        return response_json.get('sessionId')
        
    except requests.exceptions.RequestException as e:
        print(f"An error occurred: {str(e)}")
        if hasattr(e, 'response') and e.response is not None:
            print(f"Response Status Code: {e.response.status_code}")
            print(f"Response Text: {e.response.text}")
        return None

def send_completion_request(session_id, agent_name, user_prompt, attachments=[]):
    fllm_endpoint = os.getenv("FLLM_ENDPOINT")
    if not fllm_endpoint:
        print("Error: FLLM_ENDPOINT environment variable is not set")
        print("Make sure you have a .env file with FLLM_ENDPOINT defined")
        return None
    url = f"{fllm_endpoint}completions"
    
    # Get access token from environment variable
    access_token = os.getenv("FLLM_ACCESS_TOKEN")
    if not access_token:
        print("Error: FLLM_ACCESS_TOKEN environment variable is not set")
        print("Make sure you have a .env file with FLLM_ACCESS_TOKEN defined")
        return None
    
    # Headers
    headers = {
        "X-AGENT-ACCESS-TOKEN": access_token,
        "Content-Type": "application/json"
    }
    
    # Request body
    payload = {
        "user_prompt": user_prompt,
        "agent_name": agent_name,
        "session_id": session_id,
        "attachments": attachments
    }
    
    try:
        print(f"Sending completion request for user prompt: {user_prompt}")

        # Make the POST request
        response = requests.post(url, headers=headers, json=payload)
        
        # Check if the request was successful
        response.raise_for_status()
        
        # Get the response JSON
        response_json = response.json()
        
        # Print the response
        print(f"Completion Request Status Code: {response.status_code}")
        print("Completion Request Response:")
        print(json.dumps(response_json, indent=2))
        
        return response_json
        
    except requests.exceptions.RequestException as e:
        print(f"An error occurred: {str(e)}")
        if hasattr(e, 'response') and e.response is not None:
            print(f"Response Status Code: {e.response.status_code}")
            print(f"Response Text: {e.response.text}")
        return None

def upload_file_with_progress(file_path, fllm_endpoint, session_id, agent_name):

    # Get file size for progress tracking
    file_size = os.path.getsize(file_path)
    
    # Create a progress bar
    progress_bar = tqdm(
        total=file_size,
        unit='B',
        unit_scale=True,
        desc=f"Uploading {os.path.basename(file_path)}"
    )

    # Prepare the upload
    with open(file_path, 'rb') as file:
        # Create a custom file-like object that updates progress
        class ProgressFile:
            def __init__(self, file_obj, progress_bar):
                self.file_obj = file_obj
                self.progress_bar = progress_bar

            def read(self, *args, **kwargs):
                chunk = self.file_obj.read(*args, **kwargs)
                if chunk:
                    self.progress_bar.update(len(chunk))
                return chunk

        # Wrap the file with our progress tracking
        progress_file = ProgressFile(file, progress_bar)
        
        # Get the MIME type of the file
        mime_type, _ = mimetypes.guess_type(file_path)
        # If the mime type is not recognized, we will leave it as None
        # as the Core API will make its own attempt to determine it.
        
        # Prepare the multipart form data with MIME type
        files = {
            'file': (
                os.path.basename(file_path),
                progress_file,
                mime_type
            )
        }
        
        # Construct the upload URL
        upload_url = f"{fllm_endpoint}files/upload"
        
        # Add query parameters
        params = {
            'sessionId': session_id,
            'agentName': agent_name
        }
        
        # Set up headers with authentication
        # Get access token from environment variable
        access_token = os.getenv("FLLM_ACCESS_TOKEN")
        if not access_token:
            print("Error: FLLM_ACCESS_TOKEN environment variable is not set")
            print("Make sure you have a .env file with FLLM_ACCESS_TOKEN defined")
            return None
        
        # Headers
        headers = {
            "X-AGENT-ACCESS-TOKEN": access_token
        }

        try:
            # Make the POST request with streaming upload
            response = requests.post(
                upload_url,
                params=params,
                headers=headers,
                files=files
            )
            
            # Close the progress bar
            progress_bar.close()
            
            # Check if the request was successful
            if response.status_code >= 200 and response.status_code < 300:
                return response.json()
            else:
                raise Exception(f"Upload failed with status {response.status_code}: {response.text}")
                
        except Exception as e:
            progress_bar.close()
            raise e


def process_question(question, answer, filename):

    # Time the session creation
    session_start_time = time.time()
    session_id = create_session()
    time.sleep(0.1)
    session_duration = time.time() - session_start_time
    
    if not session_id:
        print(f"Failed to create session for question: {question}")
        return {
            'Question': question,
            'Answer': answer,
            'AgentAnswer': '',
            'OtherAgentContent': [],
            'Tokens': 0,
            'SessionRequestDuration': session_duration,
            'CompletionRequestDuration': 0,
            'ErrorOccured': 1,
            'ErrorDetails': 'Failed to create session'
        }
    
    # Time the completion request
    completion_start_time = time.time()
    if (not pd.isna(filename)):
        # Upload the file
        file_path = os.path.join(os.getcwd(), 'uploads', filename)
        fllm_endpoint = os.getenv("FLLM_ENDPOINT")
        upload_response = upload_file_with_progress(file_path, fllm_endpoint, session_id, AGENT_NAME)

        response = send_completion_request(
            session_id=session_id,
            agent_name=AGENT_NAME,
            user_prompt=question,
            attachments=[upload_response['objectId']]
        )
    else:   
        response = send_completion_request(
            session_id=session_id,
            agent_name=AGENT_NAME,
            user_prompt=question
        )
    completion_duration = time.time() - completion_start_time
    
    if response:
        # Initialize error tracking variables
        error_occurred = 0
        error_details = ""
        agent_answer = ""
        other_agent_content = []
        
        try:
            content_items = response.get('content', [])
            if not content_items:
                error_occurred = 1
                error_details = "Empty content array"
                print(f"Warning: Empty content array for question: {question}")
            else:
                # Get all items except the last one for other_agent_content
                other_agent_content = content_items[:-1]
                # Get the last item as agent_answer
                last_item = content_items[-1]
                agent_answer = last_item.get('value', '')
                if not agent_answer:
                    error_occurred = 1
                    error_details = "Empty content value in last item"
                    print(f"Warning: Empty content value in last item for question: {question}")
        except (IndexError, AttributeError, KeyError) as e:
            error_occurred = 1
            error_details = str(e)
            print(f"Error extracting content field for question: {question}")
            print(f"Error details: {error_details}")
        
        tokens = response.get('tokens', 0)
        
        return {
            'Question': question,
            'Answer': answer,
            'AgentAnswer': agent_answer,
            'OtherAgentContent': other_agent_content,
            'Tokens': tokens,
            'SessionRequestDuration': session_duration,
            'CompletionRequestDuration': completion_duration,
            'ErrorOccured': error_occurred,
            'ErrorDetails': error_details
        }
    else:
        print(f"Failed to get completion for question: {question}")
        return {
            'Question': question,
            'Answer': answer,
            'AgentAnswer': '',
            'OtherAgentContent': [],
            'Tokens': 0,
            'SessionRequestDuration': session_duration,
            'CompletionRequestDuration': completion_duration,
            'ErrorOccured': 1,
            'ErrorDetails': 'Failed to get completion response'
        }

def execute_tests(max_workers=2):
    # Read the CSV file with proper handling of quoted fields
    try:
        df = pd.read_csv(TEST_FILE, quotechar='"', escapechar='\\')
    except FileNotFoundError:
        print(f"Error: {TEST_FILE} file not found")
        return None
    
    # Initialize list to store results
    results = []
    
    # Process questions in parallel
    with concurrent.futures.ThreadPoolExecutor(max_workers=max_workers) as executor:
        # Create future objects for each question
        future_to_question = {
            executor.submit(process_question, row['Question'], row['Answer'], row['Filename']): index 
            for index, row in df.iterrows()
        }
        
        # Process completed futures as they finish
        for future in concurrent.futures.as_completed(future_to_question):
            index = future_to_question[future]
            try:
                result = future.result()
                if result:
                    results.append(result)
                else:
                    # Get the original question from the DataFrame
                    original_question = df.iloc[index]['Question']
                    original_answer = df.iloc[index]['Answer']
                    # Add a row with error details
                    results.append({
                        'Question': original_question,
                        'Answer': original_answer,
                        'AgentAnswer': '',
                        'OtherAgentContent': [],
                        'Tokens': 0,
                        'SessionRequestDuration': 0,
                        'CompletionRequestDuration': 0,
                        'ErrorOccured': 1,
                        'ErrorDetails': 'Result was None'
                    })
            except Exception as e:
                # Get the original question from the DataFrame
                original_question = df.iloc[index]['Question']
                original_answer = df.iloc[index]['Answer']
                # Add a row with exception details
                results.append({
                    'Question': original_question,
                    'Answer': original_answer,
                    'AgentAnswer': '',
                    'OtherAgentContent': [],
                    'Tokens': 0,
                    'SessionRequestDuration': 0,
                    'CompletionRequestDuration': 0,
                    'ErrorOccured': 1,
                    'ErrorDetails': str(e)
                })
                print(f"Error processing question at index {index}: {str(e)}")
    
    # Create DataFrame from results
    results_df = pd.DataFrame(results)
    
    # Save results to CSV
    results_df.to_csv('test_results.csv', index=False)
    
    return results_df

if __name__ == "__main__":
    # Execute tests with default 2 workers
    results = execute_tests()
    if results is not None:
        print("\nTest Results:")
        print(results) 
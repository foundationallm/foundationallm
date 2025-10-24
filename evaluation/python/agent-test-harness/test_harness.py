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

# Global variables removed - now passed as parameters to functions

# Load environment variables from .env file
load_dotenv()

# -----------------------------
# Results helpers
# -----------------------------
def _safe_dump(obj, limit=50000):
    try:
        s = json.dumps(obj, ensure_ascii=False)
    except Exception:
        return "[]"
    return s if len(s) <= limit else s[:limit] + "... [truncated]"

def _safe_text(val, limit=5000):
    if val is None:
        return ""
    s = str(val)
    return s if len(s) <= limit else s[:limit] + "... [truncated]"

def _single_line(val: str) -> str:
    try:
        return ' '.join((val or '').split())
    except Exception:
        return str(val) if val is not None else ''

def _ellipsis(val: str, limit: int = 200) -> str:
    s = _single_line(val or '')
    return s if len(s) <= limit else s[:limit] + '...'

# -----------------------------
# Management API (Prompts) helpers
# -----------------------------
def _get_management_headers():
    mgmt_token = os.getenv("FLLM_MGMT_BEARER_TOKEN")
    if not mgmt_token:
        raise RuntimeError("FLLM_MGMT_BEARER_TOKEN environment variable is not set. Acquire a bearer token with scope api://FoundationaLLM-Management/Data.Manage and set it in .env.")
    return {
        "Authorization": f"Bearer {mgmt_token}",
        "Content-Type": "application/json"
    }

def list_prompts():
    """List all Prompt resources accessible to the caller via the Management API."""
    mgmt_endpoint = os.getenv("FLLM_MGMT_ENDPOINT")
    if not mgmt_endpoint:
        raise RuntimeError("FLLM_MGMT_ENDPOINT environment variable is not set. Set it to the Management API base URL including /instances/{instanceId}/ and a trailing slash.")

    url = f"{mgmt_endpoint}providers/FoundationaLLM.Prompt/prompts"
    headers = _get_management_headers()
    try:
        response = requests.get(url, headers=headers)
        response.raise_for_status()
        return response.json()
    except requests.exceptions.RequestException as e:
        print(f"An error occurred listing prompts: {str(e)}")
        if hasattr(e, 'response') and e.response is not None:
            print(f"Response Status Code: {e.response.status_code}")
            print(f"Response Text: {e.response.text}")
        return None

def get_prompt(prompt_name: str):
    """Get a Prompt resource by name via the Management API."""
    mgmt_endpoint = os.getenv("FLLM_MGMT_ENDPOINT")
    if not mgmt_endpoint:
        raise RuntimeError("FLLM_MGMT_ENDPOINT environment variable is not set. Set it to the Management API base URL including /instances/{instanceId}/ and a trailing slash.")

    url = f"{mgmt_endpoint}providers/FoundationaLLM.Prompt/prompts/{prompt_name}"
    headers = _get_management_headers()
    try:
        response = requests.get(url, headers=headers)
        response.raise_for_status()
        payload = response.json()
        if isinstance(payload, list) and len(payload) > 0:
            return payload[0].get("resource") or payload[0]
        return payload
    except requests.exceptions.RequestException as e:
        print(f"An error occurred getting prompt '{prompt_name}': {str(e)}")
        if hasattr(e, 'response') and e.response is not None:
            print(f"Response Status Code: {e.response.status_code}")
            print(f"Response Text: {e.response.text}")
        return None

def upsert_prompt(prompt_object: dict):
    """Create or update a Prompt resource via the Management API.

    The prompt_object should follow the PromptBase contract. For multipart prompts provide keys:
    {"type":"multipart","name":"<name>","category": <optional>, "prefix": "...", "suffix": "..."}
    """
    mgmt_endpoint = os.getenv("FLLM_MGMT_ENDPOINT")
    if not mgmt_endpoint:
        raise RuntimeError("FLLM_MGMT_ENDPOINT environment variable is not set. Set it to the Management API base URL including /instances/{instanceId}/ and a trailing slash.")

    if not isinstance(prompt_object, dict) or not prompt_object.get("name"):
        raise ValueError("prompt_object must be a dict containing at least a 'name' key")

    url = f"{mgmt_endpoint}providers/FoundationaLLM.Prompt/prompts/{prompt_object['name']}"
    headers = _get_management_headers()
    try:
        response = requests.post(url, headers=headers, json=prompt_object)
        response.raise_for_status()
        return response.json()
    except requests.exceptions.RequestException as e:
        print(f"An error occurred upserting prompt '{prompt_object.get('name','')}': {str(e)}")
        if hasattr(e, 'response') and e.response is not None:
            print(f"Response Status Code: {e.response.status_code}")
            print(f"Response Text: {e.response.text}")
        return None

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


def process_question(question, answer, filename, agent_name="MAA-02", validation_rules="{}", validation_mode="hybrid"):

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
    
    # Normalize filename for output
    input_filename = '' if pd.isna(filename) else str(filename)

    # Time the completion request
    completion_start_time = time.time()
    if (not pd.isna(filename)):
        # Upload the file
        file_path = os.path.join(os.getcwd(), 'uploads', input_filename)
        fllm_endpoint = os.getenv("FLLM_ENDPOINT")
        upload_response = upload_file_with_progress(file_path, fllm_endpoint, session_id, agent_name)

        response = send_completion_request(
            session_id=session_id,
            agent_name=agent_name,
            user_prompt=question,
            attachments=[upload_response.get('object_id') or upload_response.get('objectId')]
        )
    else:   
        response = send_completion_request(
            session_id=session_id,
            agent_name=agent_name,
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

        # Extract content artifacts (handles both snake_case and camelCase)
        artifacts = response.get('content_artifacts') or response.get('contentArtifacts') or []

        # Build a compact summary per artifact
        def _summarize_artifact(a):
            meta = a.get('metadata') or {}
            return {
                'id': a.get('id'),
                'title': a.get('title'),
                'type': a.get('type'),
                'source': a.get('source'),
                'filepath': a.get('filepath'),
                'tool_result': meta.get('tool_result'),
                'tool_error': meta.get('tool_error'),
                'tool_generated_code': meta.get('tool_generated_code'),
                'tool_input_prompt': meta.get('tool_input_prompt'),
                'tool_input_files': meta.get('tool_input_files'),
                'tool_output': meta.get('tool_output')
            }

        artifacts_summary = [_summarize_artifact(a) for a in artifacts]
        artifacts_json = _safe_dump(artifacts)
        artifacts_summary_json = _safe_dump(artifacts_summary)

        # Extract details from Code tool execution artifact(s)
        # Only consider tool execution artifacts for code success/failure evaluation.
        code_artifacts = [a for a in artifacts if a.get('type') == 'ToolExecution']
        code_count = len(code_artifacts)
        code_meta = (code_artifacts[0].get('metadata') or {}) if code_artifacts else {}

        code_original_user_prompt = _safe_text(code_meta.get('original_user_prompt'))
        code_tool_input_prompt   = _safe_text(code_meta.get('tool_input_prompt'))
        code_tool_input_files    = _safe_text(code_meta.get('tool_input_files'))
        code_tool_generated_code = _safe_text(code_meta.get('tool_generated_code'))
        code_tool_output         = _safe_text(code_meta.get('tool_output'))
        code_tool_error          = _safe_text(code_meta.get('tool_error'))
        code_tool_result         = _safe_text(code_meta.get('tool_result'))

        # Determine Code tool failures across all Code artifacts
        failed_msgs = []
        for a in code_artifacts:
            tr = ((a.get('metadata') or {}).get('tool_result') or '')
            if isinstance(tr, str) and tr.startswith('Code execution failed'):
                failed_msgs.append(_safe_text(tr))
        code_tool_failed = len(failed_msgs) > 0
        if code_tool_failed:
            error_occurred = 1
            error_details = (error_details + ('; ' if error_details else '')) + ' | '.join(failed_msgs)

        # Extract file artifacts (produced files)
        file_artifacts = [a for a in artifacts if a.get('type') == 'File']
        produced_files_count = len(file_artifacts)
        def _summarize_file(a):
            meta = a.get('metadata') or {}
            return {
                'id': a.get('id'),
                'title': a.get('title'),
                'original_file_name': meta.get('original_file_name'),
                'content_type': meta.get('content_type'),
                'file_size': meta.get('file_size'),
                'file_object_id': meta.get('file_object_id'),
                'filepath': a.get('filepath')
            }
        produced_files_summary_json = _safe_dump([_summarize_file(a) for a in file_artifacts])
        
        # Prepare answer preview and sanitized error details for CSV readability
        agent_answer_full = agent_answer
        agent_answer_preview = _single_line(agent_answer_full)[:50]
        error_details_single_line = _single_line(error_details)

        # Add validation results if validation is enabled
        validation_passed = -1  # -1 = not validated, 0 = failed, 1 = passed
        validation_score = 0
        validation_details = ""
        
        # Try to import and use validator if available
        try:
            from validator import TestValidator
            validator = TestValidator()
            
            # Create test result dict for validation
            test_result = {
                'Question': question,
                'AgentAnswer': agent_answer_full,
                'ExpectedAnswer': answer,
                'ValidationRules': validation_rules,
                'ValidationMode': validation_mode,
                'ErrorOccured': error_occurred,
                'CodeToolFailed': code_tool_failed,
                'ProducedFilesCount': produced_files_count,
                'ArtifactsSummary': artifacts_summary_json
            }
            
            # Perform validation
            validation_result = validator._validate_single_test(test_result, 'hybrid', False)
            validation_passed = 1 if validation_result['passed'] else 0
            validation_score = validation_result['score']
            validation_details = validation_result['reason']
            
        except ImportError:
            # Validator not available, skip validation
            pass
        except Exception as e:
            # Validation failed, mark as not validated
            validation_details = f"Validation error: {str(e)}"
        
        return {
            'Question': question,
            'Filename': input_filename,
            'Answer': answer,
            'AgentAnswer': agent_answer_full,
            'AgentAnswerPreview': agent_answer_preview,
            'OtherAgentContent': other_agent_content,
            'Tokens': tokens,
            'SessionRequestDuration': session_duration,
            'CompletionRequestDuration': completion_duration,
            'ErrorOccured': error_occurred,
            'ErrorDetails': error_details_single_line,
            'ContentArtifacts': artifacts_json,
            'ArtifactsSummary': artifacts_summary_json,
            'CodeArtifactCount': code_count,
            'CodeOriginalUserPrompt': code_original_user_prompt,
            'CodeToolInputPrompt': code_tool_input_prompt,
            'CodeToolInputFiles': code_tool_input_files,
            'CodeToolGeneratedCode': code_tool_generated_code,
            'CodeToolOutput': code_tool_output,
            'CodeToolError': code_tool_error,
            'CodeToolResult': code_tool_result,
            'CodeToolFailed': code_tool_failed,
            'ProducedFilesCount': produced_files_count,
            'ProducedFilesSummary': produced_files_summary_json,
            'ValidationRules': validation_rules,
            'ValidationMode': validation_mode,
            'ValidationPassed': validation_passed,
            'ValidationScore': validation_score,
            'ValidationDetails': validation_details
        }
    else:
        print(f"Failed to get completion for question: {question}")
        return {
            'Question': question,
            'Filename': input_filename,
            'Answer': answer,
            'AgentAnswer': '',
            'AgentAnswerPreview': '',
            'OtherAgentContent': [],
            'Tokens': 0,
            'SessionRequestDuration': session_duration,
            'CompletionRequestDuration': completion_duration,
            'ErrorOccured': 1,
            'ErrorDetails': 'Failed to get completion response',
            'ContentArtifacts': '[]',
            'ArtifactsSummary': '[]',
            'CodeArtifactCount': 0,
            'CodeOriginalUserPrompt': '',
            'CodeToolInputPrompt': '',
            'CodeToolInputFiles': '',
            'CodeToolGeneratedCode': '',
            'CodeToolOutput': '',
            'CodeToolError': '',
            'CodeToolResult': '',
            'CodeToolFailed': False,
            'ProducedFilesCount': 0,
            'ProducedFilesSummary': '[]',
            'ValidationPassed': -1,
            'ValidationScore': 0,
            'ValidationDetails': 'Test execution failed'
        }

def execute_tests(test_file, agent_name, max_workers=5):
    # Read the CSV file with proper handling of quoted fields
    try:
        df = pd.read_csv(test_file, quotechar='"', escapechar='\\')
    except FileNotFoundError:
        print(f"Error: {test_file} file not found")
        return None
    
    # Initialize list to store results
    results = []
    
    # Process questions in parallel
    with concurrent.futures.ThreadPoolExecutor(max_workers=max_workers) as executor:
        # Create future objects for each question
        future_to_question = {
            executor.submit(process_question, row['Question'], row['Answer'], row['Filename'], agent_name, row.get('ValidationRules', '{}'), row.get('ValidationMode', 'hybrid')): index 
            for index, row in df.iterrows()
        }
        
        # Process completed futures as they finish
        for future in concurrent.futures.as_completed(future_to_question):
            index = future_to_question[future]
            try:
                result = future.result()
                if result:
                    result['Ordinal'] = index + 1
                    results.append(result)
                else:
                    # Get the original question from the DataFrame
                    original_question = df.iloc[index]['Question']
                    original_answer = df.iloc[index]['Answer']
                    # Add a row with error details
                    results.append({
                        'Ordinal': index + 1,
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
                    'Ordinal': index + 1,
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

    # Save compact CSV (summary columns kept on one line)
    summary_columns = [
        'Question','Filename','AgentAnswerPreview','ErrorOccured','ErrorDetails','Tokens',
        'SessionRequestDuration','CompletionRequestDuration','CodeArtifactCount',
        'CodeToolFailed','CodeToolError','CodeToolResult',
        'ProducedFilesCount','ProducedFilesSummary','ArtifactsSummary'
    ]
    # Only use columns that exist (older runs may not have all fields)
    summary_columns = [c for c in summary_columns if c in results_df.columns]
    results_df.to_csv('test_results.csv', index=False, columns=summary_columns)
    
    # Save full-fidelity JSON
    try:
        with open('test_results.json', 'w', encoding='utf-8') as f:
            json.dump(results, f, ensure_ascii=False, indent=2)
    except Exception as e:
        print(f"Warning: could not write test_results.json: {e}\n")
    
    return results_df


def execute_tests_from_dataframe(df, agent_name, max_workers=5):
    """Execute tests from a pandas DataFrame instead of reading from CSV file"""
    # Initialize list to store results
    results = []
    
    # Process questions in parallel
    with concurrent.futures.ThreadPoolExecutor(max_workers=max_workers) as executor:
        # Create future objects for each question
        future_to_question = {
            executor.submit(process_question, row['Question'], row['Answer'], row['Filename'], agent_name, row.get('ValidationRules', '{}'), row.get('ValidationMode', 'hybrid')): index 
            for index, row in df.iterrows()
        }
        
        # Process completed futures as they finish
        for future in concurrent.futures.as_completed(future_to_question):
            index = future_to_question[future]
            try:
                result = future.result()
                if result:
                    # Add repeat information if available
                    if '_repeat_index' in df.iloc[index]:
                        result['RepeatIndex'] = df.iloc[index]['_repeat_index']
                        result['OriginalIndex'] = df.iloc[index]['_original_index']
                    result['Ordinal'] = index + 1
                    results.append(result)
                else:
                    # Get the original question from the DataFrame
                    original_question = df.iloc[index]['Question']
                    print(f"Failed to process question: {original_question}")
                    # Create a failed result entry
                    failed_result = {
                        'Question': original_question,
                        'Filename': df.iloc[index]['Filename'],
                        'Answer': df.iloc[index]['Answer'],
                        'AgentAnswer': 'Failed to process',
                        'ErrorOccured': 1,
                        'ErrorDetails': 'Test execution failed',
                        'Ordinal': index + 1
                    }
                    # Add repeat information if available
                    if '_repeat_index' in df.iloc[index]:
                        failed_result['RepeatIndex'] = df.iloc[index]['_repeat_index']
                        failed_result['OriginalIndex'] = df.iloc[index]['_original_index']
                    results.append(failed_result)
            except Exception as e:
                print(f"Error processing question at index {index}: {e}")
                # Create a failed result entry
                original_question = df.iloc[index]['Question']
                failed_result = {
                    'Question': original_question,
                    'Filename': df.iloc[index]['Filename'],
                    'Answer': df.iloc[index]['Answer'],
                    'AgentAnswer': 'Failed to process',
                    'ErrorOccured': 1,
                    'ErrorDetails': str(e),
                    'Ordinal': index + 1
                }
                # Add repeat information if available
                if '_repeat_index' in df.iloc[index]:
                    failed_result['RepeatIndex'] = df.iloc[index]['_repeat_index']
                    failed_result['OriginalIndex'] = df.iloc[index]['_original_index']
                results.append(failed_result)
    
    # Convert results to DataFrame
    if results:
        results_df = pd.DataFrame(results)
        return results_df
    else:
        print("No results to return")
        return None


if __name__ == "__main__":
    # Legacy execution - now handled by run_tests.py
    print("This script is now called through run_tests.py")
    print("Use: python run_tests.py --suite <suite> --agent <agent>")
    print("Example: python run_tests.py --suite code-interpreter --agent MAA-02 --quick")
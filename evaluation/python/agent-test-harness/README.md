# Overview
This harness provides a simple script for bulk testing a FoundationaLLM agent against a defined list of prompts provided in a CSV file, optionally including a file to upload as context for the prompt.

# Prerequisites
1. Visual Studio Code or Cursor 
2. Python 3.10 or later installed
3. An Agent in FLLM with an Access Token enabled

# Setup
1. Open Visual Studio Code
2. Within Visual Studio Code, open the folder where you extracted the file
3. In Visual Studio Code, select View-> Command Palette, then select Python: Create Environment.
4. Select Venv
5. Select Python 3.10 or later
6. In the terminal, run `pip install -r requirements.txt`

# Test Data
1. Create a folder called uploads in the directory containing this README.md.
2. To the uploads folder, add any files you will use for testing completions from uploaded files.
3. Create a CSV file called `TestQuestions.csv` in the same directory as this README.md. It should have the following columns:
Question: The user prompt you want to submit to the agent.
Filename: The name of a file you want to submit with the completion. Should be just the filename, no path.
Answer: The expected answer, if any. This field will be passed thru so you can evaluate agent performance later.

For example:
```
Question,Filename,Answer
Who is the hero of the story?,curious_cat_story.pdf, Whiskers
What are the key points in this book?,
```

# Preparing to Run Tests
1. Open test_harness.py 
2. Change TEST_FILE (line 12) to have the value of the CSV you want to use a question source.
3. Change AGENT_NAME (line 13) to have the name of agent you want to test.
4. Copy the sample.env and rename it to .env.
5. Open the .env file. 
6. Set the value of the FLLM_ACCESS_TOKEN to the access token for the agent you want to test. It should have a value that starts with "keya." and ends in ".ayek".
7. Set the value of the FLLM_ENDPOINT to the URI for core API. It should have a value like:
https://cacoreapil42jljq2i5ox6.somewords-a0804c39.eastus2.azurecontainerapps.io/instances/8ad6074c-cdde-43cb-a140-ec0002d96d2b/

8. (Optional) To enable prompt management operations (list/get/update) via the Management API, also set:
   - `FLLM_MGMT_ENDPOINT` to the Management API base URL including `/instances/{instanceId}/` and a trailing slash.
   - `FLLM_MGMT_BEARER_TOKEN` to a bearer token with scope `api://FoundationaLLM-Management/Data.Manage`.

   Example (PowerShell):
   ```powershell
   az login
   az account set --subscription "<SUBSCRIPTION_NAME_OR_ID>"
   $env:FLLM_MGMT_BEARER_TOKEN = az account get-access-token --scope api://FoundationaLLM-Management/Data.Manage --query accessToken -o tsv
   ```

# Run Tests
1. Within Visual Studio Code, open test_harness.py.
2. Right click anywhere with that file and select Run Python->Run Python File in Terminal
3. Watch the terminal as the tests execute.
4. When the tests complete a test_results.csv file will be created in directory next to the test_harness.py file.

# Prompt Management (Optional)
If `FLLM_MGMT_ENDPOINT` and `FLLM_MGMT_BEARER_TOKEN` are set, the harness exposes helper functions:

- `list_prompts()` – returns all prompts you can access.
- `get_prompt(prompt_name)` – returns the prompt resource with its `prefix`/`suffix` (for multipart prompts).
- `upsert_prompt(prompt_object)` – creates/updates a prompt. For multipart prompts, use:
```
{
  "type": "multipart",
  "name": "<prompt-name>",
  "category": "System", // optional
  "prefix": "...",
  "suffix": "..."
}
```

Note: These calls require Management API RBAC and a valid bearer token; they are independent from the Core API agent access token.


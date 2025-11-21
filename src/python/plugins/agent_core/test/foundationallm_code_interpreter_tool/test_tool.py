"""
Test for FoundationaLLM Code Interpreter Tool.
"""
import asyncio
import json
import os
import sys
import uuid

sys.path.append('src')

from foundationallm_agent_plugins import FoundationaLLMAgentToolPluginManager
from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.agents import KnowledgeManagementCompletionRequest

from foundationallm_agent_plugins.tools.foundationallm_code_interpreter_tool import FoundationaLLMCodeInterpreterFile

operation_id = str(uuid.uuid4())
user_identity_json = {"name": "Experimental Test", "user_name":"sw@foundationaLLM.ai","upn":"sw@foundationaLLM.ai"}
full_request_json_file_name = 'test/full_request_with_files.json' # full original langchain request, contains agent, tools, exploded objects
print(os.environ['FOUNDATIONALLM_APP_CONFIGURATION_URI'])

user_identity = UserIdentity.from_json(user_identity_json)
config = Configuration()

with open(full_request_json_file_name, 'r') as f:
    request_json = json.load(f)

request = KnowledgeManagementCompletionRequest(**request_json)
agent = request.agent
objects = request.objects
workflow = request.agent.workflow
message_history = request.message_history
file_history = request.file_history
agent_tool = request.agent.tools[1] # Code Interpreter Tool is the second tool in the request

foundationallmagent_tool_plugin_manager = FoundationaLLMAgentToolPluginManager()
# The AgentTool has the configured description the LLM will use to make a tool choice.
code_interpreter_tool = foundationallmagent_tool_plugin_manager.create_tool(agent_tool, objects, user_identity, config)

#-------------------------------------------------------------------------------
# Direct tool invocation
files = [
    FoundationaLLMCodeInterpreterFile(
        path='FoundationaLLM.Attachment/a-5351de53-af13-4707-8f95-b6bccb619dc0-638778425064603831.py', 
        original_file_name='test.py'
    )
]
code = """
import subprocess

try:
    result = subprocess.run(['python', '/mnt/data/test.py'], capture_output=True, text=True)
    print("Output:", result.stdout)
    if result.stderr:
        print("Errors:", result.stderr)
except Exception as e:
    print(f"Error executing script: {e}")
"""
response, content_artifacts = asyncio.run(code_interpreter_tool._arun(code, files=files))
print("**** RESPONSE ****")
print(response)
print("**** CONTENT ARTIFACTS ****")
print(content_artifacts)
print("DONE")
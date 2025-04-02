"""
Test for FoundationaLLM Azure OpenAI Router Workflow.
"""
import asyncio
import json
import sys
import uuid

sys.path.append('src')
from foundationallm_agent_plugins import (
    FoundationaLLMAgentToolPluginManager,
    FoundationaLLMAgentWorkflowPluginManager
)
from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.agents import KnowledgeManagementCompletionRequest
from foundationallm.models.constants import ContentArtifactTypeNames
#user_prompt = "What does this file do?"
#user_prompt = "Generate a graph of y=mx+b where m=2 and b=3 and create a PDF with the graph along with text explaining the graph"
#user_prompt = "Generate a PDF document with the title 'Test' and the content 'This is a test'"
#user_prompt = "Generate an interactive graph of y=mx+b where m=2 and b=3"
#user_prompt = "Generate a graph of y=mx+b where m=2 and b=3"
#user_prompt = "Generate a PDF with the text 'Hello World'"
#user_prompt = "What is the average of 42 plus 84 plus 168. Do your calculations in Python and show your work. Also Create a bar chart of the aforementioned numbers showing the average line on that bar chart."
#user_prompt = "Generate a graph based on this data."
user_prompt = "Give me 5 examples of Geography"
operation_id = str(uuid.uuid4())

user_identity_json = {"name": "Experimental Test", "user_name":"carey@foundationaLLM.ai","upn":"carey@foundationaLLM.ai"}
full_request_json_file_name = 'test/full_request_sql.json' # full original langchain request, contains agent, tools, exploded objects

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
user_prompt_rewrite = request.user_prompt_rewrite

workflow_plugin_manager = FoundationaLLMAgentWorkflowPluginManager()
tool_plugin_manager = FoundationaLLMAgentToolPluginManager()

# prepare tools
tools = []
parsed_user_prompt = user_prompt

explicit_tool = next((tool for tool in agent.tools if parsed_user_prompt.startswith(f'[{tool.name}]:')), None)
if explicit_tool is not None:
    tools.append(tool_plugin_manager.create_tool(explicit_tool, objects, user_identity, config))
    parsed_user_prompt = parsed_user_prompt.split(':', 1)[1].strip()
else:
    # Populate tools list from agent configuration
    for tool in agent.tools:
        if tool.package_name == 'foundationallm_agent_plugins':
            tools.append(tool_plugin_manager.create_tool(tool, objects, user_identity, config))

# create the workflow
workflow = workflow_plugin_manager.create_workflow(
    agent.workflow,
    objects,
    tools,
    user_identity,
    config
)
response = asyncio.run(
    workflow.invoke_async(
        operation_id=operation_id,
        user_prompt=parsed_user_prompt,
        user_prompt_rewrite=user_prompt_rewrite,
        message_history=message_history,
        file_history=file_history
    )
)
print("++++++++++++++++++++++++++++++++++++++")
print('File content artifacts:')
for content_artifact in response.content_artifacts:
    if content_artifact.type == ContentArtifactTypeNames.FILE:
        print(content_artifact.filepath)
print("++++++++++++++++++++++++++++++++++++++")

print("*********************************")
print(response.content)
print("*********************************")

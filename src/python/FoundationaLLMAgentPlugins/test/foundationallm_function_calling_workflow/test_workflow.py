"""
Test for FoundationaLLM Azure OpenAI Router Workflow.
"""
import asyncio
import json
import os
import sys
import uuid

sys.path.append('src')
from foundationallm.operations.operations_manager import OperationsManager
from foundationallm_agent_plugins import (
    FoundationaLLMAgentToolPluginManager,
    FoundationaLLMAgentWorkflowPluginManager
)
from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.agents import KnowledgeManagementCompletionRequest
from foundationallm.models.constants import (
    ResourceObjectIdPropertyNames,
    ResourceObjectIdPropertyValues,
    ResourceProviderNames,
    PromptResourceTypeNames
)
#user_prompt = "What does this file do?"
#user_prompt = "Generate a graph of y=mx+b where m=2 and b=3 and create a PDF with the graph along with text explaining the graph"
#user_prompt = "Generate a PDF document with the title 'Test' and the content 'This is a test'"
#user_prompt = "Generate an interactive graph of y=mx+b where m=2 and b=3"
# user_prompt = "Generate a graph of y=mx+b where m=2 and b=3"
#user_prompt = "Generate a PDF with the text 'Hello World'"
#user_prompt = "What is the average of 42 plus 84 plus 168. Do your calculations in Python and show your work. Also Create a bar chart of the aforementioned numbers showing the average line on that bar chart."
#user_prompt = "Generate a graph based on this data."
# user_prompt = "how do I beat the market"
# user_prompt = "Who is the hero of the story?"
# user_prompt = "Who are you?"
# user_prompt = "Summarize the file"
# user_prompt = "Calculate the first 20 terms of the Fibonacci series"
# user_prompt = "Calculate the first 20 terms of the Fibonacci series and save the result as a text file."
# user_prompt = "Generate a graph of a complex Fourier transformation"
# user_prompt = "Create a chart based on the uploaded file"
# user_prompt = "Who are you?"
# user_prompt = "What files are in the uploaded zip?"
# user_prompt = "List the files in the uploaded zip file. Use a code tool to provide the answer."
# user_prompt = "Plot the equation y=mx^2+b"
# user_prompt = "Which are the most important ideas from the document named Selling-Guide_02-05-25_highlighted.pdf?"
# user_prompt = "Do you recognize the band?"
# user_prompt = "Best practices for trading."
# user_prompt = "Based on the most important recommendations for borrowers from the document named Selling-Guide_02-05-25_highlighted.pdf, determine whether the document is compliant or not."
# user_prompt = "What is considered acceptable income?"
# user_prompt = "Describe the main project goals"
# user_prompt = "What are the latest project health ratings?"

# user_identity_json = {
#     "name": "Experimental Test",
#     "user_name":"ciprian@foundationaLLM.ai",
#     "upn":"ciprian@foundationaLLM.ai",
#     "user_id": "949195b1-f432-4da3-8f7d-5298e3fda432",
#     "group_ids": ["c54871ba-1fa1-439a-9e86-30d74dfe4a4a"]}

user_identity_json = {
    "name": "Experimental Test",
    "user_name":"fllm-labuser-60@foundationaLLM.ai",
    "upn":"fllm-labuser-60@foundationaLLM.ai",
    "user_id": "5aece9e4-1bfb-4e6e-a570-e5eb68fa63ef",
    "group_ids": []}

full_request_json_file_name = 'test/full_request.json' # full original langchain request, contains agent, tools, exploded objects

user_identity = UserIdentity.from_json(user_identity_json)
config = Configuration()

with open(full_request_json_file_name, 'r') as f:
    request_json = json.load(f)

operation_id = request_json['operation_id']
conversation_id = request_json['session_id']
user_prompt = request_json['user_prompt']

request = KnowledgeManagementCompletionRequest(**request_json)
agent = request.agent
objects = request.objects
workflow = request.agent.workflow
message_history = request.message_history
file_history = request.file_history
user_prompt_rewrite = request.user_prompt_rewrite

files_prompt_file_path = 'test/files_prompt.txt'
if os.path.exists(files_prompt_file_path):
    prompt_object_id = request.agent.workflow.get_resource_object_id_properties(
                ResourceProviderNames.FOUNDATIONALLM_PROMPT,
                PromptResourceTypeNames.PROMPTS,
                ResourceObjectIdPropertyNames.OBJECT_ROLE,
                ResourceObjectIdPropertyValues.FILES_PROMPT
            )
    with open(files_prompt_file_path, 'r') as f:
        request.objects[prompt_object_id.object_id]['prefix'] = f.read()

code_tool_prompt_file_path = 'test/code_tool_prompt.txt'
if os.path.exists(code_tool_prompt_file_path):
    code_tool = (next((tool for tool in request.agent.tools if tool.name == 'Code'), None))
    if code_tool is not None:
        prompt_object_id = code_tool.get_resource_object_id_properties(
                    ResourceProviderNames.FOUNDATIONALLM_PROMPT,
                    PromptResourceTypeNames.PROMPTS,
                    ResourceObjectIdPropertyNames.OBJECT_ROLE,
                    ResourceObjectIdPropertyValues.MAIN_PROMPT
                )
        with open(code_tool_prompt_file_path, 'r') as f:
            request.objects[prompt_object_id.object_id]['prefix'] = f.read()

workflow_plugin_manager = FoundationaLLMAgentWorkflowPluginManager()
tool_plugin_manager = FoundationaLLMAgentToolPluginManager()
operations_manager = OperationsManager(config)

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
    operations_manager,
    user_identity,
    config
)
response = asyncio.run(
    workflow.invoke_async(
        operation_id=operation_id,
        user_prompt=parsed_user_prompt,
        user_prompt_rewrite=user_prompt_rewrite,
        message_history=message_history,
        file_history=file_history,
        conversation_id=conversation_id,
        objects=objects
    )
)
print("++++++++++++++++++++++++++++++++++++++")
print('Content artifacts:')
print(response.content_artifacts)
print("++++++++++++++++++++++++++++++++++++++")

print("*********************************")
print(response.content)
print("*********************************")

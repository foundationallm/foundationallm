# Ensure the terminal working directory is FoundationaLLMAgentPlugins.
# Note: This tests a tool directly and ignores the workflow.
import asyncio
import json
import os
import sys
from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.agents import KnowledgeManagementCompletionRequest

# import pandas as pd
# df = pd.read_json('.\\test\\ProductCatalog.json')
# df.to_parquet('.\\test\\ProductCatalog.parquet', index=False)

sys.path.append('src')
from foundationallm_agent_plugins import FoundationaLLMAgentToolPluginManager # type: ignore

user_identity_json = {"name": "Experimental Test", "user_name":"sw@foundationaLLM.ai","upn":"sw@foundationaLLM.ai"}
full_request_json_file_name = 'test/full_request_file_analysis.json' # full original langchain request, contains agent, tools, exploded objects
print(os.environ['FOUNDATIONALLM_APP_CONFIGURATION_URI'])

user_identity = UserIdentity.from_json(user_identity_json)
config = Configuration()

with open(full_request_json_file_name, 'r') as f:
    request_json = json.load(f)

request = KnowledgeManagementCompletionRequest(**request_json)
agent = request.agent
agent_tool = request.agent.tools[0]
exploded_objects_json = request.objects
workflow = request.agent.workflow

foundationallmagent_tool_plugin_manager = FoundationaLLMAgentToolPluginManager()
# The AgentTool has the configured description the LLM will use to make a tool choice.
file_analysis_tool = foundationallmagent_tool_plugin_manager.create_tool(agent_tool, exploded_objects_json, user_identity, config)

#-------------------------------------------------------------------------------
# Direct tool invocation
response, content_artifacts = asyncio.run(file_analysis_tool._arun(prompt='What is the average price of products of type Jackets?'))
print("**** RESPONSE ****")
print(response)
print("**** CONTENT ARTIFACTS ****")
print(content_artifacts)
print("DONE")

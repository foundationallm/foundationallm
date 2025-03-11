"""
Test for FoundationaLLM Azure OpenAI Router Workflow.
"""

import asyncio
import json

from foundationallm_agent import (
    FoundationaLLMAgentToolPluginManager,
    FoundationaLLMAgentWorkflowPluginManager
)

from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.agents import AgentTool, ExternalAgentWorkflow


configuration = Configuration()
user_identity_json = {
    "name": "Ciprian",
    "user_name":"ciprian@foundationaLLM.ai",
    "upn":"ciprian@foundationaLLM.ai"
}
user_identity = UserIdentity.from_json(user_identity_json)

with open('structured_analysis_agent_workflow.json', 'r', encoding='utf-8') as f:
    workflow_config_json = json.load(f)
    workflow_config = ExternalAgentWorkflow(**workflow_config_json)

with open('structured_analysis_agent_exploded_objects.json', 'r', encoding='utf-8') as f:
    exploded_objects = json.load(f)

with open('structured_analysis_agent_tool.json', 'r', encoding='utf-8') as f:
    tool_config_json = json.load(f)
    tool_config = AgentTool(**tool_config_json)

workflow_plugin_manager = FoundationaLLMAgentWorkflowPluginManager()
tool_plugin_manager = FoundationaLLMAgentToolPluginManager()

agent_tool = tool_plugin_manager.create_tool(
    tool_config,
    exploded_objects,
    user_identity,
    configuration
)

agent_workflow = workflow_plugin_manager.create_workflow(
    workflow_config,
    exploded_objects,
    [agent_tool],
    user_identity,
    configuration
)

message_history = []

response = asyncio.run(agent_workflow.invoke_async(
    '1',
    'Give me the sales count for Ciprian',
    message_history
))

print(response)
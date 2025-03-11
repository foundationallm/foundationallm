"""
Class: FoundationaLLMAgentRouterWorkflow
Description: FoundationaLLM workflow implementing a router pattern for tool invocation
using Azure OpenAI completion models.
"""
import asyncio
import json
import re
import time

from typing import Any, Dict, List, Optional
from azure.identity import (
    DefaultAzureCredential,
    get_bearer_token_provider
)
from langchain_core.messages import (
    BaseMessage,
    SystemMessage,
    HumanMessage
)
from langgraph.prebuilt import create_react_agent
from opentelemetry.trace import SpanKind

from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import (
    FoundationaLLMWorkflowBase,
    FoundationaLLMToolBase
)
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.models.agents import ExternalAgentWorkflow
from foundationallm.models.authentication import AuthenticationTypes
from foundationallm.models.constants import (
    AgentCapabilityCategories,
    ResourceObjectIdPropertyNames,
    ResourceObjectIdPropertyValues,
    ResourceProviderNames,
    AIModelResourceTypeNames,
    PromptResourceTypeNames
)
from foundationallm.models.orchestration import (
    CompletionResponse,
    ContentArtifact,
    OpenAITextMessageContentItem
)
from foundationallm.telemetry import Telemetry
from foundationallm_agent_plugins.common.constants import (
    CONTENT_ARTIFACT_TYPE_TOOL_EXECUTION,
    CONTENT_ARTIFACT_TYPE_TOOL_ERROR,
    CONTENT_ARTIFACT_TYPE_WORKFLOW_EXECUTION
)

class FoundationaLLMAgentRouterWorkflow(FoundationaLLMWorkflowBase):
    """
    FoundationaLLM workflow implementing a router pattern for tool invocation
    using Azure OpenAI completion models.
    """

    def __init__(self,
                 workflow_config: ExternalAgentWorkflow,
                 objects: Dict,
                 tools: List[FoundationaLLMToolBase],
                 user_identity: UserIdentity,
                 config: Configuration):
        """
        Initializes the FoundationaLLMWorkflowBase class with the workflow configuration.

        Parameters
        ----------
        workflow_config : ExternalAgentWorkflow
            The workflow assigned to the agent.
        objects : dict
            The exploded objects assigned from the agent.
        tools : List[FoundationaLLMToolBase]
            The tools assigned to the agent.
        user_identity : UserIdentity
            The user identity of the user initiating the request.
        config : Configuration
            The application configuration for FoundationaLLM.
        """
        super().__init__(workflow_config, objects, tools, user_identity, config)
        self.name = workflow_config.name
        self.logger : Any = Telemetry.get_logger(self.name)
        self.tracer : Any = Telemetry.get_tracer(self.name)
        #self.default_credential = DefaultAzureCredential(exclude_environment_credential=True)
        self.default_error_message = workflow_config.properties.get(
            'default_error_message',
            'An error occurred while processing the request.') \
            if workflow_config.properties else 'An error occurred while processing the request.'
        
        # Create prompt first, then LLM
        self.__create_workflow_prompt()
        self.__create_workflow_llm()        

    async def invoke_async(self,
                           operation_id: str,
                           user_prompt:str,
                           user_prompt_rewrite: Optional[str],
                           message_history: List[BaseMessage])-> CompletionResponse:
        """
        Invokes the workflow asynchronously.

        Parameters
        ----------
        operation_id : str
            The unique identifier of the FoundationaLLM operation.
        user_prompt : str
            The user prompt message.
        message_history : List[BaseMessage]
            The message history.
        """

        messages = [
            SystemMessage(content=self.workflow_prompt),
            *message_history,
            HumanMessage(content=user_prompt)
        ]

        tool_responses = ''
        content_artifacts = []
        completion_tokens = 0
        prompt_tokens = 0
        response_content = None

        with self.tracer.start_as_current_span(f'{self.name}_workflow', kind=SpanKind.INTERNAL):
            #try:
                with self.tracer.start_as_current_span(f'{self.name}_workflow_llm_call', kind=SpanKind.INTERNAL):
                    router_start_time = time.time()
                    graph = create_react_agent(self.workflow_llm, tools=self.tools, state_modifier=self.workflow_prompt)
                    result = await graph.ainvoke(
                        {'messages': messages},
                        config={"configurable": 
                                    {"original_user_prompt": user_prompt, 
                                    **({"recursion_limit": self.workflow_config.graph_recursion_limit} 
                                       if hasattr(self.workflow_config, 'graph_recursion_limit') and self.workflow_config.graph_recursion_limit is not None 
                                       else {})
                                    }
                                }
                    )
                    router_end_time = time.time()
                final_message = result["messages"][-1]
                workflow_content_artifact = self.__create_workflow_execution_content_artifact(
                    user_prompt,
                    final_message.content,
                    final_message.usage_metadata['input_tokens'],
                    final_message.usage_metadata['output_tokens'],
                    router_end_time - router_start_time)
                content_artifacts.append(workflow_content_artifact)
                
                # Initialize response_content with the result
                response_content = OpenAITextMessageContentItem(
                    value=final_message.content,
                    agent_capability_category=AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
                )

            #    if (user_prompt.startswith('(intent):')):
            #        response_content = OpenAITextMessageContentItem(
            #            value=result.content,
            #            agent_capability_category=AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
            #        )
            #    else:                
            #        processed_intents = self.__process_intents(result.content)
            #        tools_start_time = time.time()
            #        tool_results = await self.__invoke_tools_async(
            #            processed_intents,
            #            user_prompt,
            #            message_history
            #        )
            #        tools_end_time = time.time()
            #        workflow_content_artifact.metadata['tools_execution_time_seconds'] = str(tools_end_time - tools_start_time)
#
            #        for tool_result in tool_results:
            #            if tool_result[0]:
            #                tool_responses += tool_result[0]
            #            if tool_result[1]:
            #                for content_artifact in tool_result[1]:
            #                    content_artifacts.append(content_artifact)
#
            #        tool_errors = False
            #        for content_artifact in content_artifacts:
            #            if content_artifact.type == CONTENT_ARTIFACT_TYPE_TOOL_ERROR:
            #                tool_errors = True
            #            if (content_artifact.type == CONTENT_ARTIFACT_TYPE_TOOL_EXECUTION) \
            #                or (content_artifact.type == CONTENT_ARTIFACT_TYPE_WORKFLOW_EXECUTION):
            #                prompt_tokens += int(content_artifact.metadata.get('prompt_tokens', 0))
            #                completion_tokens += int(content_artifact.metadata.get('completion_tokens', 0))
#
            #        response_content = OpenAITextMessageContentItem(
            #                value=self.default_error_message,
            #                agent_capability_category=AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
            #            ) if tool_errors or (tool_responses == '') else OpenAITextMessageContentItem(
            #                value=tool_responses,
            #                agent_capability_category=AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
            #            )

            # except Exception as ex:
            #    self.logger.error('Error invoking workflow for user prompt [%s]: %s', user_prompt, ex)
            #    response_content = OpenAITextMessageContentItem(
            #        value=self.default_error_message,
            #        agent_capability_category=AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
            #    )

                return CompletionResponse(
                    operation_id=operation_id,
                    content=[response_content],
                    content_artifacts=content_artifacts,
                    user_prompt=user_prompt,
                    full_prompt='',
                    completion_tokens=completion_tokens,
                    prompt_tokens=prompt_tokens,
                    total_tokens=prompt_tokens + completion_tokens,
                    total_cost=0
                )

    def __create_workflow_llm(self):
        """ Creates the workflow LLM instance and saves it to self.workflow_llm. """
        self.logger.debug("Creating workflow LLM...")
        self.logger.debug(f"Available objects: {list(self.objects.keys())}")
        language_model_factory = LanguageModelFactory(self.objects, self.config)        
        model_object_id = self.workflow_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_AIMODEL,
            AIModelResourceTypeNames.AI_MODELS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.MAIN_MODEL
        )
        self.logger.debug(f"Got model object ID: {model_object_id}")
        if model_object_id:
            self.logger.debug(f"Creating language model with object ID: {model_object_id.object_id}")
            self.workflow_llm = language_model_factory.get_language_model(model_object_id.object_id)
            self.logger.debug("Language model created successfully")
        else:
            error_msg = "No main model found in workflow configuration"
            self.logger.error(error_msg)
            raise ValueError(error_msg)

    def __create_workflow_prompt(self):
        """ Creates the workflow prompt instance and saves it to self.workflow_prompt. """
        main_prompt = ""
        prompt_object_id = self.workflow_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_PROMPT,
            PromptResourceTypeNames.PROMPTS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.MAIN_PROMPT
        )       
        if prompt_object_id:
            main_prompt_object_id = prompt_object_id.object_id
            main_prompt_properties = self.objects[main_prompt_object_id]
            main_prompt += main_prompt_properties['prefix']

        # Check if the router prompt exists
        router_prompt_object_id = self.workflow_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_PROMPT,
            PromptResourceTypeNames.PROMPTS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            "router_prompt"
        )
        if router_prompt_object_id:
            router_prompt_object_id = router_prompt_object_id.object_id
            router_prompt_properties = self.objects[router_prompt_object_id]
            router_prompt =router_prompt_properties['prefix']            
            main_prompt = re.sub(r"{{foundationallm:router_prompt}}", router_prompt, main_prompt)

        # Iterate through the tools add their main_prompt or if that's missing, add the description
        tool_descriptions = ""
        for tool in self.tools:
            tool_main_prompt_object_id = tool.tool_config.get_resource_object_id_properties(
                ResourceProviderNames.FOUNDATIONALLM_PROMPT,
                PromptResourceTypeNames.PROMPTS,
                ResourceObjectIdPropertyNames.OBJECT_ROLE,
                "main_prompt"
            )           
            if tool_main_prompt_object_id:
                tool_main_prompt_object_id = tool_main_prompt_object_id.object_id
                tool_main_prompt_properties = self.objects[tool_main_prompt_object_id]
                tool_descriptions += tool_main_prompt_properties['prefix'] + "\n"
            else:
                # if no main_prompt exists, add the tool's description
                tool_descriptions += f"- {tool.name}: {tool.description}\n"

        main_prompt = re.sub(r"{{foundationallm:tools}}", tool_descriptions, main_prompt)
        print("****************************")
        print(main_prompt)
        print("****************************")
        self.workflow_prompt = main_prompt

    def __create_workflow_execution_content_artifact(
            self,
            original_prompt: str,
            intent: str,
            prompt_tokens: int = 0,
            completion_tokens: int = 0,
            completion_time_seconds: float = 0) -> ContentArtifact:

        content_artifact = ContentArtifact(id=self.workflow_config.name)
        content_artifact.source = self.workflow_config.name
        content_artifact.type = CONTENT_ARTIFACT_TYPE_WORKFLOW_EXECUTION
        content_artifact.content = original_prompt
        content_artifact.title = self.workflow_config.name
        content_artifact.filepath = None
        content_artifact.metadata = {
            'intent': intent,
            'prompt_tokens': str(prompt_tokens),
            'completion_tokens': str(completion_tokens),
            'completion_time_seconds': str(completion_time_seconds)
        }
        return content_artifact

#    def __process_intents(self, raw_intents: str) -> list:
#
#        if raw_intents.startswith("```json"):
#            raw_intents = raw_intents[7:]
#        if raw_intents.startswith("```"):
#            raw_intents = raw_intents[3:]
#        if raw_intents.endswith("```"):
#            raw_intents = raw_intents[:-3]
#
#        try:
#            intents = json.loads(raw_intents)
#            if not isinstance(intents, list):
#                intents = []
#        except Exception:
#            intents = []
#
#        valid_intents = [intent for intent in intents if intent.get('recommendedTool')]
#
#        if len(valid_intents) != len(intents):
#            self.logger.warning('Invalid intents detected in response: %s', raw_intents)
#
#        if len(valid_intents) == 0:
#            raise Exception('No valid intents detected in router response.')
#
#        return valid_intents
#
#    async def __invoke_tools_async(
#            self,
#            intents: List,
#            user_prompt: str,
#            message_history: List[BaseMessage]
#    ) -> List:
#
#        tool_intents = {}
#
#        for intent in intents:
#            if intent['recommendedTool'] in tool_intents:
#                tool_intents[intent['recommendedTool']].append(intent)
#            else:
#                tool_intents[intent['recommendedTool']] = [intent]
#
#        tool_invocations = [t.invoke_with_intents_async(
#            user_prompt,
#            tool_intents[t.name],
#            message_history
#        ) for t in self.tools if t.name in tool_intents]
#
#        tool_results = await asyncio.gather(*tool_invocations)
#
#        return tool_results
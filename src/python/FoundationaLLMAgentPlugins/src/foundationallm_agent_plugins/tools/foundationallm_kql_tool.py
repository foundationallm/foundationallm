"""
Implements the FoundationaLLM KQL (Kusto Query Language) tool.
"""

# Platform imports
from typing import List, Dict
import json
import pandas as pd
import requests

#Azure imports
from azure.identity import DefaultAzureCredential

# LangChain imports
from langchain_core.messages import (
    BaseMessage,
    SystemMessage,
    HumanMessage,
    AIMessage
)
from langchain_core.runnables import RunnableConfig

from opentelemetry.trace import SpanKind

# FoundationaLLM imports
from foundationallm.langchain.common import FoundationaLLMToolBase
from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.agents import AgentTool
from foundationallm.models.constants import RunnableConfigKeys

class FoundationaLLMKQLTool(FoundationaLLMToolBase):
    """
    Provides an implementation for the FoundationaLLM KQL (Kusto Query Language) tool.
    """

    def __init__(
        self,
        tool_config: AgentTool,
        objects: Dict,
        user_identity:UserIdentity,
        config: Configuration):
        """ Initializes the FoundationaLLMKQLTool class with the tool configuration,
            exploded objects collection, user_identity, and platform configuration. """

        super().__init__(tool_config, objects, user_identity, config)

        self.__setup_kql_configuration(tool_config, config)

    def _run(
        self,
        *args,
        **kwargs
        ) -> str:

        raise NotImplementedError()

    async def _arun(
        self,
        *args,
        prompt: str = None,
        message_history: List[BaseMessage] = None,
        runnable_config: RunnableConfig = None,
        **kwargs,
        ) -> str:

        prompt_tokens = 0
        completion_tokens = 0
        generated_sql_query = ''
        final_response = ''

        # Get the original prompt
        if runnable_config is None:
            original_prompt = prompt
        else:
            original_prompt = runnable_config['configurable'][RunnableConfigKeys.ORIGINAL_USER_PROMPT]

        messages = [
            SystemMessage(content=self.main_prompt),
            *message_history,
            HumanMessage(content=original_prompt)
        ]

        with self.tracer.start_as_current_span(self.name, kind=SpanKind.INTERNAL):
            try:

                with self.tracer.start_as_current_span(f'{self.name}_initial_llm_call', kind=SpanKind.INTERNAL):

                    response = await self.main_llm.ainvoke(messages, tools=self.tools)

                    completion_tokens += response.usage_metadata['input_tokens']
                    prompt_tokens += response.usage_metadata['output_tokens']

                if response.tool_calls \
                    and response.tool_calls[0]['name'] == 'query_azure_sql':

                    tool_call = response.tool_calls[0]

                    with self.tracer.start_as_current_span(f'{self.name}_tool_call', kind=SpanKind.INTERNAL) as tool_call_span:
                        tool_call_span.set_attribute("tool_call_id", tool_call['id'])
                        tool_call_span.set_attribute("tool_call_function", tool_call['name'])

                        function_name = tool_call['name']
                        function_to_call = self.available_sql_functions[function_name]
                        function_args = tool_call['args']
                        if 'query' in function_args:
                            generated_sql_query = function_args['query']

                        function_response = function_to_call(**function_args)

                    final_messages = [
                        SystemMessage(content=self.final_system_prompt),
                        HumanMessage(content=original_prompt),
                        AIMessage(content=function_response)
                    ]

                    with self.tracer.start_as_current_span(f'{self.name}_final_llm_call', kind=SpanKind.INTERNAL):

                        final_llm_response = await self.main_llm.ainvoke(final_messages, tools=None)

                        completion_tokens += final_llm_response.usage_metadata['input_tokens']
                        prompt_tokens += final_llm_response.usage_metadata['output_tokens']
                        final_response = final_llm_response.content

                return final_response, \
                    [
                        self.create_tool_content_artifact(
                            original_prompt,
                            generated_sql_query,
                            prompt_tokens,
                            completion_tokens
                        )
                    ]

            except Exception as e:
                self.logger.error('An error occured in tool %s: %s', self.name, e)
                return self.default_error_message, \
                    [
                        self.create_tool_content_artifact(
                            original_prompt,
                            prompt,
                            prompt_tokens,
                            completion_tokens
                        ),
                        self.create_error_content_artifact(
                            original_prompt,
                            e
                        )
                    ]

    def __setup_kql_configuration(
            self,
            tool_config: AgentTool,
            config: Configuration,
    ):

        self.kusto_query_endpoint = tool_config.properties['kusto_query_endpoint']
        self.kusto_database = tool_config.properties['kusto_database']

        credential = DefaultAzureCredential()
        self.kusto_token = credential.get_token("https://api.kusto.windows.net")


    def execute_kql_query(self, query: str) -> str:
        """Run a KQL query against the Fabric Kusto query endpoint."""

        try:
            headers = {}
            headers["Content-Type"] = "application/json"
            headers["Authorization"] = "Bearer " + self.kusto_token.token

            with requests.Session() as session:
                session.headers.update(headers)
                url = self.kusto_query_endpoint + "/v1/rest/query"
                response = session.get(url, timeout=120, verify=False)
                response.raise_for_status()
                return response.json()
        except requests.HTTPError as e:
            self.logger.error("Error occurred while executing a KQL query. Error: %s; Query: %s", e, query)

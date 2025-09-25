"""
Implements the FoundationaLLM KQL (Kusto Query Language) tool.
"""

# Platform imports
from typing import List, Dict, Tuple
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
from foundationallm.langchain.common import (
    FoundationaLLMToolBase,
    FoundationaLLMToolResult
)
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

        self.main_llm = self.get_main_language_model()
        self.main_prompt = self.get_main_prompt()
        self.final_prompt = self.get_prompt("final_prompt")
        self.default_error_message = "An error occurred while executing the KQL query."
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
        message_history: List[BaseMessage] = [],
        runnable_config: RunnableConfig = None,
        **kwargs,
        ) -> Tuple[str, FoundationaLLMToolResult]:

        output_tokens = 0
        input_tokens = 0
        generated_query = ''
        final_response = ''

        # Get the original prompt
        if runnable_config is None:
            original_prompt = prompt
        else:
            user_prompt = runnable_config['configurable'][RunnableConfigKeys.ORIGINAL_USER_PROMPT] \
                if RunnableConfigKeys.ORIGINAL_USER_PROMPT in runnable_config['configurable'] \
                else None
            user_prompt_rewrite = runnable_config['configurable'][RunnableConfigKeys.ORIGINAL_USER_PROMPT_REWRITE] \
                if RunnableConfigKeys.ORIGINAL_USER_PROMPT_REWRITE in runnable_config['configurable'] \
                else None
            original_prompt = user_prompt_rewrite or user_prompt or prompt

        messages = [
            SystemMessage(content=self.main_prompt),
            *message_history,
            HumanMessage(content=original_prompt)
        ]

        with self.tracer.start_as_current_span(self.name, kind=SpanKind.INTERNAL):
            try:

                with self.tracer.start_as_current_span(f'{self.name}_initial_llm_call', kind=SpanKind.INTERNAL):

                    response = await self.main_llm.ainvoke(messages, tools=self.tools)

                    input_tokens += response.usage_metadata['input_tokens']
                    output_tokens += response.usage_metadata['output_tokens']

                if response.tool_calls \
                    and response.tool_calls[0]['name'] == 'query_kql':

                    tool_call = response.tool_calls[0]

                    with self.tracer.start_as_current_span(f'{self.name}_tool_call', kind=SpanKind.INTERNAL) as tool_call_span:
                        tool_call_span.set_attribute("tool_call_id", tool_call['id'])
                        tool_call_span.set_attribute("tool_call_function", tool_call['name'])

                        function_name = tool_call['name']
                        function_to_call = self.available_sql_functions[function_name]
                        function_args = tool_call['args']
                        if 'query' in function_args:
                            generated_query = function_args['query']

                        function_response = function_to_call(**function_args)

                    final_messages = [
                        SystemMessage(content=self.final_prompt),
                        HumanMessage(content=original_prompt),
                        AIMessage(content=json.dumps(function_response))
                    ]

                    with self.tracer.start_as_current_span(f'{self.name}_final_llm_call', kind=SpanKind.INTERNAL):

                        final_llm_response = await self.main_llm.ainvoke(final_messages)

                        input_tokens += final_llm_response.usage_metadata['input_tokens']
                        output_tokens += final_llm_response.usage_metadata['output_tokens']
                        final_response = final_llm_response.content

                return final_response, FoundationaLLMToolResult(
                    content=final_response,
                    content_artifacts=[
                        self.create_content_artifact(
                            original_prompt,
                            tool_input=generated_query,
                            prompt_tokens=input_tokens,
                            completion_tokens=output_tokens
                        )
                    ],
                    input_tokens=input_tokens,
                    output_tokens=output_tokens
                )

            except Exception as e:
                self.logger.error('An error occured in tool %s: %s', self.name, e)
                return self.default_error_message, FoundationaLLMToolResult(
                    content=self.default_error_message,
                    content_artifacts=[
                        self.create_content_artifact(
                            original_prompt,
                            tool_input=generated_query,
                            prompt_tokens=input_tokens,
                            completion_tokens=output_tokens
                        ),
                        self.create_error_content_artifact(
                            original_prompt,
                            e
                        )
                    ],
                    input_tokens=input_tokens,
                    output_tokens=output_tokens
                )

    def __setup_kql_configuration(
            self,
            tool_config: AgentTool,
            config: Configuration,
    ):

        self.kusto_query_endpoint = tool_config.properties['kusto_query_endpoint']
        self.kusto_database = tool_config.properties['kusto_database']

        credential = DefaultAzureCredential()
        self.kusto_token = credential.get_token("https://api.kusto.windows.net")

        self.tools = [
            {
                "type": "function",
                "function": {
                    "name": "query_kql",
                    "description": "Execute a KQL query to retrieve information from a database",
                    "parameters": {
                        "type": "object",
                        "properties": {
                            "query": {
                                "type": "string",
                                "description": "The KQL query to execute",
                            },
                        },
                        "required": ["query"],
                    },
                }
            }
        ]

        self.available_sql_functions = {
            "query_kql": self.execute_kql_query
        }


    def execute_kql_query(self, query: str) -> str:
        """Run a KQL query against the Fabric Kusto query endpoint."""

        try:
            headers = {}
            headers["Content-Type"] = "application/json"
            headers["Authorization"] = "Bearer " + self.kusto_token.token

            with requests.Session() as session:
                session.headers.update(headers)
                url = self.kusto_query_endpoint + "/v1/rest/query"
                body = {
                    "db": self.kusto_database,
                    "csl": query
                }
                response = session.post(url, data=json.dumps(body), timeout=120, verify=True)
                response.raise_for_status()
                return response.json()['Tables'][0]
        except requests.HTTPError as e:
            self.logger.error("Error occurred while executing a KQL query. Error: %s; Query: %s", e, query)

"""
Provides an implementation for the FoundationaLLM Databricks tool.
"""

# Platform imports
from typing import List, Dict, Tuple

from datetime import datetime, time, date
import json
import pandas as pd
import pyodbc
import re
import struct
from itertools import chain, repeat

#Azure imports
from azure.identity import DefaultAzureCredential

# Databricks imports
from databricks.sdk.core import Config, oauth_service_principal
from databricks import sql

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
from foundationallm.models.constants import (
    RunnableConfigKeys,
    PromptResourceTypeNames,
    ResourceObjectIdPropertyNames,
    ResourceProviderNames)
from foundationallm.models.orchestration import ContentArtifact

class FoundationaLLMDatabricksTool(FoundationaLLMToolBase):
    """
    Provides an implementation for the FoundationaLLM Databricks tool.
    """

    def __init__(
        self,
        tool_config: AgentTool,
        objects: Dict,
        user_identity:UserIdentity,
        config: Configuration):
        """ Initializes the FoundationaLLMDatabricksTool class with the tool configuration,
            exploded objects collection, user_identity, and platform configuration. """

        super().__init__(tool_config, objects, user_identity, config)

        self.main_llm = self.get_main_language_model()
        self.main_prompt = self.get_main_prompt()
        self.final_prompt = self.get_prompt("final_prompt")
        self.default_error_message = "An error occurred while executing the SQL query."
        self.__setup_databricks_configuration(tool_config, config)
        self.vector_store_metadata_filter = self.tool_config.properties.get("vector_store_metadata_filter", None)

    def _run(
        self,
        *args,
        **kwargs
        ) -> Tuple[str, List[ContentArtifact]]:

        raise NotImplementedError()

    async def _arun(
        self,
        *args,
        prompt: str = None,
        message_history: List[BaseMessage] = [],
        runnable_config: RunnableConfig = None,
        **kwargs,
        ) -> Tuple[str, FoundationaLLMToolResult]:

        input_tokens = 0
        output_tokens = 0
        sql_query_to_execute = ''
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
                sql_response = ''

                if self.main_prompt.startswith('-- STATIC SQL --'):

                    # The SQL query is static, we can directly execute it

                    sql_query_to_execute = self.main_prompt.split('-- STATIC SQL --')[1].strip()

                    if self.vector_store_metadata_filter:
                        tool_runtime_properties = runnable_config['configurable'][self.name] if self.name in runnable_config['configurable'] else {}
                        for key, value in self.vector_store_metadata_filter.items():
                            if isinstance(value, str) and value.startswith('__COMPLETION_REQUEST_METADATA__'):
                                value = tool_runtime_properties.get(value, None)
                                if value is None:
                                    raise ValueError(f"Metadata key '{value}' not found in tool runtime properties.")
                            sql_query_to_execute = sql_query_to_execute.replace(f'{{{{{key}}}}}', str(value))

                    response = self.query_databricks_sql(sql_query_to_execute)
                    sql_response = response

                else:
                    # The SQL query will be generated dynamically.

                    with self.tracer.start_as_current_span(f'{self.name}_initial_llm_call', kind=SpanKind.INTERNAL):

                        response = await self.main_llm.ainvoke(messages, tools=self.tools)

                        input_tokens += response.usage_metadata['input_tokens']
                        output_tokens += response.usage_metadata['output_tokens']

                    if response.tool_calls \
                        and response.tool_calls[0]['name'] == 'query_databricks_sql':

                        tool_call = response.tool_calls[0]

                        with self.tracer.start_as_current_span(f'{self.name}_tool_call', kind=SpanKind.INTERNAL) as tool_call_span:
                            tool_call_span.set_attribute("tool_call_id", tool_call['id'])
                            tool_call_span.set_attribute("tool_call_function", tool_call['name'])

                            function_name = tool_call['name']
                            function_to_call = self.available_sql_functions[function_name]
                            function_args = tool_call['args']
                            if 'query' in function_args:
                                sql_query_to_execute = function_args['query']

                            sql_response = function_to_call(**function_args)

                final_messages = [
                    SystemMessage(content=self.final_prompt),
                    HumanMessage(content=original_prompt),
                    AIMessage(content=sql_response)
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
                            tool_input=sql_query_to_execute,
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
                            tool_input=sql_query_to_execute,
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

    def __setup_databricks_configuration(
            self,
            tool_config: AgentTool,
            config: Configuration,
    ):
        self.databricks_server_hostname = tool_config.properties['databricks_server_hostname']
        self.databricks_http_path = tool_config.properties['databricks_http_path']
        self.databricks_client_id = tool_config.properties['databricks_client_id']
        self.databricks_client_secret = config.get_value(tool_config.properties['databricks_client_secret'])

        self.tools = [
            {
                "type": "function",
                "function": {
                    "name": "query_databricks_sql",
                    "description": "Execute a SQL query to retrieve information from a Databricks warehouse.",
                    "parameters": {
                        "type": "object",
                        "properties": {
                            "query": {
                                "type": "string",
                                "description": "The SQL query to execute",
                            },
                        },
                        "required": ["query"],
                    },
                }
            },
            # {
            #     "type": "function",
            #     "function": {
            #         "name": "get_table_schema",
            #         "description": "Get the schema of a table in Azure SQL",
            #         "parameters": {
            #             "type": "object",
            #             "properties": {
            #                 "table_name": {
            #                     "type": "string",
            #                     "description": "The name of the table to get the schema for",
            #                 },
            #             },
            #             "required": ["table_name"],
            #         },
            #     }
            # },
            # {
            #     "type": "function",
            #     "function": {
            #         "name": "get_table_rows",
            #         "description": "Preview the first 5 rows of a table in Azure SQL",
            #         "parameters": {
            #             "type": "object",
            #             "properties": {
            #                 "table_name": {
            #                     "type": "string",
            #                     "description": "The name of the table to get the preview for",
            #                 },
            #             },
            #             "required": ["table_name"],
            #         },
            #     }
            # },
            # {
            #     "type": "function",
            #     "function": {
            #         "name": "get_column_values",
            #         "description": "Get the unique values of a column in a table in Azure SQL, only use this if the main query has a WHERE clause",
            #         "parameters": {
            #             "type": "object",
            #             "properties": {
            #                 "table_name": {
            #                     "type": "string",
            #                     "description": "The name of the table to get the column values for",
            #                 },
            #                 "column_name": {
            #                     "type": "string",
            #                     "description": "The name of the column to get the values for",
            #                 },
            #             },
            #             "required": ["table_name", "column_name"],
            #         },
            #     }
            # },
            ## {
            #     "type": "function",
            #     "function": {
            #         "name": "agent_query_validator",
            #         "description": "Validate a SQL query for common mistakes, always call this before calling query_azure_sql",
            #         "parameters": {
            #             "type": "object",
            #             "properties": {
            #                 "query": {
            #                     "type": "string",
            #                     "description": "The SQL query to validate",
            #                 }
            #             },
            #             "required": ["query"],
            #         },
            #     }
            # }

        ]

        self.available_sql_functions = {
            "query_databricks_sql":self.query_databricks_sql,
            # "get_table_schema":self.get_table_schema,
            #"get_table_rows":self.get_table_rows,
            #"get_column_values":self.get_column_values,
            # "agent_query_validator":self.agent_query_validator
        }

    def databricks_credential_provider(self):
        cfg = Config(
            host=f"https://{self.databricks_server_hostname}",
            client_id=self.databricks_client_id,
            client_secret=self.databricks_client_secret,
        )
        return oauth_service_principal(cfg)

    def query_databricks_sql(self, query: str) -> str:
        """Run a SQL query on Databricks and return results as a pandas DataFrame"""

        try:
            
            with sql.connect(
                server_hostname=self.databricks_server_hostname,
                http_path=self.databricks_http_path,
                credentials_provider=self.databricks_credential_provider,
            ) as conn:
                with conn.cursor() as cursor:
                    cursor.execute(query)
                    tbl = cursor.fetchall_arrow()       # pyarrow.Table
                    df = tbl.to_pandas()                # pandas.DataFrame

                    df = self.__convert_datetime_columns_to_string(df)
                    return json.dumps(df.to_dict(orient='records'))

        except Exception as e:
            self.logger.error("Error occurred while executing a SQL query. Error: %s; Query: %s", e, query)
            raise e
    
    def __convert_to_string(self, val):
        if val is None \
            or pd.isna(val):
            return ''
        if isinstance(val, datetime):
            return val.strftime('%Y-%m-%d %H:%M:%S')
        if isinstance(val, date):
            return val.strftime('%Y-%m-%d')
        if isinstance(val, time):
            return val.strftime('%H:%M:%S')

        return str(val)

    def __convert_datetime_columns_to_string(self, df: pd.DataFrame) -> pd.DataFrame:
        """Convert any datetime and timestamp columns in a DataFrame to strings."""
        try:
            df = df.applymap(self.__convert_to_string)
            return df
        except Exception as e:
            self.logger.error(f"Error occurred while converting SQL datetime columns to string. Error : {e}")

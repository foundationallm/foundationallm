# pylint: disable=W0221

import json
from typing import List, Optional, Tuple, Type

from pydantic import BaseModel

from langchain_core.callbacks import CallbackManagerForToolRun, AsyncCallbackManagerForToolRun
from langchain_core.runnables import RunnableConfig
from langchain_core.tools import ToolException
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import (
    FoundationaLLMToolBase,
    FoundationaLLMToolResult
)
from foundationallm.models.agents import AgentTool, VectorDatabaseConfiguration
from foundationallm.models.constants import (
    ContentArtifactTypeNames,
    ResourceObjectIdPropertyNames,
    RunnableConfigKeys
)
from foundationallm.models.orchestration import CompletionRequestObjectKeys, ContentArtifact
from foundationallm.models.resource_providers import ResourcePath
from foundationallm.models.resource_providers.configuration import APIEndpointConfiguration
from foundationallm.services import HttpClientService
from foundationallm.utils import ObjectUtils

from foundationallm_agent_plugins.utils import AzureAISearchConversationRetriever

from .foundationallm_knowledge_tool_input import FoundationaLLMKnowledgeToolInput

class FoundationaLLMKnowledgeTool(FoundationaLLMToolBase):
    """
    FoundationaLLM knowledge tool.
    """
    args_schema: Type[BaseModel] = FoundationaLLMKnowledgeToolInput

    def __init__(self, tool_config: AgentTool, objects: dict, user_identity:UserIdentity, config: Configuration):
        """ Initializes the FoundationaLLMKnowledgeTool class with the tool configuration,
            exploded objects collection, and platform configuration. """
        super().__init__(tool_config, objects, user_identity, config)
        self.main_llm = self.get_main_language_model()
        self.main_prompt = self.get_main_prompt()
        self.knowledge_source_id = self.get_knowledge_source_id()
        self.vector_store_query = self.tool_config.properties.get("vector_store_query", None)
        self.knowledge_graph_query = self.tool_config.properties.get("knowledge_graph_query", None)
        self.vector_store_metadata_filter = self.tool_config.properties.get("vector_store_metadata_filter", None)
        self.context_api_client = self.get_context_api_client(user_identity, config)
        self.instance_id = objects.get(CompletionRequestObjectKeys.INSTANCE_ID, None)

        self.use_conversation_as_vector_store = \
            'vector_store_provider' in self.tool_config.properties and \
            self.tool_config.properties['vector_store_provider'] == 'conversation'

        # When configuring the tool on an agent, the description will be set providing context to the document source.
        self.description = self.tool_config.description or "Answers questions by searching through documents."

    def _run(self,
            prompt: str,
            file_name: Optional[str] = None,
            run_manager: Optional[CallbackManagerForToolRun] = None
            ) -> str:
        raise ToolException("This tool does not support synchronous execution. Please use the async version of the tool.")

    async def _arun(self,
            prompt: str,
            file_name: Optional[str] = None,
            run_manager: Optional[AsyncCallbackManagerForToolRun] = None,
            runnable_config: RunnableConfig = None,
    ) -> Tuple[str, FoundationaLLMToolResult]:
        """ Retrieves documents from an index based on the proximity to the prompt to answer the prompt."""

        input_tokens = 0
        output_tokens = 0

        # Get the original prompt
        if runnable_config is None:
            raise ToolException("RunnableConfig is required for the execution of the tool.")

        # Retrieve the conversation id from the runnable config if available
        conversation_id = None
        if RunnableConfigKeys.CONVERSATION_ID in runnable_config['configurable']:
            conversation_id = runnable_config['configurable'][RunnableConfigKeys.CONVERSATION_ID]

        user_prompt = runnable_config['configurable'][RunnableConfigKeys.ORIGINAL_USER_PROMPT] \
            if RunnableConfigKeys.ORIGINAL_USER_PROMPT in runnable_config['configurable'] \
            else None
        user_prompt_rewrite = runnable_config['configurable'][RunnableConfigKeys.ORIGINAL_USER_PROMPT_REWRITE] \
            if RunnableConfigKeys.ORIGINAL_USER_PROMPT_REWRITE in runnable_config['configurable'] \
            else None
        original_prompt = user_prompt_rewrite or user_prompt or prompt

        # Prepare the knowledge source query request
        query_request = {
                'user_prompt': original_prompt,
                'vector_store_query': self.vector_store_query,
                'knowledge_graph_query': self.knowledge_graph_query,
                'vector_store_id': conversation_id if self.use_conversation_as_vector_store else None,
                'vector_store_metadata_filter': {
                    'FileName': file_name
                } if file_name else None,
                "format_response": True
            }
        # Merge with the vector store metadata filter if it exists
        if self.vector_store_metadata_filter:
            query_request['vector_store_metadata_filter'] = {
                **query_request['vector_store_metadata_filter'],
                **self.vector_store_metadata_filter
            } if query_request['vector_store_metadata_filter'] else self.vector_store_metadata_filter

        query_response = await self.context_api_client.post_async(
            endpoint = f"/instances/{self.instance_id}/knowledgeSources/{self.knowledge_source_id}/query",
            data = json.dumps(query_request)
        )

        if query_response.get('success', False):

            context = query_response.get('text_response','')
            completion_prompt = self.main_prompt.replace('{{context}}', context).replace('{{prompt}}', prompt)

            completion = await self.main_llm.ainvoke(completion_prompt)
            input_tokens += completion.usage_metadata['input_tokens']
            output_tokens += completion.usage_metadata['output_tokens']
        else:
            raise ToolException(f"Failed to query knowledge source: {query_response.get('error_message', 'Unknown error')}")

        content_artifacts = [] # self.retriever.get_document_content_artifacts() or []
        # Token usage content artifact
        # Transform all completion.usage_metadata property values to string
        metadata = {
            'prompt_tokens': str(input_tokens),
            'completion_tokens': str(output_tokens),
            'input_prompt': prompt,
            'input_file_name': file_name
        }
        content_artifacts.append(ContentArtifact(
            id = self.name,
            title = self.name,
            content = original_prompt,
            source = self.name,
            type = ContentArtifactTypeNames.TOOL_EXECUTION,
            metadata=metadata))

        reference_file_names = list(set([text_chunk['metadata'].get('FileName', '') for text_chunk in query_response.get('text_chunks', [])]))
        reference_content_artifacts = [
            ContentArtifact(
                id = file_name,
                title = file_name,
                content = None,
                source = None,
                type = ContentArtifactTypeNames.FILE,
                metadata = {
                    'original_file_name': file_name,
                }
            ) for file_name in reference_file_names if file_name != ''
        ]

        content_artifacts.extend(reference_content_artifacts)

        return completion.content, FoundationaLLMToolResult(
            content=completion.content,
            content_artifacts=content_artifacts,
            input_tokens=input_tokens,
            output_tokens=output_tokens
        )

    def get_knowledge_source_id(self) -> str:
        """
        Gets the knowledge source identifier from the tool configuration.
        """

        knowledge_source_object_id = self.tool_config.get_resource_object_id_properties(
            "FoundationaLLM.Context",
            "knowledgeSources",
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            "knowledge_source"
        )
        return knowledge_source_object_id.resource_path.main_resource_id

    def get_context_api_client(self, user_identity:UserIdentity, config: Configuration) -> HttpClientService:

        context_api_endpoint_configuration = APIEndpointConfiguration(**self.objects.get(CompletionRequestObjectKeys.CONTEXT_API_ENDPOINT_CONFIGURATION, None))
        if context_api_endpoint_configuration:
            client = HttpClientService(
                context_api_endpoint_configuration,
                user_identity,
                config
            )
            client.headers['X-USER-IDENTITY'] = user_identity.model_dump_json()
            return client
        else:
            raise ToolException("The Context API endpoint configuration is required to use the knowledge tool.")

# pylint: disable=W0221

import copy
import json
from typing import Optional, Tuple, Type

from pydantic import BaseModel

from langchain_core.callbacks import CallbackManagerForToolRun, AsyncCallbackManagerForToolRun
from langchain_core.runnables import RunnableConfig
from langchain_core.tools import ToolException
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import (
    FoundationaLLMToolBase,
    FoundationaLLMToolResult
)
from foundationallm.models.agents import AgentTool
from foundationallm.models.constants import (
    ContentArtifactTypeNames,
    ResourceObjectIdPropertyNames,
    RunnableConfigKeys
)
from foundationallm.models.orchestration import CompletionRequestObjectKeys, ContentArtifact
from foundationallm.models.resource_providers.configuration import APIEndpointConfiguration
from foundationallm.services import HttpClientService

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
        self.knowledge_source_id = self.get_knowledge_source_id()
        self.vector_store_query = self.tool_config.properties.get("vector_store_query", None)
        self.knowledge_graph_query = self.tool_config.properties.get("knowledge_graph_query", None)
        self.knowledge_unit_vector_store_filters = self.tool_config.properties.get("knowledge_unit_vector_store_filters", None)
        self.context_api_client = self.get_context_api_client(user_identity, config)
        self.instance_id = objects.get(CompletionRequestObjectKeys.INSTANCE_ID, None)
        self.agent_object_id = objects.get(CompletionRequestObjectKeys.AGENT_OBJECT_ID, None)

        self.use_conversation_as_vector_store = \
            'vector_store_provider' in self.tool_config.properties and \
            self.tool_config.properties['vector_store_provider'] == 'conversation'

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

        main_prompt = self.get_main_prompt()

        input_tokens = 0
        output_tokens = 0

        # Get the original prompt
        if runnable_config is None:
            raise ToolException("RunnableConfig is required for the execution of the tool.")

        # Retrieve the conversation id from the runnable config if available
        conversation_id = None
        if RunnableConfigKeys.CONVERSATION_ID in runnable_config['configurable']:
            conversation_id = runnable_config['configurable'][RunnableConfigKeys.CONVERSATION_ID]

        # Retrieve the agent name from the runnable config if availabl;e
        agent_name = None
        if 'agent_name' in runnable_config['configurable']:
            agent_name = runnable_config['configurable']['agent_name']

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
                'knowledge_unit_vector_store_filters': copy.deepcopy(self.knowledge_unit_vector_store_filters), # Use a copy to avoid mutating the original
                "format_response": True,
                'agent_object_id': self.agent_object_id
            }
        
        if query_request['knowledge_unit_vector_store_filters']:

            # Get the tool's runtime properties, if any.
            tool_runtime_properties = runnable_config['configurable'][self.name] if self.name in runnable_config['configurable'] else {}

            for vector_store_filter in query_request['knowledge_unit_vector_store_filters']:
                if not vector_store_filter['vector_store_metadata_filter']:
                    vector_store_filter['vector_store_metadata_filter'] = {}

                # Add the file name to the metadata filter if it exists
                if file_name:
                    vector_store_filter['vector_store_metadata_filter']['FileName'] = file_name

                # Parse all metadata filters and replace placeholders with runtime properties
                keys_to_remove = []
                for key, value in vector_store_filter['vector_store_metadata_filter'].items():
                    if isinstance(value, str):
                        request_metadata_required = value.startswith('__COMPLETION_REQUEST_METADATA_!__')
                        request_metadata_optional = value.startswith('__COMPLETION_REQUEST_METADATA__')
                        if request_metadata_required or request_metadata_optional:
                            request_value = tool_runtime_properties.get(value, None)
                            if (request_value is None):
                                if request_metadata_required:
                                    raise ValueError(f"Metadata key {key} is required and is missing from the tool runtime properties.")
                                keys_to_remove.append(key)
                            else:
                                vector_store_filter['vector_store_metadata_filter'][key] = request_value
                for key in keys_to_remove:
                    del vector_store_filter['vector_store_metadata_filter'][key]

                # Handle the well-known knowledge unit named Conversations
                if vector_store_filter['knowledge_unit_id'] == 'Conversations':
                    if self.use_conversation_as_vector_store:
                        if conversation_id:
                            vector_store_filter['vector_store_id'] = conversation_id
                        else:
                            raise ToolException("The conversation id is required to query the Conversations knowledge unit..")
                        
                # Handle the well known knowledge unit named AgentPrivateStores
                if vector_store_filter['knowledge_unit_id'] == 'AgentPrivateStores':
                    if agent_name:
                        vector_store_filter['vector_store_id'] = agent_name
                    else:
                        raise ToolException("The agent name is required to query the AgentPrivateStores knowledge unit.")

        query_response = await self.context_api_client.post_async(
            endpoint = f"/instances/{self.instance_id}/knowledgeSources/{self.knowledge_source_id}/query",
            data = json.dumps(query_request)
        )

        context = query_response.get('text_response','')
        completion_prompt = main_prompt.replace('{{context}}', context).replace('{{prompt}}', prompt)

        completion = await self.main_llm.ainvoke(completion_prompt)
        input_tokens += completion.usage_metadata['input_tokens']
        output_tokens += completion.usage_metadata['output_tokens']

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

        content_references = {}
        for content_reference in query_response.get('content_references', []):
            file_id = content_reference.get('FileId', None)
            file_name = content_reference.get('FileName', None)
            if file_id and file_name:
                if file_id not in content_references:
                    content_references[file_id] = {
                        'FileId': file_id,
                        'FileName': file_name,
                        'ReferenceCount': 1
                    }
                else:
                    content_references[file_id]['ReferenceCount'] += 1

        reference_content_artifacts = [
            ContentArtifact(
                id = content_reference['FileId'],
                title = content_reference['FileName'],
                content = None,
                source = None,
                type = 'ContentReference', # ContentArtifactTypeNames.FILE,
                metadata = {
                    'reference_count': content_reference['ReferenceCount'],
                    'original_file_name': content_reference['FileName']
                }
            ) for content_reference in content_references.values()
        ]

        # Only string metadata properties are allowed.
        for reference_content_artifact in reference_content_artifacts:
            reference_content_artifact.metadata['reference_count'] = \
                str(reference_content_artifact.metadata['reference_count'])

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

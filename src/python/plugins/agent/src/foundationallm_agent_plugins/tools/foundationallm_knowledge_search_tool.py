from typing import List, Optional, Tuple
from langchain_core.callbacks import CallbackManagerForToolRun, AsyncCallbackManagerForToolRun
from langchain_core.runnables import RunnableConfig
from langchain_core.tools import ToolException
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import (
    FoundationaLLMToolBase,
    FoundationaLLMToolResult
)
from foundationallm.langchain.retrievers.retriever_factory import RetrieverFactory
from foundationallm.models.agents import AgentTool, KnowledgeManagementIndexConfiguration
from foundationallm.models.constants import (
    ContentArtifactTypeNames,
    ResourceObjectIdPropertyNames,
    ResourceObjectIdPropertyValues,
    ResourceProviderNames,
    RunnableConfigKeys
)
from foundationallm.models.orchestration import CompletionRequestObjectKeys, ContentArtifact
from foundationallm.models.resource_providers import ResourcePath
from foundationallm.models.resource_providers.configuration import APIEndpointConfiguration
from foundationallm.models.resource_providers.vectorization import (
    AzureOpenAIEmbeddingProfile,
    EmbeddingProfileSettingsKeys,
    AzureAISearchIndexingProfile)
from foundationallm.services.gateway_text_embedding import GatewayTextEmbeddingService
from foundationallm.utils import ObjectUtils

class FoundationaLLMKnowledgeSearchTool(FoundationaLLMToolBase):
    """
    FoundationaLLM knowledge search tool.
    """

    def __init__(self, tool_config: AgentTool, objects: dict, user_identity:UserIdentity, config: Configuration):
        """ Initializes the FoundationaLLMKnowledgeSearchTool class with the tool configuration,
            exploded objects collection, and platform configuration. """
        super().__init__(tool_config, objects, user_identity, config)
        self.retriever = self._get_document_retriever()
        self.client = self.get_main_language_model()
        # When configuring the tool on an agent, the description will be set providing context to the document source.
        self.description = self.tool_config.description or "Answers questions by searching through documents."

    def _run(self,
            prompt: str,
            run_manager: Optional[CallbackManagerForToolRun] = None
            ) -> str:
        raise ToolException("This tool does not support synchronous execution. Please use the async version of the tool.")

    async def _arun(self,
            prompt: str,
            run_manager: Optional[AsyncCallbackManagerForToolRun] = None,
            runnable_config: RunnableConfig = None) -> Tuple[str, FoundationaLLMToolResult]:
        """ Retrieves documents from an index based on the proximity to the prompt to answer the prompt."""

        input_tokens = 0
        output_tokens = 0

        # Azure AI Search retriever only supports synchronous execution.
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

        docs = self.retriever.invoke(prompt)
        context = self.retriever.format_docs(docs)
        rag_prompt = f"Answer the question using only the context provided.\n\nContext:\n{context}\n\nQuestion:{prompt}"

        completion = await self.client.ainvoke(rag_prompt)
        input_tokens += completion.usage_metadata['input_tokens']
        output_tokens += completion.usage_metadata['output_tokens']
        content_artifacts = self.retriever.get_document_content_artifacts() or []
        # Token usage content artifact
        # Transform all completion.usage_metadata property values to string
        metadata = {
            'prompt_tokens': str(input_tokens),
            'completion_tokens': str(output_tokens),
            'tool_input': prompt
        }
        content_artifacts.append(ContentArtifact(
            id = self.name,
            title = self.name,
            content = original_prompt,
            source = self.name,
            type = ContentArtifactTypeNames.TOOL_EXECUTION,
            metadata=metadata))
        # Full prompt recording content artifact
        #content_artifacts.append(ContentArtifact(
        #    id = "full_prompt",
        #    title="Full prompt context",
        #    content = rag_prompt,
        #    source = "tool",
        #    type = "full_prompt"))
        return completion.content, FoundationaLLMToolResult(
            content=completion.content,
            content_artifacts=content_artifacts,
            input_tokens=input_tokens,
            output_tokens=output_tokens
        )

    def _get_document_retriever(self):
        """
        Gets the document retriever
        """
        retriever = None

        text_embedding_profile_definition = self.tool_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_VECTORIZATION,
            "textEmbeddingProfiles",
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.EMBEDDING_PROFILE)

        text_embedding_profile = ObjectUtils.get_object_by_id(
            text_embedding_profile_definition.object_id,
            self.objects,
            AzureOpenAIEmbeddingProfile)

        # text_embedding_profile has the embedding model name in settings.
        text_embedding_model_name = text_embedding_profile.settings[EmbeddingProfileSettingsKeys.MODEL_NAME]

        # There can be multiple indexing_profile role objects in the resource object ids.
        indexing_profile_definitions = [
            v for v in self.tool_config.resource_object_ids.values() \
                if v.resource_path.resource_provider == ResourceProviderNames.FOUNDATIONALLM_VECTORIZATION \
                    and v.resource_path.main_resource_type == "indexingProfiles" \
                        and ResourceObjectIdPropertyNames.OBJECT_ROLE in v.properties \
                            and v.properties[ResourceObjectIdPropertyNames.OBJECT_ROLE] == ResourceObjectIdPropertyValues.INDEXING_PROFILE
        ]

        # Only supporting GatewayTextEmbedding
        # Objects dictionary has the gateway API endpoint configuration by default.
        gateway_endpoint_configuration = ObjectUtils.get_object_by_id(
            CompletionRequestObjectKeys.GATEWAY_API_ENDPOINT_CONFIGURATION,
            self.objects,
            APIEndpointConfiguration)

        gateway_embedding_service = GatewayTextEmbeddingService(
            instance_id= ResourcePath.parse(gateway_endpoint_configuration.object_id).instance_id,
            user_identity=self.user_identity,
            gateway_api_endpoint_configuration=gateway_endpoint_configuration,
            model_name = text_embedding_model_name,
            config=self.config)

        # array of objects containing the indexing profile(s) and associated endpoint configuration
        index_configurations = []
        for profile in indexing_profile_definitions:
            indexing_profile = ObjectUtils.get_object_by_id(
                profile.object_id,
                self.objects,
                AzureAISearchIndexingProfile)

            # indexing profile has indexing_api_endpoint_configuration_object_id in Settings.
            indexing_api_endpoint_configuration = ObjectUtils.get_object_by_id(
                indexing_profile.settings.api_endpoint_configuration_object_id,
                self.objects,
                APIEndpointConfiguration)

            index_configurations.append(
                KnowledgeManagementIndexConfiguration(
                    indexing_profile = indexing_profile,
                    api_endpoint_configuration = indexing_api_endpoint_configuration
                ))

        retriever_factory = RetrieverFactory(
                        index_configurations=index_configurations,
                        gateway_text_embedding_service=gateway_embedding_service,
                        config=self.config)

        retriever = retriever_factory.get_retriever()
        return retriever

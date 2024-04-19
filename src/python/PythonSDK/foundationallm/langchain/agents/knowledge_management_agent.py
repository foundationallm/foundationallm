"""
Responsible for executing knowledge management-related completion requests over text,
whether it be from a user prompt or RAG pattern over a vector store.
"""
from typing import List
from langchain_community.callbacks import get_openai_callback
from langchain_core.documents import Document
from langchain_core.prompts import PromptTemplate
from langchain_core.runnables import RunnablePassthrough, RunnableLambda
from langchain_core.output_parsers import StrOutputParser
from foundationallm.config import Configuration
from foundationallm.langchain.agents.agent_base import AgentBase
from foundationallm.langchain.retrievers import RetrieverFactory, CitationRetrievalBase
from foundationallm.models.orchestration import KnowledgeManagementCompletionRequest, CompletionResponse
from foundationallm.resources import ResourceProvider

class KnowledgeManagementAgent(AgentBase):
    """
    Agent for pass-through user_prompt or RAG pattern over a vector store.
    """
    def __init__(
            self,
            completion_request: KnowledgeManagementCompletionRequest,
            config: Configuration,
            resource_provider: ResourceProvider):
        """
        Initializes a knowledge management agent.

        Parameters
        ----------
        completion_request : CompletionRequest
            The completion request object containing the user prompt to execute, message history,
            and agent settings.
        config : Configuration
            Application configuration class for retrieving configuration settings.
        resource_provider : ResourceProvider
            Resource provider for retrieving embedding and indexing profiles.        
        """       

        self.llm = self._get_completion_model(
            config = config,
            agent_orchestration_settings = completion_request.agent.orchestration_settings,
            override_settings = completion_request.settings
        )

        self.prompt_prefix = None
        self.prompt_suffix = None
        prompt = resource_provider.get_resource(completion_request.agent.prompt_object_id)
        if prompt is not None:
            self.prompt_prefix = prompt.prefix
            self.prompt_suffix = prompt.suffix
        
        self.conversation_history = completion_request.agent.conversation_history
        self.message_history = completion_request.message_history

        self.full_prompt = ""

        self.retriever = None
        if completion_request.agent.vectorization is not None:
            retriever_factory = RetrieverFactory(
                            indexing_profile_object_id = completion_request.agent.vectorization.indexing_profile_object_id,
                            text_embedding_profile_object_id= completion_request.agent.vectorization.text_embedding_profile_object_id,
                            config = config,
                            resource_provider = resource_provider,
                            settings = completion_request.settings)

            self.retriever = retriever_factory.get_retriever()

    def __format_docs(self, docs:List[Document]) -> str:
        """
        Generates a formatted string from a list of documents for use
        as the context for the completion request.
        """
        return "\n\n".join(doc.page_content for doc in docs)
    
    def invoke(self, prompt: str) -> CompletionResponse:
        """
        Executes a completion request by querying the vector index with the user prompt.

        Parameters
        ----------
        prompt : str
            The prompt for which a summary completion is begin generated.
        
        Returns
        -------
        CompletionResponse
            Returns a CompletionResponse with the generated summary, the user_prompt,
            generated full prompt with context and token utilization and execution cost details.
        """
        with get_openai_callback() as cb:
            try:
                prompt_builder = ''

                # Add the prefix, if it exists.
                if self.prompt_prefix is not None:
                    prompt_builder = f'{self.prompt_prefix}\n\n'

                # Add the message history, if it exists.
                if self.conversation_history.enabled:
                    prompt_builder += self._build_conversation_history(self.message_history, self.conversation_history.max_history)

                # Insert the context into the template.
                prompt_builder += '{context}'   

                # Add the suffix, if it exists.
                if self.prompt_suffix is not None:
                    prompt_builder += f'\n\n{self.prompt_suffix}'

                # Insert the user prompt into the template.
                if self.retriever is not None:    
                    prompt_builder += "\n\nQuestion: {question}"

                # Create the prompt template.
                prompt_template = PromptTemplate.from_template(prompt_builder)

                if self.retriever is not None:
                    chain_context = { "context": self.retriever | self.__format_docs, "question": RunnablePassthrough() }
                else:
                    chain_context = { "context": RunnablePassthrough() }

                # Compose LCEL chain
                chain = (
                    chain_context
                    | prompt_template
                    | RunnableLambda(self._record_full_prompt)
                    | self.llm
                    | StrOutputParser()
                )

                completion = chain.invoke(prompt)
                citations = []
                if isinstance(self.retriever, CitationRetrievalBase):
                    citations = self.retriever.get_document_citations()
                    
                return CompletionResponse(
                    completion = completion,
                    citations = citations,
                    user_prompt = prompt,
                    full_prompt = self.full_prompt.text,
                    completion_tokens = cb.completion_tokens,
                    prompt_tokens = cb.prompt_tokens,
                    total_tokens = cb.total_tokens,
                    total_cost = cb.total_cost
                )
            except Exception as e:
                raise e

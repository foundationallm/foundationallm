from typing import List
from langchain_community.callbacks import get_openai_callback
from langchain_core.prompts import PromptTemplate
from langchain_core.runnables import RunnablePassthrough, RunnableLambda
from langchain_core.output_parsers import StrOutputParser
from foundationallm.langchain.agents import LangChainAgentBase
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.langchain.retrievers import RetrieverFactory, CitationRetrievalBase
from foundationallm.models.orchestration import (
    CompletionResponse
)
from foundationallm.models.agents import KnowledgeManagementCompletionRequest
from foundationallm.models.resource_providers.vectorization import (
    AzureAISearchIndexingProfile,
    AzureOpenAIEmbeddingProfile
)

class LangChainKnowledgeManagementAgent(LangChainAgentBase):
    """
    The LangChain Knowledge Management agent.
    """
    
    def invoke(self, request: KnowledgeManagementCompletionRequest) -> CompletionResponse:
        """
        Executes a completion request by querying the vector index with the user prompt.

        Parameters
        ----------
        request : KnowledgeManagementCompletionRequest
            The completion request to execute.
        
        Returns
        -------
        CompletionResponse
            Returns a CompletionResponse with the generated summary, the user_prompt,
            generated full prompt with context and token utilization and execution cost details.
        """
        self._validate_request(request)
        agent = request.agent

        prompt = self._get_prompt_from_object_id(agent.prompt_object_id, agent.orchestration_settings.agent_parameters)
        
        with get_openai_callback() as cb:
            try:
                prompt_builder = ''

                # Add the prefix, if it exists.
                if prompt.prefix is not None:
                    prompt_builder = f'{prompt.prefix}\n\n'

                # Add the message history, if it exists.
                conversation_history = agent.conversation_history
                if conversation_history is not None and conversation_history.enabled:
                    prompt_builder += self._build_conversation_history(
                        request.message_history,
                        conversation_history.max_history)

                # Insert the context into the template.
                prompt_builder += '{context}'   

                # Add the suffix, if it exists.
                if prompt.suffix is not None:
                    prompt_builder += f'\n\n{prompt.suffix}'

                # Get the vector document retriever, if it exists.
                retriever = None
                if request.agent.vectorization is not None:
                    indexing_profile = AzureAISearchIndexingProfile.from_object(
                        agent.orchestration_settings.agent_parameters[
                            agent.vectorization.indexing_profile_object_id])

                    text_embedding_profile = AzureOpenAIEmbeddingProfile.from_object(
                        agent.orchestration_settings.agent_parameters[
                            agent.vectorization.text_embedding_profile_object_id])

                    if (indexing_profile is not None) and (text_embedding_profile is not None):
                        retriever_factory = RetrieverFactory(
                                        indexing_profile,
                                        text_embedding_profile,
                                        self.config,
                                        request.settings)
                        retriever = retriever_factory.get_retriever()

                # Insert the user prompt into the template.
                if retriever is not None:    
                    prompt_builder += "\n\nQuestion: {question}"

                # Create the prompt template.
                prompt_template = PromptTemplate.from_template(prompt_builder)

                if retriever is not None:
                    chain_context = { "context": retriever | retriever.format_docs, "question": RunnablePassthrough() }
                else:
                    chain_context = { "context": RunnablePassthrough() }

                # Compose LCEL chain
                chain = (
                    chain_context
                    | prompt_template
                    | RunnableLambda(self._record_full_prompt)
                    | self._get_language_model(agent.orchestration_settings, request.settings)
                    | StrOutputParser()
                )

                completion = chain.invoke(request.user_prompt)
                citations = []
                if isinstance(retriever, CitationRetrievalBase):
                    citations = retriever.get_document_citations()
                    
                return CompletionResponse(
                    completion = completion,
                    citations = citations,
                    user_prompt = request.user_prompt,
                    full_prompt = self.full_prompt.text,
                    completion_tokens = cb.completion_tokens,
                    prompt_tokens = cb.prompt_tokens,
                    total_tokens = cb.total_tokens,
                    total_cost = cb.total_cost
                )
            except Exception as e:
                raise LangChainException(f"An unexpected exception occurred when executing the completion request: {str(e)}", 500) 

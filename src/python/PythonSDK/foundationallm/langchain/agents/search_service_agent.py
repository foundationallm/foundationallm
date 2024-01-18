"""
Class: SearchServiceAgent
Description: A RAG agent for performing hybrid searches on Azure AI Search.
"""
from typing import List
from azure.core.credentials import AzureKeyCredential
from langchain.base_language import BaseLanguageModel
from langchain.callbacks import get_openai_callback
from langchain.prompts import PromptTemplate
from langchain.schema.document import Document
from langchain.schema.runnable import RunnablePassthrough
from langchain.schema import StrOutputParser
from foundationallm.config import Configuration
from foundationallm.langchain.agents.agent_base import AgentBase
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse
from foundationallm.langchain.retrievers import SearchServiceRetriever
from foundationallm.langchain.message_history import build_message_history

class SearchServiceAgent(AgentBase):
    """
    A RAG agent for performing hybrid searches on Azure AI Search.
    """

    def __init__(self, completion_request: CompletionRequest,
                 llm: BaseLanguageModel, config: Configuration):
        """
        Initializes an Azure AI Search agent.

        Parameters
        ----------
        completion_request : CompletionRequest
            The completion request object containing the user prompt to execute, message history,
            and agent and data source metadata.
        llm: BaseLanguageModel
            The language model class to use for embedding and completion.
        config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        self.llm = llm.get_completion_model(completion_request.language_model)        
        self.prompt_prefix = completion_request.agent.prompt_prefix        
        self.retriever = SearchServiceRetriever( 
            endpoint = completion_request.data_source.configuration.endpoint,
            index_name = completion_request.data_source.configuration.index_name,
            top_n = completion_request.data_source.configuration.top_n,
            embedding_field_name = completion_request.data_source.configuration.embedding_field_name,
            text_field_name = completion_request.data_source.configuration.text_field_name,
            credential = AzureKeyCredential(
                config.get_value(
                    completion_request.data_source.configuration.key_secret
                )
            ),
            embedding_model = llm.get_embedding_model(completion_request.embedding_model)
        )
        self.message_history = completion_request.message_history        
        
    def __format_docs(self, docs:List[Document]) -> str:
        """
        Generates a formatted string from a list of documents for use
        as the context for the completion request.
        """
        return "\n\n".join(doc.page_content for doc in docs)        

    def run(self, prompt: str) -> CompletionResponse:
        """
        Executes a completion request by performing a hybrid search with the user prompt.

        Parameters
        ----------
        prompt : str
            The prompt for which a summary completion is begin generated.
        
        Returns
        -------
        CompletionResponse
            Returns a CompletionResponse with the generated summary, the user_prompt,
            and token utilization and execution cost details.
        """
        with get_openai_callback() as cb:            
            prompt_builder = self.prompt_prefix + build_message_history(self.message_history) + \
                        "\n\nQuestion: {question}\n\nContext: {context}\n\nAnswer:"
            custom_prompt = PromptTemplate.from_template(prompt_builder)

            rag_chain = (
                { "context": self.retriever | self.__format_docs, "question": RunnablePassthrough()}
                | custom_prompt
                | self.llm
                | StrOutputParser()
            )

            return CompletionResponse(
                completion = rag_chain.invoke(prompt),
                user_prompt = prompt,
                completion_tokens = cb.completion_tokens,
                prompt_tokens = cb.prompt_tokens,
                total_tokens = cb.total_tokens,
                total_cost = cb.total_cost
            )

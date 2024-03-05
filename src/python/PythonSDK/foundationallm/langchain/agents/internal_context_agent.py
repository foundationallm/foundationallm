"""
Class: InternalContextAgent
Description:
    The InternalContextAgent class is responsible for executing a completion request
    over text directing the user prompt directly to the language model.
"""
from typing import List
from langchain_community.callbacks import get_openai_callback
from langchain_core.documents import Document
from langchain_core.language_models import BaseLanguageModel
from langchain_core.prompts import PromptTemplate
from langchain_core.runnables import RunnablePassthrough, RunnableLambda
from langchain_core.output_parsers import StrOutputParser
from foundationallm.config import Configuration
from foundationallm.langchain.message_history import build_message_history
from foundationallm.langchain.agents.agent_base import AgentBase
from foundationallm.langchain.retrievers import RetrieverFactory
from foundationallm.models.orchestration import InternalContextCompletionRequest, CompletionResponse
from foundationallm.resources import ResourceProvider

class InternalContextAgent(AgentBase):
    """
    Agent for pass-through user_prompt to the LLM.
    """
    def __init__(
            self,
            completion_request: InternalContextCompletionRequest,
            llm: BaseLanguageModel,
            config: Configuration,
            resource_provider: ResourceProvider):
        """
        Initializes an internal context agent.
        Internal context agents pass through the user prompt to the language model.

        Parameters
        ----------
        completion_request : CompletionRequest
            The completion request object containing the user prompt to execute, message history,
            and agent metadata.
        llm: BaseLanguageModel
            The language model class to use for embedding and completion.
        config : Configuration
            Application configuration class for retrieving configuration settings.
        resource_provider : ResourceProvider
            Resource provider for retrieving embedding and indexing profiles.        
        """       
        self.llm = llm.get_completion_model(completion_request.agent.language_model)

        self.agent_prompt = None
        prompt_resource = resource_provider.get_resource(completion_request.agent.prompt_object_id)
        if prompt_resource is not None and prompt_resource.prefix is not None:
            self.agent_prompt = prompt_resource.prefix
                
        self.conversation_history = completion_request.agent.conversation_history
        
        self.message_history = completion_request.message_history
        self.full_prompt = ""
    
    def __record_full_prompt(self, prompt: str) -> str:
        """
        Records the full prompt for the completion request.

        Parameters
        ----------
        prompt : str
            The prompt that is populated with context.
        
        Returns
        -------
        str
            Returns the full prompt.
        """
        self.full_prompt = prompt
        return prompt

    def run(self, prompt: str) -> CompletionResponse:
        """
        Executes a completion request by passing the prompt
        through to the language model.

        Parameters
        ----------
        prompt : str
            The prompt for which a completion is begin generated.
        
        Returns
        -------
        CompletionResponse
            Returns a CompletionResponse with the generated response, the user_prompt,
            generated full prompt with context
            and token utilization and execution cost details.
        """
        with get_openai_callback() as cb:
            try:
                prompt_builder = ''
                
                if self.agent_prompt is not None or self.agent_prompt != '':
                    prompt_builder = f'{self.agent_prompt}\n\n'

                if self.conversation_history.enabled:
                    prompt_builder += build_message_history(self.message_history, self.conversation_history.max_history)
                
                # The prompt is the context
                prompt_builder += '{context}'
                
                prompt_template = PromptTemplate.from_template(prompt_builder)

                # Compose LCEL chain
                chain = (
                    { "context": RunnablePassthrough() }
                    | prompt_template
                    | RunnableLambda(self.__record_full_prompt)
                    | self.llm
                    | StrOutputParser()
                )

                return CompletionResponse(
                    completion = chain.invoke(prompt),
                    user_prompt = prompt,
                    full_prompt = self.full_prompt.text,
                    completion_tokens = cb.completion_tokens,
                    prompt_tokens = cb.prompt_tokens,
                    total_tokens = cb.total_tokens,
                    total_cost = cb.total_cost
                )
            except Exception as e:
                raise e

"""
Class: FoundationaLLMLangChainLCELWorkflow
Description: FoundationaLLM agent workflow based on LangChain LCEL.
"""

import time
from typing import Dict, List, Optional
from opentelemetry.trace import SpanKind

from langchain_classic.callbacks import get_openai_callback
from langchain_core.output_parsers import StrOutputParser
from langchain_core.prompts import PromptTemplate
from langchain_core.runnables import RunnablePassthrough, RunnableLambda

from foundationallm.config import (
    Configuration,
    UserIdentity
)
from foundationallm.langchain.common import (
    FoundationaLLMWorkflowBase,
    FoundationaLLMToolBase
)
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.models.agents import (
    GenericAgentWorkflow,
    AgentWorkflowBase
)
from foundationallm.models.constants import (
    AgentCapabilityCategories
)
from foundationallm.models.language_models import LanguageModelProvider
from foundationallm.models.messages import MessageHistoryItem
from foundationallm.models.orchestration import (
    CompletionRequestObjectKeys,
    CompletionResponse,
    ContentArtifact,
    FileHistoryItem,
    OpenAITextMessageContentItem
)
from foundationallm.operations import OperationsManager


class FoundationaLLMLangChainLCELWorkflow(FoundationaLLMWorkflowBase):
    """
    FoundationaLLM workflow based on LangChain LCEL.
    """

    def __init__(
        self,
        workflow_config: GenericAgentWorkflow | AgentWorkflowBase,
        objects: Dict,
        tools: List[FoundationaLLMToolBase],
        operations_manager: OperationsManager,
        user_identity: UserIdentity,
        config: Configuration,
        intercept_http_calls: bool = False
    ):
        """
        Initializes the FoundationaLLMLangChainLCELWorkflow class with the workflow configuration.

        Parameters
        ----------
        workflow_config : GenericAgentWorkflow | AgentWorkflowBase
            The workflow assigned to the agent.
        objects : dict
            The exploded objects assigned from the agent.
        tools : List[FoundationaLLMToolBase]
            The tools assigned to the agent.
        user_identity : UserIdentity
            The user identity of the user initiating the request.
        config : Configuration
            The application configuration for FoundationaLLM.
        intercept_http_calls : bool, optional
            Whether to intercept HTTP calls made by the workflow, by default False.
        """
        super().__init__(workflow_config, objects, tools, operations_manager, user_identity, config)
        self.name = workflow_config.name
        self.default_error_message = workflow_config.properties.get(
            'default_error_message',
            'An error occurred while processing the request.') \
            if workflow_config.properties else 'An error occurred while processing the request.'

        # Sets self.workflow_llm
        self.create_workflow_llm(intercept_http_calls=intercept_http_calls)
        self.instance_id = objects.get(CompletionRequestObjectKeys.INSTANCE_ID, None)

    async def invoke_async(
        self,
        operation_id: str,
        user_prompt:str,
        user_prompt_rewrite: Optional[str],
        message_history: List[MessageHistoryItem],
        file_history: List[FileHistoryItem],
        conversation_id: Optional[str] = None,
        objects: dict = None
    )-> CompletionResponse:

        """
        Invokes the workflow asynchronously.

        Parameters
        ----------
        operation_id : str
            The unique identifier of the FoundationaLLM operation.
        user_prompt : str
            The user prompt message.
        user_prompt_rewrite : str
            The user prompt rewrite message containing additional context to clarify the user's intent.
        message_history : List[BaseMessage]
            The message history.
        file_history : List[FileHistoryItem]
            The file history.
        conversation_id : Optional[str]
            The conversation identifier for the workflow execution.
        objects : dict
            The exploded objects assigned from the agent. This is used to pass additional context to the workflow.
        """

        workflow_start_time = time.time()

        if objects is None:
            objects = {}

        content_artifacts: List[ContentArtifact] = []
        input_tokens = 0
        output_tokens = 0

        llm_prompt = user_prompt_rewrite or user_prompt
        workflow_main_prompt = self.create_workflow_main_prompt()

        # Get the prompt template.
        prompt_template = self.__get_prompt_template(
            workflow_main_prompt,
            message_history
        )

        chain_context = { "context": RunnablePassthrough() }

        # Compose LCEL chain
        chain = (
            chain_context
            | prompt_template
            | RunnableLambda(self.__record_full_prompt)
            | self.workflow_llm
        )

        retvalue = None

        ai_model = self.get_workflow_main_model_definition()
        api_endpoint = self.get_ai_model_api_endpoint_configuration(ai_model)

        if api_endpoint.provider == LanguageModelProvider.MICROSOFT or api_endpoint.provider == LanguageModelProvider.OPENAI:
            # OpenAI compatible models
            with get_openai_callback() as cb:
                # add output parser to openai callback
                chain = chain | StrOutputParser()
                try:
                    with self.tracer.start_as_current_span('langchain_invoke_lcel_chain', kind=SpanKind.SERVER):
                        completion = await chain.ainvoke(llm_prompt)

                    workflow_end_time = time.time()

                    response_content = OpenAITextMessageContentItem(
                        value = completion,
                        agent_capability_category = AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
                    )

                    output_tokens = cb.completion_tokens
                    input_tokens = cb.prompt_tokens

                    workflow_content_artifact = self.create_workflow_execution_content_artifact(
                            llm_prompt,
                            input_tokens,
                            output_tokens,
                            workflow_end_time - workflow_start_time)
                    content_artifacts.append(workflow_content_artifact)

                    retvalue = CompletionResponse(
                            operation_id=operation_id,
                            content = [response_content],
                            content_artifacts=content_artifacts,
                            user_prompt=llm_prompt,
                            full_prompt=self.full_prompt.text,
                            completion_tokens = output_tokens,
                            prompt_tokens = input_tokens,
                            total_tokens = input_tokens + output_tokens,
                            total_cost = cb.total_cost
                        )

                except Exception as e:
                    raise LangChainException(f"An unexpected exception occurred when executing the completion request: {str(e)}", 500)
        else:
            with self.tracer.start_as_current_span('langchain_invoke_lcel_chain', kind=SpanKind.SERVER):
                completion = await chain.ainvoke(llm_prompt)

            workflow_end_time = time.time()

            response_content = OpenAITextMessageContentItem(
                value = completion.content,
                agent_capability_category = AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
            )

            output_tokens = completion.usage_metadata["output_tokens"]
            input_tokens = completion.usage_metadata["input_tokens"]

            workflow_content_artifact = self.create_workflow_execution_content_artifact(
                    llm_prompt,
                    input_tokens,
                    output_tokens,
                    workflow_end_time - workflow_start_time)
            content_artifacts.append(workflow_content_artifact)

            retvalue = CompletionResponse(
                    operation_id=operation_id,
                    content = [response_content],
                    content_artifacts=content_artifacts,
                    user_prompt=llm_prompt,
                    full_prompt=self.full_prompt.text,
                    completion_tokens = output_tokens,
                    prompt_tokens = input_tokens,
                    total_tokens = input_tokens + output_tokens,
                    total_cost=0
                )

        return retvalue

    def __build_conversation_history(
        self,
        messages:List[MessageHistoryItem]=None
    ) -> str:
        """
        Builds a chat history string from a list of MessageHistoryItem objects to
        be added to the prompt for the completion request.

        Parameters
        ----------
        messages : List[MessageHistoryItem]
            The list of messages from which to build the chat history.
        message_count : int
            The number of messages to include in the chat history.
        """
        if messages is None or len(messages)==0:
            return ""
        chat_history = "Chat History:\n"
        for msg in messages:
            chat_history += msg.sender + ": " + msg.text + "\n"
        chat_history += "\n\n"
        return chat_history

    def __get_prompt_template(
        self,
        prompt: str,
        message_history: List[MessageHistoryItem]
    ) -> PromptTemplate:
        """
        Build a prompt template.
        """

        prompt_builder = f'{prompt}\n\n'

        # Add the message history, if it exists.
        prompt_builder += self.__build_conversation_history(message_history)

        # Insert the context into the template.
        prompt_builder += '{context}'

        # Create the prompt template.
        return PromptTemplate.from_template(prompt_builder)

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

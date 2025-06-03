"""
Class: FoundationaLLMFunctionCallingWorkflow
Description: FoundationaLLM Function Calling workflow to invoke tools at a low level.
"""
import time
from typing import Dict, List, Optional
from logging import Logger
from opentelemetry.trace import Tracer, SpanKind

from langchain_core.messages import (
    AIMessage,
    BaseMessage,
    HumanMessage,
    SystemMessage
)
from langchain_core.runnables import RunnableConfig

from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import (
    FoundationaLLMWorkflowBase,
    FoundationaLLMToolBase
)
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.models.agents import ExternalAgentWorkflow
from foundationallm.models.constants import (
    AgentCapabilityCategories,
    ContentArtifactTypeNames,
    ResourceObjectIdPropertyNames,
    ResourceObjectIdPropertyValues,
    ResourceProviderNames,
    RunnableConfigKeys,
    AIModelResourceTypeNames,
    PromptResourceTypeNames,
    TemplateVariables
)
from foundationallm.models.messages import MessageHistoryItem
from foundationallm.models.orchestration import (
    CompletionRequestObjectKeys,
    CompletionResponse,
    ContentArtifact,
    FileHistoryItem,
    OpenAITextMessageContentItem,
    OpenAIImageFileMessageContentItem,
    OpenAIFilePathMessageContentItem
)
from foundationallm.telemetry import Telemetry

class FoundationaLLMFunctionCallingWorkflow(FoundationaLLMWorkflowBase):
    """
    FoundationaLLM workflow implementing a router pattern for tool invocation
    using Azure OpenAI completion models.
    """

    def __init__(self,
                 workflow_config: ExternalAgentWorkflow,
                 objects: Dict,
                 tools: List[FoundationaLLMToolBase],
                 user_identity: UserIdentity,
                 config: Configuration):
        """
        Initializes the FoundationaLLMWorkflowBase class with the workflow configuration.

        Parameters
        ----------
        workflow_config : ExternalAgentWorkflow
            The workflow assigned to the agent.
        objects : dict
            The exploded objects assigned from the agent.
        tools : List[FoundationaLLMToolBase]
            The tools assigned to the agent.
        user_identity : UserIdentity
            The user identity of the user initiating the request.
        config : Configuration
            The application configuration for FoundationaLLM.
        """
        super().__init__(workflow_config, objects, tools, user_identity, config)
        self.name = workflow_config.name
        self.logger : Logger = Telemetry.get_logger(self.name)
        self.tracer : Tracer = Telemetry.get_tracer(self.name)
        self.default_error_message = workflow_config.properties.get(
            'default_error_message',
            'An error occurred while processing the request.') \
            if workflow_config.properties else 'An error occurred while processing the request.'

        self.__create_workflow_main_prompt()
        self.__create_workflow_files_prompt()
        self.__create_workflow_final_prompt()
        self.__create_workflow_llm()

    async def invoke_async(self,
                           operation_id: str,
                           user_prompt:str,
                           user_prompt_rewrite: Optional[str],
                           message_history: List[MessageHistoryItem],
                           file_history: List[FileHistoryItem],
                           conversation_id: Optional[str] = None,
                           objects: dict = None)-> CompletionResponse:
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
        if objects is None:
            objects = {}
        llm_prompt = user_prompt_rewrite or user_prompt
        
        runnable_config = self.__get_tools_runnable_config(
            user_prompt,
            user_prompt_rewrite,
            file_history,
            conversation_id=conversation_id,
            objects=objects
        )

        messages = self.__get_message_list(
            message_history,
            objects
        )

        content_artifacts: List[ContentArtifact] = []
        input_tokens = 0
        output_tokens = 0
        intermediate_responses: List[str] = []
        final_response = None

        with self.tracer.start_as_current_span(f'{self.name}_workflow', kind=SpanKind.INTERNAL):

            with self.tracer.start_as_current_span(f'{self.name}_workflow_llm_call', kind=SpanKind.INTERNAL):
                router_start_time = time.time()
                llm_bound_tools = self.workflow_llm.bind_tools(self.tools)
                llm_response = await llm_bound_tools.ainvoke(
                    messages + [HumanMessage(content=llm_prompt)],
                    tool_choice='auto')
                input_tokens += llm_response.usage_metadata['input_tokens']
                output_tokens += llm_response.usage_metadata['output_tokens']
                router_end_time = time.time()

            if llm_response.tool_calls:

                intermediate_responses.append(str(llm_response.content))
                
                for tool_call in llm_response.tool_calls:

                    with self.tracer.start_as_current_span(f'{self.name}_tool_call', kind=SpanKind.INTERNAL) as tool_call_span:
                        tool_call_span.set_attribute('tool_call_id', tool_call['id'])
                        tool_call_span.set_attribute('tool_call_function', tool_call['name'])

                        # Get the tool from the tools list
                        tool = next((t for t in self.tools if t.name == tool_call['name']), None)
                        if tool:
                            tool_result = await tool.ainvoke(tool_call, runnable_config)
                            content_artifacts.extend(tool_result.artifact.content_artifacts)
                            intermediate_responses.append(str(tool_result.artifact.content))
                            input_tokens += tool_result.artifact.input_tokens
                            output_tokens += tool_result.artifact.output_tokens
                        else:
                            self.logger.error(
                                'Tool %s not found in the tools list. Skipping tool call.', tool_call["name"])

                # Ask the LLM to verify if the answer is correct if not, loop again with the current messages.
                # verification_messages = messages_with_toolchain.copy()
                # verification_messages.append(HumanMessage(content=f'Verify the requirements are met for this request: "{llm_prompt}", use the other messages only for context. If yes, answer with the single word "DONE". If not, generate more detailed instructions to satisfy the request.'))
                # verification_llm_response = await self.workflow_llm.ainvoke(verification_messages, tools=None)
                # verification_response = verification_llm_response.content
                # if verification_response.strip().upper() == 'DONE':
                #     break # exit the loop if the requirements are met.
                # else:
                #     messages_with_toolchain.append(AIMessage(content=verification_response))
                #     continue # loop again if the requirements are not met.

                final_message = self.__get_message_for_final_response(
                    intermediate_responses,
                    llm_prompt
                )

                with self.tracer.start_as_current_span(f'{self.name}_final_llm_call', kind=SpanKind.INTERNAL):
                    
                    final_llm_response = await self.workflow_llm.ainvoke(
                        messages + [final_message],
                        tools=None)
                    input_tokens += final_llm_response.usage_metadata['input_tokens']
                    output_tokens += final_llm_response.usage_metadata['output_tokens']
                    final_response = final_llm_response.content

            workflow_content_artifact = self.__create_workflow_execution_content_artifact(
                llm_prompt,
                llm_response.content,
                input_tokens,
                output_tokens,
                router_end_time - router_start_time)
            content_artifacts.append(workflow_content_artifact)

            # Initialize response_content with the result, taking final_response as priority.
            response_content = []
            final_response_content = OpenAITextMessageContentItem(
                value= final_response or llm_response.content or 'Failed to generate a response.',
                agent_capability_category=AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
            )
            response_content.append(final_response_content)

            # Process any generated files.
            for artifact in content_artifacts:
                if artifact.type == ContentArtifactTypeNames.FILE:
                    # if the file path has an image extension, add it to the response_content as an OpenAIImageFileMessageContentItem.
                    if any(artifact.metadata['original_file_name'].lower().endswith(ext) for ext in ['.png', '.jpg', '.jpeg', '.gif', '.bmp', '.tiff', '.ico', '.webp']):
                        response_content.append(OpenAIImageFileMessageContentItem(
                            text = artifact.metadata['original_file_name'],
                            file_id = artifact.filepath,
                            agent_capability_category=AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
                        ))
                    else:
                        # if it's not an image, add it to the final_response_content as an annotation of type OpenAIFilePathMessageContentItem.
                        response_content[0].annotations.append(OpenAIFilePathMessageContentItem(
                            text = artifact.metadata['original_file_name'],
                            file_id = artifact.filepath,
                            agent_capability_category=AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
                        ))

            retvalue = CompletionResponse(
                operation_id=operation_id,
                content = response_content,
                content_artifacts=content_artifacts,
                user_prompt=llm_prompt,
                full_prompt=self.workflow_main_prompt,
                completion_tokens=output_tokens,
                prompt_tokens=input_tokens,
                total_tokens=output_tokens + input_tokens,
                total_cost=0
            )
            return retvalue

    def __create_workflow_llm(self):
        """ Creates the workflow LLM instance and saves it to self.workflow_llm. """
        language_model_factory = LanguageModelFactory(self.objects, self.config)
        model_object_id = self.workflow_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_AIMODEL,
            AIModelResourceTypeNames.AI_MODELS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.MAIN_MODEL
        )
        if model_object_id:
            self.workflow_llm = language_model_factory.get_language_model(model_object_id.object_id)
        else:
            error_msg = 'No main model found in workflow configuration'
            self.logger.error(error_msg)
            raise ValueError(error_msg)

    def __create_workflow_main_prompt(self):
        """ Creates the workflow main prompt and saves it to self.workflow_main_prompt. """
        prompt_object_id = self.workflow_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_PROMPT,
            PromptResourceTypeNames.PROMPTS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.MAIN_PROMPT
        )
        if prompt_object_id:
            main_prompt_object_id = prompt_object_id.object_id
            main_prompt_properties = self.objects[main_prompt_object_id]
            self.workflow_main_prompt = main_prompt_properties['prefix']
        else:
            error_message = 'No main prompt found in workflow configuration'
            self.logger.error(error_message)
            raise ValueError(error_message)

    def __create_workflow_files_prompt(self):
        """ Creates the workflow files prompt and saves it to self.workflow_files_prompt. """
        files_prompt_properties = self.workflow_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_PROMPT,
            PromptResourceTypeNames.PROMPTS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.FILES_PROMPT
        )
        if files_prompt_properties:
            files_prompt_object_id = files_prompt_properties.object_id
            self.workflow_files_prompt = \
                self.objects[files_prompt_object_id]['prefix'] if files_prompt_object_id in self.objects else None
        else:
            warning_message = 'No files prompt found in workflow configuration'
            self.logger.warning(warning_message)
            self.workflow_files_prompt = None

    def __create_workflow_final_prompt(self):
        """ Creates the workflow final prompt and saves it to self.workflow_final_prompt. """
        final_prompt_properties = self.workflow_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_PROMPT,
            PromptResourceTypeNames.PROMPTS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.FINAL_PROMPT
        )
        if final_prompt_properties:
            final_prompt_object_id = final_prompt_properties.object_id
            self.workflow_final_prompt = \
                self.objects[final_prompt_object_id]['prefix'] if final_prompt_object_id in self.objects else None
        else:
            warning_message = 'No final prompt found in workflow configuration'
            self.logger.warning(warning_message)
            self.workflow_final_prompt = None

    def __create_workflow_execution_content_artifact(
            self,
            original_prompt: str,
            intent: str,
            input_tokens: int = 0,
            output_tokens: int = 0,
            completion_time_seconds: float = 0) -> ContentArtifact:

        content_artifact = ContentArtifact(id=self.workflow_config.name)
        content_artifact.source = self.workflow_config.name
        content_artifact.type = ContentArtifactTypeNames.WORKFLOW_EXECUTION
        content_artifact.content = original_prompt
        content_artifact.title = self.workflow_config.name
        content_artifact.filepath = None
        content_artifact.metadata = {
            'intent': intent,
            'prompt_tokens': str(input_tokens),
            'completion_tokens': str(output_tokens),
            'completion_time_seconds': str(completion_time_seconds)
        }
        return content_artifact

    def __get_tools_runnable_config(
            self,
            user_prompt:str,
            user_prompt_rewrite: Optional[str],
            file_history: List[FileHistoryItem],
            conversation_id: Optional[str] = None,
            objects: dict = None) -> RunnableConfig:
        """
        Returns the runnable config used in tool invocation.

        Parameters
        ----------
        user_prompt : str
            The user prompt message.
        user_prompt_rewrite : Optional[str]
            The user prompt rewrite message containing additional context to clarify the user's intent.
        file_history : List[FileHistoryItem]
            The file history.
        conversation_id : Optional[str]
            The conversation identifier for the workflow execution.
        objects : dict
            The exploded objects assigned from the agent.
        """
        if objects is None:
            objects = {}

        runnable_config = RunnableConfig(
            {
                RunnableConfigKeys.ORIGINAL_USER_PROMPT: user_prompt,
                RunnableConfigKeys.ORIGINAL_USER_PROMPT_REWRITE: user_prompt_rewrite,
                RunnableConfigKeys.CONVERSATION_ID: conversation_id,
                RunnableConfigKeys.FILE_HISTORY: file_history
            }
        )
        for tool in self.tools:
            if tool.name in objects:
                # Add the tool properties to the runnable config if it exists in the objects.
                runnable_config[tool.name] = objects[tool.name]

        return runnable_config

    def __get_message_list(
        self,
        message_history: List[MessageHistoryItem],
        objects: dict
    ) -> List[BaseMessage]:
        """
        Returns the message history in the format required by the workflow.

        Parameters
        ----------
        message_history : List[MessageHistoryItem]
            The message history to be processed.
        objects : dict
            The exploded objects assigned from the agent. This is used to pass additional context to the workflow.
        """

        if objects is None:
            objects = {}

        # Convert message history to LangChain message types
        messages = []
        for message in message_history:
            # Convert MessageHistoryItem to appropriate LangChain message type
            if message.sender == "User":
                messages.append(HumanMessage(content=message.text))
            else:
                messages.append(AIMessage(content=message.text))

        conversation_files = objects.get(
            CompletionRequestObjectKeys.WORKFLOW_INVOCATION_CONVERSATION_FILES, '')
        attached_files = objects.get(
            CompletionRequestObjectKeys.WORKFLOW_INVOCATION_ATTACHED_FILES, '')

        files_prompt = self.workflow_files_prompt \
            .replace(f'{{{{{TemplateVariables.CONVERSATION_FILES}}}}}', '\n'.join(conversation_files)) \
            .replace(f'{{{{{TemplateVariables.ATTACHED_FILES}}}}}', '\n'.join(attached_files)) \
            if self.workflow_files_prompt else ''
        
        main_prompt = self.workflow_main_prompt \
            .replace(f'{{{{{TemplateVariables.FILES_PROMPT}}}}}', files_prompt)

        self.logger.debug('Workflow prompt: %s', main_prompt)
        print(f'Workflow prompt console: {main_prompt}')

        return [
            SystemMessage(content=main_prompt),
            *messages
        ]

    def __get_message_for_final_response(
            self,
            tool_responses: List[str],
            llm_prompt: str,
    ) -> HumanMessage:
        """
        Returns the final response message for the workflow.

        Parameters
        ----------
        tool_responses : List[str]
            The list of tool responses to be included in the final response.
        llm_prompt : str
            The original LLM prompt used to generate the response.
        """
        tool_results = '\n\n'.join(tool_responses)
        if self.workflow_final_prompt:
            final_response_content = self.workflow_final_prompt \
                .replace(f'{{{{{TemplateVariables.TOOL_RESULTS}}}}}', tool_results) \
                .replace(f'{{{{{TemplateVariables.PROMPT}}}}}', llm_prompt)
        else:
            final_response_content = f'Answer the QUESTION based on the provided CONTEXT. If there is no context, answer directly:\n\nCONTEXT\n{tool_results}\n\nQUESTION:\n{llm_prompt}'

        return \
            HumanMessage(content=final_response_content)

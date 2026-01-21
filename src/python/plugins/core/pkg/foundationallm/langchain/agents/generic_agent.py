import uuid
from azure.identity import DefaultAzureCredential, get_bearer_token_provider
from openai import AsyncAzureOpenAI as async_aoi
from openai.types import CompletionUsage
from opentelemetry.trace import SpanKind

from foundationallm.langchain.agents import AgentBase
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.langchain.tools import ToolFactory
from foundationallm.langchain.workflows import WorkflowFactory
from foundationallm.models.agents import (
    GenericAgentWorkflow,
    AzureAIAgentServiceAgentWorkflow,
    AzureOpenAIAssistantsAgentWorkflow,
    LangChainAgentWorkflow,
    LangGraphReactAgentWorkflow,
    LangChainExpressionLanguageAgentWorkflow,
    ExternalAgentWorkflow
)
from foundationallm.models.constants import (
    ResourceObjectIdPropertyNames,
    ResourceObjectIdPropertyValues,
    ResourceProviderNames,
    AIModelResourceTypeNames,
    PromptResourceTypeNames
)
from foundationallm.models.operations import OperationTypes
from foundationallm.models.orchestration import (
    CompletionRequestObjectKeys,
    CompletionResponse
)
from foundationallm.models.resource_providers.ai_models import AIModelBase
from foundationallm.models.resource_providers.configuration import APIEndpointConfiguration
from foundationallm.models.agents import (
    AgentConversationHistorySettings,
    CompletionRequest
)
from foundationallm.models.attachments import AttachmentProviders
from foundationallm.models.authentication import AuthenticationTypes
from foundationallm.models.language_models import LanguageModelProvider
from foundationallm.models.resource_providers.prompts import MultipartPrompt
from foundationallm.models.services import OpenAIAssistantsAPIRequest
from foundationallm.services import (
    ImageService,
    OpenAIAssistantsApiService
)
from foundationallm.utils import ObjectUtils

from langchain_core.language_models import BaseLanguageModel

class GenericAgent(AgentBase):
    """
    The LangChain Knowledge Management agent.
    """

    def _validate_conversation_history(self, conversation_history_settings: AgentConversationHistorySettings):
        """
        Validates that the agent contains all required properties.

        Parameters
        ----------
        agent : KnowledgeManagementAgent
            The agent to validate.
        """
        if conversation_history_settings is None:
            raise LangChainException("The ConversationHistory property of the agent cannot be null.", 400)

        if conversation_history_settings.enabled is None:
            raise LangChainException("The Enabled property of the agent's ConversationHistory property cannot be null.", 400)

        if conversation_history_settings.enabled and conversation_history_settings.max_history is None:
            raise LangChainException("The MaxHistory property of the agent's ConversationHistory property cannot be null.", 400)

    def _validate_request(self, request: CompletionRequest):
        """
        Validates that the completion request contains all required properties.

        Parameters
        ----------
        request : KnowledgeManagementCompletionRequest
            The completion request to validate.
        """
        if request.agent is None:
            raise LangChainException("The agent property on the completion request cannot be null.", 400)

        if request.objects is None:
            raise LangChainException("The objects property on the completion request cannot be null.", 400)

        ai_model_object_id = request.agent.workflow.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_AIMODEL,
            AIModelResourceTypeNames.AI_MODELS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.MAIN_MODEL
        )
        if ai_model_object_id is None:
            raise LangChainException("The agent's workflow AI models requires a main_model.", 400)
        ai_model = ObjectUtils.get_object_by_id(ai_model_object_id.object_id, request.objects, AIModelBase)

        prompt_object_id = request.agent.workflow.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_PROMPT,
            PromptResourceTypeNames.PROMPTS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.MAIN_PROMPT
        )
        if prompt_object_id is None:
            raise LangChainException("The agent's workflow prompt object dictionary requires a main_prompt.", 400)
        prompt = ObjectUtils.get_object_by_id(prompt_object_id.object_id, request.objects, MultipartPrompt)

        if ai_model.endpoint_object_id is None or ai_model.endpoint_object_id == '':
            raise LangChainException("The AI model object provided in the request's objects dictionary is invalid because it is missing an endpoint_object_id value.", 400)
        if ai_model.deployment_name is None or ai_model.deployment_name == '':
            raise LangChainException("The AI model object provided in the request's objects dictionary is invalid because it is missing a deployment_name value.", 400)
        if ai_model.model_parameters is None:
            raise LangChainException("The AI model object provided in the request's objects dictionary is invalid because the model_parameters value is None.", 400)
        api_endpoint = ObjectUtils.get_object_by_id(ai_model.endpoint_object_id, request.objects, APIEndpointConfiguration)
        if api_endpoint.provider is None or api_endpoint.provider == '':
            raise LangChainException("The API endpoint object provided in the request's objects dictionary is invalid because it is missing a provider value.", 400)
        try:
            LanguageModelProvider(api_endpoint.provider)
        except ValueError:
            raise LangChainException(f"The LLM provider {api_endpoint.provider} is not supported.", 400)
        if api_endpoint.provider == LanguageModelProvider.MICROSOFT:
            # Verify the api_endpoint_configuration includes the api_version property for Azure OpenAI models.
            if api_endpoint.api_version is None or api_endpoint.api_version == '':
                raise LangChainException("The api_version property of the api_endpoint_configuration object cannot be null or empty.", 400)
        if api_endpoint.url is None or api_endpoint.url == '':
            raise LangChainException("The API endpoint object provided in the request's objects dictionary is invalid because it is missing a url value.", 400)
        if api_endpoint.authentication_type is None or api_endpoint.authentication_type == '':
            raise LangChainException("The API endpoint object provided in the request's objects dictionary is invalid because it is missing an authentication_type value.", 400)

        try:
            AuthenticationTypes(api_endpoint.authentication_type)
        except ValueError:
            raise LangChainException(f"The authentication_type {self.api_endpoint.authentication_type} is not supported.", 400)

        if prompt.prefix is None or prompt.prefix == '':
            raise LangChainException("The Prompt object provided in the request's objects dictionary is invalid because it is missing a prefix value.", 400)

        if isinstance(request.agent.workflow, AzureOpenAIAssistantsAgentWorkflow):
            if request.agent.workflow.assistant_id is None or request.agent.workflow.assistant_id == '':
                raise LangChainException("The AzureOpenAIAssistantsAgentWorkflow object provided in the request's agent property is invalid because it is missing an assistant_id value.", 400)

        self._validate_conversation_history(request.agent.conversation_history_settings)

    def _get_image_gen_language_model(self, api_endpoint_object_id, objects: dict) -> BaseLanguageModel:
        api_endpoint = ObjectUtils.get_object_by_id(api_endpoint_object_id, objects, APIEndpointConfiguration)
        scope = api_endpoint.authentication_parameters.get('scope', 'https://cognitiveservices.azure.com/.default')
        # Set up a Azure AD token provider.
        token_provider = get_bearer_token_provider(
            DefaultAzureCredential(exclude_environment_credential=True),
            scope
        )

        return async_aoi(
            azure_endpoint=api_endpoint.url,
            api_version=api_endpoint.api_version,
            azure_ad_token_provider=token_provider,
        )

    async def invoke_async(self, request: CompletionRequest) -> CompletionResponse:
        """
        Executes an async completion request.
        If a vector index exists, it will be queryied with the user prompt.

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

        #----------------------------------------------------------------------
        # Legacy Azure OpenAI Assistants API agent workflow
        #----------------------------------------------------------------------

        if isinstance(agent.workflow, AzureOpenAIAssistantsAgentWorkflow):

            # Legacy image analysis

            ai_model_object_properties = request.agent.workflow.get_resource_object_id_properties(
                ResourceProviderNames.FOUNDATIONALLM_AIMODEL,
                AIModelResourceTypeNames.AI_MODELS,
                ResourceObjectIdPropertyNames.OBJECT_ROLE,
                ResourceObjectIdPropertyValues.MAIN_MODEL
            )
            ai_model_object_id = ai_model_object_properties.object_id
            prompt_object_properties = request.agent.workflow.get_resource_object_id_properties(
                ResourceProviderNames.FOUNDATIONALLM_PROMPT,
                PromptResourceTypeNames.PROMPTS,
                ResourceObjectIdPropertyNames.OBJECT_ROLE,
                ResourceObjectIdPropertyValues.MAIN_PROMPT
            )
            prompt_object_id = prompt_object_properties.object_id
            prompt = ObjectUtils.get_object_by_id(prompt_object_id, request.objects, MultipartPrompt)

            language_model_factory = LanguageModelFactory(request.objects, self.config)

            # Used by image analysis
            ai_model = ObjectUtils.get_object_by_id(ai_model_object_id, request.objects, AIModelBase)

            image_analysis_results = None
            image_analysis_token_usage = CompletionUsage(prompt_tokens=0, completion_tokens=0, total_tokens=0)
            # Get image attachments that are images with URL file paths.
            image_attachments = [attachment for attachment in request.attachments if (attachment.provider == AttachmentProviders.FOUNDATIONALLM_ATTACHMENT and attachment.content_type.startswith('image/'))] if request.attachments is not None else []
            if len(image_attachments) > 0:
                image_client = language_model_factory.get_language_model(ai_model_object_id, override_operation_type=OperationTypes.IMAGE_SERVICES)
                image_svc = ImageService(config=self.config, client=image_client, deployment_name=ai_model.deployment_name)
                image_analysis_results, usage = await image_svc.analyze_images_async(image_attachments)
                if usage is not None:
                    image_analysis_token_usage.prompt_tokens += usage.prompt_tokens
                    image_analysis_token_usage.completion_tokens += usage.completion_tokens
                    image_analysis_token_usage.total_tokens += usage.total_tokens

            # Legacy Assistants API implementation

            assistant_id = agent.workflow.assistant_id

            # create the service
            assistant_svc = OpenAIAssistantsApiService(
                azure_openai_client=language_model_factory.get_language_model(ai_model_object_id, override_operation_type=OperationTypes.ASSISTANTS_API),
                operations_manager=self.operations_manager
            )

            # populate service request object
            assistant_req = OpenAIAssistantsAPIRequest(
                document_id=str(uuid.uuid4()),
                operation_id=request.operation_id,
                instance_id=self.config.get_value("FoundationaLLM:Instance:Id"),
                assistant_id=assistant_id,
                thread_id=request.objects[CompletionRequestObjectKeys.OPENAI_THREAD_ID],
                attachments=[attachment.provider_file_name for attachment in request.attachments if attachment.provider == AttachmentProviders.FOUNDATIONALLM_AZURE_OPENAI],
                user_prompt=request.user_prompt
            )

            # Add user and assistant messages related to image analysis to the Assistants API request.
            if image_analysis_results is not None:
                # Add user message
                await assistant_svc.add_thread_message_async(
                    thread_id = assistant_req.thread_id,
                    role = "user",
                    content = "Analyze any attached images.",
                    attachments = []
                )
                # Add assistant message
                await assistant_svc.add_thread_message_async(
                    thread_id = assistant_req.thread_id,
                    role = "assistant",
                    content = image_svc.format_results(image_analysis_results),
                    attachments = []
                )

            image_service = None
            if any(tool.name == "DALLEImageGeneration" for tool in agent.tools):
                dalle_tool = next((tool for tool in agent.tools if tool.name == "DALLEImageGeneration"), None)

                model_object_id = dalle_tool.get_resource_object_id_properties(
                    ResourceProviderNames.FOUNDATIONALLM_AIMODEL,
                    AIModelResourceTypeNames.AI_MODELS,
                    ResourceObjectIdPropertyNames.OBJECT_ROLE,
                    ResourceObjectIdPropertyValues.MAIN_MODEL
                )

                image_generation_deployment_model = request.objects[model_object_id.object_id]["deployment_name"]
                api_endpoint_object_id = request.objects[model_object_id.object_id]["endpoint_object_id"]
                image_generation_client = self._get_image_gen_language_model(api_endpoint_object_id=api_endpoint_object_id, objects=request.objects)
                image_service=ImageService(
                    config=self.config,
                    client=image_generation_client,
                    deployment_name=image_generation_deployment_model,
                    image_generator_tool_description=dalle_tool.description)

            # invoke/run the service
            assistant_response = await assistant_svc.run_async(
                assistant_req,
                image_service=image_service
            )

            # Verify the Assistants API response
            if assistant_response is None:
                print("Assistants API response was None.")
                return CompletionResponse(
                    operation_id = request.operation_id,
                    full_prompt = prompt.prefix,
                    user_prompt = request.user_prompt,
                    user_prompt_rewrite = request.user_prompt_rewrite,
                    errors = [ "Assistants API response was None." ],
                    is_error = True
                )

            # create the CompletionResponse object
            return CompletionResponse(
                id = assistant_response.document_id,
                operation_id = request.operation_id,
                full_prompt = prompt.prefix,
                content = assistant_response.content,
                analysis_results = assistant_response.analysis_results,
                completion_tokens = assistant_response.completion_tokens + image_analysis_token_usage.completion_tokens,
                prompt_tokens = assistant_response.prompt_tokens + image_analysis_token_usage.prompt_tokens,
                total_tokens = assistant_response.total_tokens + image_analysis_token_usage.total_tokens,
                user_prompt = request.user_prompt,
                user_prompt_rewrite = request.user_prompt_rewrite,
                errors = assistant_response.errors,
                is_error = len(assistant_response.errors) > 0

            )
        # End Assistants API implementation

        #----------------------------------------------------------------------
        # Plugin-based agent workflows
        #----------------------------------------------------------------------

        if isinstance(agent.workflow, (
            GenericAgentWorkflow,
            AzureAIAgentServiceAgentWorkflow,
            LangChainAgentWorkflow,
            LangGraphReactAgentWorkflow,
            LangChainExpressionLanguageAgentWorkflow,
            ExternalAgentWorkflow)):

            # Ensure legacy agents do not require definition changes to use
            # the plugin-based workflows

            if isinstance(agent.workflow, (LangGraphReactAgentWorkflow)):
                agent.workflow.package_name = "foundationallm_agent_plugins_langchain"
                agent.workflow.class_name = "FoundationaLLMLangGraphReActAgentWorkflow"

            if isinstance(agent.workflow, (LangChainExpressionLanguageAgentWorkflow)):
                agent.workflow.package_name = "foundationallm_agent_plugins_langchain"
                agent.workflow.class_name = "FoundationaLLMLangChainLCELWorkflow"

            with self.tracer.start_as_current_span('langchain_prepare_plugin_workflow', kind=SpanKind.SERVER) as span:
                span.set_attribute("agent_name", agent.name)
                span.set_attribute("conversation_id", request.session_id if request.session_id is not None else '')
                span.set_attribute("operation_id", request.operation_id)

                # prepare tools
                tool_factory = ToolFactory(self.plugin_manager)
                workflow_tools = []

                # Populate tools list from agent configuration
                for tool_config in agent.tools:
                    tool_instance = \
                        tool_factory.get_tool(agent.name, tool_config, request.objects, self.user_identity, self.config)
                    tool_instance.description = tool_config.description
                    tool_instance.tool_config = tool_config
                    tool_instance.objects = request.objects
                    workflow_tools.append(tool_instance)

                request.objects['message_history'] = request.message_history[:agent.conversation_history_settings.max_history*2]

                # create the workflow
                workflow_factory = WorkflowFactory(
                    self.plugin_manager,
                    self.operations_manager)
                workflow = workflow_factory.get_workflow(
                    agent.workflow,
                    request.objects,
                    workflow_tools,
                    self.user_identity,
                    self.config)

            with self.tracer.start_as_current_span('langchain_invoke_plugin_workflow', kind=SpanKind.SERVER) as span:
                span.set_attribute("agent_name", agent.name)
                span.set_attribute("conversation_id", request.session_id if request.session_id is not None else '')
                span.set_attribute("operation_id", request.operation_id)

                response = await workflow.invoke_async(
                    operation_id=request.operation_id,
                    user_prompt=request.user_prompt,
                    user_prompt_rewrite=request.user_prompt_rewrite,
                    message_history=request.message_history,
                    file_history=request.file_history,
                    conversation_id=request.session_id,
                    is_new_conversation=request.is_new_conversation,
                    objects=request.objects
                )
                # Ensure the user prompt rewrite is returned in the response
                response.user_prompt_rewrite = request.user_prompt_rewrite
            return response

        # The agent workflow type is not supported.

        return CompletionResponse(
            operation_id = request.operation_id,
            content = f'The agent workflow type {agent.workflow.type} is not supported.',
            user_prompt = request.user_prompt,
            user_prompt_rewrite = request.user_prompt_rewrite,
            full_prompt = '',
            completion_tokens = 0,
            prompt_tokens = 0,
            total_tokens = 0,
            total_cost = 0
        )

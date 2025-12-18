"""
Class: AzureAIAgentServiceWorkflow
Description: Workflow that integrates with the Azure AI Agent Service.
"""
import json
import uuid
from typing import Any,Dict, Optional, List
from logging import Logger
from azure.identity.aio import DefaultAzureCredential
from azure.ai.projects.aio import AIProjectClient
from azure.ai.agents.models import (
    AsyncAgentEventHandler,
    CodeInterpreterToolDefinition,
    FileSearchToolDefinition,
    FunctionTool,
    ListSortOrder,
    MessageAttachment,
    MessageRole,
    MessageDeltaChunk,
    MessageImageFileContent,
    MessageTextContent,
    MessageTextFilePathAnnotation,
    MessageTextFileCitationAnnotation,
    RequiredFunctionToolCall,
    RunStep, 
    RunStepCodeInterpreterToolCall,
    RunStepFunctionToolCall,
    SubmitToolOutputsAction,
    ToolOutput,
    ThreadMessage,
    ThreadRun
)
from opentelemetry.trace import Tracer

from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import (
    FoundationaLLMWorkflowBase,
    FoundationaLLMToolBase
)
from foundationallm.models.agents import (
    GenericAgentWorkflow,
    ExternalAgentWorkflow
)
from foundationallm.models.constants import (
    AgentCapabilityCategories,
    AzureAIResourceTypeNames,
    ContentArtifactTypeNames,
    ResourceObjectIdPropertyNames,
    ResourceObjectIdPropertyValues,
    ResourceProviderNames
)
from foundationallm.models.messages import MessageHistoryItem
from foundationallm.models.orchestration import (
    AnalysisResult,
    CompletionRequestObjectKeys,
    CompletionResponse,
    ContentArtifact,
    FileHistoryItem,
    OpenAITextMessageContentItem,
    OpenAIImageFileMessageContentItem,
    OpenAIFilePathMessageContentItem
)
from foundationallm.operations.operations_manager import OperationsManager
from foundationallm.telemetry import Telemetry

class AzureAIAgentServiceAgentAsyncEventHandler(AsyncAgentEventHandler):
    """
    Event handler for the Azure AI Agent Service.
    """
    def __init__(self,
                    functions: FunctionTool,
                    instance_id: str,
                    operation_id: str,
                    user_prompt:str,
                    user_prompt_rewrite: Optional[str],
                    operations_manager:OperationsManager,
                    project_client: AIProjectClient) -> None:
        super().__init__()
        self.logger : Logger = Telemetry.get_logger("AzureAIAgentServiceAgentAsyncEventHandler")
        self.functions = functions
        self.instance_id = instance_id
        self.operation_id = operation_id
        self.operations_manager = operations_manager
        self.run_steps = {}       
        self.messages = {}
        self.run_id = None
        self.stop_tokens = [".", ",", ":", ";", "\n", " ", ")"] # Use stop tokens to determine when to write to State API.
        self.interim_result = CompletionResponse(
            id = str(uuid.uuid4()),
            operation_id = operation_id,
            user_prompt = user_prompt,
            user_prompt_rewrite = user_prompt_rewrite,
            content = [],
            analysis_results = []
        )

    async def on_message_delta(self, message_delta: MessageDeltaChunk) -> None:       
        if message_delta.text.endswith(tuple(self.stop_tokens)): # Use stop tokens to determine when to write to State API.
            await self.update_state_api_content_async()                  

    async def on_thread_message(self, message: ThreadMessage) -> None:
        if message.status == "created":
            self.messages[message.id] = message            
        if message.status == "completed":
            self.messages[message.id] = message
            await self.update_state_api_content_async()        

    async def on_thread_run(self, run: ThreadRun) -> None:        
        self.run_id = run.id
        if run.status == "failed":
            self.logger.warn(f"Run failed. Error: {run.last_error}")
            print(f"Run failed. Error: {run.last_error}")

        if run.status == "requires_action" and isinstance(run.required_action, SubmitToolOutputsAction):
            tool_calls = run.required_action.submit_tool_outputs.tool_calls
            tool_outputs = []
            for tool_call in tool_calls:
                if isinstance(tool_call, RequiredFunctionToolCall):
                    try:
                        output = await self.functions.execute(tool_call)
                        tool_outputs.append(
                            ToolOutput(
                                tool_call_id=tool_call.id,
                                output=output,
                            )
                        )
                    except Exception as e:
                        self.logger.warn(f"Error executing function tool_call {tool_call.id}: {e}")
                        print(f"Error executing tool_call {tool_call.id}: {e}")            
            if tool_outputs:
                await self.project_client.agents.submit_tool_outputs_to_stream(
                    thread_id=run.thread_id, run_id=run.id, tool_outputs=tool_outputs, event_handler=self
                )
              
    async def on_run_step(self, step: RunStep) -> None:        
        if step.status == "created":
            self.run_steps[step.id] = step
        elif(step.type == "tool_calls" and step.status == "completed"):            
            self.run_steps[step.id] = step

    async def on_error(self, data: str) -> None:
        self.logger.error(f"Event Hander Error: {data}")
        print(f"Event Hander Error: {data}")
        raise Exception(f"Event Hander Error: {data}")
          
    async def on_unhandled_event(self, event_type: str, event_data: Any) -> None:
        self.logger.warn(f"Unhandled Event Type: {event_type}, Data: {event_data}")
        print(f"Unhandled Event Type: {event_type}, Data: {event_data}")

    def get_run_steps(self) -> List[RunStep]:
        """
        Returns the run steps accumulated during the stream.
        """
        return list(self.run_steps.values())

    def get_last_run_id(self) -> str:
        """
        Returns the last run id associated with the stream.
        """
        return self.run_id

    async def update_state_api_analysis_results_async(self):
        self.interim_result.analysis_results = [] # Clear the analysis results list before adding new results.
        for k, v in self.run_steps.items():
            if not v:
                continue
            analysis_result = parse_run_step(v)
            if analysis_result:
                self.interim_result.analysis_results.append(analysis_result)
        await self.operations_manager.set_operation_result_async(self.operation_id, self.instance_id, self.interim_result)

    async def update_state_api_content_async(self):
        self.interim_result.content = [] # Clear the content list before adding new messages.
        for k, v in self.messages.items():
            content_items = parse_message(v)
            self.interim_result.content.extend(content_items)
        await self.operations_manager.set_operation_result_async(self.operation_id, self.instance_id, self.interim_result)

class AzureAIAgentServiceWorkflow(FoundationaLLMWorkflowBase):
    """
    FoundationaLLM workflow implementing an integration with the Azure AI Agent Service.
    """
    def __init__(self,
                 workflow_config: GenericAgentWorkflow | ExternalAgentWorkflow,
                 objects: Dict,
                 tools: List[FoundationaLLMToolBase],
                 user_identity: UserIdentity,
                 config: Configuration,
                 operations_manager: OperationsManager):
        """
        Initializes the FoundationaLLMWorkflowBase class with the workflow configuration.

        Parameters
        ----------
        workflow_config : GenericAgentWorkflow | ExternalAgentWorkflow
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
        self.operations_manager = operations_manager
        self.logger : Logger = Telemetry.get_logger(self.name)
        self.tracer : Tracer = Telemetry.get_tracer(self.name)        
        self.instance_id = self.objects.get(CompletionRequestObjectKeys.INSTANCE_ID)
        # Resolves agent_id, thread_id, and project_connection_string
        self.__resolve_ai_agent_service_resources()
       
    async def invoke_async(self,
        operation_id: str,
        user_prompt:str,
        user_prompt_rewrite: Optional[str],
        message_history: List[MessageHistoryItem],
        file_history: List[FileHistoryItem])-> CompletionResponse:
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
        """
        llm_prompt = user_prompt_rewrite or user_prompt
        analysis_results = []

        # Check file_history for any items for the current attachments, associate the appropriate tool(s).
        # https://learn.microsoft.com/en-us/azure/ai-services/agents/how-to/tools/code-interpreter?tabs=python&pivots=supported-filetypes
        # https://learn.microsoft.com/en-us/azure/ai-services/agents/how-to/tools/file-search?tabs=python&pivots=supported-filetypes
        code_interpreter_extensions = ['.c', '.cpp', '.csv', 'docx', '.html', '.java', '.json', '.md', '.pdf', '.php', '.pptx', '.py', '.rb' '.tex', '.txt', '.css', '.jpeg', '.jpg', '.js', '.gif', '.png', '.tar', '.ts', '.xlsx', '.xml', '.zip']
        file_search_extensions = ['.c', '.cs', '.cpp', '.doc', '.docx', '.html', '.java', '.json', '.md', '.pdf', '.php', '.pptx', '.py', '.rb', '.tex', '.txt', '.css', '.js', '.sh', '.ts']
        message_file_attachments = [
            MessageAttachment(
                tools=[
                    tool for ext, tool in [
                        (code_interpreter_extensions, CodeInterpreterToolDefinition()),
                        (file_search_extensions, FileSearchToolDefinition())
                    ] if any(file.original_file_name.endswith(e) for e in ext)
                ],
                file_id=file.secondary_provider_object_id
            ) for file in file_history
            if file.current_message_attachment  # Grab only the current message attachments
            and file.secondary_provider == ResourceProviderNames.FOUNDATIONALLM_AZUREAI
        ]
        
        async with DefaultAzureCredential(exclude_environment_credential=True) as credential:
            async with AIProjectClient.from_connection_string(
                credential = credential,
                conn_str = self.project_connection_string
            ) as project_client:               
                # Add current message to the thread.
                message = await project_client.agents.create_message(
                    thread_id = self.thread_id,
                    role = MessageRole.USER,
                    content = llm_prompt,
                    attachments=message_file_attachments
                )                
                event_handler = AzureAIAgentServiceAgentAsyncEventHandler(
                    functions = None,
                    instance_id = self.instance_id,
                    operation_id = operation_id,
                    user_prompt = user_prompt,
                    user_prompt_rewrite = user_prompt_rewrite,
                    operations_manager = self.operations_manager,
                    project_client = project_client
                )
                async with await project_client.agents.create_stream(
                    thread_id = self.thread_id, 
                    agent_id = self.agent_id, 
                    event_handler = event_handler
                ) as stream:
                    await stream.until_done()              

                # Get messages from the thread
                messages = await project_client.agents.list_messages(
                    thread_id = self.thread_id,
                    order = ListSortOrder.ASCENDING,
                    after = message.id
                )
                content = [] 
                for message in messages.data:
                    message_content = parse_message(message)              
                    if message_content:
                        content.extend(message_content)
                        
                # event_handler has an accumulator for completed tool calling run steps. 
                # It also has a property with the run_id for the run associated with the stream.
                run_steps = event_handler.get_run_steps()        
                if(len(run_steps) > 0):
                    for step in run_steps:
                        analysis_result = parse_run_step(step)
                        if analysis_result is not None:
                            analysis_results.append(analysis_result)
                run = await project_client.agents.get_run(
                    thread_id=self.thread_id,
                    run_id=event_handler.get_last_run_id()
                )
                # Get final usage details from the run  
                #  prompt_tokens = run.usage.prompt_tokens
                prompt_tokens = 0
                completion_tokens = 0
                total_tokens = 0                  
                if run.usage:
                    prompt_tokens = run.usage.prompt_tokens
                    completion_tokens = run.usage.completion_tokens
                    total_tokens = run.usage.total_tokens                       
                workflow_execution_content_artifact = self.__create_workflow_execution_content_artifact(
                    llm_prompt, 
                    prompt_tokens, 
                    completion_tokens, 
                    total_tokens)
                retvalue = CompletionResponse(
                    operation_id = operation_id,
                    analysis_results = analysis_results,
                    content = content,
                    content_artifacts = [workflow_execution_content_artifact],
                    user_prompt = llm_prompt,
                    full_prompt = run.instructions,
                    completion_tokens = completion_tokens,
                    prompt_tokens = prompt_tokens,
                    total_tokens = total_tokens,
                    total_cost = 0
                )               
                return retvalue    
    
    def __resolve_ai_agent_service_resources(self):
        """
        Resolves the Azure AI Agent Service resources from the objects dictionary.
        Populates the agent_id, thread_id, and project_connection_string.
        """
        # Populate the agent id and thread id from the objects dictionary
        self.agent_id = self.objects.get(CompletionRequestObjectKeys.AZUREAI_AGENT_ID)
        self.thread_id = self.objects.get(CompletionRequestObjectKeys.AZUREAI_AGENT_THREAD_ID)
        if not self.agent_id:
            raise ValueError('Azure AI Agent Service Agent ID is not set in the objects dictionary.')    
        if not self.thread_id:
            raise ValueError('Azure AI Agent Service Thread ID is not set in the objects dictionary.')
        
        project_object_id = self.workflow_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_AZUREAI,
            AzureAIResourceTypeNames.PROJECTS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.AI_PROJECT            
        )
        if not project_object_id:
            raise ValueError('Azure AI Project ID is not set.')
        
        # Get the Project object from the objects dictionary keyed by project_object_id
        project_object = self.objects.get(project_object_id.object_id)
        if not project_object:
            raise ValueError('Azure AI Project object not found in objects dictionary.')
        self.project_connection_string = project_object['project_connection_string']
        
    def __create_workflow_execution_content_artifact(
            self,
            original_prompt: str,            
            prompt_tokens: int = 0,
            completion_tokens: int = 0,
            completion_time_seconds: float = 0) -> ContentArtifact:
        """
        Creates a content artifact for the workflow execution.

        Parameters
        ----------
        original_prompt : str
            The original prompt.
        prompt_tokens : int
            The number of prompt tokens.
        completion_tokens : int
            The number of completion tokens.
        completion_time_seconds : float
            The completion time in seconds.
        """
        content_artifact = ContentArtifact(id=self.workflow_config.name)
        content_artifact.source = self.workflow_config.name
        content_artifact.type = ContentArtifactTypeNames.WORKFLOW_EXECUTION
        content_artifact.content = original_prompt
        content_artifact.title = self.workflow_config.name
        content_artifact.filepath = None
        content_artifact.metadata = {            
            'prompt_tokens': str(prompt_tokens),
            'completion_tokens': str(completion_tokens),
            'completion_time_seconds': str(completion_time_seconds)
        }
        return content_artifact

# Shared methods
def parse_run_step(run_step: RunStep) -> AnalysisResult:
    """
    Parses a run step from the Azure AI Agent Service.

    Parameters
    ----------
    run_step : RunStep
        The run step to parse.

    Returns
    -------
    AnalysisResult
        The analysis result from the run step.
    OR None
        If the run step does not contain a tool call
        to the code interpreter tool.
    """
    step_details = run_step.step_details
    if step_details and step_details.type == "tool_calls":
        tool_call_detail = step_details.tool_calls
        for details in tool_call_detail:
            if isinstance(details, RunStepCodeInterpreterToolCall):
                result = AnalysisResult(
                    tool_name = details.type,
                    agent_capability_category = AgentCapabilityCategories.AZURE_AI_AGENTS
                )
                result.tool_input += details.code_interpreter.input  # Source code
                for output in details.code_interpreter.outputs:  # Tool execution output                        
                    if hasattr(output, 'image') and output.image:
                        result.tool_output += "# Generated image file: " + output.image.file_id
                    elif hasattr(output, 'logs') and output.logs:
                        result.tool_output += output.logs
                return result
            elif isinstance(details, RunStepFunctionToolCall):
                result = AnalysisResult(
                    tool_name = details.function.name,
                    agent_capability_category = AgentCapabilityCategories.AZURE_AI_AGENTS
                )
                result.tool_input += details.function.arguments
                if details.function.output:
                    fn_output = json.loads(details.function.output)
                    if 'data' in fn_output:
                        output_data = json.loads(details.function.output)['data'][0]
                        result.tool_output += json.dumps({"url": output_data['url'], "description": output_data['revised_prompt']})
                    else:
                        # indicative of a failure during the function call, append error message to output
                        print("Error in function call: " + fn_output)
                        result.tool_output += json.dumps(fn_output)                            
                                
    return None
    
def parse_message(message: ThreadMessage):
    """
    Parses a message from the Azure AI Agent Service.

    Parameters
    ----------
    message : Message
        The message to parse.

    Returns
    -------
    List[MessageContentItemBase]
        The content items within the message along with any annotations.
    """
    ret_content = []
    # for each content item in the message
    for ci in message.content:
            match ci:
                case MessageTextContent():
                    text_ci = OpenAITextMessageContentItem(
                        value=ci.text.value,
                        agent_capability_category = AgentCapabilityCategories.AZURE_AI_AGENTS
                    )
                    for annotation in ci.text.annotations:
                        match annotation:
                            case MessageTextFilePathAnnotation():
                                file_an = OpenAIFilePathMessageContentItem(
                                    file_id=annotation.file_path.file_id,
                                    start_index=annotation.start_index,
                                    end_index=annotation.end_index,
                                    text=annotation.text,
                                    agent_capability_category = AgentCapabilityCategories.AZURE_AI_AGENTS
                                )
                                text_ci.annotations.append(file_an)
                            case MessageTextFileCitationAnnotation():
                                file_cit = OpenAIFilePathMessageContentItem(
                                    file_id=annotation.file_citation.file_id,
                                    start_index=annotation.start_index,
                                    end_index=annotation.end_index,
                                    text=annotation.text,
                                    agent_capability_category = AgentCapabilityCategories.AZURE_AI_AGENTS
                                )
                                text_ci.annotations.append(file_cit)
                    ret_content.append(text_ci)
                case MessageImageFileContent():
                    ci_img = OpenAIImageFileMessageContentItem(
                        file_id=ci.image_file.file_id,
                        agent_capability_category = AgentCapabilityCategories.AZURE_AI_AGENTS
                    )
                    ret_content.append(ci_img)
                #case MessageImageURLContent():
                #    ci_img_url = OpenAIImageFileMessageContentItem(
                #        file_url=ci.image_url.url,
                #        agent_capability_category = AgentCapabilityCategories.AZURE_AI_AGENTS
                #    )
                #    ret_content.append(ci_img_url)
    return ret_content

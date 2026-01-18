# pylint: disable=W0221

from typing import Optional, Tuple, Type, List, ClassVar, Any, Dict
from uuid import uuid4
import json

from langchain_core.callbacks import CallbackManagerForToolRun, AsyncCallbackManagerForToolRun
from langchain_core.messages import (
    SystemMessage,
    HumanMessage
)
from langchain_core.runnables import RunnableConfig
from langchain_core.tools import ToolException

from opentelemetry.trace import SpanKind
from pydantic import BaseModel

from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import (
    FoundationaLLMToolBase,
    FoundationaLLMToolResult
)
from foundationallm.models.agents import AgentTool
from foundationallm.models.constants import (
    ContentArtifactTypeNames,
    RunnableConfigKeys
)
from foundationallm.models.orchestration import CompletionRequestObjectKeys, ContentArtifact
from foundationallm.models.resource_providers.configuration import APIEndpointConfiguration
from foundationallm.services import HttpClientService
from foundationallm.utils import LoggingAsyncHttpClient

from .foundationallm_code_interpreter_tool_input import FoundationaLLMCodeInterpreterToolInput


# Skill content artifact type names (for procedural memory)
# These will be added to ContentArtifactTypeNames in the SDK
SKILL_SAVED_ARTIFACT_TYPE = "skill_saved"
SKILL_USED_ARTIFACT_TYPE = "skill_used"


class FoundationaLLMCodeInterpreterTool(FoundationaLLMToolBase):
    """
    A tool for executing Python code in a code interpreter.
    
    Supports procedural memory capabilities when enabled on the agent:
    - search_skills: Find relevant skills by semantic similarity
    - use_skill: Execute a previously saved skill
    - register_skill: Save successful code as a reusable skill
    """
    args_schema: Type[BaseModel] = FoundationaLLMCodeInterpreterToolInput
    DYNAMIC_SESSION_ENDPOINT: ClassVar[str] = "code_session_endpoint"
    DYNAMIC_SESSION_ID: ClassVar[str] = "code_session_id"

    def __init__(
        self,
        tool_config: AgentTool,
        objects: dict,
        user_identity: UserIdentity,
        config: Configuration,
        intercept_http_calls: bool = False
    ):
        """
        Initializes the FoundationaLLMCodeInterpreterTool class.
        
        Args:
            tool_config: The tool configuration from the agent.
            objects: The exploded objects collection from the completion request.
            user_identity: The user identity making the request.
            config: The platform configuration.
            intercept_http_calls: Whether to intercept HTTP calls for logging.
        """
        super().__init__(tool_config, objects, user_identity, config)

        context_api_endpoint_configuration = APIEndpointConfiguration(
            **objects.get(CompletionRequestObjectKeys.CONTEXT_API_ENDPOINT_CONFIGURATION, None)
        )
        if context_api_endpoint_configuration:
            self.context_api_client = HttpClientService(
                context_api_endpoint_configuration,
                user_identity,
                config
            )
        else:
            raise ToolException(
                "The Context API endpoint configuration is required to use the Code Interpreter tool."
            )
        
        self.instance_id = objects.get(CompletionRequestObjectKeys.INSTANCE_ID, None)
        self.main_llm = self.get_main_language_model(
            http_async_client=LoggingAsyncHttpClient(timeout=30.0)
        ) if intercept_http_calls else self.get_main_language_model()
        
        # Extract procedural memory settings from agent configuration
        self.agent = objects.get(CompletionRequestObjectKeys.AGENT, None)
        self.procedural_memory_enabled = self._get_procedural_memory_enabled()
        self.procedural_memory_settings = self._get_procedural_memory_settings()
    
    def _get_procedural_memory_enabled(self) -> bool:
        """Check if procedural memory is enabled for this agent."""
        if self.agent is None:
            return False
        settings = getattr(self.agent, 'procedural_memory_settings', None)
        if settings is None:
            return False
        return getattr(settings, 'enabled', False)
    
    def _get_procedural_memory_settings(self) -> Optional[Dict[str, Any]]:
        """Get the procedural memory settings for this agent."""
        if self.agent is None:
            return None
        settings = getattr(self.agent, 'procedural_memory_settings', None)
        if settings is None:
            return None
        return {
            'enabled': getattr(settings, 'enabled', False),
            'auto_register_skills': getattr(settings, 'auto_register_skills', True),
            'require_skill_approval': getattr(settings, 'require_skill_approval', False),
            'max_skills_per_user': getattr(settings, 'max_skills_per_user', 0),
            'skill_search_threshold': getattr(settings, 'skill_search_threshold', 0.8),
            'prefer_skills': getattr(settings, 'prefer_skills', True),
        }

    def _run(
        self,
        prompt: str,
        file_names: Optional[List[str]] = [],
        operation: str = "execute",
        skill_name: Optional[str] = None,
        skill_description: Optional[str] = None,
        skill_parameters: Optional[Dict[str, Any]] = None,
        run_manager: Optional[CallbackManagerForToolRun] = None,
        **kwargs: Any) -> Any:
        raise ToolException("This tool does not support synchronous execution. Please use the async version of the tool.")

    async def _arun(
        self,
        prompt: str,
        file_names: Optional[List[str]] = [],
        operation: str = "execute",
        skill_name: Optional[str] = None,
        skill_description: Optional[str] = None,
        skill_parameters: Optional[Dict[str, Any]] = None,
        run_manager: Optional[AsyncCallbackManagerForToolRun] = None,
        runnable_config: RunnableConfig = None,
        **kwargs: Any
    ) -> Tuple[str, FoundationaLLMToolResult]:
        """
        Execute the code interpreter tool.
        
        Supports multiple operations:
        - execute: Generate and run Python code (default, backwards compatible)
        - search_skills: Search for relevant skills (requires procedural memory)
        - use_skill: Execute a saved skill (requires procedural memory)
        - register_skill: Save code as a reusable skill (requires procedural memory)
        """
        # Route to appropriate operation handler
        # Backwards compatibility: if procedural memory is not enabled, always use execute
        if not self.procedural_memory_enabled or operation == "execute":
            return await self._execute_code(
                prompt=prompt,
                file_names=file_names,
                runnable_config=runnable_config
            )
        
        # Procedural memory operations
        if operation == "search_skills":
            return await self._search_skills(
                query=prompt,
                runnable_config=runnable_config
            )
        elif operation == "use_skill":
            return await self._use_skill(
                skill_name=skill_name,
                skill_parameters=skill_parameters or {},
                file_names=file_names,
                runnable_config=runnable_config
            )
        elif operation == "register_skill":
            return await self._register_skill(
                skill_name=skill_name,
                skill_description=skill_description,
                code=prompt,
                runnable_config=runnable_config
            )
        else:
            # Unknown operation - fall back to execute for backwards compatibility
            return await self._execute_code(
                prompt=prompt,
                file_names=file_names,
                runnable_config=runnable_config
            )

    async def _execute_code(
        self,
        prompt: str,
        file_names: Optional[List[str]] = [],
        runnable_config: RunnableConfig = None
    ) -> Tuple[str, FoundationaLLMToolResult]:
        """
        Execute the original code generation and execution flow.
        This is the backwards-compatible default operation.
        """
        main_prompt = self.get_main_prompt()

        # Get the original prompt
        if runnable_config is None:
            user_prompt = None
            user_prompt_rewrite = None
        else:
            user_prompt = runnable_config['configurable'][RunnableConfigKeys.ORIGINAL_USER_PROMPT] \
                if RunnableConfigKeys.ORIGINAL_USER_PROMPT in runnable_config['configurable'] \
                else None
            user_prompt_rewrite = runnable_config['configurable'][RunnableConfigKeys.ORIGINAL_USER_PROMPT_REWRITE] \
                if RunnableConfigKeys.ORIGINAL_USER_PROMPT_REWRITE in runnable_config['configurable'] \
                else None

        session_id = runnable_config['configurable'][self.tool_config.name][self.DYNAMIC_SESSION_ID]

        llm_prompt = prompt or user_prompt_rewrite or user_prompt
        content_artifacts = []
        operation_id = None
        input_tokens = 0
        output_tokens = 0
        generated_code = ''

        with self.tracer.start_as_current_span(f'{self.name}_initial_llm_call', kind=SpanKind.INTERNAL):

            available_file_names = '\n'.join([f'/{file_name}/' for file_name in file_names])
            code_generation_prompt = main_prompt.replace('{{file_names}}', available_file_names)

            messages = [
                SystemMessage(content=code_generation_prompt),
                HumanMessage(content=llm_prompt)
            ]

            response = await self.main_llm.ainvoke(messages)

            input_tokens += response.usage_metadata['input_tokens']
            output_tokens += response.usage_metadata['output_tokens']

            generated_code = self.__get_code_from_content_blocks(response.content_blocks)

        if generated_code.strip() == '':
            code_execution_response = {
                'status': 'Failed',
                'execution_result': '',
                'error_output': 'No code was generated by the language model to execute.',
                'standard_output': ''
            }
        else:
            # Start the process of executing the code in the code interpreter

            # returns the operation_id
            self.context_api_client.headers['X-USER-IDENTITY'] = self.user_identity.model_dump_json()
            operation_response = await self.context_api_client.post_async(
                endpoint = f"/instances/{self.instance_id}/codeSessions/{session_id}/uploadFiles",
                data = json.dumps({
                    "file_names": file_names
                })
            )
            operation_id = operation_response['operation_id']

            # Obtain beginning file list from the context API
            beginning_files_list = []
            beginning_files_list_response = await self.context_api_client.post_async(
                    endpoint = f"/instances/{self.instance_id}/codeSessions/{session_id}/downloadFiles",
                    data = json.dumps({
                        "operation_id": operation_id
                    })
                )
            beginning_files_list = beginning_files_list_response['file_records']

            try:
                # Execute the code
                code_execution_response = await self.context_api_client.post_async(
                    endpoint = f"/instances/{self.instance_id}/codeSessions/{session_id}/executeCode",
                    data = json.dumps({
                        "code_to_execute": generated_code
                    })
                )

                # Get an updated list of files from the code interpreter
                files_list = []
                if operation_id:
                    files_list_response = await self.context_api_client.post_async(
                        endpoint = f"/instances/{self.instance_id}/codeSessions/{session_id}/downloadFiles",
                        data = json.dumps({
                            "operation_id": operation_id
                        })
                    )
                    files_list = files_list_response['file_records']
                    # Remove files that were already present in the beginning of the session
                    files_list = {key: value for key, value in files_list.items() if key not in beginning_files_list}

                if files_list:
                    # Download the files from the code interpreter to the user storage container
                    for file_name, file_data in files_list.items():
                        content_artifacts.append(ContentArtifact(
                            id = self.name,
                            title = f'{self.name} (file)',
                            type = ContentArtifactTypeNames.FILE,
                            filepath = file_name,
                            metadata = {
                                'file_object_id': file_data['file_object_id'],
                                'original_file_name': file_data['file_name'],
                                'file_path': file_data['file_path'],
                                'file_size': str(file_data['file_size_bytes']),
                                'content_type': file_data['content_type'],
                                'conversation_id': file_data['conversation_id']
                            }
                        ))
            except Exception as e:
                # Handle cases where the code execution container does not get a chance to return
                # a response (e.g., crashes, timeouts, etc.)
                code_execution_response = {
                    'status': 'Failed',
                    'execution_result': '',
                    'error_output': str(e),
                    'standard_output': ''
                }

        final_response = ""
        if code_execution_response['status'] == 'Failed':
            final_response = '\n'.join([
                "The generated code could not be executed successfully. ",
                code_execution_response['execution_result'],
                code_execution_response.get('error_output', '')]
            )
        elif code_execution_response['status'] == 'Succeeded':
            if (code_execution_response.get('error_output') or '').strip() != '':
                # If there is error output, prioritize that
                final_response = code_execution_response.get('error_output', '')
            elif (code_execution_response.get('execution_result') or '') != '{}':
                final_response = code_execution_response.get('execution_result', '')
            else:
                final_response = (code_execution_response.get('standard_output') or '').strip()
            
            # If we have files but no output, provide context about what was created
            if not final_response.strip() and files_list:
                file_descriptions = [f"- {file_data['file_name']}" for file_data in files_list.values()]
                final_response = "Code executed successfully. The following files were created:\n" + "\n".join(file_descriptions)

        else:
            status = code_execution_response.get('status', 'Unknown')
            error_output = code_execution_response.get('error_output', '')
            standard_output = code_execution_response.get('standard_output', '')

            final_response = (
                f"Code execution failed with unexpected status '{status}'. "
                f"Error details: {error_output if error_output else 'No error details available'}. "
                f"Output: {standard_output if standard_output else 'No output available'}"
            )

        content_artifacts.append(ContentArtifact(
            id = self.name,
            title = self.name,
            type = ContentArtifactTypeNames.TOOL_EXECUTION,
            filepath = str(uuid4()), # needs to have a unique filepath to not be filtered out upstream.
            metadata = {
                'original_user_prompt': user_prompt_rewrite or user_prompt,
                'tool_input_prompt': prompt,
                'tool_input_files': ', '.join(file_names) if file_names else '',
                'tool_generated_code': generated_code,
                'tool_output': code_execution_response.get('standard_output', ''),
                'tool_error': code_execution_response.get('error_output', ''),
                'tool_result': final_response
            }
        ))

        return final_response, FoundationaLLMToolResult(
            content=final_response,
            content_artifacts=content_artifacts,
            input_tokens=input_tokens,
            output_tokens=output_tokens
        )

    def __get_code_from_content_blocks(self, content_blocks: List[dict]) -> str:
        """ Extracts code from content blocks returned by the LLM. """
        if isinstance(content_blocks, list):
            text_parts = [self.__prepare_code(block["text"]) for block in content_blocks if block.get("type") == "text"]
            text = "".join(text_parts)
            return text
        else:
            return ""

    def __prepare_code(self, code: str) -> str:
        """ Prepares the code for execution by removing any leading/trailing whitespace and ensuring it is valid code. """
        # Remove leading/trailing whitespace
        code = code.strip()
        # Ensure the code is valid Python code

        if code.startswith('```python'):
            code = code[9:].strip()
        if code.startswith('```'):
            code = code[3:].strip()
        if code.endswith('```'):
            code = code[:-3].strip()

        if not code.endswith('\n'):
            code += '\n'
        return code

    # =========================================================================
    # Procedural Memory (Skill) Operations
    # =========================================================================

    async def _search_skills(
        self,
        query: str,
        runnable_config: RunnableConfig = None
    ) -> Tuple[str, FoundationaLLMToolResult]:
        """
        Search for relevant skills using semantic similarity.
        
        Note: Skills are stored in Cosmos DB via Core API. This method retrieves
        skills for the current user filtered by agent, then performs local filtering.
        Full semantic search requires vector embeddings (future enhancement).
        
        Args:
            query: Natural language description of the desired skill.
            runnable_config: The runnable configuration.
            
        Returns:
            A tuple of (response text, tool result with content artifacts).
        """
        try:
            # Get user identity for scoping
            user_id = self.user_identity.upn if self.user_identity else None
            agent_object_id = getattr(self.agent, 'object_id', None) if self.agent else None
            
            if not user_id or not agent_object_id:
                return "Unable to search skills: missing user or agent context.", FoundationaLLMToolResult(
                    content="Unable to search skills: missing user or agent context.",
                    content_artifacts=[],
                    input_tokens=0,
                    output_tokens=0
                )
            
            # Get skills from Core API (stored in Cosmos DB)
            # Skills are automatically filtered by user (via authentication) and optionally by agent
            self.context_api_client.headers['X-USER-IDENTITY'] = self.user_identity.model_dump_json()
            skills_response = await self.context_api_client.get_async(
                endpoint=f"/instances/{self.instance_id}/skills?agentObjectId={agent_object_id}"
            )
            
            # skills_response is a list of SkillReference objects
            skills = skills_response if isinstance(skills_response, list) else []
            
            # Filter to only active skills
            active_skills = [s for s in skills if s.get('status') == 'Active']
            
            if not active_skills:
                response_text = "No matching skills found. You may need to generate new code."
            else:
                # Simple text-based matching for now (vector search is a future enhancement)
                query_lower = query.lower()
                matching_skills = []
                for skill in active_skills:
                    name = skill.get('name', '').lower()
                    description = (skill.get('description') or '').lower()
                    if query_lower in name or query_lower in description or any(
                        word in name or word in description for word in query_lower.split()
                    ):
                        matching_skills.append(skill)
                
                if not matching_skills:
                    # Return all active skills if no text match
                    matching_skills = active_skills[:5]
                
                skill_descriptions = []
                for skill in matching_skills[:5]:
                    skill_descriptions.append(
                        f"- {skill.get('name', 'Unknown')}: {skill.get('description', 'No description')}"
                    )
                response_text = f"Found {len(matching_skills)} skill(s):\n" + "\n".join(skill_descriptions)
            
            return response_text, FoundationaLLMToolResult(
                content=response_text,
                content_artifacts=[],
                input_tokens=0,
                output_tokens=0
            )
            
        except Exception as e:
            error_msg = f"Error searching skills: {str(e)}"
            return error_msg, FoundationaLLMToolResult(
                content=error_msg,
                content_artifacts=[],
                input_tokens=0,
                output_tokens=0
            )

    async def _use_skill(
        self,
        skill_name: str,
        skill_parameters: Dict[str, Any],
        file_names: Optional[List[str]] = [],
        runnable_config: RunnableConfig = None
    ) -> Tuple[str, FoundationaLLMToolResult]:
        """
        Execute a previously saved skill.
        
        Skills are stored in Cosmos DB via Core API and scoped to the agent-user combination.
        
        Args:
            skill_name: Name of the skill to execute.
            skill_parameters: Parameters to pass to the skill.
            file_names: Files to make available for the skill.
            runnable_config: The runnable configuration.
            
        Returns:
            A tuple of (response text, tool result with content artifacts).
        """
        content_artifacts = []
        
        try:
            # Get user identity for scoping
            user_id = self.user_identity.upn if self.user_identity else None
            agent_object_id = getattr(self.agent, 'object_id', None) if self.agent else None
            
            if not skill_name:
                return "Unable to use skill: skill_name is required.", FoundationaLLMToolResult(
                    content="Unable to use skill: skill_name is required.",
                    content_artifacts=[],
                    input_tokens=0,
                    output_tokens=0
                )
            
            # Build skill ID based on agent-user scoping
            safe_user_id = user_id.replace('@', '_').replace('.', '_') if user_id else 'unknown'
            safe_agent_id = agent_object_id.split('/')[-1] if agent_object_id else 'unknown'
            skill_id = f"{skill_name}_{safe_agent_id}_{safe_user_id}"
            
            # Retrieve the skill from Core API (Cosmos DB)
            self.context_api_client.headers['X-USER-IDENTITY'] = self.user_identity.model_dump_json()
            
            skill = await self.context_api_client.get_async(
                endpoint=f"/instances/{self.instance_id}/skills/{skill_id}"
            )
            
            skill_code = skill.get('code', '')
            
            if not skill_code:
                return f"Skill '{skill_name}' not found or has no code.", FoundationaLLMToolResult(
                    content=f"Skill '{skill_name}' not found or has no code.",
                    content_artifacts=[],
                    input_tokens=0,
                    output_tokens=0
                )
            
            # Check if skill is active
            if skill.get('status') != 'Active':
                return f"Skill '{skill_name}' is not active (status: {skill.get('status')}).", FoundationaLLMToolResult(
                    content=f"Skill '{skill_name}' is not active.",
                    content_artifacts=[],
                    input_tokens=0,
                    output_tokens=0
                )
            
            # Build the code to execute with parameters
            param_assignments = "\n".join([
                f"{key} = {json.dumps(value)}" for key, value in skill_parameters.items()
            ])
            
            full_code = f"{param_assignments}\n\n{skill_code}" if param_assignments else skill_code
            
            # Execute the skill code in the code session
            session_id = runnable_config['configurable'][self.tool_config.name][self.DYNAMIC_SESSION_ID]
            
            # Upload files if needed
            if file_names:
                await self.context_api_client.post_async(
                    endpoint=f"/instances/{self.instance_id}/codeSessions/{session_id}/uploadFiles",
                    data=json.dumps({"file_names": file_names})
                )
            
            # Execute the code
            code_execution_response = await self.context_api_client.post_async(
                endpoint=f"/instances/{self.instance_id}/codeSessions/{session_id}/executeCode",
                data=json.dumps({"code_to_execute": full_code})
            )
            
            # Build response
            execution_success = code_execution_response.get('status') == 'Succeeded'
            if execution_success:
                result = code_execution_response.get('execution_result', '')
                output = code_execution_response.get('standard_output', '')
                final_response = result if result and result != '{}' else output
            else:
                final_response = f"Skill execution failed: {code_execution_response.get('error_output', 'Unknown error')}"
            
            # Update skill execution statistics (fire and forget)
            try:
                await self.context_api_client.post_async(
                    endpoint=f"/instances/{self.instance_id}/skills/{skill_id}/execute",
                    data=json.dumps({"success": execution_success})
                )
            except Exception:
                pass  # Don't fail if stats update fails
            
            # Add skill_used content artifact for User Portal review
            content_artifacts.append(ContentArtifact(
                id="skill_used",
                title=f"Skill Used: {skill_name}",
                type=SKILL_USED_ARTIFACT_TYPE,
                filepath=skill_id,
                metadata={
                    'skill_id': skill_id,
                    'skill_name': skill_name,
                    'skill_description': skill.get('description', ''),
                    'skill_code': skill_code,
                    'skill_status': skill.get('status', 'Active'),
                    'execution_count': skill.get('execution_count', 0) + 1,
                    'success_rate': skill.get('success_rate', 1.0),
                    'agent_object_id': agent_object_id,
                    'user_id': user_id
                }
            ))
            
            return final_response, FoundationaLLMToolResult(
                content=final_response,
                content_artifacts=content_artifacts,
                input_tokens=0,
                output_tokens=0
            )
            
        except Exception as e:
            error_msg = f"Error using skill '{skill_name}': {str(e)}"
            return error_msg, FoundationaLLMToolResult(
                content=error_msg,
                content_artifacts=content_artifacts,
                input_tokens=0,
                output_tokens=0
            )

    async def _register_skill(
        self,
        skill_name: str,
        skill_description: str,
        code: str,
        runnable_config: RunnableConfig = None
    ) -> Tuple[str, FoundationaLLMToolResult]:
        """
        Register (save) code as a reusable skill.
        
        Skills are stored in Cosmos DB via Core API and scoped to the agent-user combination.
        
        Args:
            skill_name: Name for the new skill.
            skill_description: Description of what the skill does.
            code: The Python code to save as a skill.
            runnable_config: The runnable configuration.
            
        Returns:
            A tuple of (response text, tool result with content artifacts).
        """
        content_artifacts = []
        
        try:
            # Get user identity for scoping
            user_id = self.user_identity.upn if self.user_identity else None
            agent_object_id = getattr(self.agent, 'object_id', None) if self.agent else None
            
            if not skill_name or not code:
                return "Unable to register skill: skill_name and code are required.", FoundationaLLMToolResult(
                    content="Unable to register skill: skill_name and code are required.",
                    content_artifacts=[],
                    input_tokens=0,
                    output_tokens=0
                )
            
            # Prepare skill code (clean up markdown formatting if present)
            clean_code = self.__prepare_code(code)
            
            # Determine initial status based on settings
            require_approval = self.procedural_memory_settings.get('require_skill_approval', False) if self.procedural_memory_settings else False
            initial_status = "PendingApproval" if require_approval else "Active"
            
            # Build skill ID based on agent-user scoping
            safe_user_id = user_id.replace('@', '_').replace('.', '_') if user_id else 'unknown'
            safe_agent_id = agent_object_id.split('/')[-1] if agent_object_id else 'unknown'
            skill_id = f"{skill_name}_{safe_agent_id}_{safe_user_id}"
            
            # Create the skill document for Cosmos DB
            skill_data = {
                "id": skill_id,
                "type": "skill",
                "upn": user_id,
                "agent_object_id": agent_object_id,
                "name": skill_name,
                "description": skill_description or f"Skill: {skill_name}",
                "code": clean_code,
                "status": initial_status,
                "execution_count": 0,
                "success_rate": 1.0,
                "version": 1,
                "example_prompts": [],
                "parameters": [],
                "tags": []
            }
            
            self.context_api_client.headers['X-USER-IDENTITY'] = self.user_identity.model_dump_json()
            await self.context_api_client.post_async(
                endpoint=f"/instances/{self.instance_id}/skills",
                data=json.dumps(skill_data)
            )
            
            response_text = f"Skill '{skill_name}' has been saved and is ready to use."
            if require_approval:
                response_text = f"Skill '{skill_name}' has been saved and is pending approval."
            
            # Add skill_saved content artifact for User Portal review
            content_artifacts.append(ContentArtifact(
                id="skill_saved",
                title=f"Skill Saved: {skill_name}",
                type=SKILL_SAVED_ARTIFACT_TYPE,
                filepath=skill_id,
                metadata={
                    'skill_id': skill_id,
                    'skill_name': skill_name,
                    'skill_description': skill_data['description'],
                    'skill_code': clean_code,
                    'skill_status': initial_status,
                    'agent_object_id': agent_object_id,
                    'user_id': user_id
                }
            ))
            
            return response_text, FoundationaLLMToolResult(
                content=response_text,
                content_artifacts=content_artifacts,
                input_tokens=0,
                output_tokens=0
            )
            
        except Exception as e:
            error_msg = f"Error registering skill '{skill_name}': {str(e)}"
            return error_msg, FoundationaLLMToolResult(
                content=error_msg,
                content_artifacts=content_artifacts,
                input_tokens=0,
                output_tokens=0
            )

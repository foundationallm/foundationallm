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
    
    When procedural memory is enabled, the tool automatically manages skills:
    - Automatically searches for relevant skills when given a prompt
    - Uses skills if found and similarity exceeds threshold
    - Generates new code if no suitable skill is found
    - Optionally registers successful code as a skill
    
    Skill operations are internal - the tool interface remains simple (prompt + file_names only).
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
        
        # Extract procedural memory settings from tool configuration
        self.procedural_memory_enabled = self._get_procedural_memory_enabled()
        self.procedural_memory_settings = self._get_procedural_memory_settings()
    
    def _get_procedural_memory_enabled(self) -> bool:
        """Check if procedural memory is enabled for this agent."""
        settings = self._get_procedural_memory_settings()
        if settings is None:
            return False
        return settings.get('enabled', False)
    
    def _get_procedural_memory_settings(self) -> Optional[Dict[str, Any]]:
        """Get the procedural memory settings for this tool."""
        if not getattr(self, "tool_config", None) or not self.tool_config.properties:
            return None

        pm_settings = self.tool_config.properties.get("procedural_memory_settings")
        if not pm_settings:
            return None

        if isinstance(pm_settings, str):
            try:
                pm_settings = json.loads(pm_settings)
            except json.JSONDecodeError:
                return None

        if not isinstance(pm_settings, dict):
            return None

        return {
            'enabled': pm_settings.get('enabled', False),
            'auto_register_skills': pm_settings.get('auto_register_skills', True),
            'require_skill_approval': pm_settings.get('require_skill_approval', False),
            'max_skills_per_user': pm_settings.get('max_skills_per_user', 0),
            'skill_search_threshold': pm_settings.get('skill_search_threshold', 0.8),
            'prefer_skills': pm_settings.get('prefer_skills', True),
        }

    def _run(
        self,
        prompt: str,
        file_names: Optional[List[str]] = [],
        run_manager: Optional[CallbackManagerForToolRun] = None,
        **kwargs: Any) -> Any:
        raise ToolException("This tool does not support synchronous execution. Please use the async version of the tool.")

    async def _arun(
        self,
        prompt: str,
        file_names: Optional[List[str]] = [],
        run_manager: Optional[AsyncCallbackManagerForToolRun] = None,
        runnable_config: RunnableConfig = None,
        **kwargs: Any
    ) -> Tuple[str, FoundationaLLMToolResult]:
        """
        Execute the code interpreter tool.
        
        When procedural memory is enabled, the tool automatically:
        1. Searches for relevant skills matching the prompt
        2. Uses a skill if found and similarity > threshold
        3. Generates new code if no suitable skill is found
        4. Optionally registers successful code as a skill
        
        When procedural memory is disabled, the tool generates and executes code directly.
        """
        # Backwards compatibility: if procedural memory is not enabled, execute code directly
        if not self.procedural_memory_enabled:
            return await self._execute_code(
                prompt=prompt,
                file_names=file_names,
                runnable_config=runnable_config
            )
        
        # Procedural memory enabled - handle skill management internally
        return await self._execute_with_procedural_memory(
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

    async def _execute_with_procedural_memory(
        self,
        prompt: str,
        file_names: Optional[List[str]] = [],
        runnable_config: RunnableConfig = None
    ) -> Tuple[str, FoundationaLLMToolResult]:
        """
        Execute code with procedural memory enabled.
        
        Internal flow:
        1. Search for relevant skills matching the prompt
        2. Use skill if found and similarity > threshold
        3. Generate new code if no suitable skill found
        4. Optionally register successful code as a skill
        """
        settings = self.procedural_memory_settings or {}
        prefer_skills = settings.get('prefer_skills', True)
        search_threshold = settings.get('skill_search_threshold', 0.8)
        auto_register = settings.get('auto_register_skills', True)
        
        # Step 1: Search for relevant skills (if prefer_skills is enabled)
        matching_skill = None
        if prefer_skills:
            matching_skills = await self._search_skills_internal(prompt)
            if matching_skills and len(matching_skills) > 0:
                # Use the best matching skill if similarity exceeds threshold
                best_match = matching_skills[0]
                similarity = best_match.get('similarity', 0.0)
                if similarity >= search_threshold:
                    matching_skill = best_match
        
        # Step 2: Use skill if found
        if matching_skill:
            return await self._use_skill_internal(
                skill=matching_skill,
                prompt=prompt,
                file_names=file_names,
                runnable_config=runnable_config
            )
        
        # Step 3: Generate and execute new code
        result = await self._execute_code(
            prompt=prompt,
            file_names=file_names,
            runnable_config=runnable_config
        )
        
        # Step 4: Optionally register successful code as a skill
        if auto_register and result.content and not result.content.startswith("The generated code could not be executed"):
            # Check if execution was successful (basic heuristic)
            execution_successful = (
                "Failed" not in result.content and
                "error" not in result.content.lower() and
                len(result.content_artifacts) > 0
            )
            
            if execution_successful:
                # Extract generated code from content artifacts
                generated_code = None
                for artifact in result.content_artifacts:
                    if artifact.type == ContentArtifactTypeNames.TOOL_EXECUTION:
                        generated_code = artifact.metadata.get('tool_generated_code')
                        break
                
                if generated_code and generated_code.strip():
                    # Auto-generate skill name and description from prompt
                    skill_name = self._generate_skill_name(prompt)
                    skill_description = self._generate_skill_description(prompt, generated_code)
                    
                    # Register skill and merge skill_saved artifact into result
                    registration_result = await self._register_skill_internal(
                        skill_name=skill_name,
                        skill_description=skill_description,
                        code=generated_code,
                        runnable_config=runnable_config
                    )
                    
                    # Merge skill_saved content artifact into the result for user review
                    if registration_result and registration_result.content_artifacts:
                        result.content_artifacts.extend(registration_result.content_artifacts)
        
        return result

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
    # Procedural Memory (Skill) Operations - Internal Methods
    # =========================================================================

    async def _search_skills_internal(
        self,
        query: str
    ) -> List[Dict[str, Any]]:
        """
        Internal method to search for relevant skills.
        Returns a list of matching skills with similarity scores.
        """
        try:
            # Get user identity for scoping
            user_id = self.user_identity.upn if self.user_identity else None
            agent_object_id = getattr(self.agent, 'object_id', None) if self.agent else None
            
            if not user_id or not agent_object_id:
                return []
            
            # Get skills from Core API (stored in Cosmos DB)
            self.context_api_client.headers['X-USER-IDENTITY'] = self.user_identity.model_dump_json()
            skills_response = await self.context_api_client.get_async(
                endpoint=f"/instances/{self.instance_id}/skills?agentObjectId={agent_object_id}"
            )
            
            # skills_response is a list of SkillReference objects
            skills = skills_response if isinstance(skills_response, list) else []
            
            # Filter to only active skills
            active_skills = [s for s in skills if s.get('status') == 'Active']
            
            if not active_skills:
                return []
            
            # Simple text-based matching for now (vector search is a future enhancement)
            query_lower = query.lower()
            matching_skills = []
            for skill in active_skills:
                name = skill.get('name', '').lower()
                description = (skill.get('description') or '').lower()
                
                # Calculate simple similarity score (0.0 to 1.0)
                similarity = 0.0
                query_words = set(query_lower.split())
                
                # Check name match
                if query_lower in name:
                    similarity = 0.9
                elif any(word in name for word in query_words):
                    similarity = 0.7
                
                # Check description match
                if query_lower in description:
                    similarity = max(similarity, 0.8)
                elif any(word in description for word in query_words):
                    similarity = max(similarity, 0.6)
                
                if similarity > 0.0:
                    matching_skills.append({
                        'skill': skill,
                        'similarity': similarity
                    })
            
            # Sort by similarity (highest first)
            matching_skills.sort(key=lambda x: x['similarity'], reverse=True)
            
            return matching_skills
            
        except Exception:
            return []

    async def _use_skill_internal(
        self,
        skill: Dict[str, Any],
        prompt: str,
        file_names: Optional[List[str]] = [],
        runnable_config: RunnableConfig = None
    ) -> Tuple[str, FoundationaLLMToolResult]:
        """
        Internal method to execute a skill.
        Returns execution results with skill_used content artifact.
        """
        skill_data = skill.get('skill', {})
        skill_name = skill_data.get('name', '')
        
        # Extract parameters from prompt if needed (simple heuristic)
        # In a more sophisticated implementation, we could use an LLM to extract parameters
        skill_parameters = self._extract_skill_parameters(prompt, skill_data)
        
        return await self._use_skill(
            skill_name=skill_name,
            skill_parameters=skill_parameters,
            file_names=file_names,
            runnable_config=runnable_config
        )

    async def _register_skill_internal(
        self,
        skill_name: str,
        skill_description: str,
        code: str,
        runnable_config: RunnableConfig = None
    ) -> Optional[FoundationaLLMToolResult]:
        """
        Internal method to register a skill.
        Returns the tool result with skill_saved content artifact for user review.
        
        Returns None if registration fails (non-blocking).
        """
        try:
            _, result = await self._register_skill(
                skill_name=skill_name,
                skill_description=skill_description,
                code=code,
                runnable_config=runnable_config
            )
            return result
        except Exception:
            # Don't raise exceptions for internal skill registration
            # Return None to indicate registration failed (non-blocking)
            return None

    def _generate_skill_name(self, prompt: str) -> str:
        """
        Generate a skill name from a prompt.
        Simple heuristic - can be enhanced with LLM in the future.
        """
        # Extract key words and create a snake_case name
        words = prompt.lower().split()
        # Remove common words
        stop_words = {'the', 'a', 'an', 'and', 'or', 'but', 'in', 'on', 'at', 'to', 'for', 'of', 'with', 'by', 'from', 'as', 'is', 'was', 'are', 'were', 'be', 'been', 'being', 'have', 'has', 'had', 'do', 'does', 'did', 'will', 'would', 'should', 'could', 'may', 'might', 'must', 'can', 'this', 'that', 'these', 'those'}
        key_words = [w for w in words if w not in stop_words and len(w) > 2][:5]
        skill_name = '_'.join(key_words) if key_words else 'generated_skill'
        # Sanitize for valid identifier
        skill_name = ''.join(c if c.isalnum() or c == '_' else '_' for c in skill_name)
        return skill_name[:50]  # Limit length

    def _generate_skill_description(self, prompt: str, code: str) -> str:
        """
        Generate a skill description from prompt and code.
        Simple heuristic - can be enhanced with LLM in the future.
        """
        # Use the prompt as the description, truncated if too long
        description = prompt[:200] if len(prompt) <= 200 else prompt[:197] + "..."
        return description

    def _extract_skill_parameters(self, prompt: str, skill_data: Dict[str, Any]) -> Dict[str, Any]:
        """
        Extract parameters for a skill from the prompt.
        Simple heuristic - can be enhanced with LLM or pattern matching in the future.
        """
        # For now, return empty dict - parameters would need to be extracted intelligently
        # This is a placeholder for future enhancement
        return {}

    # =========================================================================
    # Procedural Memory (Skill) Operations - Legacy Methods (kept for reference)
    # =========================================================================

    async def _search_skills(
        self,
        query: str,
        runnable_config: RunnableConfig = None
    ) -> Tuple[str, FoundationaLLMToolResult]:
        """
        Legacy method: Search for relevant skills using semantic similarity.
        
        Note: This method is kept for backwards compatibility but is no longer
        called directly by agents. The tool now uses _search_skills_internal()
        which is called automatically when procedural memory is enabled.
        
        Args:
            query: Natural language description of the desired skill.
            runnable_config: The runnable configuration.
            
        Returns:
            A tuple of (response text, tool result with content artifacts).
        """
        matching_skills = await self._search_skills_internal(query)
        
        if not matching_skills:
            response_text = "No matching skills found. You may need to generate new code."
        else:
            skill_descriptions = []
            for match in matching_skills[:5]:
                skill = match.get('skill', {})
                similarity = match.get('similarity', 0.0)
                skill_descriptions.append(
                    f"- {skill.get('name', 'Unknown')} (similarity: {similarity:.2f}): {skill.get('description', 'No description')}"
                )
            response_text = f"Found {len(matching_skills)} skill(s):\n" + "\n".join(skill_descriptions)
        
        return response_text, FoundationaLLMToolResult(
            content=response_text,
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

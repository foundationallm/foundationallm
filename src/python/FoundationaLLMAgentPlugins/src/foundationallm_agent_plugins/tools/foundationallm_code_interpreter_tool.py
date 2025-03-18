from typing import Optional, Tuple, Type, List, ClassVar
from uuid import uuid4
import json

from langchain_azure_dynamic_sessions import SessionsPythonREPLTool
from langchain_core.callbacks import CallbackManagerForToolRun, AsyncCallbackManagerForToolRun
from langchain_core.runnables import RunnableConfig
from langchain_core.tools import ToolException

from pydantic import BaseModel, Field

from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import FoundationaLLMToolBase
from foundationallm.models.agents import AgentTool
from foundationallm.models.orchestration import ContentArtifact
from foundationallm.storage import BlobStorageManager
from foundationallm_agent_plugins.common.constants import CONTENT_ARTIFACT_TYPE_TOOL_EXECUTION

class FoundationaLLMCodeInterpreterFile(BaseModel):
    """ A file to upload to the code interpreter. """
    path: str = Field(description="The path to the file.")
    original_file_name: str = Field(description="The original file name of the file.")

class FoundationaLLMCodeInterpreterToolInput(BaseModel):
    """ Input data model for the Code Intepreter tool. """
    python_code: str = Field(description="The Python code to execute.")
    files: Optional[List[FoundationaLLMCodeInterpreterFile]] = Field(description="The file paths of the files to upload to the code interpreter. This can be code files or data files.")

class FoundationaLLMCodeInterpreterTool(FoundationaLLMToolBase):
    """ A tool for executing Python code in a code interpreter. """
    args_schema: Type[BaseModel] = FoundationaLLMCodeInterpreterToolInput
    BLOB_STORAGE_CONTAINER_NAME: ClassVar[str] = "resource-provider"

    def __init__(self, tool_config: AgentTool, objects: dict, user_identity:UserIdentity, config: Configuration):
        """ Initializes the FoundationaLLMCodeInterpreterTool class with the tool configuration,
            exploded objects collection, user_identity, and platform configuration. """
        super().__init__(tool_config, objects, user_identity, config)
        self.repl = SessionsPythonREPLTool(   
            session_id=tool_config.properties['session_id'] if 'session_id' in tool_config.properties else str(uuid4()),
            pool_management_endpoint=tool_config.properties['pool_management_endpoint']
        )
        self.description = tool_config.description or self.repl.description
        # Setup storage client
        authentication_type = config.get_value('FoundationaLLM:ResourceProviders:Attachment:Storage:AuthenticationType')
        if authentication_type == "AzureIdentity":
            self.storage_client = BlobStorageManager(
                container_name=self.BLOB_STORAGE_CONTAINER_NAME,
                account_name=config.get_value('FoundationaLLM:ResourceProviders:Attachment:Storage:AccountName'),
                authentication_type="AzureIdentity"
            )
        else:
            self.storage_client = BlobStorageManager(
                container_name=self.BLOB_STORAGE_CONTAINER_NAME,
                account_name=config.get_value('FoundationaLLM:ResourceProviders:Attachment:Storage:ConnectionString')
            )        
    
    def _run(self,                 
            python_code: str = None,   
            files: Optional[List[FoundationaLLMCodeInterpreterFile]] = None,
            run_manager: Optional[CallbackManagerForToolRun] = None
            ) -> str:
        raise ToolException("This tool does not support synchronous execution. Please use the async version of the tool.")
    
    async def _arun(self,                 
            python_code: str = None,
            files: Optional[List[FoundationaLLMCodeInterpreterFile]] = None,
            run_manager: Optional[AsyncCallbackManagerForToolRun] = None,
            runnable_config: RunnableConfig = None) -> Tuple[str, List[ContentArtifact]]:
        # SessionsPythonREPLTool only supports synchronous execution.
        # Get the original prompt
        original_prompt = python_code
        if runnable_config is not None and 'original_user_prompt' in runnable_config['configurable']:        
            original_prompt = runnable_config['configurable']['original_user_prompt']

        # Upload any files to the code interpreter
        if files:
            for file in files:  
                original_file_path = file.path
                # if the file path begins with the container name, remove it
                if file.path.startswith(self.BLOB_STORAGE_CONTAINER_NAME):
                    file.path = file.path[len(self.BLOB_STORAGE_CONTAINER_NAME) + 1:]
                # Get the byte stream of the file through the storage client
                file_bytes = self.storage_client.read_file_content(file.path, read_into_stream=True)
                upload_result = self.repl.upload_file(data=file_bytes, remote_file_path=file.original_file_name)
                # Replace the file path with the remote path in the python code
                python_code = python_code.replace(original_file_path, upload_result.full_path)

        result = self.repl.invoke(python_code)
        response = json.loads(result)
        content = str(response.get('result', '')) or str(response.get('stdout', '')) or str(response.get('stderr', ''))
        content_artifact = ContentArtifact(
            id = self.name,
            title = self.name,
            type = CONTENT_ARTIFACT_TYPE_TOOL_EXECUTION,
            content = content,
            metadata = {
                'original_user_prompt': original_prompt,
                'tool_input': python_code,
                'tool_output': str(response.get('stdout', '')),
                'tool_error': str(response.get('stderr', '')),
                'tool_result': str(response.get('result', ''))
            }
        )
        return content, [content_artifact]

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
from foundationallm_agent_plugins.common.constants import CONTENT_ARTIFACT_TYPE_TOOL_EXECUTION, CONTENT_ARTIFACT_TYPE_FILE

class FoundationaLLMCodeInterpreterFile(BaseModel):
    """ A file to upload to the code interpreter. """
    path: str = Field(
        description="The full path to the file in the storage container. Example: 'resource-provider/FoundationaLLM.Attachment/abc123.py'"
    )
    original_file_name: str = Field(
        description="The original name of the file as it should appear in the code interpreter. Example: 'test.py'"
    )
    local_file_path: str = Field(
        description="The local path once the file is uploaded to the code interpreter. This path is in the format '/mnt/data/original_file_name'. Example: '/mnt/data/test.py'"
    )

class FoundationaLLMCodeInterpreterToolInput(BaseModel):
    """ Input data model for the Code Intepreter tool. """
    python_code: str = Field(
        description="The Python code to execute. This should be the complete code including any file operations."
    )
    files: Optional[List[FoundationaLLMCodeInterpreterFile]] = Field(
        default=None,
        description="""List of files to upload to the code interpreter. Each file should have:
        - path: The full path to the file in the storage container
        - original_file_name: The name the file should have in the code interpreter
        - local_file_path: The local path once the file is uploaded to the code interpreter. This path is in the format '/mnt/data/original_file_name'. Example: '/mnt/data/test.py'
        Example:
        {
            "python_code": "import pandas as pd\n\ndf = pd.read_csv('/mnt/data/data.csv')",
            "files": [
                {
                    "path": "resource-provider/FoundationaLLM.Attachment/abc123.csv",
                    "original_file_name": "data.csv",
                    "local_file_path": "/mnt/data/data.csv"
                }
            ]
        }
        """
    )

class FoundationaLLMCodeInterpreterTool(FoundationaLLMToolBase):
    """ A tool for executing Python code in a code interpreter. """
    args_schema: Type[BaseModel] = FoundationaLLMCodeInterpreterToolInput
    DYNAMIC_SESSION_ENDPOINT: ClassVar[str] = "foundationallm_aca_code_execution_endpoint"
    DYNAMIC_SESSION_ID: ClassVar[str] = "foundationallm_aca_code_execution_session_id"

    def __init__(self, tool_config: AgentTool, objects: dict, user_identity:UserIdentity, config: Configuration):
        """ Initializes the FoundationaLLMCodeInterpreterTool class with the tool configuration,
            exploded objects collection, user_identity, and platform configuration. """
        super().__init__(tool_config, objects, user_identity, config)
        self.repl = SessionsPythonREPLTool(   
            session_id=tool_config.properties[self.DYNAMIC_SESSION_ID] if self.DYNAMIC_SESSION_ID in tool_config.properties else str(uuid4()),
            pool_management_endpoint=tool_config.properties[self.DYNAMIC_SESSION_ENDPOINT]
        )
        self.description = tool_config.description or self.repl.description       
    
    def _run(self,                 
            python_code: str,
            files: Optional[List[FoundationaLLMCodeInterpreterFile]],
            run_manager: Optional[CallbackManagerForToolRun] = None
            ) -> str:
        raise ToolException("This tool does not support synchronous execution. Please use the async version of the tool.")
    
    async def _arun(self,                 
            python_code: str,
            files: Optional[List[FoundationaLLMCodeInterpreterFile]] = None,
            run_manager: Optional[AsyncCallbackManagerForToolRun] = None,
            runnable_config: RunnableConfig = None) -> Tuple[str, List[ContentArtifact]]:
        # SessionsPythonREPLTool only supports synchronous execution.
        # Get the original prompt
        original_prompt = python_code
        if runnable_config is not None and 'original_user_prompt' in runnable_config['configurable']:        
            original_prompt = runnable_config['configurable']['original_user_prompt']

        content_artifacts = []
        # Upload any files to the code interpreter
        ## TO DO: Add context api to upload files to the dynamic session using the list of files.
               
        # Execute the code
        result = self.repl.invoke(python_code)

        # Get the list of files from the code interpreter
        files_list = self.repl.list_files()
        if files_list:
            # Disregard the files in the code interpreter that were uploaded (based on the original file name)
            generated_files_list = [file for file in files_list if file.filename not in {f.original_file_name for f in (files or [])}]            
            # Download the files from the code interpreter to the user storage container
            # TO DO: Call into the context api to create the files
            for generated_file in generated_files_list:                
                content_artifacts.append(ContentArtifact(
                    id = self.name,
                    title = self.name,
                    type = CONTENT_ARTIFACT_TYPE_FILE,
                    source = "TBD",
                    metadata = {}
                ))

        response = json.loads(result)
        content = str(response.get('result', '')) or str(response.get('stdout', '')) or str(response.get('stderr', ''))
        content_artifacts.append(ContentArtifact(
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
        ))        
        return content, content_artifacts

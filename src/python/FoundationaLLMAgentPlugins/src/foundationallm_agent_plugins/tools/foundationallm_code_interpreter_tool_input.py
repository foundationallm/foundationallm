from typing import Optional, List

from pydantic import BaseModel, Field

class FoundationaLLMCodeInterpreterToolInput(BaseModel):
    """ Input data model for the Code Intepreter tool. """
    prompt: str = Field(
        description="The prompt used by the tool to generate the Python code."
    )
    file_names: List[str] = Field(
        default=[],
        description="List of file names required to provide the response."
    )

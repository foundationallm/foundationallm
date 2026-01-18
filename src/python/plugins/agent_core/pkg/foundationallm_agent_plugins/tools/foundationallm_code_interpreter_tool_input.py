from typing import Optional, List

from pydantic import BaseModel, Field

class FoundationaLLMCodeInterpreterToolInput(BaseModel):
    """
    Input data model for the Code Interpreter tool.
    
    When procedural memory is enabled, the tool automatically:
    - Searches for relevant skills matching the prompt
    - Uses skills if found and suitable (similarity > threshold)
    - Generates new code if no suitable skill is found
    - Optionally registers successful code as a skill
    
    Skill operations are handled internally - no explicit parameters needed.
    """
    prompt: str = Field(
        description="The prompt used by the tool to generate the Python code. "
                   "When procedural memory is enabled, the tool will automatically "
                   "search for relevant skills before generating new code."
    )
    file_names: List[str] = Field(
        default=[],
        description="List of file names required to provide the response."
    )

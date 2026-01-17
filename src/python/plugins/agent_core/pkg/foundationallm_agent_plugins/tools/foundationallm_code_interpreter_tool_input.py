from typing import Optional, List, Dict, Any

from pydantic import BaseModel, Field

class FoundationaLLMCodeInterpreterToolInput(BaseModel):
    """
    Input data model for the Code Interpreter tool.
    
    Supports both standard code execution and procedural memory operations
    (skill search, use, and registration) when procedural memory is enabled.
    """
    prompt: str = Field(
        description="The prompt used by the tool to generate the Python code, or the search query for skills."
    )
    file_names: List[str] = Field(
        default=[],
        description="List of file names required to provide the response."
    )
    
    # Skill-related parameters (only used when procedural memory is enabled)
    operation: str = Field(
        default="execute",
        description="Operation to perform: 'execute' (default - generate and run code), "
                    "'search_skills' (find relevant skills), "
                    "'use_skill' (run a saved skill), "
                    "'register_skill' (save code as a reusable skill)"
    )
    skill_name: Optional[str] = Field(
        default=None,
        description="Name of the skill to use or register."
    )
    skill_description: Optional[str] = Field(
        default=None,
        description="Description for skill registration (used for semantic search)."
    )
    skill_parameters: Optional[Dict[str, Any]] = Field(
        default=None,
        description="Parameters to pass when using a skill."
    )

from typing import List, Optional, Union
from pydantic import BaseModel
from .citation import Citation

class CompletionResponse(BaseModel):
    """
    Response from a language model.
    """
    user_prompt: str
    full_prompt: Optional[str] = None
    completion: Union[str, set, List[str]]
    citations: Optional[List[Citation]] = []
    user_prompt_embedding: Optional[List[float]] = []
    prompt_tokens: int = 0
    completion_tokens: int = 0
    total_tokens: int = 0
    total_cost: float = 0.0

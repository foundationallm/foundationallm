from pydantic import BaseModel
from typing import List, Union

class CompletionResponse(BaseModel):
    """
    Response from a language model.
    """
    user_prompt: str
    completion: Union[str, set, List[str]]
    user_prompt_embedding: List[float] = list()
    prompt_tokens: int = 0
    completion_tokens: int = 0
    total_tokens: int = 0
    total_cost: float = 0.0
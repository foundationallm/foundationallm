from typing import List, Union
from .orchestration_response import OrchestrationResponse

class CompletionResponse(OrchestrationResponse):
    """
    Response from a language model.
    """
    
    completion: Union[str, set, List[str]]
    user_prompt_embedding: List[float] = list()
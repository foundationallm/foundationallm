from typing import List, Optional, Union, Annotated
from pydantic import BaseModel, Field

from foundationallm.models.orchestration import (
    AnalysisResult,
    ContentArtifact,
    OpenAIImageFileMessageContentItem,
    OpenAITextMessageContentItem
)

class CompletionResponse(BaseModel):
    """
    Response from a language model.
    """
    id: Optional[str] = None
    operation_id: str
    user_prompt: str
    user_prompt_rewrite: Optional[str] = None
    full_prompt: Optional[str] = None
    completion: Optional[str] = None
    content: Optional[
        List[
            Annotated[
                Union[
                    OpenAIImageFileMessageContentItem,
                    OpenAITextMessageContentItem
                ],
                Field(discriminator='type')
            ]
        ]
    ] = None
    analysis_results: Optional[List[AnalysisResult]] = []
    content_artifacts: Optional[List[ContentArtifact]] = []
    conversation_name: Optional[str] = None
    user_prompt_embedding: Optional[List[float]] = []
    prompt_tokens: int = 0
    completion_tokens: int = 0
    total_tokens: int = 0
    total_cost: float = 0.0
    errors: Optional[List[str]] = []
    is_error: bool = False

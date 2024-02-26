from typing import List, Optional
from pydantic import Field
from foundationallm.models.metadata import Agent
from foundationallm.models.metadata import DataSource
from foundationallm.models.language_models import LanguageModel, EmbeddingModel
from .completion_request_base import CompletionRequestBase

class CompletionRequest(CompletionRequestBase):
    """
    Orchestration completion request.
    """
    type: Optional[str] = Field(None, alias='$type')
    agent: Optional[Agent] = None
    data_sources: Optional[List[DataSource]] = []
    language_model: Optional[LanguageModel] = None
    embedding_model: Optional[EmbeddingModel] = None    

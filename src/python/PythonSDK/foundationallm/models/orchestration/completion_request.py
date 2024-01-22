from typing import List, Optional
from pydantic import BaseModel
from foundationallm.models.orchestration import MessageHistoryItem
from foundationallm.models.metadata import Agent
from foundationallm.models.metadata import DataSource
from foundationallm.models.language_models import LanguageModel, EmbeddingModel

class CompletionRequest(BaseModel):
    """
    Orchestration completion request.
    """
    user_prompt: str
    agent: Optional[Agent] = None
    data_sources: Optional[List[DataSource]] = []
    language_model: Optional[LanguageModel] = None
    embedding_model: Optional[EmbeddingModel] = None
    message_history: Optional[List[MessageHistoryItem]] = []

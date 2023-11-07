from pydantic import BaseModel
from typing import List, Optional

from foundationallm.models.orchestration import MessageHistoryItem
from foundationallm.models.metadata import Agent
from foundationallm.models.metadata import DataSource
from foundationallm.models.metadata import LanguageModel

class CompletionRequest(BaseModel):
    """
    Orchestration completion request.
    """
    user_prompt: str
    agent: Optional[Agent] = None
    data_source: Optional[DataSource] = None
    language_model: Optional[LanguageModel] = None
    message_history: Optional[List[MessageHistoryItem]] = list()
import uuid
from typing import List, Optional
from pydantic import BaseModel
from foundationallm.models.orchestration import MessageHistoryItem, OrchestrationSettings

class CompletionRequestBase(BaseModel):
    """
    Orchestration completion request.
    """
    request_id: Optional[str] = str(uuid.uuid4())
    session_id: Optional[str] = None
    user_prompt: str
    attachments: Optional[List[str]] = None
    message_history: Optional[List[MessageHistoryItem]] = []
    settings: Optional[OrchestrationSettings] = None

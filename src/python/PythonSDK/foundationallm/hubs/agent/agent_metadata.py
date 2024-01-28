from typing import List, Optional
from foundationallm.hubs import Metadata
from foundationallm.models.language_models import EmbeddingModel, LanguageModel
from .agent_type import AgentType

class AgentMetadata(Metadata):
    """Class representing the metadata for an agent."""
    name: str
    description: str
    type: AgentType
    allowed_data_source_names: Optional[List[str]] = None
    embedding_model: Optional[EmbeddingModel] = None
    language_model: Optional[LanguageModel] = None
    orchestrator: Optional[str] = None
    max_message_history_size: Optional[int] = None
    prompt_container: Optional[str] = None

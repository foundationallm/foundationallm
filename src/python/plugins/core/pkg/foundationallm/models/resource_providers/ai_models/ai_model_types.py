from enum import Enum

class AIModelTypes(str, Enum):
    """Enumerator of the AI Model Types."""
    COMPLETION = "completion"
    EMBEDDING = "embedding"

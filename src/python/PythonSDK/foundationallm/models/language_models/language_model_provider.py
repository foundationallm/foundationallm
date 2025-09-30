from enum import Enum

class LanguageModelProvider(str, Enum):
    """Enumerator of the Language Model providers."""
    AZUREAI = "azureai"
    MICROSOFT = "microsoft"
    OPENAI = "openai"
    BEDROCK = "bedrock"
    VERTEXAI = "vertexai"
    DATABRICKS = "databricks"

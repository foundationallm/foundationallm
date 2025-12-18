from enum import Enum

class AttachmentProviders(str, Enum):
    """Enumerator of the Attachment Providers."""
    FOUNDATIONALLM_ATTACHMENT = "FoundationaLLM.Attachment"
    FOUNDATIONALLM_AZURE_OPENAI = "FoundationaLLM.AzureOpenAI"

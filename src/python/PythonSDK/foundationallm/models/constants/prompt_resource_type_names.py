from enum import Enum

class PromptResourceTypeNames(str, Enum):
    """The names of the resource types managed by the FoundationaLLM.Prompt resource provider."""
    PROMPTS = 'prompts'
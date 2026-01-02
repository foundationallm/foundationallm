from enum import Enum

class AgentCapabilityCategories(str, Enum):
    """Enumerator of the Agent Capability Categories."""
    AZURE_AI_AGENTS = "AzureAI.AgentService"
    OPENAI_ASSISTANTS = "OpenAI.Assistants"
    FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT = "FoundationaLLM.KnowledgeManagement"
    FOUNDATIONALLM_IMAGE_GENERATION = "FoundationaLLM.ImageGeneration"

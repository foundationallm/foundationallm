from langchain_core.language_models import BaseLanguageModel
from foundationallm.config import Configuration
from foundationallm.langchain.language_models.openai import OpenAIModel
from foundationallm.models.orchestration import OrchestrationSettings

class LanguageModelFactory:
    """
    Factory class for determine which language models to use.
    """
    def __init__(self, orchestration_settings: OrchestrationSettings, config: Configuration):
        self.config = config

    def get_llm(self) -> BaseLanguageModel:
        """
        Retrieves the language model to use for completion and embedding requests.
        
        Returns
        -------
        BaseLanguageModel
            Returns the language model to use for completion and embedding requests.
        """
        return OpenAIModel(config = self.config)

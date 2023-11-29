from abc import ABC, abstractmethod
from langchain.base_language import BaseLanguageModel
from langchain.embeddings.base import Embeddings
from foundationallm.config import Configuration
from foundationallm.models.language_models import EmbeddingModel, LanguageModel

class LanguageModelBase(ABC):
    """Abstract base class for language models."""

    def __init__(self, config: Configuration):
        """
        Initializer
        
        Parameters
        ----------
        app_config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        self.config = config

    @abstractmethod
    def get_completion_model(self, language_model: LanguageModel) -> BaseLanguageModel:
        """
        Retrieve the completion model.
        
        Returns
        -------
        BaseLanguageModel
            The completion large language model to use.
        """

    @abstractmethod
    def get_embedding_model(self, embedding_model: EmbeddingModel) -> Embeddings:
        """
        Retrieve the embeddings model.
        
        Returns
        -------
        Embeddings
            The embeddings large language model to use.
        """

from langchain.base_language import BaseLanguageModel
from langchain.chat_models import AzureChatOpenAI, ChatOpenAI
from langchain.embeddings import OpenAIEmbeddings
from langchain.embeddings.base import Embeddings
from langchain.llms import AzureOpenAI, OpenAI

from foundationallm.config import Configuration
from foundationallm.langchain.language_models import LanguageModelBase
from foundationallm.models.language_models import (
    AzureOpenAIAPIType,
    EmbeddingModel,
    LanguageModel,
    LanguageModelProvider
)

class OpenAIModel(LanguageModelBase):
    """OpenAI Completion model."""
    config_value_base_name: str

    def __init__(self, config: Configuration):
        """
        Initializes an OpenAI completion language model.
        
        Parameters
        ----------
        language_model: LanguageModel
            The language model metadata class.
        config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        self.config = config

    def get_completion_model(self, language_model: LanguageModel) -> BaseLanguageModel:
        """
        Returns an OpenAI completion model.
        
        Returns
        -------
        BaseLanguageModel
            Returns an OpenAI completion model.
        """
        use_chat = language_model.use_chat

        if language_model.provider == LanguageModelProvider.MICROSOFT:        
            openai_api_type = AzureOpenAIAPIType.AZURE
        else:        
            openai_api_type = None

        openai_api_base = self.config.get_value(language_model.api_endpoint)
        openai_api_key = self.config.get_value(language_model.api_key)
        openai_api_version = self.config.get_value(language_model.api_version)
        openai_deployment_name = self.config.get_value(language_model.deployment)
        
        temperature = language_model.temperature or 0

        if openai_api_type == AzureOpenAIAPIType.AZURE:
            if use_chat:
                return AzureChatOpenAI(
                    temperature = temperature,
                    openai_api_base = openai_api_base,
                    openai_api_key = openai_api_key,
                    openai_api_type = openai_api_type,
                    openai_api_version = openai_api_version,
                    model_version = self.config.get_value(language_model.version),
                    deployment_name = openai_deployment_name
                    #max_tokens = self.config.get_value(f'{config_value_base_name}:Completions:MaxTokens')
                )
            else:
                return AzureOpenAI(
                    temperature = temperature,
                    openai_api_base = openai_api_base,
                    openai_api_key = openai_api_key,
                    openai_api_type = openai_api_type,
                    openai_api_version = openai_api_version,
                    deployment_name = openai_deployment_name
                    #max_tokens = self.config.get_value(f'{config_value_base_name}:Completions:MaxTokens')
                )
        else:
            if use_chat:
                return ChatOpenAI(
                    openai_api_base = openai_api_base,
                    openai_api_key = openai_api_key,
                    temperature = temperature
                )
            else:
                return OpenAI(
                    openai_api_base = openai_api_base,
                    openai_api_key = openai_api_key,
                    temperature = temperature
                )

    def get_embedding_model(self, embedding_model: EmbeddingModel) -> Embeddings:
        """
        Retrieves the OpenAI embedding model.
        
        Returns
        -------
        Embeddings
            Returns an OpenAI embeddings model.
        """
        if embedding_model is None:
            raise ValueError('Expected populated embedding_model, got None.')

        if embedding_model.provider == LanguageModelProvider.MICROSOFT:            
            openai_api_type = AzureOpenAIAPIType.AZURE
        else:
            openai_api_type = None

        return OpenAIEmbeddings(
            openai_api_base = self.config.get_value(embedding_model.api_endpoint),
            openai_api_key = self.config.get_value(embedding_model.api_key),
            openai_api_type = openai_api_type,
            openai_api_version = self.config.get_value(embedding_model.api_version),
            chunk_size = embedding_model.chunk_size,
            deployment = self.config.get_value(embedding_model.deployment),
            model = self.config.get_value(embedding_model.model)
        )

from langchain.embeddings.base import Embeddings
from langchain.embeddings import OpenAIEmbeddings

from foundationallm.config import Configuration
from foundationallm.langchain.language_models.openai import AzureOpenAIAPIType

class AzureOpenAIEmbeddings():
    """Azure OpenAI Emeddings model."""
    config_value_base_name: str = 'FoundationaLLM:AzureOpenAI:API'
   
    def __init__(self, config: Configuration):
        """
        Initializer
        
        Parameters
        ----------
        app_config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        self.config = config
        self.openai_api_type = AzureOpenAIAPIType.AZURE
        self.openai_api_key = self.config.get_value(f'{self.config_value_base_name}:Key')
        self.openai_api_base = self.config.get_value(f'{self.config_value_base_name}:Endpoint')
        self.openai_api_version = self.config.get_value(f'{self.config_value_base_name}:Version')
        self.chunk_size = self.config.get_value(f'{self.config_value_base_name}:Embeddings:ChunkSize')
        self.deployment_name = self.config.get_value(f'{self.config_value_base_name}:Embeddings:DeploymentName')
        self.model_name = self.config.get_value(f'{self.config_value_base_name}:Embeddings:ModelName')
        
    def get_embeddings_model(self) -> Embeddings:
        """
        Retrieve the embeddings model.
        
        Returns
        -------
        Embeddings
            The embeddings model to use.
        """
        return OpenAIEmbeddings(
            openai_api_base = self.openai_api_base,
            openai_api_key = self.openai_api_key,
            openai_api_type = self.openai_api_type,
            openai_api_version = self.openai_api_version,
            deployment = self.deployment_name,
            model = self.model_name,
            chunk_size = self.chunk_size
        )
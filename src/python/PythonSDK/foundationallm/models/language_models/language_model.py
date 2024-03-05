from typing import Annotated, Optional
from pydantic import BaseModel, confloat
from foundationallm.models.language_models import LanguageModelType, LanguageModelProvider

class LanguageModel(BaseModel):
    """Language model metadata model."""
    type: str = LanguageModelType.OPENAI
    provider: str = LanguageModelProvider.MICROSOFT
    temperature: Annotated[float, confloat(ge=0, le=1)] = 0
    use_chat: bool = True
    api_endpoint: Optional[str] = "FoundationaLLM:AzureOpenAI:API:Endpoint"
    api_key: Optional[str] = "FoundationaLLM:AzureOpenAI:API:Key"
    api_version: Optional[str] = "FoundationaLLM:AzureOpenAI:API:Version"
    version: Optional[str] = "FoundationaLLM:AzureOpenAI:API:Completions:ModelVersion"
    deployment: Optional[str] = None #"FoundationaLLM:AzureOpenAI:API:Completions:DeploymentName"

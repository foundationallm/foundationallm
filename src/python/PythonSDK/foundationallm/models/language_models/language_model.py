from typing import Annotated
from pydantic import BaseModel, confloat
from foundationallm.models.language_models import LanguageModelType, LanguageModelProvider

class LanguageModel(BaseModel):
    """Language model metadata model."""
    type: str = LanguageModelType.OPENAI
    provider: str = LanguageModelProvider.MICROSOFT
    temperature: Annotated[float, confloat(ge=0, le=1)] = 0
    use_chat: bool = True

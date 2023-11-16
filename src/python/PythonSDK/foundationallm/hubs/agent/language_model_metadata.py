from foundationallm.hubs import Metadata
from foundationallm.hubs.agent import LanguageModelType
from foundationallm.hubs.agent import LanguageModelProvider

class LanguageModelMetadata(Metadata):
    type: LanguageModelType
    provider: LanguageModelProvider
    temperature: float = 0.0
    use_chat: bool = True
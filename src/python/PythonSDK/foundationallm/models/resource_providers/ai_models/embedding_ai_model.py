from foundationallm.models.resource_providers.ai_models import AIModelBase, AIModelTypes

class EmbeddingAIModel(AIModelBase):
    """
    An embedding AI model.
    """
    type: str = AIModelTypes.EMBEDDING

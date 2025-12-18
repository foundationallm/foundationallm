from foundationallm.models.resource_providers.ai_models import AIModelBase, AIModelTypes

class CompletionAIModel(AIModelBase):
    """
    A completion AI model.
    """
    type: str = AIModelTypes.COMPLETION

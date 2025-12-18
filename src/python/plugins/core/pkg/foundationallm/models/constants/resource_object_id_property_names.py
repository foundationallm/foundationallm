from enum import Enum

class ResourceObjectIdPropertyNames(str, Enum):
    """Allowed keys for resource object id properties dictionary entries."""
    OBJECT_ROLE = 'object_role'
    MODEL_PARAMETERS = 'model_parameters'
    TEXT_EMBEDDING_MODEL_NAME = 'text_embedding_model_name'
    TEXT_EMBEDDING_MODEL_PARAMETERS = 'text_embedding_model_parameters'
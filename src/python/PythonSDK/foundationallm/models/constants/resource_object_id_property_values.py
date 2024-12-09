from enum import Enum

class ResourceObjectIdPropertyValues(str, Enum):
    """Allowed values for resource object id properties dictionary entries."""
    MAIN_MODEL = 'main_model'
    MAIN_PROMPT = 'main_prompt'
    MAIN_INDEXING_PROFILE = 'main_indexing_profile'
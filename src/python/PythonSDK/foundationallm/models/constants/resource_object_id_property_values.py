from enum import Enum

class ResourceObjectIdPropertyValues(str, Enum):
    """Allowed values for resource object id properties dictionary entries."""
    MAIN_MODEL = 'main_model'
    MAIN_PROMPT = 'main_prompt'
    FILES_PROMPT = 'files_prompt'
    FINAL_PROMPT = 'final_prompt'
    MAIN_INDEXING_PROFILE = 'main_indexing_profile'
    INDEXING_PROFILE = 'indexing_profile'
    EMBEDDING_PROFILE = 'embedding_profile'
    AI_PROJECT = 'ai_project'

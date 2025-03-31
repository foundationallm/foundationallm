from enum import Enum

class RunnableConfigKeys(str, Enum):
    """The keys for RunnableConfig values."""
    ORIGINAL_USER_PROMPT = 'original_user_prompt'
    ORIGINAL_USER_PROMPT_REWRITE = 'original_user_prompt_rewrite'

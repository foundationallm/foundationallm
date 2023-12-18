from pydantic import BaseModel
from foundationallm.hubs.prompt import PromptMetadata

class PromptHubResponse(BaseModel):
    prompt: PromptMetadata

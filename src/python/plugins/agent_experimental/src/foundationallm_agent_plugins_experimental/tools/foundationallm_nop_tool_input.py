from pydantic import BaseModel, Field

class FoundationaLLMNopToolInput(BaseModel):
    """ Input data model for the No-Operation (NOP) tool. """
    prompt: str = Field(
        description="The prompt used by the tool to generate the response."
    )

from typing import Optional

from pydantic import BaseModel, Field

class FoundationaLLMKnowledgeToolInput(BaseModel):
    """ Input data model for the FoundationaLLM Knowledge tool. """
    prompt: str = Field(
        description="The prompt to search for relevant documents and answer the question."
    )
    file_name: Optional[str] = Field(
        default=None,
        description="Optional file name of a document to be used for answering the question."
    )

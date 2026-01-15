from typing import List, Literal, Optional

from pydantic import BaseModel, Field

KnowledgeTask = Literal["summary", "content"]

class FoundationaLLMKnowledgeToolInput(BaseModel):
    """ Input data model for the FoundationaLLM Knowledge tool. """
    prompt: str = Field(
        description="The prompt to search for relevant documents and answer the question."
    )
    task: KnowledgeTask = Field(
        description="Select summary for overview; content for exact, authoritative details."
    )
    file_name: Optional[str] = Field(
        default=None,
        description="Optional file name of a document to be used for answering the question."
    )

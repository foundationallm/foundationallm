from typing import List, Literal, Optional

from pydantic import BaseModel, Field

KnowledgeTask = Literal["summary", "raw_content"]

class FoundationaLLMKnowledgeToolInput(BaseModel):
    """ Input data model for the FoundationaLLM Knowledge tool. """
    prompt: str = Field(
        description="The prompt to search for relevant documents and answer the question."
    )
    task: KnowledgeTask = Field(
        description=(
            "The operation to perform on ALL provided files. "
            "Apply it independently and uniformly to each file."
        )
    )
    file_name: Optional[str] = Field(
        default=None,
        description="Optional file name of a document to be used for answering the question."
    )

from typing import List, Optional

from pydantic import BaseModel, Field

class FoundationaLLMKnowledgeToolInput(BaseModel):
    """ Input data model for the FoundationaLLM Knowledge tool. """
    prompt: str = Field(
        description="The prompt to search for relevant documents and answer the question."
    )
    file_names: Optional[List[str]] = Field(
        default=None,
        description="Optional list of file names of documents to be used for answering the question."
    )

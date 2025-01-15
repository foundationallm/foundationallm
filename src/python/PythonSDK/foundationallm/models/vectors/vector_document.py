from typing import Optional
from langchain_core.documents import Document

class VectorDocument(Document):

    id : str
    score: float
    rerank_score: Optional[float]
    
    class Config:
        extra = "allow"

from langchain_core.documents import Document

class VectorDocument(Document):

    id : str
    score: float
    rerank_score: float
    
    class Config:
        extra = "allow"

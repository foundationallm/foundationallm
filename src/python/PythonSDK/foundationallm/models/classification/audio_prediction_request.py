from pydantic import BaseModel

class AudioPredictionRequest(BaseModel):

    file: str = None
    embeddings: list = None
    #similarity_threshold: float = 29.0

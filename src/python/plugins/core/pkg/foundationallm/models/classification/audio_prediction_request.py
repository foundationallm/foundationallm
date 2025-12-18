from pydantic import BaseModel
from typing import Optional

class AudioPredictionRequest(BaseModel):

    file: Optional[str] = None
    deployment_name: Optional[str] = None

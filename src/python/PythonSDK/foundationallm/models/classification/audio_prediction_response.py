from pydantic import BaseModel

class AudioPredictionResponse(BaseModel):

    predicted_species_id: str = None
    species_ebird_code: str = None
    common_name: str = None
    scientific_name: str = None
    label: str = None
    score: float = None

from pydantic import BaseModel

class ListOption(BaseModel):
    name: str
    description: str

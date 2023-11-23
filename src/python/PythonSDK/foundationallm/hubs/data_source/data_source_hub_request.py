from pydantic import BaseModel
from typing import List, Optional

class DataSourceHubRequest(BaseModel):
    """
    DataSourceHubRequest contains the information needed to retrieve data sources from the DataSourceHub.
    
    data_sources: the list of data sources to retrieve, send an empty list or None to retrieve all data sources.
    session_id: the session ID value; can be used for caching.
    
    """
    data_sources: Optional[List[str]] = None
    session_id:Optional[str] = None
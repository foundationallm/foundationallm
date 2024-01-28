"""
The endpoint for managing the LangChainAPI.
"""
import time
from fastapi import APIRouter, Depends, HTTPException
from app.dependencies import get_config, handle_exception, validate_api_key_header

# Initialize API routing
router = APIRouter(
    prefix = '/manage',
    tags = ['manage'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}}
)

@router.post('/cache/{name}/refresh')
async def refresh_cache(name: str):
    """
    Refreshes the cache for the named object.

    Parameters
    ----------
    name : str
        The name of the cache object to refresh.
        "config", for example.
    """
    start = time.time()
        
    if name=='config' or name=='configuration':
        try:
            get_config('refresh')
        except Exception as e:
            handle_exception(e)
    else:
        raise HTTPException(status_code=404, detail=f'Cache named {name} not found.')
            
    end = time.time()
    
    return {'detail':f'The {name} cache was refreshed in {round(end-start, 3)} seconds.'}

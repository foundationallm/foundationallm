"""
The endpoint for managing the LangChainAPI.
"""
import time
from fastapi import APIRouter, Depends, HTTPException
from foundationallm.telemetry import Telemetry
from app.dependencies import (
    API_NAME,
    get_config,
    handle_exception,
    validate_api_key_header
)

logger = Telemetry.get_logger(__name__)
tracer = Telemetry.get_tracer(__name__)

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
    with tracer.start_as_current_span('refresh_cache') as span:
        span.set_attribute('cache_name', name)
        span.add_event(f'{API_NAME} {name} cache refresh requested.')

        start = time.time()

        if name=='config' or name=='configuration':
            try:
                get_config('refresh')
            except Exception as e:
                Telemetry.record_exception(span, e)
                handle_exception(e)
        else:
            raise HTTPException(status_code=404, detail=f'Cache named {name} not found.')

        end = time.time()

        detail = f'The {API_NAME} {name} cache was refreshed in {round(end-start, 3)} seconds.'
        span.add_event(detail)
        return {'detail':detail}

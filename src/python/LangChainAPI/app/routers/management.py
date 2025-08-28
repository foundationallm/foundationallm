"""
The API endpoint for returning the completion from the LLM for the specified user prompt.
"""

from typing import Optional

from app.dependencies import validate_api_key_header
from fastapi import (
    APIRouter,
    Depends,
    Header,
    HTTPException,
    Request,
    status
)
from opentelemetry.trace import SpanKind

from foundationallm.plugins import PluginManager
from foundationallm.telemetry import Telemetry

# Initialize telemetry logging
logger = Telemetry.get_logger(__name__)
tracer = Telemetry.get_tracer(__name__)

# Initialize API routing
router = APIRouter(
    prefix='/instances/{instance_id}',
    tags=['management'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}}
)

@router.post(
    '/plugins/reload',
    summary = 'Reloads the external plugins.',
    status_code = status.HTTP_200_OK,
    responses = {
        200: {'description': 'Plugins reloaded.'},
    }
)
async def reload_plugins(
    instance_id: str,
    request: Request,
    x_user_identity: Optional[str] = Header(None)
) -> str:
    """
    Reloads the external plugins for the specified instance.

    Returns
    -------
    str
        A message indicating that the plugins are being reloaded.
    """
    with tracer.start_as_current_span('langchainapi_plugins_reload', kind=SpanKind.SERVER) as span:
        try:
            request.app.state.plugin_manager.load_external_modules(reload_modules=True)
            request.app.state.plugin_manager.clear_cache()

            return 'Plugins reloaded successfully.'

        except Exception as e:
            handle_exception(e)

@router.post(
    '/plugins/cache/clear',
    summary = 'Clears the plugin cache.',
    status_code = status.HTTP_200_OK,
    responses = {
        200: {'description': 'Plugins cache cleared.'},
    }
)
async def clear_plugins_cache(
    instance_id: str,
    request: Request,
    x_user_identity: Optional[str] = Header(None)
) -> str:
    """
    Clears the plugin cache for the specified instance.

    Returns
    -------
    str
        A message indicating that the plugins cache is being cleared.
    """
    with tracer.start_as_current_span('langchainapi_plugins_cache_clear', kind=SpanKind.SERVER) as span:
        try:
            request.app.state.plugin_manager.clear_cache()

            return 'Plugins cache cleared successfully.'

        except Exception as e:
            handle_exception(e)

@router.get(
    '/plugins/cache/stats',
    summary = 'Retrieves the plugin cache statistics.',
    status_code = status.HTTP_200_OK,
    responses = {
        200: {'description': 'Plugins cache statistics retrieved.'},
    }
)
async def get_plugins_cache_stats(
    instance_id: str,
    request: Request,
    x_user_identity: Optional[str] = Header(None)
) -> dict:
    """
    Retrieves the plugin cache statistics for the specified instance.

    Returns
    -------
    dict
        A dictionary containing the plugins cache statistics.
    """
    with tracer.start_as_current_span('langchainapi_plugins_cache_stats', kind=SpanKind.SERVER) as span:
        try:
            stats = list(request.app.state.plugin_manager.object_cache.keys())
            return stats

        except Exception as e:
            handle_exception(e)

def handle_exception(exception: Exception, status_code: int = 500):
    """
    Handles an exception that occurred while processing a request.

    Parameters
    ----------
    exception : Exception
        The exception that occurred.
    """
    logger.error(exception, stack_info=True, exc_info=True)
    raise HTTPException(
        status_code = status_code,
        detail = str(exception)
    ) from exception

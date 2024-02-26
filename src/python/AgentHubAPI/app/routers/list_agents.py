"""
The API endpoint for listing available agents.
"""
from typing import List
from fastapi import APIRouter, Depends, Request
from foundationallm.hubs.agent import AgentHub
from foundationallm.telemetry import Telemetry
from app.dependencies import validate_api_key_header, handle_exception

logger = Telemetry.get_logger(__name__)
tracer = Telemetry.get_tracer(__name__)

router = APIRouter(
    prefix='/list',
    tags=['list'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}},
    redirect_slashes=False
)

@router.get('')
async def list_agents(request: Request) -> List:
    """
    Retrieves a list of available agents.

    Parameters
    ----------
    request : Request
        The underlying HTTP request.
    
    Returns
    -------
    List
        Returns a list of metadata objects for all available agents.
    """
    with tracer.start_as_current_span('list_agents') as span:
        try:
            return AgentHub(config=request.app.extra['config']).list()
        except Exception as e:
            Telemetry.record_exception(span, e)
            handle_exception(e)

"""
The API endpoint for listing available agents.
"""
from typing import List
from fastapi import APIRouter, Depends, Request
from foundationallm.hubs.agent import AgentHub
from app.dependencies import validate_api_key_header, handle_exception

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
    try:
        return AgentHub(config=request.app.extra['config']).list()
    except Exception as e:
        handle_exception(e)

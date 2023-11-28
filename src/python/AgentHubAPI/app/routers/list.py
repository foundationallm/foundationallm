import logging
from fastapi import APIRouter, Depends, HTTPException
from app.dependencies import validate_api_key_header
from foundationallm.hubs.agent import AgentHub
from typing import List

router = APIRouter(
    prefix='/list',
    tags=['list'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}},
    redirect_slashes=False
)

@router.get('')
async def list() -> List:
    """
    Retrieves a list of available agents.
    
    Returns
    -------
    List
        Returns a list of metadata objects for all available agents.
    """
    try:
        return AgentHub().list()
    except Exception as e:
        logging.error(e, stack_info=True, exc_info=True)
        raise HTTPException(
            status_code = 500,
            detail = str(e)
        )
from fastapi import APIRouter, Depends
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
    return AgentHub().list()
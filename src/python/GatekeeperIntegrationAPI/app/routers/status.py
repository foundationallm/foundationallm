"""
Status API endpoint that acts as a health check for the API.
"""
import os
from fastapi import APIRouter
from foundationallm.integration.config.environment_variables import HOSTNAME, FOUNDATIONALLM_VERSION
from app.dependencies import API_NAME

router = APIRouter(
    prefix='/status',
    tags=['status'],
    responses={404: {'description':'Not found'}}
)

@router.get('')
async def get_status():
    """
    Retrieves the status of the API.
    
    Returns
    -------
    JSON
        Object containing the name, instance, version, and status of the API.
    """    
    statusMessage = {
        "name": API_NAME,
        "instance": os.environ[HOSTNAME],
        "version": os.environ[FOUNDATIONALLM_VERSION],
        "status": "ready"
    }
    return statusMessage

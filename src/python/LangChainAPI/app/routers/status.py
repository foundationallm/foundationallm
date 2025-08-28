"""
Status API endpoint that acts as a health check for the API.
"""
import os
from fastapi import APIRouter
from foundationallm.config.environment_variables import HOSTNAME, FOUNDATIONALLM_VERSION

router = APIRouter(
    prefix='',
    tags=['status'],
    responses={404: {'description':'Not found'}}
)

@router.get(
    '/status',
    summary = 'Get the status of the LangChainAPI.'
)
async def get_status():
    """
    Get the status of the LangChainAPI.
    
    Returns
    -------
    str
        A JSON object containing the name, version, and status of the LangChainAPI.
    """    
    status_message = {
        "name": 'LangChainAPI',
        "instance_name": os.getenv(HOSTNAME, ''),
        "version": os.getenv(FOUNDATIONALLM_VERSION),
        "status": "ready"
    }
    return status_message

@router.get(
    '/instances/{instance_id}/status',
    summary = 'Get the status of a specified instance of the LangChainAPI.'
)
async def get_instance_status(instance_id: str):
    """
    Get the status of a specified instance of the LangChainAPI.
    
    Returns
    -------
    str
        Object containing the name, instance, version, and status of the FoundationaLLM instance of the LangChainAPI.
    """    
    instance_status_message = {
        "name": 'LangChainAPI',
        "instance_id": instance_id,
        "instance_name": os.getenv(HOSTNAME, ''),
        "version": os.getenv(FOUNDATIONALLM_VERSION),
        "status": "ready"
    }
    return instance_status_message

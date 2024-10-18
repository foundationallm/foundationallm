"""
Status API endpoint that acts as a health check for the API.
"""
import os
from fastapi import APIRouter
from foundationallm.config.environment_variables import HOSTNAME, FOUNDATIONALLM_VERSION
from app.dependencies import API_NAME

router = APIRouter(
    prefix='',
    tags=['status'],
    responses={404: {'description':'Not found'}}
)

@router.get(
    '/status',
    summary = f'Get the status of the {API_NAME}.'
)
async def get_status():
    f"""
    Get the status of the {API_NAME}.
    
    Returns
    -------
    str
        A JSON object containing the name, version, and status of the {API_NAME}.
    """    
    status_message = {
        "name": API_NAME,
        "instance_name": os.environ[HOSTNAME],
        "version": os.environ[FOUNDATIONALLM_VERSION],
        "status": "ready"
    }
    return status_message

@router.get(
    '/instances/{instance_id}/status',
    summary = f'Get the status of a specified instance of the {API_NAME}.'
)
async def get_instance_status(instance_id: str):
    f"""
    Get the status of a specified instance of the {API_NAME}.
    
    Returns
    -------
    str
        Object containing the name, instance, version, and status of the FoundationaLLM instance of the {API_NAME}.
    """    
    instance_status_message = {
        "name": API_NAME,
        "instance_id": instance_id,
        "instance_name": os.environ[HOSTNAME],
        "version": os.environ[FOUNDATIONALLM_VERSION],
        "status": "ready"
    }
    return instance_status_message

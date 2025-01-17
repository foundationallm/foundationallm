"""
Provides dependencies for API calls.
"""
from app.lifespan_manager import get_config
from fastapi import Depends, HTTPException
from fastapi.security import APIKeyHeader
from foundationallm.config import Configuration
from foundationallm.telemetry import Telemetry

# Initialize telemetry logging
logger = Telemetry.get_logger(__name__)

async def validate_api_key_header(x_api_key: str = Depends(APIKeyHeader(name='X-API-Key')), config: Configuration = Depends(get_config)) -> bool:
    """
    Validates that the X-API-Key value in the request header matches the key expected for this API.
    
    Parameters
    ----------
    x_api_key : str
        The X-API-Key value in the request header.
        
    Returns
    bool
        Returns True of the X-API-Key value from the request header matches the expected value.
        Otherwise, returns False.
    """
    result = x_api_key == config.get_value(f'FoundationaLLM:APIEndpoints:LangChainAPI:Essentials:APIKey')

    if not result:
        logger.error('Invalid API key. You must provide a valid API key in the X-API-KEY header.')
        raise HTTPException(
            status_code = 401,
            detail = 'Invalid API key. You must provide a valid API key in the X-API-KEY header.'
        )

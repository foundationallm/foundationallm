import logging
from typing import Annotated
from fastapi import Depends, HTTPException
from fastapi.security import APIKeyHeader
from foundationallm.config import Configuration

__config: Configuration = None

def get_config() -> Configuration:
    return __config or Configuration()

async def validate_api_key_header(config: Annotated[Configuration, Depends()], x_api_key: str = Depends(APIKeyHeader(name='X-API-Key'))) -> bool:
    """
    Validates that the X-API-Key value in the request header matches the key expected for this API.
    
    Parameters
    ----------
    app_config : Configuration
        Used for retrieving application configuration settings.
    x_api_key : str
        The X-API-Key value in the request header.
        
    Returns
    bool
        Returns True of the X-API-Key value from the request header matches the expected value.
        Otherwise, returns False.
    """

    result = x_api_key == config.get_value('FoundationaLLM:LangChainAPI:Key')
    
    if not result:
        logging.error('Invalid API key. You must provide a valid API key in the X-API-KEY header.')
        raise HTTPException(
            status_code = 401,
            detail = f'Invalid API key. You must provide a valid API key in the X-API-KEY header.'
        )
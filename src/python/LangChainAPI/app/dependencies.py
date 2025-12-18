"""
Provides dependencies for API calls.
"""
from fastapi import (
    Body,
    Depends,
    HTTPException,
    Request
)
from fastapi.security import APIKeyHeader

from foundationallm.models.agents import CompletionRequest
from foundationallm.models.orchestration import CompletionRequestBase
from foundationallm.telemetry import Telemetry

# Initialize telemetry logging
logger = Telemetry.get_logger(__name__)

async def validate_api_key_header(
        request: Request,
        x_api_key: str = Depends(APIKeyHeader(name='X-API-Key'))) -> bool:
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
    result = x_api_key == request.app.state.config.get_value(
        'FoundationaLLM:APIEndpoints:LangChainAPI:Essentials:APIKey')

    if not result:
        logger.error('Invalid API key. You must provide a valid API key in the X-API-KEY header.')
        raise HTTPException(
            status_code = 401,
            detail = 'Invalid API key. You must provide a valid API key in the X-API-KEY header.'
        )

async def resolve_completion_request(request_body: dict = Body(...)) -> CompletionRequestBase:
    """
    Resolves the completion request from the request body.
    """
    agent_type = request_body.get("agent", {}).get("type", None)

    match agent_type:
        case "generic-agent":
            request = CompletionRequest(**request_body)
            request.agent.type = agent_type
            return request
        case _:
            logger.error("Unsupported agent type: %s", agent_type)
            raise ValueError(f"Unsupported agent type: {agent_type}")

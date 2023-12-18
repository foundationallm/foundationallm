"""
The API endpoint for returning the appropriate agent prompt for the specified user prompt.
"""
import logging
from typing import Optional
from fastapi import APIRouter, Depends, HTTPException, Header
from foundationallm.config import Context
from foundationallm.models import AgentHint
from foundationallm.hubs.prompt import PromptHubRequest, PromptHubResponse, PromptHub
from app.dependencies import validate_api_key_header

router = APIRouter(
    prefix='/resolve',
    tags=['resolve'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}}
)

@router.post('')
async def resolve(request: PromptHubRequest,
                  x_user_identity: Optional[str] = Header(None),
                  x_agent_hint: str = Header(None)) -> PromptHubResponse:
    """
    Retrieves the prompt to use for a specified agent and prompt name.

    Parameters
    ----------
    request : PromptHubRequest
        The request object containing the agent and prompt names to use 
        in resolving the prompt to return.
    x_user_identity : str
        The optional X-USER-IDENTITY header value.
    x_agent_hint : str
        The optional X-AGENT-HINT header value.
        
    Returns
    -------
    PromptHubResponse
        Object containing the metadata for the resolved prompt.
    """
    try:
        context = Context(user_identity=x_user_identity)
        if x_agent_hint is not None and len(x_agent_hint.strip()) > 0:
            agent_hint = AgentHint.model_validate_json(x_agent_hint)
            return PromptHub().resolve(request=request, user_context=context, hint=agent_hint)
        return PromptHub().resolve(request, user_context=context)
    except Exception as e:
        logging.error(e, stack_info=True, exc_info=True)
        raise HTTPException(
            status_code = 500,
            detail = str(e)
        ) from e

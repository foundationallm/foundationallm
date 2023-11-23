import logging
from fastapi import APIRouter, Depends, Header, HTTPException
from typing import Optional
from app.dependencies import validate_api_key_header
from foundationallm.config import Context
from foundationallm.hubs.agent import AgentHub, AgentHubRequest, AgentHubResponse

router = APIRouter(
    prefix='/resolve',
    tags=['resolve'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}},
    redirect_slashes=False
)

@router.post('')
async def resolve(request: AgentHubRequest, x_user_identity: Optional[str] = Header(None), x_agent_hint: Optional[str] = Header(None)) -> AgentHubResponse:
    """
    Resolves the best agent to use for the specified user prompt.

    Parameters
    ----------
    request : AgentHubRequest
        The request object containing the user prompt to use in resolving the best agent to return.
    x_user_identity : str
        The optional X-USER-IDENTITY header value.
    x_agent_hint : str
        The optional X-AGENT-HINT header value.
    
    Returns
    -------
    AgentHubResponse
        Object containing the metadata for the resolved agent.
    """
    try:
        context = Context(user_identity=x_user_identity)   
        return AgentHub().resolve(request=request, user_context=context, hint=x_agent_hint)
    except Exception as e:
        logging.error(e, stack_info=True, exc_info=True)
        raise HTTPException(
            status_code = 500,
            detail = e.message
        )
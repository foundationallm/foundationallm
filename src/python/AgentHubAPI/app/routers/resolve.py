"""
The API endpoint for returning the appropriate agent for the specified user prompt.
"""
from typing import Optional
from fastapi import APIRouter, Depends, Header, Request
from foundationallm.config import Context
from foundationallm.hubs.agent import AgentHub, AgentHubRequest, AgentHubResponse
from foundationallm.telemetry import Telemetry
from app.dependencies import handle_exception, validate_api_key_header

logger = Telemetry.get_logger(__name__)
tracer = Telemetry.get_tracer(__name__)

router = APIRouter(
    prefix='/resolve',
    tags=['resolve'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}},
    redirect_slashes=False
)

@router.post('')
async def resolve(
    agent_request: AgentHubRequest,
    request : Request,
    x_user_identity: Optional[str] = Header(None)) -> AgentHubResponse:

    """
    Resolves the best agent to use for the specified user prompt.

    Parameters
    ----------
    agent_request : AgentHubRequest
        The request object containing the user prompt to use in resolving the best agent to return.
    request : Request
        The underlying HTTP request.
    x_user_identity : str
        The optional X-USER-IDENTITY header value.
    
    Returns
    -------
    AgentHubResponse
        Object containing the metadata for the resolved agent.
    """
    with tracer.start_as_current_span('resolve') as span:
        try:
            return AgentHub(config = request.app.extra['config']).resolve(
                request = agent_request,
                user_context = Context(user_identity=x_user_identity)
            )
        except Exception as e:
            Telemetry.record_exception(span, e)
            handle_exception(e)

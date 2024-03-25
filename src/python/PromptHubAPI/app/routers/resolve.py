"""
The API endpoint for returning the appropriate agent prompt for the specified user prompt.
"""
from typing import Optional
from fastapi import APIRouter, Depends, Header, Request
from foundationallm.config import Context
from foundationallm.telemetry import Telemetry
from foundationallm.hubs.prompt import PromptHubRequest, PromptHubResponse, PromptHub
from app.dependencies import handle_exception, validate_api_key_header

logger = Telemetry.get_logger(__name__)
tracer = Telemetry.get_tracer(__name__)

router = APIRouter(
    prefix='/resolve',
    tags=['resolve'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}}
)

@router.post('')
async def resolve(
    prompt_request: PromptHubRequest,
    request : Request,
    x_user_identity: Optional[str] = Header(None)) -> PromptHubResponse:
    """
    Retrieves the prompt to use for a specified agent and prompt name.

    Parameters
    ----------
    prompt_request : PromptHubRequest
        The request object containing the agent and prompt names to use
        in resolving the prompt to return.
    request : Request
        The underlying HTTP request.
    x_user_identity : str
        The optional X-USER-IDENTITY header value.
        
    Returns
    -------
    PromptHubResponse
        Object containing the metadata for the resolved prompt.
    """
    with tracer.start_as_current_span('resolve') as span:
        try:
            return PromptHub(config = request.app.extra['config']).resolve(
                request = prompt_request,
                user_context = Context(user_identity=x_user_identity)
            )
        except Exception as e:
            Telemetry.record_exception(span, e)
            handle_exception(e)

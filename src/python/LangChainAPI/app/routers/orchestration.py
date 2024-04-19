"""
The API endpoint for returning the completion from the LLM for the specified user prompt.
"""
from typing import Optional
from urllib import request
from fastapi import APIRouter, Depends, Header, Request, Body
from foundationallm.config import Context
from foundationallm.models.orchestration import (
    CompletionRequestBase,
    KnowledgeManagementCompletionRequest,
    CompletionResponse
)
from foundationallm.langchain.orchestration import OrchestrationManager
from foundationallm.telemetry import Telemetry
from app.dependencies import handle_exception, validate_api_key_header

# Initialize telemetry logging
logger = Telemetry.get_logger(__name__)
tracer = Telemetry.get_tracer(__name__)

# Initialize API routing
router = APIRouter(
    prefix='/orchestration',
    tags=['orchestration'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}}
)

# temporary to support legacy agents alongside the knowledge-management and internal context agent
async def resolve_completion_request(request_body: dict = Body(...)) -> CompletionRequestBase:   
    agent_type = request_body.get("agent", {}).get("type", None)    
    
    match agent_type:
        case "knowledge-management" | "internal-context":
            request = KnowledgeManagementCompletionRequest(**request_body)
            request.agent.type = "knowledge-management"
            return request
        case _:
            raise ValueError(f"Unsupported agent type: {agent_type}")

@router.post('/completion')
async def get_completion(
    request : Request,
    completion_request: CompletionRequestBase = Depends(resolve_completion_request),    
    x_user_identity: Optional[str] = Header(None)) -> CompletionResponse:
    """
    Retrieves a completion response from a language model.
    
    Parameters
    ----------
    completion_request : CompletionRequestBase
        The request object containing the metadata required to build a LangChain agent
        and generate a completion.
    request : Request
        The underlying HTTP request.
    x_user_identity : str
        The optional X-USER-IDENTITY header value.

    Returns
    -------
    CompletionResponse
        Object containing the completion response and token usage details.
    """
    with tracer.start_as_current_span('completion') as span:
        try:
            span.set_attribute('request_id', completion_request.request_id)

            orchestration_manager = OrchestrationManager(
                completion_request = completion_request,
                configuration=request.app.extra['config'],
                context=Context(user_identity=x_user_identity)
            )
            return orchestration_manager.invoke(completion_request.user_prompt)
        except Exception as e:
            handle_exception(e)

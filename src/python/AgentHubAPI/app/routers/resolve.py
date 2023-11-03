from fastapi import APIRouter, Depends, Header
from typing import Optional
from app.dependencies import validate_api_key_header
from foundationallm.context import Context
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
    context = Context(user_identity=x_user_identity)   
    return AgentHub().resolve(request=request, user_context=context, hint=x_agent_hint)
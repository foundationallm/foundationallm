"""
The API endpoint for returning the requested data source metadata.
"""
from typing import Optional
from fastapi import APIRouter, Depends, Header, Request
from foundationallm.config import Context
from foundationallm.models import AgentHint
from foundationallm.hubs.data_source import (
    DataSourceHubRequest,
    DataSourceHubResponse,
    DataSourceHub
)
from app.dependencies import handle_exception, validate_api_key_header

router = APIRouter(
    prefix='/resolve',
    tags=['resolve'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}}
)

@router.post('')
async def resolve(
    datasource_request:DataSourceHubRequest,
    request : Request,
    x_user_identity: Optional[str] = Header(None),
    x_agent_hint: str = Header(None)) -> DataSourceHubResponse:
    """
    Resolves the requested data source metadata.

    Parameters
    ----------
    datasource_request : DataSourceHubRequest
        The request object containing the data sources to resolve.
    request : Request
        The underlying HTTP request.
    x_user_identity : str
        The optional X-USER-IDENTITY header value.
    x_agent_hint : str
        The optional X-AGENT-HINT header value.
        
    Returns
    -------
    DataSourceHubResponse
        Object containing the metadata for the resolved data source.
    """
    try:
        context = Context(user_identity=x_user_identity)
        if x_agent_hint is not None and len(x_agent_hint.strip()) > 0:
            agent_hint = AgentHint.model_validate_json(x_agent_hint)
            return DataSourceHub(config=request.app.extra['config']).resolve(request=datasource_request, user_context=context, hint=agent_hint)
        return DataSourceHub(config=request.app.extra['config']).resolve(request=datasource_request, user_context=context)
    except Exception as e:
        handle_exception(e)

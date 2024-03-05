"""
The API endpoint for returning the requested data source metadata.
"""
from typing import Optional
from fastapi import APIRouter, Depends, Header, Request
from foundationallm.config import Context
from foundationallm.telemetry import Telemetry
from foundationallm.hubs.data_source import (
    DataSourceHubRequest,
    DataSourceHubResponse,
    DataSourceHub
)
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
    datasource_request:DataSourceHubRequest,
    request : Request,
    x_user_identity: Optional[str] = Header(None)) -> DataSourceHubResponse:
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
        
    Returns
    -------
    DataSourceHubResponse
        Object containing the metadata for the resolved data source.
    """
    with tracer.start_as_current_span('resolve') as span:
        try:
            return DataSourceHub(config = request.app.extra['config']).resolve(
                request = datasource_request,
                user_context = Context(user_identity=x_user_identity)
            )
        except Exception as e:
            Telemetry.record_exception(span, e)
            handle_exception(e)

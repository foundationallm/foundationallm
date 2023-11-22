import logging
from fastapi import APIRouter, Depends, HTTPException
from app.dependencies import validate_api_key_header
from foundationallm.hubs.data_source import DataSourceHubRequest, DataSourceHubResponse, DataSourceHub

router = APIRouter(
    prefix='/resolve',
    tags=['resolve'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}}
)

@router.post('')
async def resolve(request:DataSourceHubRequest) -> DataSourceHubResponse:    
    """
    Resolves the best data source to use for the specified user prompt.

    Parameters
    ----------
    request : DataSourceHubRequest
        The request object containing the user prompt to use in resolving the best data source to return.
    
    Returns
    -------
    DataSourceHubResponse
        Object containing the metadata for the resolved data source.
    """
    try:
        return DataSourceHub().resolve(request)
    except Exception as e:
        logging.error(e, stack_info=True, exc_info=True)
        raise HTTPException(
            status_code = 500,
            detail = e.message
        )
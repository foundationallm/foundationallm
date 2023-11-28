import logging
from fastapi import APIRouter, Depends, HTTPException
from app.dependencies import validate_api_key_header
from foundationallm.hubs.prompt import PromptHubRequest, PromptHubResponse, PromptHub

router = APIRouter(
    prefix='/resolve',
    tags=['resolve'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}}
)

@router.post('')
async def resolve(request: PromptHubRequest) -> PromptHubResponse:
    """
    Retrieves the prompt to use for a specified agent and prompt name.

    Parameters
    ----------
    request : PromptHubRequest
        The request object containing the agent and prompt names to use in resolving the prompt to return.
    
    Returns
    -------
    PromptHubResponse
        Object containing the metadata for the resolved prompt.
    """
    try:
        return PromptHub().resolve(request)
    except Exception as e:
        logging.error(e, stack_info=True, exc_info=True)
        raise HTTPException(
            status_code = 500,
            detail = str(e)
        )
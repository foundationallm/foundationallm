import logging
from fastapi import APIRouter, Depends, HTTPException
from app.dependencies import validate_api_key_header
from typing import List

router = APIRouter(
    prefix='/analyze',
    tags=['analyze'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}},
    redirect_slashes=False
)

@router.post('')
async def analyze(text: str | None = None) -> List:
    """
    Analyze text to identify PII (personally identifiable information) entities.
    
    Returns
    -------
    List
        Returns a list of PII (personally identifiable information) entities identified in the analyzed text.
    """
    try:
        return ["Not implemented"]
    except Exception as e:
        logging.error(e, stack_info=True, exc_info=True)
        raise HTTPException(
            status_code = 500,
            detail = str(e)
        )
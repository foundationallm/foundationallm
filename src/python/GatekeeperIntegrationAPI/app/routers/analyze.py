"""
The API endpoint for analyzing text to identify PII (personally identifiable information) entities.
"""
from typing import List
from fastapi import APIRouter, Depends
from app.dependencies import validate_api_key_header, handle_exception

router = APIRouter(
    prefix='/analyze',
    tags=['analyze'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}},
    redirect_slashes=False
)

@router.post('')
# Temporary pylint disable until implemented
#pylint: disable=unused-argument
async def analyze(text: str | None = None) -> List:
    """
    Analyze text to identify PII.
    
    Returns
    -------
    List
        Returns a list of PII (personally identifiable information) entities identified
        in the analyzed text.
    """
    try:
        return ["Not implemented"]
    except Exception as e:
        handle_exception(e)

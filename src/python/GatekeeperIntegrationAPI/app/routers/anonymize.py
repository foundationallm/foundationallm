"""
The API Endpoint that anonymizes/scrubs text with identified PII.
"""
from fastapi import APIRouter, Depends
from app.dependencies import validate_api_key_header, handle_exception

router = APIRouter(
    prefix='/anonymize',
    tags=['anonymize'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}},
    redirect_slashes=False
)

@router.post('')
# Temporary pylint disable until implemented
#pylint: disable=unused-argument
async def anonymize(text: str | None = None) -> str:
    """
    Anonymize text with identified PII (personally identifiable information) entities.
    
    Returns
    -------
    str
        Returns the anonymized text.
    """
    try:
        return "Not implemented"
    except Exception as e:
        handle_exception(e)

import logging
from fastapi import APIRouter, Depends, HTTPException
from app.dependencies import validate_api_key_header

router = APIRouter(
    prefix='/anonymize',
    tags=['anonymize'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}},
    redirect_slashes=False
)

@router.post('')
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
        logging.error(e, stack_info=True, exc_info=True)
        raise HTTPException(
            status_code = 500,
            detail = str(e)
        )
from fastapi import APIRouter

router = APIRouter(
    prefix='/status',
    tags=['status'],
    responses={404: {'description':'Not found'}}
)

@router.get('')
async def get_status():
    """
    Retrieves the status of the API.
    
    Returns
    -------
    string
        String containing the current status of the API.
    """
    return 'ready'
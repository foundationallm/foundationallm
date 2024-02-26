"""
The API endpoint for analyzing textual content to identify
PII (personally identifiable information) entities.
"""
from fastapi import APIRouter, Depends
from foundationallm.integration.models import AnalyzeRequest, AnalyzeResponse
from foundationallm.integration.mspresidio import Analyzer
#from foundationallm.telemetry import Telemetry
from app.dependencies import validate_api_key_header, handle_exception

#logger = Telemetry.get_logger(__name__)
#tracer = Telemetry.get_tracer(__name__)

router = APIRouter(
    prefix='/analyze',
    tags=['analyze'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}},
    redirect_slashes=False
)

@router.post('')
async def analyze(request: AnalyzeRequest) -> AnalyzeResponse:
    """
    Analyze textual content to identify PII with the option to
    anonymize.
    
    Returns
    -------
    AnalyzeResponse
        Returns the original content along with a list of PII
        entities identified in the analyzed text.

        If the request includes anonymize=True, the original content
        will be returned anonymized.
    """
    #with tracer.start_as_current_span('analyze') as span:
    try:
        analyzer = Analyzer(request)
        return analyzer.analyze()
    except Exception as e:
        #Telemetry.record_exception(span, e)
        handle_exception(e)

"""
The API endpoint for returning the completion from the LLM for the specified user prompt.
"""
import logging
import json
from typing import Optional
from fastapi import APIRouter, Depends, Header, HTTPException, Request
from foundationallm.config import Context
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse
from foundationallm.langchain.orchestration import OrchestrationManager
from foundationallm.models.metadata import DataSource
from foundationallm.langchain.data_sources.stock import StockConfiguration
from app.dependencies import get_config, validate_api_key_header

# Initialize API routing
router = APIRouter(
    prefix='/orchestration',
    tags=['orchestration'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}}
)

@router.post('/completion')
async def get_completion(completion_request: CompletionRequest, request : Request,
                         x_user_identity: Optional[str] = Header(None)) -> CompletionResponse:
    """
    Retrieves a completion response from a language model.

    Parameters
    ----------
    completion_request : CompletionRequest
        The request object containing the metadata required to build a LangChain agent
        and generate a completion.

    Returns
    -------
    CompletionResponse
        Object containing the completion response and token usage details.
    """
    try:
        #get the orginal request content
        content = await request.body()

        jData = json.loads(content.decode())

        #figure out what the agent type is...
        agent_type = jData['agent']['type']

        #cast the datasource to the correct type
        if ( agent_type == 'stock'):
            completion_request.data_source = DataSource(**completion_request.data_source.dict())
            completion_request.data_source.configuration = StockConfiguration(**jData['data_source']['configuration'])

        config=request.app.extra['config']

        orchestration_manager = OrchestrationManager(completion_request = completion_request,
                                                     configuration=config,
                                                     context=Context(user_identity=x_user_identity))
        return orchestration_manager.run(completion_request.user_prompt)
    except Exception as e:
        logging.error(e, stack_info=True, exc_info=True)
        raise HTTPException(
            status_code = 500,
            detail = str(e)
        ) from e

"""
The API endpoint for returning the completion from the LLM for the specified user prompt.
"""
import asyncio
import json
from aiohttp import ClientSession
from app.dependencies import validate_api_key_header
from app.lifespan_manager import get_config, get_http_client_session, get_plugin_manager
from fastapi import (
    APIRouter,
    Body,
    Depends,
    Header,
    HTTPException,
    status
)
from opentelemetry.trace import SpanKind
from typing import Optional, List
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.orchestration import OrchestrationManager
from foundationallm.models.agents import KnowledgeManagementCompletionRequest
from foundationallm.models.operations import (
    LongRunningOperation,
    LongRunningOperationLogEntry,
    OperationStatus
)
from foundationallm.models.orchestration import CompletionRequestBase, CompletionResponse
from foundationallm.operations import OperationsManager
from foundationallm.plugins import PluginManager
from foundationallm.telemetry import Telemetry

# Initialize telemetry logging
logger = Telemetry.get_logger(__name__)
tracer = Telemetry.get_tracer(__name__)

# Initialize API routing
router = APIRouter(
    prefix='/instances/{instance_id}',
    tags=['completions'],
    dependencies=[Depends(validate_api_key_header)],
    responses={404: {'description':'Not found'}}
)

async def resolve_completion_request(request_body: dict = Body(...)) -> CompletionRequestBase:
    agent_type = request_body.get("agent", {}).get("type", None)

    match agent_type:
        case "knowledge-management":
            request = KnowledgeManagementCompletionRequest(**request_body)
            request.agent.type = agent_type
            return request
        case _:
            raise ValueError(f"Unsupported agent type: {agent_type}")

@router.post(
    '/async-completions',
    summary = 'Submit an async completion request.',
    status_code = status.HTTP_202_ACCEPTED,
    responses = {
        202: {'description': 'Completion request accepted.'},
    }
)
async def submit_completion_request(
    instance_id: str,
    completion_request: CompletionRequestBase = Depends(resolve_completion_request),
    config: Configuration = Depends(get_config),
    http_client_session: ClientSession = Depends(get_http_client_session),
    plugin_manager: PluginManager = Depends(get_plugin_manager),
    x_user_identity: Optional[str] = Header(None)
) -> LongRunningOperation:
    """
    Initiates the creation of a completion response in the background.

    Returns
    -------
    CompletionOperation
        Object containing the operation ID and status.
    """
    with tracer.start_as_current_span('langchainapi_submit_completion_request', kind=SpanKind.SERVER) as span:
        try:
            # Get the operation_id from the completion request.
            operation_id = completion_request.operation_id

            span.set_attribute('operation_id', operation_id)
            span.set_attribute('instance_id', instance_id)
            span.set_attribute('user_identity', x_user_identity)

            # Create an operations manager to create the operation.
            operations_manager = OperationsManager(config, http_client_session, logger)
            # Submit the completion request operation to the state API.
            operation = await operations_manager.create_operation_async(operation_id, instance_id, x_user_identity)

            # Start a background task to perform the completion request.
            asyncio.create_task(
                create_completion_response(
                    operation_id,
                    instance_id,
                    completion_request,
                    config,
                    plugin_manager,
                    operations_manager,
                    x_user_identity
                )
            )

            # Return the long running operation object.
            return operation

        except Exception as e:
            handle_exception(e)

async def create_completion_response(
    operation_id: str,
    instance_id: str,
    completion_request: KnowledgeManagementCompletionRequest,
    config: Configuration,
    plugin_manager: PluginManager,
    operations_manager: OperationsManager,
    x_user_identity: Optional[str] = Header(None)
):
    """
    Generates the completion response for the specified completion request.
    """
    with tracer.start_as_current_span('langchainapi_create_completion_response', kind=SpanKind.SERVER) as span:
        try:
            span.set_attribute('operation_id', operation_id)
            span.set_attribute('instance_id', instance_id)
            span.set_attribute('user_identity', x_user_identity)

            # Change the operation status to 'InProgress' using an async task.
            await operations_manager.update_operation_async(
                operation_id,
                instance_id,
                status = OperationStatus.INPROGRESS,
                status_message = 'Operation state changed to in progress.',
                user_identity = x_user_identity
            )

            # Create the user identity object from the x_user_identity header.
            user_identity = UserIdentity(**json.loads(x_user_identity))

            # Create an orchestration manager to process the completion request.
            orchestration_manager = OrchestrationManager(
                completion_request = completion_request,
                configuration = config,
                plugin_manager = plugin_manager,
                operations_manager = operations_manager,
                instance_id = instance_id,
                user_identity = user_identity
            )

            # Await the completion response from the orchestration manager.
            completion_response = await orchestration_manager.invoke_async(completion_request)
            completion_status = OperationStatus.COMPLETED if completion_response.errors == [] else OperationStatus.FAILED
            completion_status_message = "Operation completed successfully." if completion_response.errors == [] else "Operation failed."

            # Send the completion response to the State API and mark the operation as completed.
            await asyncio.gather(
                operations_manager.set_operation_result_async(
                    operation_id = operation_id,
                    instance_id = instance_id,
                    completion_response = completion_response),
                operations_manager.update_operation_async(
                    operation_id = operation_id,
                    instance_id = instance_id,
                    status = completion_status,
                    status_message = completion_status_message,
                    user_identity = x_user_identity
                )
            )
        except Exception as e:
            # Send the completion response to the State API and mark the operation as failed.
            print (f'Error: {e}')
            completion_response = CompletionResponse(
                operation_id = operation_id,
                user_prompt = completion_request.user_prompt,
                content = [],
                errors=[f"{e}"]
            )
            await asyncio.gather(
                operations_manager.set_operation_result_async(
                    operation_id = operation_id,
                    instance_id = instance_id,
                    completion_response = completion_response),
                operations_manager.update_operation_async(
                    operation_id = operation_id,
                    instance_id = instance_id,
                    status = OperationStatus.FAILED,
                    status_message = f"{e}",
                    user_identity = x_user_identity
                )
            )

@router.get(
    '/async-completions/{operation_id}/status',
    summary = 'Retrieve the status of the completion request operation with the specified operation ID.',
    responses = {
        200: {'description': 'The operation status was retrieved successfully.'},
        404: {'description': 'The specified operation was not found.'}
    }
)
async def get_operation_status(
    instance_id: str,
    operation_id: str,
    config: Configuration = Depends(get_config),
    http_client_session: ClientSession = Depends(get_http_client_session)
) -> LongRunningOperation:
    with tracer.start_as_current_span('langchainapi_get_operation_status', kind=SpanKind.SERVER) as span:
        # Create an operations manager to get the operation status.
        operations_manager = OperationsManager(config, http_client_session, logger)

        try:
            span.set_attribute('operation_id', operation_id)
            span.set_attribute('instance_id', instance_id)

            operation = await operations_manager.get_operation_async(
                operation_id,
                instance_id
            )

            if operation is None:
                raise HTTPException(status_code=404, detail=f"An operation with the id '{operation_id}' does not exist.")

            return operation
        except HTTPException as he:
            handle_exception(he, he.status_code)
        except Exception as e:
            handle_exception(e)

@router.get(
    '/async-completions/{operation_id}/result',
    summary = 'Retrieve the completion result of the operation with the specified operation ID.',
    responses = {
        200: {'description': 'The operation result was retrieved successfully.'},
        404: {'description': 'The specified operation or its result was not found.'}
    }
)
async def get_operation_result(
    instance_id: str,
    operation_id: str,
    config: Configuration = Depends(get_config),
    http_client_session: ClientSession = Depends(get_http_client_session)
) -> CompletionResponse:
    with tracer.start_as_current_span('langchainapi_get_operation_result', kind=SpanKind.SERVER) as span:
        # Create an operations manager to get the operation result.
        operations_manager = OperationsManager(config, http_client_session, logger)

        try:
            span.set_attribute('operation_id', operation_id)
            span.set_attribute('instance_id', instance_id)

            completion_response = await operations_manager.get_operation_result_async(
                operation_id,
                instance_id
            )

            if completion_response is None:
                raise HTTPException(status_code=404)

            return completion_response
        except HTTPException as he:
            handle_exception(he, he.status_code)
        except Exception as e:
            handle_exception(e)

@router.get(
    '/async-completions/{operation_id}/logs',
    summary = 'Retrieve the log of operational steps for the specified operation ID.',
    responses = {
        200: {'description': 'The operation log was retrieved successfully.'},
        404: {'description': 'The specified operation or its log was not found.'}
    }
)
async def get_operation_logs(
    instance_id: str,
    operation_id: str,
    config: Configuration = Depends(get_config),
    http_client_session: ClientSession = Depends(get_http_client_session)
) -> List[LongRunningOperationLogEntry]:
    with tracer.start_as_current_span('langchainapi_get_operation_log', kind=SpanKind.SERVER) as span:
        # Create an operations manager to get the operation log.
        operations_manager = OperationsManager(config, http_client_session, logger)

        try:
            span.set_attribute('operation_id', operation_id)
            span.set_attribute('instance_id', instance_id)

            log = await operations_manager.get_operation_logs_async(
                operation_id,
                instance_id
            )

            if log is None:
                raise HTTPException(status_code=404)

            return log
        except HTTPException as he:
            handle_exception(he, he.status_code)
        except Exception as e:
            handle_exception(e)

def handle_exception(exception: Exception, status_code: int = 500):
    """
    Handles an exception that occurred while processing a request.
    
    Parameters
    ----------
    exception : Exception
        The exception that occurred.
    """
    logger.error(exception, stack_info=True, exc_info=True)
    raise HTTPException(
        status_code = status_code,
        detail = str(exception)
    ) from exception
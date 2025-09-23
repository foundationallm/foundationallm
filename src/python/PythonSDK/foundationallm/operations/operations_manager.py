from aiohttp import ClientSession
import json
import os
from typing import List, Optional
from foundationallm.config import Configuration
from foundationallm.models.operations import (
    LongRunningOperation,
    LongRunningOperationLogEntry,
    OperationStatus
)

from foundationallm.models.orchestration import (
    CompletionResponse,
    OpenAITextMessageContentItem
)
from foundationallm.telemetry import Telemetry
from logging import Logger

class OperationsManager():
    """
    Class for managing long running operations via calls to the StateAPI.
    """
    def __init__(self, config: Configuration, http_client_session: ClientSession = None, logger: Logger = None):
        self.http_client_session = http_client_session or ClientSession()
        self.logger = logger or Telemetry.get_logger(__name__)
        # Retrieve the State API configuration settings.
        self.state_api_url = config.get_value('FoundationaLLM:APIEndpoints:StateAPI:Essentials:APIUrl').rstrip('/')
        self.state_api_key = config.get_value('FoundationaLLM:APIEndpoints:StateAPI:Essentials:APIKey')

        # Determine if the State API should be accessed using HTTPS.
        self.use_ssl = os.environ.get('FOUNDATIONALLM_ENV', 'prod') == 'prod'
    
    async def create_operation_async(
        self,
        operation_id: str,
        instance_id: str,
        user_identity: str
    ) -> Optional[LongRunningOperation]:
        """
        Creates a background operation by settings its initial state through the State API.

        POST {state_api_url}/instances/{instanceId}/operations/{operationId} -> LongRunningOperation
        
        Parameters
        ----------
        operation_id : str
            The unique identifier for the operation.
        instance_id : str
            The unique identifier for the FLLM instance.
        user_identity : str
            The user identity object containing the user principal name of the user who initiated the operation.
        
        Returns
        -------
        Optional[LongRunningOperation]
            Object representing the operation if successful, None if not found.
        """               
        try:
            body = {
                "operation_id": operation_id,
                "instance_id": instance_id,
                "upn": self.__get_upn_from_user_identity(user_identity)
            }

            async with self.http_client_session.post(
                f'{self.state_api_url}/instances/{instance_id}/operations/{operation_id}',
                json = body,
                headers = self.__get_standard_headers(),
                ssl = self.use_ssl
            ) as response:
                if response.status == 404:
                    return None
                if response.status != 200:
                    raise Exception(f'An error occurred while creating the operation {operation_id}: ({response.status}) {await response.text()}')

                return LongRunningOperation(**await response.json())
        except Exception as e:
            self.logger.exception(f'An error occurred while creating the operation {operation_id}: {e}')
            raise

    async def update_operation_async(
        self,
        operation_id: str,
        instance_id: str,
        status: OperationStatus,
        status_message: str,
        user_identity: str
    ) -> Optional[LongRunningOperation]:
        """
        Updates the state of a background operation through the State API.

        PUT {state_api_url}/instances/{instanceId}/operations/{operationId} -> LongRunningOperation
        
        Parameters
        ----------
        operation : LongRunningOperation
            The operation to update.
        instance_id : str
            The unique identifier for the FLLM instance.
        status: OperationStatus
            The new status to assign to the operation.
        status_message: str
            The message to associate with the new status.
        user_identity : str
            The user identity object containing the user principal name of the user who initiated the operation.
        
        Returns
        -------
        Optional[LongRunningOperation]
            Object representing the operation if successful, None if not found.
        """
        try:
            operation = LongRunningOperation(
                operation_id = operation_id,
                status = status,
                status_message = status_message,
                upn = self.__get_upn_from_user_identity(user_identity)
            )
            
            # Call the State API to create a new operation.
            async with self.http_client_session.put(
                f'{self.state_api_url}/instances/{instance_id}/operations/{operation_id}',
                json = operation.model_dump(exclude_unset=True),
                headers = self.__get_standard_headers(),
                ssl = self.use_ssl
            ) as response:
                if response.status == 404:
                    return None
                if response.status != 200:
                    raise Exception(f'An error occurred while updating the status of operation {operation_id}: ({response.status}) {await response.text()}')

                return LongRunningOperation(**await response.json())
        except Exception as e:
            self.logger.exception(f'An error occurred while updating the status of operation {operation_id}: {e}')
            raise
        
    async def get_operation_async(
        self,
        operation_id: str,
        instance_id: str
    ) -> Optional[LongRunningOperation]:
        """
        Retrieves the state of a background operation through the State API.

        GET {state_api_url}/instances/{instanceId}/operations/{operationId} -> LongRunningOperation
        
        Parameters
        ----------
        operation_id : str
            The unique identifier for the operation.
        instance_id : str
            The unique identifier for the FLLM instance.
        
        Returns
        -------
        Optional[LongRunningOperation]
            Object representing the operation if successful, None if not found.
        """
        try:
            # Call the State API to create a new operation.
            async with self.http_client_session.get(
                f'{self.state_api_url}/instances/{instance_id}/operations/{operation_id}',
                headers = self.__get_standard_headers(),
                ssl = self.use_ssl
            ) as response:
                if response.status == 404:
                    return None
                if response.status != 200:
                    raise Exception(f'An error occurred while retrieving the status of the operation {operation_id}: ({response.status}) {await response.text()}')

                return LongRunningOperation(**await response.json())
        except Exception as e:
            self.logger.exception(f'An error occurred while retrieving the status of operation {operation_id}: {e}')
            raise

    async def set_operation_result_async(
        self,
        operation_id: str,
        instance_id: str,
        completion_response: CompletionResponse):
        """
        Sets the result of a completion operation through the State API.

        PUT {state_api_url}/instances/{instanceId}/operations/{operationId}/result -> CompletionResponse
        
        Parameters
        ----------
        operation_id : str
            The unique identifier for the operation.
        instance_id : str
            The unique identifier for the FLLM instance.
        completion_response : CompletionResponse
            The result of the operation.
        """
        try:
            # Call the State API to create a new operation.
            async with self.http_client_session.post(
                f'{self.state_api_url}/instances/{instance_id}/operations/{operation_id}/result',
                json = completion_response.model_dump(),
                headers = self.__get_standard_headers(),
                ssl = self.use_ssl
            ) as response:
                if response.status == 404:
                    return None
                if response.status != 200:
                    raise Exception(f'An error occurred while submitting the result of operation {operation_id}: ({response.status}) {await response.text()}')
        except Exception as e:
            self.logger.exception(f'An error occurred while submitting the result of operation {operation_id}: {e}')
            raise

    async def get_operation_result_async(
        self,
        operation_id: str,
        instance_id: str
    ) -> Optional[CompletionResponse]:
        """
        Retrieves the result of an async completion operation through the State API.

        GET {state_api_url}/instances/{instanceId}/operations/{operationId}/result -> CompletionResponse
        
        Parameters
        ----------
        operation_id : str
            The unique identifier for the operation.
        instance_id : str
            The unique identifier for the FLLM instance.
        
        Returns
        -------
        CompletionResponse
            Object representing the operation result.
        """
        try:
            # Call the State API to create a new operation.
            async with self.http_client_session.get(
                f'{self.state_api_url}/instances/{instance_id}/operations/{operation_id}/result',
                headers = self.__get_standard_headers(),
                ssl = self.use_ssl
            ) as response:
                if response.status == 404:
                    return None
                if response.status != 200:
                    raise Exception(f'An error occurred while retrieving the result of operation {operation_id}: ({response.status}) {await response.text()}')

                return CompletionResponse(**await response.json())
        except Exception as e:
            self.logger.exception(f'An error occurred while retrieving the result of operation {operation_id}: {e}')
            raise

    async def get_operation_logs_async(
        self,
        operation_id: str,
        instance_id: str
    ) -> Optional[List[LongRunningOperationLogEntry]]:
        """
        Retrieves a list of log entries for an async operation through the State API.

        GET {state_api_url}/instances/{instanceId}/operations/{operationId}/log -> List[LongRunningOperationLogEntry]
        
        Parameters
        ----------
        operation_id : str
            The unique identifier for the operation.
        instance_id : str
            The unique identifier for the FLLM instance.
        
        Returns
        -------
        Optional[List[LongRunningOperationLogEntry]]
            List of log entries for the operation if successful, None if not found.
        """
        try:
            # Call the State API to create a new operation.
            async with self.http_client_session.get(
                f'{self.state_api_url}/instances/{instance_id}/operations/{operation_id}/logs',
                headers = self.__get_standard_headers(),
                ssl = self.use_ssl
            ) as response:
                if response.status == 404:
                    return None
                if response.status != 200:
                    raise Exception(f'An error occurred while retrieving the log for operation {operation_id}: ({response.status}) {await response.text()}')

                return LongRunningOperationLogEntry(**await response.json())
        except Exception as e:
            self.logger.exception(f'An error occurred while retrieving the log for operation {operation_id}: {e}')
            raise

    async def update_operation_with_text_result_async(
        self,
        operation_id: str,
        instance_id: str,
        status: OperationStatus,
        status_message: str,
        result_message: str,
        user_identity: str
    ) -> Optional[LongRunningOperation]:
        
        """
        Updates the state of a background operation through the State API, including a text result.
        
        Parameters
        ----------
        operation : LongRunningOperation
            The operation to update.
        instance_id : str
            The unique identifier for the FLLM instance.
        status: OperationStatus
            The new status to assign to the operation.
        status_message: str
            The message to associate with the new status.
        result_message: str
            The text result to used as the result of the operation.
        user_identity : str
            The user identity object containing the user principal name of the user who initiated the operation.
        
        Returns
        -------
        Optional[LongRunningOperation]
            Object representing the operation if successful, None if not found.
        """
        try:
            
            await self.set_operation_result_async(
                operation_id,
                instance_id,
                CompletionResponse(
                    operation_id=operation_id,
                    user_prompt='',
                    completion=result_message,
                    content=[
                        OpenAITextMessageContentItem(value=result_message)
                    ]
                ))
            
            operation = await self.update_operation_async(
                operation_id,
                instance_id,
                status,
                status_message,
                user_identity
            )

            return operation

        except Exception as e:
            self.logger.exception(f'An error occurred while updating the status of operation {operation_id}: {e}')
            raise

    def __get_standard_headers(self):
        """
        Retrieves the standard headers for interacting with the State API.
        """
        return {
            "x-api-key": self.state_api_key,
            "charset":"utf-8",
            "Content-Type":"application/json"
        }
    
    def __get_upn_from_user_identity(self, user_identity: str) -> str:
        """
        Retrieves the user principal name from the user identity object.

        Parameters
        ----------
        user_identity : str
            The user identity object containing the user principal name of the user who initiated the operation.
        
        Returns
        -------
        str
            The user principal name.
        """
        user_identity_dict = json.loads(user_identity)
        return user_identity_dict.get('upn', '')
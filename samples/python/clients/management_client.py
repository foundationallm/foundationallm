'''
Management API client for FoundationLLM.
This client is used to interact with the FoundationLLM Management API.
'''

import requests
from simple_jwt import jwt
from urllib.parse import urlparse

from azure.identity import AzureCliCredential


class ManagementClient:
    '''
    Management API client for FoundationLLM.
    This client is used to interact with the FoundationLLM Management API.
    '''

    def __init__(
        self,
        management_api_scope: str,
        management_api_endpoint: str,
        foundationallm_instance_id: str):

        self.__management_api_scope = management_api_scope
        self.__management_api_endpoint = management_api_endpoint
        self.__foundationallm_instance_id = foundationallm_instance_id

        self.__verify_ssl = False if urlparse(management_api_endpoint.lower()).hostname == "localhost" else True

        self.__credential = AzureCliCredential()
        self.__token = self.__credential.get_token(self.__management_api_scope).token

        self.default_text_partitioning_profile = "text_partition_default"
        self.default_text_embedding_profile = "text_embedding_profile_gateway_embedding3large"

    # region: Basic REST API methods

    def __ensure_valid_token(self):
        '''
        Ensure that the token is valid.
        If the token is expired, get a new one.
        '''
        if not self.__token:
            self.__token = self.__credential.get_token(self.__management_api_scope).token

        if jwt.is_expired(self.__token):
            self.__token = self.__credential.get_token(self.__management_api_scope).token

    def __post_request(self, route:str, data: dict) -> dict:
        '''
        Post a request to the Management API.
        - param data: The data to post.
        - param route: The route to post to.
        - return: The response from the API.
        '''
        self.__ensure_valid_token()

        headers = {
            "Authorization": f"Bearer {self.__token}",
            "Content-Type": "application/json"
        }

        response = requests.post(
            f"{self.__management_api_endpoint}/instances/{self.__foundationallm_instance_id}/{route}",
            headers=headers,
            json=data,
            timeout=60,
            verify=self.__verify_ssl
        )

        if response.status_code != 200:
            raise Exception(f"Error posting to Management API: {response.text}")

        return response.json()

    def __get_request(self, route:str) -> dict:
        '''
        Post a request to the Management API.
        - param data: The data to post.
        - param route: The route to post to.
        - return: The response from the API.
        '''
        self.__ensure_valid_token()

        headers = {
            "Authorization": f"Bearer {self.__token}"
        }

        response = requests.get(
            f"{self.__management_api_endpoint}/instances/{self.__foundationallm_instance_id}/{route}",
            headers=headers,
            timeout=60,
            verify=self.__verify_ssl
        )

        if response.status_code != 200:
            raise Exception(f"Error posting to Management API: {response.text}")

        return response.json()

    # endregion

    # region: Agents

    # region: Agent workflows

    def get_agent_workflows(self) -> dict:
        '''
        Retrieves the list of agent workflows.
        '''

        result = self.__get_request(
            "providers/FoundationaLLM.Agent/workflows"
        )

        return result

    # endregion

    # endregion

    # region: Vectorization pipelines

    def create_vectorization_pipeline(
        self,
        pipeline_name: str,
        pipeline_description: str,
        data_source_name: str,
        vector_store_name: str
    ) -> dict:
        '''
        Creates a vectorization pipeline.
        - pipeline_name: The name of the pipeline.
        - pipeline_description: The description of the pipeline.
        - data_source_name: The name of the data source.
        - vector_store_name: The name of the vector store.
        '''
        
        result = self.__post_request(
            f"providers/FoundationaLLM.Vectorization/vectorizationPipelines/{pipeline_name}",
            {
                "name": pipeline_name,
                "description": pipeline_description,
                "data_source_object_id": f"/instances/{self.__foundationallm_instance_id}/providers/FoundationaLLM.DataSource/dataSources/{data_source_name}",
                "text_partitioning_profile_object_id": f"/instances/{self.__foundationallm_instance_id}/providers/FoundationaLLM.Vectorization/textPartitioningProfiles/{self.default_text_partitioning_profile}",
                "text_embedding_profile_object_id": f"/instances/{self.__foundationallm_instance_id}/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles/{self.default_text_embedding_profile}",
                "indexing_profile_object_id": f"/instances/{self.__foundationallm_instance_id}/providers/FoundationaLLM.Vectorization/indexingProfiles/{vector_store_name}",
                "trigger_type": "Manual"
            }
        )

        return result

    def get_vectorization_pipeline(
        self,
        pipeline_name: str
    ) -> dict:
        '''
        Retrieves a vectorization pipeline.
        - pipeline_name: The name of the pipeline.
        '''

        result = self.__get_request(
            f"providers/FoundationaLLM.Vectorization/vectorizationPipelines/{pipeline_name}"
        )

        return result

    def activate_vectorization_pipeline(
        self,
        pipeline_name: str
    ) -> dict:
        '''
        Activates a vectorization pipeline.
        - pipeline_name: The name of the pipeline.
        '''

        result = self.__post_request(
            f"providers/FoundationaLLM.Vectorization/vectorizationPipelines/{pipeline_name}/activate",
            {}
        )

        return result

    def get_vectorization_pipeline_execution(
        self,
        pipeline_name: str,
        execution_id: str
    ) -> dict:
        '''
        Retrieves a vectorization pipeline execution.
        - pipeline_name: The name of the pipeline.
        - execution_id: The ID of the execution.
        '''

        result = self.__get_request(
            f"providers/FoundationaLLM.Vectorization/vectorizationPipelines/{pipeline_name}/vectorizationPipelineExecutions/{execution_id}"
        )

        return result
    
    # endregion

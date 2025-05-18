'''
Generic API client for FoundationLLM.
This client is used to interact with the FoundationLLM APIs.
'''

from urllib.parse import urlparse

import requests
from simple_jwt import jwt

from azure.identity import AzureCliCredential

class APIClient:
    '''
    Generic API client for FoundationLLM.
    This client is used to interact with the FoundationLLM APIs.
    '''

    def __init__(
        self,
        api_scope: str,
        api_endpoint: str,
        foundationallm_instance_id: str):

        self.__api_scope = api_scope
        self.__api_endpoint = api_endpoint
        self.__foundationallm_instance_id = foundationallm_instance_id

        self.__verify_ssl = (urlparse(api_endpoint.lower()).hostname != "localhost")

        self.__credential = AzureCliCredential()
        self.__token = self.__credential.get_token(self.__api_scope).token

    def __ensure_valid_token(self):
        '''
        Ensure that the token is valid.
        If the token is expired, get a new one.
        '''
        if not self.__token:
            self.__token = self.__credential.get_token(self.__api_scope).token

        if jwt.is_expired(self.__token):
            self.__token = self.__credential.get_token(self.__api_scope).token

    def post_request(
            self,
            route: str,
            data: dict,
            include_instance: bool = True) -> dict:
        '''
        Post a request to the API.
        - param data: The data to post.
        - param route: The route to post to.
        - param include_instance: Whether to include the instance ID in the route.
        
        Return: The response from the API.
        '''
        self.__ensure_valid_token()

        headers = {
            "Authorization": f"Bearer {self.__token}",
            "Content-Type": "application/json"
        }

        response = requests.post(
            f"{self.__api_endpoint}/instances/{self.__foundationallm_instance_id}/{route}" if include_instance \
                else f"{self.__api_endpoint}/{route}",
            headers=headers,
            json=data,
            timeout=60,
            verify=self.__verify_ssl
        )

        response.raise_for_status()

        return response.json()

    def get_request(
            self,
            route:str,
            include_instance: bool = True) -> dict:
        '''
        Post a request to the API.
        - param route: The route to post to.
        - param include_instance: Whether to include the instance ID in the route.
        
        Return: The response from the API.
        '''
        self.__ensure_valid_token()

        headers = {
            "Authorization": f"Bearer {self.__token}"
        }

        response = requests.get(
            f"{self.__api_endpoint}/instances/{self.__foundationallm_instance_id}/{route}" if include_instance \
                else f"{self.__api_endpoint}/{route}",
            headers=headers,
            timeout=60,
            verify=self.__verify_ssl
        )

        response.raise_for_status()

        return response.json()

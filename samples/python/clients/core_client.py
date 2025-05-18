'''
Core API client for FoundationLLM.
This client is used to interact with the FoundationLLM Core API.
'''

from .api_client import APIClient

class CoreClient(APIClient):
    '''
    Core API client for FoundationLLM.
    This client is used to interact with the FoundationLLM Core API.
    '''

    def get_agents(self) -> dict:
        '''
        Retrieves the list of agents.
        '''

        result = self.get_request(
            "completions/agents"
        )

        return result
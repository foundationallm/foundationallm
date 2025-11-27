"""
Authentication Manager for FoundationaLLM Agent Tests

Provides authentication headers for FoundationaLLM API calls.
"""

from datetime import datetime, timezone
import os
import threading

import jwt

from azure.identity import AzureCliCredential
from dotenv import load_dotenv

class TestAPIAccessToken:
    """
    Manages access tokens for FoundationaLLM frontier APIs (e.g., Core API, Management API).
    """

    __api_name: str = None
    __api_scope: str = None
    __static_access_token: str = None
    __static_access_token_header_name: str = None
    __bearer_access_token: str = None
    __bearer_access_token_expiration: int = 0
    __use_static_token: bool = False
    __azure_credential: AzureCliCredential = None
    __lock = threading.Lock()

    def __init__(
        self,
        api_name: str,
        static_access_token: str = None,
        static_access_token_header_name: str = None,
        api_scope: str = None,
        azure_credential: AzureCliCredential = None,
    ):
        self.__api_name = api_name
        self.__static_access_token = static_access_token
        self.__static_access_token_header_name = static_access_token_header_name
        self.__use_static_token = True
        if not self.__static_access_token:
            self.__use_static_token = False
            print(
                f"A static access token for {self.__api_name} was not found in environment variables. "
                "Falling back to Azure CLI authentication."
            )
            self.__api_scope = api_scope
            self.__azure_credential = azure_credential if azure_credential else AzureCliCredential()
            self.__refresh_bearer_token()
        else:
            print(f"Using static access token for {self.__api_name} from environment variables.")

    def get_api_auth_header(self):
        """Get authentication header for the API."""

        if self.__use_static_token:
            return {self.__static_access_token_header_name: self.__static_access_token}

        # Since the token refresh logic is protected by a lock to ensure thread safety,
        # perform a quick expiration check before comitting to run the locked code.
        if datetime.now(timezone.utc).timestamp() >= \
            self.__bearer_access_token_expiration:
            self.__refresh_bearer_token()

        return {"Authorization": f"Bearer {self.__bearer_access_token}"}

    def __get_bearer_token_expiration_time(
        self
    ) -> int:
        """Get the expiration time of a JWT token in epoch seconds."""

        try:
            decoded_token = jwt.decode(
                self.__bearer_access_token,
                options={"verify_signature": False, "verify_exp": False})
            return decoded_token.get("exp", 0)
        # pylint: disable-next=broad-exception-caught
        except Exception as exception:
            print(
                f"Failed to decode {self.__api_name} bearer token: {exception}. "
                "The token will be considered expired.")
            return 0

    def __refresh_bearer_token(self):
        """Refresh bearer token."""

        with self.__lock:
            # An expiration check was likely already performed before acquiring the lock,
            # but check again to avoid redundant refreshes in multi-threaded scenarios.
            if datetime.now(timezone.utc).timestamp() >= \
                self.__bearer_access_token_expiration:

                print(f"{self.__api_name} bearer token expired. Refreshing...")
                token = self.__azure_credential.get_token(self.__api_scope)
                self.__bearer_access_token = token.token
                self.__bearer_access_token_expiration = \
                    self.__get_bearer_token_expiration_time()
                print(f"Successfully obtained the {self.__api_name} bearer token via Azure CLI.")
            else:
                print(f"{self.__api_name} bearer token is still valid. No refresh needed.")

class TestAuthenticationManagerException(Exception):
    """Exception for Test Authentication Manager errors."""

class TestAuthenticationManager:
    """
    Test Authentication Manager for FoundationaLLM Agent Tests
    """

    CORE_API_SCOPE = "api://FoundationaLLM-Core/.default"
    MANAGEMENT_API_SCOPE = "api://FoundationaLLM-Management/.default"

    __core_api_access_token: TestAPIAccessToken = None
    __management_api_access_token: TestAPIAccessToken = None
    __initialized: bool = False

    def initialize(self):
        """Initialize the authentication manager."""
        print("Initializing Test Authentication Manager...")
        load_dotenv()

        core_api_static_access_token = os.getenv("FLLM_ACCESS_TOKEN")
        self.__core_api_access_token = TestAPIAccessToken(
            api_name="Core API",
            static_access_token=core_api_static_access_token,
            static_access_token_header_name="X-AGENT-ACCESS-TOKEN",
            api_scope=self.CORE_API_SCOPE,
        )

        management_api_static_access_token = os.getenv("FLLM_MGMT_BEARER_TOKEN")
        self.__management_api_access_token = TestAPIAccessToken(
            api_name="Management API",
            static_access_token=management_api_static_access_token,
            static_access_token_header_name="Authorization",
            api_scope=self.MANAGEMENT_API_SCOPE,
        )

        print("Test Authentication Manager initialized.")
        self.__initialized = True

    def get_core_api_auth_header(self):
        """Get authentication header for Core API."""

        self.__ensure_initialized()

        return self.__core_api_access_token.get_api_auth_header()

    def get_management_api_auth_header(self):
        """Get authentication header for Management API."""

        self.__ensure_initialized()

        return self.__management_api_access_token.get_api_auth_header()

    def __ensure_initialized(self):
        """Ensure the authentication manager is initialized."""
        if not self.__initialized:
            raise TestAuthenticationManagerException(
                "TestAuthenticationManager is not initialized. Call 'initialize()' before using.")

if __name__ == "__main__":
    authentication_manager = TestAuthenticationManager()
    authentication_manager.initialize()

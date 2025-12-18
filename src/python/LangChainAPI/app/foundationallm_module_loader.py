"""
Loader for the foundationallm module from Azure Blob Storage.
Downloads the wheel file and adds it to sys.path for normal import.
"""
from logging import Logger
import json
import os
import sys

from azure.appconfiguration import AzureAppConfigurationClient
from azure.storage.blob import BlobServiceClient
from azure.identity import DefaultAzureCredential

LANGCHAINAPI_CONFIGURATION_SECTION = \
    'FoundationaLLM:APIEndpoints:LangChainAPI:Configuration'
STORAGE_ACCOUNT_NAME_KEY = \
    f'{LANGCHAINAPI_CONFIGURATION_SECTION}:ExternalModules:Storage:AccountName'
STORAGE_AUTH_TYPE_KEY = \
    f'{LANGCHAINAPI_CONFIGURATION_SECTION}:ExternalModules:Storage:AuthenticationType'
STORAGE_CONTAINER = 'resource-provider'
PACKAGE_BLOB_PATH = 'FoundationaLLM.Plugin/Python-FoundationaLLM.json'
LOCAL_MODULES_FOLDER = 'foundationallm_external_modules'


class FoundationaLLMModuleLoader:
    """
    Loads the foundationallm module from Azure Blob Storage.
    """

    def __init__(self, logger: Logger):
        self.logger = logger
        self.local_path = f'./{LOCAL_MODULES_FOLDER}'
        self.loaded = False

        if not os.path.exists(self.local_path):
            os.makedirs(self.local_path)

    def _get_credential(self):
        """Get Azure credential based on context."""
        if os.getenv('FOUNDATIONALLM_CONTEXT', 'NONE') == 'DEBUG':
            return DefaultAzureCredential(
                exclude_workload_identity_credentials=True,
                exclude_developer_cli_credential=False,
                exclude_cli_credential=False,
                exclude_environment_credential=True,
                exclude_managed_identity_credential=True,
                exclude_powershell_credential=True,
                exclude_visual_studio_code_credential=True,
                exclude_shared_token_cache_credentials=True,
                exclude_interactive_browser_credential=True
            )
        return DefaultAzureCredential(exclude_environment_credential=True)

    def _get_config_value(self, key: str) -> str:
        """Read a value from Azure App Configuration."""
        app_config_uri = os.getenv('FOUNDATIONALLM_APP_CONFIGURATION_URI')
        if not app_config_uri:
            raise ValueError('FOUNDATIONALLM_APP_CONFIGURATION_URI environment variable not set.')

        credential = self._get_credential()
        client = AzureAppConfigurationClient(base_url=app_config_uri, credential=credential)
        setting = client.get_configuration_setting(key=key)
        return setting.value

    def load(self):
        """Downloads and loads the foundationallm module from blob storage."""
        try:
            storage_account = self._get_config_value(STORAGE_ACCOUNT_NAME_KEY)
        except Exception:
            self.logger.exception('Failed to read storage configuration from App Configuration.')
            return

        self.logger.info(f'Loading foundationallm module from storage account: {storage_account}')

        try:
            credential = self._get_credential()
            blob_service = BlobServiceClient(
                account_url=f"https://{storage_account}.blob.core.windows.net",
                credential=credential
            )
            container = blob_service.get_container_client(STORAGE_CONTAINER)

            # Read package descriptor
            package_blob = container.get_blob_client(PACKAGE_BLOB_PATH)
            package_info = json.loads(package_blob.download_blob().readall())
            wheel_path = package_info['package_file_path']

            # Download wheel file
            wheel_blob = container.get_blob_client(wheel_path)
            local_wheel = f'{self.local_path}/{os.path.basename(wheel_path)}'

            self.logger.info(f'Downloading wheel to: {local_wheel}')
            with open(local_wheel, 'wb') as f:
                f.write(wheel_blob.download_blob().readall())

            # Add to sys.path
            if local_wheel not in sys.path:
                sys.path.insert(0, local_wheel)

            self.loaded = True
            self.logger.info('foundationallm module loaded successfully.')

        except Exception as e:
            self.logger.exception(f'Failed to load foundationallm module: {e}')

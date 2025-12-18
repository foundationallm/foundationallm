'''
Handles the FastAPI application lifespan events.
'''
import logging

from contextlib import asynccontextmanager

from aiohttp import ClientSession
from fastapi import FastAPI

# Ensure foundationallm imports work.
# For this, we need to load the foundationallm module first.
from .foundationallm_module_loader import FoundationaLLMModuleLoader
loader = FoundationaLLMModuleLoader(logging.getLogger(__name__))
loader.load()

# pylint: disable=C0411,C0413
from foundationallm.config import Configuration
from foundationallm.plugins import PluginManager
from foundationallm.storage import BlobStorageManager
from foundationallm.telemetry import Telemetry
# pylint: enable=C0411

COMPLETION_REQUESTS_CONFIGURATION_NAMESPACE = \
    'FoundationaLLM:APIEndpoints:OrchestrationAPI:Configuration:CompletionRequestsStorage'
COMPLETION_REQUESTS_STORAGE_ACCOUNT_NAME = \
    f'{COMPLETION_REQUESTS_CONFIGURATION_NAMESPACE}:AccountName'
COMPLETION_REQUESTS_STORAGE_AUTHENTICATION_TYPE = \
    f'{COMPLETION_REQUESTS_CONFIGURATION_NAMESPACE}:AuthenticationType'
COMPLETION_REQUESTS_STORAGE_CONTAINER = \
    f'{COMPLETION_REQUESTS_CONFIGURATION_NAMESPACE}:ContainerName'

@asynccontextmanager
async def lifespan(app: FastAPI):
    """Async context manager for the FastAPI application lifespan."""

    # Create the application configuration
    app.state.config = Configuration()

    # Create an aiohttp client session
    app.state.http_client_session = ClientSession()

    # Configure telemetry monitoring
    Telemetry.configure_monitoring(
        app.state.config,
        'FoundationaLLM:APIEndpoints:LangChainAPI:Essentials:AppInsightsConnectionString',
        'LangChainAPI')

    # Create the plugin manager
    app.state.plugin_manager = PluginManager(
        app.state.config,
        Telemetry.get_logger(__name__))
    app.state.plugin_manager.load_external_modules()

    storage_account_name = app.state.config.get_value(
        COMPLETION_REQUESTS_STORAGE_ACCOUNT_NAME)
    storage_authentication_type = app.state.config.get_value(
        COMPLETION_REQUESTS_STORAGE_AUTHENTICATION_TYPE)
    storage_container_name = app.state.config.get_value(
        COMPLETION_REQUESTS_STORAGE_CONTAINER)
    app.state.completion_requests_storage_manager = BlobStorageManager(
        account_name = storage_account_name,
        authentication_type = storage_authentication_type,
        container_name = storage_container_name
    )

    yield

    # Perform shutdown actions here
    await app.state.http_client_session.close()

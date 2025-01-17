from aiohttp import ClientSession
from contextlib import asynccontextmanager
from foundationallm.config import Configuration
from foundationallm.plugins import PluginManager, plugin_manager
from foundationallm.telemetry import Telemetry

config: Configuration = None
http_client_session: ClientSession = None
plugin_manager: PluginManager = None

@asynccontextmanager
async def lifespan(app):
    """Async context manager for the FastAPI application lifespan."""
    global config
    global http_client_session
    global plugin_manager

    # Create the application configuration
    config = Configuration()

    # Create an aiohttp client session
    http_client_session = ClientSession()

    # Create the plugin manager
    Telemetry.configure_monitoring(config, f'FoundationaLLM:APIEndpoints:LangChainAPI:Essentials:AppInsightsConnectionString', 'LangChainAPI')
    plugin_manager = PluginManager(config, Telemetry.get_logger(__name__))
    plugin_manager.load_external_modules()

    yield

    # Perform shutdown actions here
    await http_client_session.close()

async def get_config() -> Configuration:
    """Retrieves the application configuration."""
    return config

async def get_http_client_session() -> ClientSession:
    """Retrieves the aiohttp client session."""
    return http_client_session

async def get_plugin_manager() -> PluginManager:
    """Retrieves the plugin manager."""
    return plugin_manager

'''
Manages the plugins in the system.'''
from importlib import import_module, reload
from logging import Logger
import json
import os
import sys

from foundationallm.config import Configuration
from foundationallm.storage import BlobStorageManager

from .external_module import ExternalModule
from .plugin_manager_types import PluginManagerTypes

PLUGIN_MANAGER_CONFIGURATION_NAMESPACE = \
    'FoundationaLLM:APIEndpoints:LangChainAPI:Configuration:ExternalModules'
PLUGIN_MANAGER_STORAGE_ACCOUNT_NAME = \
    f'{PLUGIN_MANAGER_CONFIGURATION_NAMESPACE}:Storage:AccountName'
PLUGIN_MANAGER_STORAGE_AUTHENTICATION_TYPE = \
    f'{PLUGIN_MANAGER_CONFIGURATION_NAMESPACE}:Storage:AuthenticationType'
PLUGIN_MANAGER_STORAGE_CONTAINER = 'resource-provider'
PLUGIN_MANAGER_STORAGE_ROOT_PATH = 'FoundationaLLM.Plugin'
PLUGIN_MANAGER_LOCAL_STORAGE_FOLDER_NAME = \
    'foundationallm_external_modules'

class PluginManager():
    """
    Manages the plugins in the system.
    """

    object_cache : dict[str, object] = {}

    def __init__(self, config:Configuration, logger:Logger):
        """
        Initializes the plugin manager.

        Parameters
        ----------
        config : Configuration
            The configuration object for the system.
        logger : Logger
            The logger object used for logging.
        """
        self.config = config
        self.logger = logger
        self.external_modules: dict[str, ExternalModule] = {}
        self.modules_local_path = f'./{PLUGIN_MANAGER_LOCAL_STORAGE_FOLDER_NAME}'

        if not os.path.exists(self.modules_local_path):
            os.makedirs(self.modules_local_path)

        self.initialized = False
        valid_configuration = False

        try:
            storage_account_name = config.get_value(PLUGIN_MANAGER_STORAGE_ACCOUNT_NAME)
            storage_authentication_type = config.get_value(PLUGIN_MANAGER_STORAGE_AUTHENTICATION_TYPE)
            valid_configuration = True
        except Exception:
            self.logger.exception('The plugin manager configuration is not set up correctly. No plugins will be loaded.')

        if valid_configuration:

            self.logger.info((
                'Initializing plugin manager with the following configuration:\n',
                f'Storage account name:: {storage_account_name}\n',
                f'Storage authentication type: {storage_authentication_type}\n',
                f'Storage container name: {PLUGIN_MANAGER_STORAGE_CONTAINER}\n',
                f'Storage root path: {PLUGIN_MANAGER_STORAGE_ROOT_PATH}\n',
                f'Modules local path: {self.modules_local_path}\n'
            ))

            try:

                self.storage_manager = BlobStorageManager(
                    account_name=storage_account_name,
                    container_name=PLUGIN_MANAGER_STORAGE_CONTAINER,
                    authentication_type=storage_authentication_type
                )

                plugin_blobs = self.storage_manager.list_blobs(
                    f'{PLUGIN_MANAGER_STORAGE_ROOT_PATH}/Python-')
                plugin_blob_names = [blob.name for blob in plugin_blobs]

                for plugin_blob_name in plugin_blob_names:
                    self.logger.info(f'Loading plugin package from: {plugin_blob_name}')

                    plugin_package_content = self.storage_manager.read_file_content(plugin_blob_name)
                    plugin_package = json.loads(plugin_package_content)

                    module_file = plugin_package['package_file_path']
                    module_name = plugin_package['properties']['module_name']
                    plugin_manager_class_names = plugin_package['properties']['plugin_managers']

                    for plugin_manager_class_name in plugin_manager_class_names.split(','):

                        if module_name in self.external_modules:
                            self.external_modules[module_name].plugin_manager_class_names.append(plugin_manager_class_name)
                        else:
                            self.external_modules[module_name] = ExternalModule(
                                module_file=module_file,
                                module_name=module_name,
                                plugin_manager_class_names=[plugin_manager_class_name]
                            )

                self.initialized = True
                self.logger.info('The plugin manager initialized successfully.')

            except Exception as e:
                self.logger.exception('An error occurred while initializing the plugin manager storage manager. No plugins will be loaded.')
                self.logger.error(f'Exception details: {e}')

    def load_external_modules(self, reload_modules:bool=False):
        """
        Loads the external modules into the system.
        """
        if not self.initialized:
            self.logger.error('The plugin manager is not initialized. No plugins will be loaded.')
            return

        loaded_modules = set()

        for module_name, external_module in self.external_modules.items():

            module_file_name = external_module.module_file
            local_module_file_name = f'{self.modules_local_path}/{os.path.basename(module_file_name)}'
            self.logger.info(f'Loading module from {module_file_name}')

            try:

                if module_name in loaded_modules:
                    self.logger.info(f'Module {module_name} and all plugin managers are already loaded.')
                    continue
                if module_name in sys.modules:
                    # Module was already loaded externally (not by PluginManager)
                    self.logger.info(f'Module {module_name} was already loaded externally. Reusing existing module.')
                    external_module.module = sys.modules[module_name]
                    external_module.module_loaded = True
                    loaded_modules.add(module_name)

                    # NOTE: Modules loaded outside of PluginManager are not expected to have plugin managers.
                    # Consequently, we skip loading plugin managers for such modules.
                    continue

                if (self.storage_manager.file_exists(module_file_name)):
                    self.logger.info(f'Copying module file to: {local_module_file_name}')
                    module_file_binary_content = self.storage_manager.read_file_content(module_file_name)
                    with open(local_module_file_name, 'wb') as f:
                        f.write(module_file_binary_content)

                sys.path.insert(0, local_module_file_name)

                if reload_modules:
                    external_module.module = reload(external_module.module)
                    self.logger.info(f'Module {module_name} reloaded successfully.')
                else:
                    external_module.module = import_module(external_module.module_name)
                    self.logger.info(f'Module {module_name} loaded successfully.')

                external_module.module_loaded = True
                loaded_modules.add(module_name)

                for plugin_manager_class_name in external_module.plugin_manager_class_names:
                    # Note the () at the end of the getattr call - this is to call the class constructor, not just get the class.
                    external_module.plugin_managers.append(getattr(external_module.module, plugin_manager_class_name)())

            except Exception as e:
                self.logger.exception(f'An error occurred while loading module {module_name}: {str(e)}')

    def clear_cache(self):
        """
        Clears the object cache.
        """
        self.object_cache.clear()
        self.logger.info('Object cache cleared.')


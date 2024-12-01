from importlib import import_module
import os
import sys
import uuid

from foundationallm.config import Configuration
from foundationallm.models.plugins import ExternalModule
from foundationallm.storage import BlobStorageManager
from logging import Logger

PLUGIN_MANAGER_CONFIGURATION_NAMESPACE = 'FoundationaLLM:APIEndpoints:LangChainAPI:Configuration:ExternalModules'
PLUGIN_MANAGER_STORAGE_ACCOUNT_NAME = f'{PLUGIN_MANAGER_CONFIGURATION_NAMESPACE}:Storage:AccountName'
PLUGIN_MANAGER_STORAGE_AUTHENTICATION_TYPE = f'{PLUGIN_MANAGER_CONFIGURATION_NAMESPACE}:Storage:AuthenticationType'
PLUGIN_MANAGER_STORAGE_CONTAINER = f'{PLUGIN_MANAGER_CONFIGURATION_NAMESPACE}:RootStorageContainer'
PLUGIN_MANAGER_MODULES = f'{PLUGIN_MANAGER_CONFIGURATION_NAMESPACE}:Modules'

TOOLS_PLUGIN_MANAGER_TYPE = 'tools'

class PluginManager():
    """
    Manages the plugins in the system.
    """

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
        self.module_configurations: dict[str, ExternalModule] = {}
        self.modules_local_path = f'/foundationallm_external_modules_{uuid.uuid4()}'

        if not os.path.exists(self.modules_local_path):
            os.makedirs(self.modules_local_path)

        self.initialized = False
        valid_configuration = False

        try:
            storage_account_name = config.get_value(PLUGIN_MANAGER_STORAGE_ACCOUNT_NAME)
            storage_authentication_type = config.get_value(PLUGIN_MANAGER_STORAGE_AUTHENTICATION_TYPE)
            storage_container_name = config.get_value(PLUGIN_MANAGER_STORAGE_CONTAINER)
            modules_list = config.get_value(PLUGIN_MANAGER_MODULES)
            valid_configuration = True
        except:
            self.logger.exception('The plugin manager configuration is not set up correctly. No plugins will be loaded.')

        if valid_configuration:

            self.logger.info((
                f'Initializing plugin manager with the following configuration:\n',
                f'Storage account name:: {storage_account_name}\n',
                f'Storage authentication type: {storage_authentication_type}\n',
                f'Storage container name: {storage_container_name}\n',
                f'Modules list: {modules_list}\n',
                f'Modules local path: {self.modules_local_path}\n'
            ))

            try:

                self.storage_manager = BlobStorageManager(
                    account_name=storage_account_name,
                    container_name=storage_container_name,
                    authentication_type=storage_authentication_type
                )

                for module_configuration in [x.split('|') for x in modules_list.split(',')]:
                    module_name = module_configuration[0]
                    class_name = None
                    if (module_configuration[1] == TOOLS_PLUGIN_MANAGER_TYPE):
                        class_name = module_configuration[2]
                    else:
                        raise ValueError(f'The plugin manager type {module_configuration[1]} is not recognized.')

                    if module_name in self.module_configurations:
                        self.module_configurations[module_name].tool_plugin_manager_class_name = class_name
                    else:
                        self.module_configurations[module_name] = ExternalModule(
                            module_name=module_name,
                            tool_plugin_manager_class_name=class_name
                        )

                self.initialized = True
                self.logger.info('The plugin manager initialized successfully.')

            except:
                self.logger.exception('An error occurred while initializing the plugin manager storage manager. No plugins will be loaded.')

    def load_external_modules(self):
        """
        Loads the external modules into the system.
        """
        if not self.initialized:
            self.logger.error('The plugin manager is not initialized. No plugins will be loaded.')
            return

        for module_name in self.module_configurations.keys():

            module_file_name = f'{module_name}.zip'
            local_module_file_name = f'{self.modules_local_path}/{module_file_name}'
            self.logger.info(f'Loading module from {module_file_name}')

            try:
                if (self.storage_manager.file_exists(module_file_name)):
                    self.logger.info(f'Copying module file to: {local_module_file_name}')
                    module_file_binary_content = self.storage_manager.read_file_content(module_file_name)
                    with open(local_module_file_name, 'wb') as f:
                        f.write(module_file_binary_content)

                sys.path.insert(0, local_module_file_name)
                self.module_configurations[module_name].module = import_module(module_name)

                self.logger.info(f'Module {module_name} loaded successfully.')
                self.module_configurations[module_name].module_loaded = True

                for class_name in self.module_configurations[module_name].plugin_manager_names:
                    self.module_configurations[module_name].tool_plugin_manager = getattr(self.module_configurations[module_name].module, class_name)

            except Exception as e:
                self.logger.exception(f'An error occurred while loading module: {module_name}')


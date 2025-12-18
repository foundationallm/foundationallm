from types import ModuleType
from typing import Union, List

from .workflows.workflow_plugin_manager_base import WorkflowPluginManagerBase
from .tools.tool_plugin_manager_base import ToolPluginManagerBase

class ExternalModule():
    """
    Encapsulates properties useful for configuring an external module.
        module_file: str - The name of the module file.
        module_name: str - The name of the module.
        module_loaded: bool - Indicates whether the module is loaded.
        module: ModuleType - The module object.
        plugin_manager_class_names: List[str] - The list of plugin manager class names for the module.
        plugin_manager: List[Union[ToolPluginManager, WorkflowPluginManager]] - The list of plugin managers for the module.
    """

    module_file: str
    module_name: str
    module_loaded: bool = False
    module: ModuleType = None
    plugin_manager_class_names: List[str] = None
    plugin_managers: List[Union[ToolPluginManagerBase, WorkflowPluginManagerBase]] = None

    def __init__(self, module_file: str, module_name: str, plugin_manager_class_names: List[str]):
        """
        Initializes the external module.

        Parameters
        ----------
        module_file : str
            The name of the module file.
        module_name : str
            The name of the module.
        plugin_manager_class_name : List[Union[ToolPluginManager, WorkflowPluginManager]]
            The list of plugin managers for the module.
        """
        self.module_file = module_file
        self.module_name = module_name
        self.plugin_manager_class_names = plugin_manager_class_names
        self.plugin_managers = []

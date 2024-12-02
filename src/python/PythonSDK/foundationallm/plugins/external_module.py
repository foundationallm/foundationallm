from types import ModuleType

from .tools.tool_plugin_manager_base import ToolPluginManagerBase

class ExternalModule():
    """
    Encapsulates properties useful for configuring an external module.
        module_file: str - The name of the module file.
        module_name: str - The name of the module.
        module_loaded: bool - Indicates whether the module is loaded.
        module: ModuleType - The module object.
        tool_plugin_manager_class_name: str - The name of the tool plugin manager class for the module.
        tool_plugin_manager: ToolPluginManager - The tool plugin manager for the module.
    """

    module_file: str
    module_name: str
    module_loaded: bool = False
    module: ModuleType = None
    tool_plugin_manager_class_name: str = None
    tool_plugin_manager: ToolPluginManagerBase = None

    def __init__(self, module_file: str, module_name: str, tool_plugin_manager_class_name: str):
        """
        Initializes the external module.

        Parameters
        ----------
        module_file : str
            The name of the module file.
        module_name : str
            The name of the module.
        tool_plugin_manager_class_name : str
            The name of the tool plugin manager class for the module.
        """
        self.module_file = module_file
        self.module_name = module_name
        self.tool_plugin_manager_class_name = tool_plugin_manager_class_name

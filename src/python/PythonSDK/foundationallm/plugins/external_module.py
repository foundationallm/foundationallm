from pydantic import BaseModel
from typing import List, ModuleType

from .tools.tool_plugin_manager import ToolPluginManager

class ExternalModule(BaseModel):
    """
    Encapsulates properties useful for configuring an external module.
        module_name: str - The name of the module.
        module_loaded: bool - Indicates whether the module is loaded.
        module: ModuleType - The module object.
        tool_plugin_manager_class_name: str - The name of the tool plugin manager class for the module.
        tool_plugin_manager: ToolPluginManager - The tool plugin manager for the module.
    """

    module_name: str
    module_loaded: bool = False
    module: ModuleType = None
    tool_plugin_manager_class_name: str = None
    tool_plugin_manager: ToolPluginManager = None
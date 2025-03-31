from enum import Enum

class ContentArtifactTypeNames(str, Enum):
    """The names of the content artifact types."""
    TOOL_EXECUTION = 'ToolExecution'
    TOOL_ERROR = 'ToolError'
    WORKFLOW_EXECUTION = 'WorkflowExecution'
    FILE = 'File'

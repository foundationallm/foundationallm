from enum import Enum

class OperationStatus(str, Enum):
    """Enumerator of valid Operation Status values."""
    PENDING = "Pending"
    INPROGRESS = "InProgress"
    COMPLETED = "Completed"
    FAILED = "Failed"

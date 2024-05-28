from enum import Enum

class OperationTypes(str, Enum):
   """Enumerator of the Operation Types."""
   COMPLETIONS = "completions"
   CHAT = "chat"

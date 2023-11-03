from typing import List
from foundationallm.models.orchestration import MessageHistoryItem

def build_message_history(messages:List[MessageHistoryItem]=None) -> str:
    """
    Builds a chat history string from a list of MessageHistoryItem objects to
    be added to the prompt for the completion request.
    """
    if messages is None or len(messages)==0:
        return ""        
    chat_history = "\n\nChat History:\n"
    for msg in messages:        
        chat_history += msg.sender + ": " + msg.text + "\n"        
    chat_history += "\n\n"
    return chat_history
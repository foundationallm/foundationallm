from enum import Enum

class MessageContentItemTypes(str, Enum):
   """Enumerator Message Content Item Types"""
   TEXT = "text"
   IMAGE_FILE = "image_file"
   FILE_PATH = "file_path"

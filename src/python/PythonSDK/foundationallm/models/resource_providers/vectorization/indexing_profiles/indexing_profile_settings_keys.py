from enum import Enum

class IndexingProfileSettingsKeys(str, Enum):
   """Enumerator of the Indexing Profile Settings Keys."""
   INDEX_NAME = "IndexName"
   TOP_N = "TopN"
   FILTERS = "Filters"
   EMBEDDING_FIELD_NAME = "EmbeddingFieldName"
   TEXT_FIELD_NAME = "TextFieldName"
   API_ENDPOINT_CONFIGURATION_OBJECT_ID = "api_endpoint_configuration_object_id"

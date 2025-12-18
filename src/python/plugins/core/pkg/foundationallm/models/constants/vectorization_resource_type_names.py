from enum import Enum

class VectorizationResourceTypeNames(str, Enum):
    """The names of the resource types managed by the FoundationaLLM.Vectorization resource provider."""
    EMBEDDING_PROFILE = 'textEmbeddingProfiles'
    INDEXING_PROFILE = 'indexingProfiles'

from typing import List
from abc import ABC, abstractmethod
from foundationallm.models.orchestration import ContentArtifact

class ContentArtifactRetrievalBase(ABC):
    """
    Abstract base class indicating the ability for a retriever to retrieve sources.
    """
    @abstractmethod
    def get_document_content_artifacts(self) -> List[ContentArtifact]:
        """
        Gets content artifacts (sources) from documents retrieved from the retriever.
        
        Returns:
            List of content artifacts (sources) from the retrieved documents.
        """
        raise NotImplementedError()

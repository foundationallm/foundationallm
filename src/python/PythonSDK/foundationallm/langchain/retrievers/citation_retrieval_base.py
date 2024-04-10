from typing import List
from abc import ABC, abstractmethod
from foundationallm.models.orchestration import Citation

class CitationRetrievalBase(ABC):
    """
    Abstract base class indicating the ability for a retriever to retrieve citations.
    """
    @abstractmethod
    def get_document_citations(self) -> List[Citation]:
        """
        Gets sources from documents retrieved from the retriever.
        
        Returns:
            List of citations from the retrieved documents.
        """

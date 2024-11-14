"""
The Analyzer is responsible for analyzing textual content and returning
the PII entities found in the text, optionally anonymizing the text.
"""
import re
from typing import List
from presidio_analyzer import AnalyzerEngine, RecognizerResult
from presidio_anonymizer import AnonymizerEngine
from presidio_anonymizer.entities.engine.result import EngineResult
from foundationallm.integration.models import (
        AnalyzeRequest, AnalyzeResponse,
        PIIResult, PIIResultAnonymized
    )

# Analyzer only has one method by design.
# pylint: disable=too-few-public-methods
class Analyzer:
    """
    The Analyzer is responsible for analyzing textual content and returning
    the PII entities found in the text, optionally anonymizing the text.
    """
    def __init__(self, request: AnalyzeRequest):
        """
        Initializes the analyzer and anonymizer engines and sets the request

        Parameters:
            request (AnalyzeRequest): The request to analyze        
        """
        self.request = request
        self.analyzer = AnalyzerEngine()
        self.anonymizer = AnonymizerEngine()
        self.entities = [entity for recognizer in self.analyzer.get_recognizers()
                                for entity in recognizer.supported_entities]

    def analyze(self) -> AnalyzeResponse:
        """
        Analyzes the text and returns the PII entities found in the text.
        If the request specifies anonymization, the text is anonymized
        """
        analyze_results = self.__analyze()
        if self.request.anonymize:
            # anonymize the text results based on the analyzer results
            anonymized_results = self.__anonymize(analyze_results)
            results = [PIIResultAnonymized(entity_type=p.entity_type, start_index=p.start,
                                           end_index=p.end, anonymized_text=self.__token_replacement(p.text),
                                           operator=p.operator)
                       for p in anonymized_results.items]
            return AnalyzeResponse(content=self.__token_replacement(anonymized_results.text), results=results)

        # pii_result response
        results = [PIIResult(entity_type=p.entity_type, start_index=p.start,
                        end_index=p.end) for p in analyze_results]
        return AnalyzeResponse(content=self.request.content, results=results)

    def __analyze(self) -> List[RecognizerResult]:
        """
        Uses the Presidio Analyzer to analyze the content and return the PII entities
        found in the text
        """
        return self.analyzer.analyze(text=self.request.content, language=self.request.language)

    def __anonymize(self, results: List[RecognizerResult]) -> EngineResult:
        """
        Uses the Presidio Anonymizer to anonymize the content based on the PII entities found
        in the text
        """
        return self.anonymizer.anonymize(text=self.request.content, analyzer_results=results)

    def __token_replacement(self, text:str) -> str:
        """
        Replaces the Presidio tokens with square bracket syntax
        """
        pattern = re.compile(r"<(" + "|".join(map(re.escape, self.entities)) + r")>")
        return pattern.sub(r"[\1]", text)

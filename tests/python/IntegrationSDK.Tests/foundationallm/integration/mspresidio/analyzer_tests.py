import pytest
from foundationallm.integration.models import AnalyzeRequest
from foundationallm.integration.mspresidio.analyzer import Analyzer

class AnalyzerTests:
    """
    AnalyzerTests is responsible for testing the analyzer functionality
    that identifies PII in textual content.
    """
    def test_analyzer_identifies_phone_entity(self):        
        request = AnalyzeRequest(content="My cell is (555)555-5555", 
                                 anonymize=False, language="en")
        sut = Analyzer(request)
        response = sut.analyze()
        print(response)
        assert any(obj.entity_type == "PHONE_NUMBER" for obj in response.results)
        
    def test_analyzer_identifies_person_entity(self):        
        request = AnalyzeRequest(content="My name is Inigo Montoya, I want to use your LLM.", 
                                 anonymize=False, language="en")
        sut = Analyzer(request)
        response = sut.analyze()
        print(response)
        assert any(obj.entity_type == "PERSON" for obj in response.results)
        
    def test_analyzer_identifies_multiple_entities(self):
        request = AnalyzeRequest(content="""My name is Inigo Montoya and I want to use your LLM. 
                                 My cell is (555)555-5555, prepare to be amazed!""", 
                                 anonymize=False, language="en")
        sut = Analyzer(request)
        response = sut.analyze()
        print(response)
        assert len(response.results) > 1
        
    def test_analyzer_anonymizes_entity(self):
        request = AnalyzeRequest(content="My cell is (555)555-5555", 
                                 anonymize=True, language="en")
        sut = Analyzer(request)
        response = sut.analyze()
        print(response)
        assert any(obj.entity_type == "PHONE_NUMBER" for obj in response.results) and \
               "<PHONE_NUMBER>" in response.content
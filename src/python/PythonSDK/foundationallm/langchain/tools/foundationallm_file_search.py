from .fllm_tool_base import FLLMToolBase
from foundationallm.config import Configuration
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.models.agents import AgentTool

class FoundationaLLMFileSearchTool(FLLMToolBase):
    """
    FoundationaLLM file search tool.    
    """
    #args_schema: Type[BaseModel] = FoundationaLLMFileSearchToolInput
        
    def __init__(self, tool_config: AgentTool, objects: dict, config: Configuration):
        """ Initializes the FoundationaLLMFileSearchTool class with the tool configuration,
            exploded objects collection, and platform configuration. """
        super().__init__(tool_config, objects, config)      
        self.client = self._get_client()

    def _get_client(self):
        """ Creates a client for the FoundationaLLM file search tool. """
        language_model_factory = LanguageModelFactory(self.objects, self.config)
        self.client = language_model_factory._get_language_model(self.tool_config.ai_model_object_ids["main_model"])
        


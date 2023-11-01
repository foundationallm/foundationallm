from typing import List
from foundationallm.langchain.agents.agent_base import AgentBase
from foundationallm.models.orchestration import OrchestrationResponse
from foundationallm.config import Configuration
from .resolver_configuration_repository import ResolverConfigurationRepository
from .generic_resolver_agent_output_parser import GenericResolverAgentOutputParser
from foundationallm.langchain.message_history import build_message_history
from langchain.callbacks import get_openai_callback
from foundationallm.models.orchestration import CompletionResponse, MessageHistoryItem
from foundationallm.models import ListOption

class GenericResolverAgent(AgentBase):
    """
    The GenericResolverAgent is responsible for choosing one or more
        options from a list of options consisting of a name and description.
        This agent determines the best matches based on the incoming user_prompt and message history.
        The user prompt may request one or more options from the list.
    """
    def __init__(self,user_prompt:str=None, message_history: List[MessageHistoryItem]=None, options_list:List[ListOption]=None,  config: Configuration = None):
        self.user_prompt = user_prompt
        resolver_repo = ResolverConfigurationRepository(config=config)
        self.llm = resolver_repo.get_generic_resolver_llm_details()
        # prompt template expects options list, history and user_prompt as inputs
        prompt_template = resolver_repo.get_generic_resolver_prompt()
        options = self.build_options_list(options_list = options_list)
        history = build_message_history(message_history)
        if len(history) == 0:
            history = "Message History:\n\nNo message history available."
        self.formatted_prompt = prompt_template.format(options=options, history=history, user_prompt=user_prompt)
    
    def build_options_list(self, options_list:List[ListOption]=None) -> str:
        """
        Builds a list of options using their name and descriptions for the resolver prompt.
        """
        if options_list is None or len(options_list)==0:
            return ""        
        options_str = "\n\nOptions List:\n"
        for option in options_list:        
            options_str +=  "Name: " + option.name + "\n"
            options_str +=  "Description: " + option.description + "\n\n"        
        return options_str
 
    def run(self, prompt: str) -> OrchestrationResponse:
        """
        Evaluates a list of options against the incoming user prompt.

        Parameters
        ----------
        prompt : str
            The prompt that contains option information, message history, and user prompt.
        
        Returns
        -------
        CompletionResponse
            Returns a CompletionResponse with the name(s) of the selected option(s), the user_prompt,
            and token utilization and execution cost details.
        """
        try:           
            with get_openai_callback() as cb:                
                full_completion = self.llm(prompt=self.formatted_prompt)                
                parser = GenericResolverAgentOutputParser()
                completion = parser.parse(full_completion)               
                return CompletionResponse(
                    completion = completion,
                    user_prompt = prompt,
                    completion_tokens = cb.completion_tokens,
                    prompt_tokens = cb.prompt_tokens,
                    total_tokens = cb.total_tokens,
                    total_cost = cb.total_cost
                )
        except Exception as e:            
            return CompletionResponse(
                    completion = "A problem on my side prevented me from responding.",
                    user_prompt = prompt
                ) 

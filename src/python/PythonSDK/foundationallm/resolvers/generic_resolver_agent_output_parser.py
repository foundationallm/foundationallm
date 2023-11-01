import re
from typing import List
from langchain.agents.agent import AgentOutputParser

class GenericResolverAgentOutputParser(AgentOutputParser):
    """Output parser for the Generic Resolver Agent output."""
    
    def parse(self, text: str) -> List[str]:
        """
        The first line of the response is the list of names. 
        Example: "['Inaccuracy', 'Speed', 'Scalable']"
        The prompt already includes the Final Answer:label.       
        """        
        final_answer = text.split("\n")[0].strip()
        match = re.search(r'\[(.*?)\]', final_answer)  
        if match:  
            result_list = [s.strip() for s in match.group(1).split(',')]
            result_list = [s.strip("'") for s in result_list]  # remove outer quotes  
            return result_list  
        else:  
            return []  
          

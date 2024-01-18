from io import StringIO
from operator import itemgetter
import pandas as pd
from langchain.agents import AgentExecutor, ZeroShotAgent
from langchain.chains import LLMChain
from langchain.memory import ConversationBufferMemory
from langchain.tools.python.tool import PythonAstREPLTool
from langchain.callbacks import get_openai_callback
from foundationallm.config import Configuration
from foundationallm.langchain.agents import AgentBase
from foundationallm.langchain.language_models import LanguageModelBase
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse
from foundationallm.storage import BlobStorageManager

class CSVAgent(AgentBase):
    """
    Agent for analyzing the contents of delimited files (e.g., CSV).
    """

    def __init__(self, completion_request: CompletionRequest,
                 llm: LanguageModelBase, config: Configuration):
        """
        Initializes a CSV agent.
        
        Note: The CSV agent supports a single file.

        Parameters
        ----------
        completion_request : CompletionRequest
            The completion request object containing the user prompt to execute, message history,
            and agent and data source metadata.
        llm : LanguageModelBase
            The language model to use for executing the completion request.
        config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        self.prompt_prefix = completion_request.agent.prompt_prefix
        self.prompt_suffix = completion_request.agent.prompt_suffix
        self.llm = llm.get_completion_model(completion_request.language_model)
        self.message_history = completion_request.message_history

        storage_manager = BlobStorageManager(
            blob_connection_string = config.get_value(
                completion_request.data_source.configuration.connection_string_secret),
            container_name = completion_request.data_source.configuration.container
        )

        file_name = completion_request.data_source.configuration.files[0]
        file_content = storage_manager.read_file_content(file_name).decode('utf-8')
        sio = StringIO(file_content)
        df = pd.read_csv(sio)
        tools = [
            PythonAstREPLTool(
                locals={"df": df},
                name=completion_request.data_source.data_description or 'CSV data',
                description=completion_request.data_source.description \
                    or 'Useful for when you need to answer questions about data in CSV files.'
            )
        ]
        memory = ConversationBufferMemory(memory_key="chat_history", return_messages=True)
        # Add previous messages to the memory
        for i in range(0, len(self.message_history), 2):
            history_pair = itemgetter(i,i+1)(self.message_history)
            for message in history_pair:
                if message.sender.lower() == 'user':
                    user_input = message.text
                else:
                    ai_output = message.text
            memory.save_context({"input": user_input}, {"output": ai_output})

        prompt = ZeroShotAgent.create_prompt(
            tools,
            prefix = self.prompt_prefix,
            suffix = self.prompt_suffix,
            input_variables = ['input', 'chat_history', 'df_head', 'agent_scratchpad']
        )
        partial_prompt = prompt.partial(
            df_head=str(df.head(3).to_markdown())
        )
        zsa = ZeroShotAgent(
            llm_chain=LLMChain(llm=self.llm, prompt=partial_prompt),
            allowed_tools=[tool.name for tool in tools]
        )
        self.agent = AgentExecutor.from_agent_and_tools(
            agent=zsa,
            tools=tools,
            verbose=True,
            memory=memory,
            handle_parsing_errors='Check your output and make sure it conforms!'
        )

    @property
    def prompt_template(self) -> str:
        """
        Property for viewing the agent's prompt template.
        
        Returns
        str
            Returns the prompt template for the agent.
        """
        return self.agent.agent.llm_chain.prompt.template

    def run(self, prompt: str) -> CompletionResponse:
        """
        Executes a query against the contents of a CSV file.
        
        Parameters
        ----------
        prompt : str
            The prompt for which a completion is begin generated.
        
        Returns
        -------
        CompletionResponse
            Returns a CompletionResponse with the CSV file query completion response, 
            the user_prompt, and token utilization and execution cost details.
        """
        with get_openai_callback() as cb:
            return CompletionResponse(
                completion = self.agent.run(prompt),
                user_prompt = prompt,
                completion_tokens = cb.completion_tokens,
                prompt_tokens = cb.prompt_tokens,
                total_tokens = cb.total_tokens,
                total_cost = cb.total_cost
            )

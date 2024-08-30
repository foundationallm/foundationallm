from openai import AssistantEventHandler
from openai.types.beta.threads.runs import RunStep
from typing_extensions import override
from foundationallm.models.orchestration.completion_response import CompletionResponse
from foundationallm.operations import OperationsManager
from foundationallm.models.services import OpenAIAssistantsAPIRequest
from foundationallm.utils import OpenAIAssistantsHelpers

class OpenAIAssistantEventHandler(AssistantEventHandler):

    def __init__(self, operations_manager: OperationsManager, request: OpenAIAssistantsAPIRequest):
        super().__init__()
        self.operations_manager = operations_manager
        self.request = request
        self.analysis_results = []

    @override
    def on_run_step_done(self, run_step: RunStep) -> None:
        details = run_step.step_details
        if details.type == "tool_calls":
            for tool in details.tool_calls:
                if tool.type == "code_interpreter":
                    step_result = OpenAIAssistantsHelpers.parse_run_step(run_step)
                    self.analysis_results.append(step_result)
                    interim_result = CompletionResponse(
                        operation_id = self.request.operation_id,
                        user_prompt = self.request.user_prompt,
                        content = [],
                        analysis_results = self.analysis_results
                    )
                    self.operations_manager.set_operation_result(self.request.operation_id, self.request.instance_id, interim_result)

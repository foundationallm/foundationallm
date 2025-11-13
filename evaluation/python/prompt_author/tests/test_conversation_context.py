import sys
from pathlib import Path

PROJECT_ROOT = Path(__file__).resolve().parents[2]
if str(PROJECT_ROOT) not in sys.path:
    sys.path.insert(0, str(PROJECT_ROOT))

from prompt_author.conversation_context import (  # noqa: E402
    build_prompt_messages,
    format_prompt_messages,
    persist_prompt_messages,
)
from prompt_author.prompt_catalog import PromptReference  # noqa: E402


class FakeManagementClient:
    def __init__(self, payloads):
        self.payloads = payloads

    def get_prompt(self, prompt_name: str):
        return self.payloads[prompt_name]


def test_build_prompt_messages_marks_targets(tmp_path: Path):
    prompt_refs = [
        PromptReference(
            prompt_name="MainPrompt",
            object_id="prompt/main",
            contexts=["workflow.resource_object_ids.0"],
            role="main_prompt",
        ),
        PromptReference(
            prompt_name="ToolPrompt",
            object_id="prompt/tool",
            contexts=["tools.0.resource_object_ids.0"],
            role="tool_prompt",
        ),
    ]
    client = FakeManagementClient(
        {
            "MainPrompt": {"type": "multipart", "prefix": "System instructions"},
            "ToolPrompt": {"type": "simple", "content": "Tool usage guidance"},
        }
    )

    messages = build_prompt_messages(
        management_client=client,
        prompt_refs=prompt_refs,
        selected_prompt_names=["MainPrompt"],
    )

    assert len(messages) == 2
    main_message = next(msg for msg in messages if msg.name == "MainPrompt")
    tool_message = next(msg for msg in messages if msg.name == "ToolPrompt")
    assert main_message.is_target is True
    assert tool_message.is_target is False
    assert main_message.role == "system"
    assert "System instructions" in main_message.content

    report = format_prompt_messages(messages)
    assert "MainPrompt" in report
    assert "REFERENCE" in report

    output_file = tmp_path / "context.txt"
    persist_prompt_messages(output_path=output_file, messages=messages)
    assert output_file.exists()
    saved_content = output_file.read_text(encoding="utf-8")
    assert "Tool usage guidance" in saved_content

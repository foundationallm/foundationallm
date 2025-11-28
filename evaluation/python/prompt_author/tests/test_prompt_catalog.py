import sys
from pathlib import Path

PROJECT_ROOT = Path(__file__).resolve().parents[2]
if str(PROJECT_ROOT) not in sys.path:
    sys.path.insert(0, str(PROJECT_ROOT))

from prompt_author.prompt_catalog import (  # noqa: E402
    extract_prompt_references,
    PromptReference,
    select_prompts,
)


def _sample_agent_resource():
    return {
        "name": "SampleAgent",
        "prompt_object_id": "/instances/123/providers/FoundationaLLM.Prompt/prompts/MainPrompt",
        "workflow": {
            "resource_object_ids": {
                "/instances/123/providers/FoundationaLLM.Prompt/prompts/MainPrompt": {
                    "object_id": "/instances/123/providers/FoundationaLLM.Prompt/prompts/MainPrompt",
                    "properties": {"object_role": "main_prompt"},
                },
                "/instances/123/providers/FoundationaLLM.Prompt/prompts/SecondaryPrompt": {
                    "object_id": "/instances/123/providers/FoundationaLLM.Prompt/prompts/SecondaryPrompt",
                    "properties": {"object_role": "fallback_prompt"},
                },
            }
        },
        "tools": [
            {
                "name": "SomeTool",
                "resource_object_ids": {
                    "/instances/123/providers/FoundationaLLM.Prompt/prompts/ToolPrompt": {
                        "object_id": "/instances/123/providers/FoundationaLLM.Prompt/prompts/ToolPrompt",
                        "properties": {"object_role": "tool_prompt"},
                    }
                },
            }
        ],
    }


def test_extract_prompt_references_detects_unique_prompts():
    prompts = extract_prompt_references(_sample_agent_resource())

    names = {prompt.prompt_name for prompt in prompts}
    assert names == {"MainPrompt", "SecondaryPrompt", "ToolPrompt"}

    main_prompt = next(prompt for prompt in prompts if prompt.prompt_name == "MainPrompt")
    assert "main_prompt" in (main_prompt.role or "")
    assert "prompt_object_id" in main_prompt.contexts


def test_select_prompts_filters_requested_subset():
    prompts = [
        PromptReference(prompt_name="MainPrompt", object_id="id1"),
        PromptReference(prompt_name="SecondaryPrompt", object_id="id2"),
    ]

    selected = select_prompts(prompts, "MainPrompt")
    assert len(selected) == 1
    assert selected[0].prompt_name == "MainPrompt"


def test_select_prompts_validates_unknown_names():
    prompts = [
        PromptReference(prompt_name="MainPrompt", object_id="id1"),
    ]

    try:
        select_prompts(prompts, "UnknownPrompt")
    except ValueError as exc:
        assert "Unknown prompt" in str(exc)
    else:
        raise AssertionError("Expected ValueError for unknown prompt name")

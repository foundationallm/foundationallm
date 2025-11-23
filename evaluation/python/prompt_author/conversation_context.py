"""
Aggregates agent prompt resources into an ordered message view.
"""

from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path
from typing import Dict, Iterable, List, Optional, Sequence, Tuple

from .foundationallm_client import FoundationaLLMManagementClient
from .llm_refiner import render_prompt_body
from .prompt_catalog import PromptReference


@dataclass
class PromptMessage:
    """Represents a prompt-related message contributing to completions."""

    name: str
    role: str
    content: str
    contexts: List[str]
    is_target: bool
    metadata: Dict[str, str]


def _determine_role(prompt_ref: PromptReference) -> str:
    """
    Infer a friendly role label for the prompt reference.
    """
    if prompt_ref.role:
        role_lower = prompt_ref.role.lower()
        if "system" in role_lower or "main_prompt" in role_lower:
            return "system"
        if "user" in role_lower:
            return "user"
        if "tool" in role_lower:
            return "tool"
    for context in prompt_ref.contexts:
        context_lower = context.lower()
        if "workflow" in context_lower or "system" in context_lower:
            return "system"
        if "tool" in context_lower:
            return "tool"
    return "other"


def build_prompt_messages(
    *,
    management_client: FoundationaLLMManagementClient,
    prompt_refs: Sequence[PromptReference],
    selected_prompt_names: Iterable[str],
) -> List[PromptMessage]:
    """
    Hydrate prompt payloads and organise them into an ordered message list.
    """
    selected_set = {name for name in selected_prompt_names}
    messages: List[PromptMessage] = []

    for prompt_ref in prompt_refs:
        prompt_payload = management_client.get_prompt(prompt_ref.prompt_name)
        content = render_prompt_body(prompt_payload)
        role = _determine_role(prompt_ref)
        metadata = {
            "object_id": prompt_ref.object_id,
            "raw_role": prompt_ref.role or "",
        }
        messages.append(
            PromptMessage(
                name=prompt_ref.prompt_name,
                role=role,
                content=content,
                contexts=sorted(prompt_ref.contexts),
                is_target=prompt_ref.prompt_name in selected_set,
                metadata=metadata,
            )
        )

    # Custom ordering to match agent execution sequence
    def get_sort_key(msg: PromptMessage) -> int:
        role = msg.role
        name_lower = msg.name.lower()
        # Desired order: workflow-main, workflow-router, workflow-files, tool-router, tool-main, workflow-final, others
        if role == "system" and "main" in name_lower:
            return 0  # workflow-main
        elif role == "system" and "router" in name_lower:
            return 1  # workflow-router
        elif role == "system" and "files" in name_lower:
            return 2  # workflow-files
        elif role == "tool" and "router" in name_lower:
            return 3  # tool-router
        elif role == "tool" and "main" in name_lower:
            return 4  # tool-main
        elif role == "system" and "final" in name_lower:
            return 5  # workflow-final
        else:
            return 6  # others

    messages.sort(key=lambda msg: (get_sort_key(msg), msg.name.lower()))
    return messages


def format_prompt_messages(messages: Sequence[PromptMessage]) -> str:
    """
    Produce a human-readable report that mirrors conversation transcript style.
    """
    lines: List[str] = []
    lines.append("=" * 80)
    lines.append("=== AGGREGATED PROMPT CONTEXT ===")
    lines.append("=" * 80)
    for idx, message in enumerate(messages, start=1):
        status = "TARGET" if message.is_target else "REFERENCE"
        lines.append("")
        lines.append(f"Message {idx} ({status}, role={message.role}) - {message.name}")
        lines.append("-" * 80)
        lines.append(message.content.strip())
        lines.append("")
        lines.append("Contexts: " + ", ".join(message.contexts) if message.contexts else "Contexts: (none)")
        if message.metadata.get("raw_role"):
            lines.append(f"Raw Role Metadata: {message.metadata['raw_role']}")
        lines.append(f"Object ID: {message.metadata.get('object_id', '')}")
    lines.append("")
    lines.append("=" * 80)
    return "\n".join(lines)


def persist_prompt_messages(
    *,
    output_path: Path,
    messages: Sequence[PromptMessage],
) -> None:
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text(format_prompt_messages(messages), encoding="utf-8")

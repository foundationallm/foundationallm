"""
Utilities for discovering and representing prompt references within an agent definition.
"""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import Any, Dict, Iterable, List, Optional

PROMPT_URI_FRAGMENT = "/providers/FoundationaLLM.Prompt/prompts/"


@dataclass
class PromptReference:
    """Represents a prompt referenced by an agent."""

    prompt_name: str
    object_id: str
    contexts: List[str] = field(default_factory=list)
    role: Optional[str] = None
    properties: Dict[str, Any] = field(default_factory=dict)

    def add_context(self, context: str, *, role: Optional[str], properties: Optional[Dict[str, Any]]) -> None:
        if context not in self.contexts:
            self.contexts.append(context)

        if role and not self.role:
            self.role = role

        if properties:
            for key, value in properties.items():
                if key not in self.properties:
                    self.properties[key] = value


def _is_prompt_value(value: Any) -> bool:
    return isinstance(value, str) and PROMPT_URI_FRAGMENT in value


def _extract_prompt_name(object_id: str) -> str:
    return object_id.split("/")[-1]


def _context_path(parts: Iterable[str]) -> str:
    return ".".join(part for part in parts if part != "")


def extract_prompt_references(agent_resource: Dict[str, Any]) -> List[PromptReference]:
    """
    Traverse an agent resource payload and identify all prompt references.
    """

    references: Dict[str, PromptReference] = {}

    def visit(node: Any, path: List[str]) -> None:
        if isinstance(node, dict):
            for key, value in node.items():
                next_path = path + [key]

                if _is_prompt_value(value):
                    prompt_name = _extract_prompt_name(value)
                    roles = None
                    properties = None

                    if isinstance(node.get("properties"), dict):
                        properties = node["properties"]
                        roles = properties.get("object_role")
                    elif isinstance(node.get("object_role"), str):
                        roles = node.get("object_role")

                    ref = references.setdefault(
                        value,
                        PromptReference(
                            prompt_name=prompt_name,
                            object_id=value,
                        ),
                    )
                    ref.add_context(_context_path(next_path), role=roles, properties=properties)
                else:
                    visit(value, next_path)
        elif isinstance(node, list):
            for index, item in enumerate(node):
                visit(item, path + [str(index)])
        elif _is_prompt_value(node):
            prompt_name = _extract_prompt_name(str(node))
            ref = references.setdefault(
                str(node),
                PromptReference(prompt_name=prompt_name, object_id=str(node)),
            )
            ref.add_context(_context_path(path), role=None, properties=None)

    visit(agent_resource, [])

    # Include the explicit prompt_object_id if provided (common for simple agents)
    prompt_object_id = agent_resource.get("prompt_object_id")
    if _is_prompt_value(prompt_object_id):
        prompt_name = _extract_prompt_name(prompt_object_id)
        ref = references.setdefault(
            prompt_object_id,
            PromptReference(prompt_name=prompt_name, object_id=prompt_object_id),
        )
        ref.add_context("prompt_object_id", role="main_prompt", properties=None)

    # Return sorted references by prompt name for stable CLI output
    return sorted(references.values(), key=lambda ref: ref.prompt_name.lower())

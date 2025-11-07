"""
LLM-powered prompt refinement helpers.
"""

from __future__ import annotations

import json
import textwrap
from dataclasses import dataclass, field
from typing import Any, Dict, Optional

from .foundationallm_client import FoundationaLLMCompletionClient, FoundationaLLMClientError


@dataclass
class PromptImprovementResult:
    revised_body: str
    reasoning: str
    confidence: Optional[float] = None
    additional_notes: Dict[str, Any] = field(default_factory=dict)
    raw_response: Dict[str, Any] = field(default_factory=dict)


def _extract_text_from_completion(response: Dict[str, Any]) -> str:
    content_items = response.get("content") or response.get("choices") or []
    if isinstance(content_items, list):
        parts = []
        for item in content_items:
            if isinstance(item, dict):
                value = item.get("value") or item.get("text") or ""
                if value:
                    parts.append(str(value))
        return "\n".join(parts).strip()
    if isinstance(content_items, str):
        return content_items
    return ""


def _ensure_json_payload(text: str) -> Dict[str, Any]:
    """
    Attempt to parse the LLM output into a JSON payload.
    Handles fenced code blocks and extra commentary gracefully.
    """
    stripped = text.strip()

    if stripped.startswith("```"):
        # Remove optional language hint
        stripped = stripped.strip("`")
        first_newline = stripped.find("\n")
        if first_newline != -1 and not stripped[:first_newline].startswith("{"):
            stripped = stripped[first_newline + 1 :]

    # Locate JSON braces if extra commentary exists
    start = stripped.find("{")
    end = stripped.rfind("}")
    if start != -1 and end != -1:
        candidate = stripped[start : end + 1]
    else:
        candidate = stripped

    return json.loads(candidate)


def render_prompt_body(prompt_payload: Dict[str, Any]) -> str:
    """
    Extract the editable body of a prompt regardless of shape.
    """
    prompt_type = (prompt_payload or {}).get("type", "").lower()

    if prompt_type == "multipart":
        return prompt_payload.get("prefix", "") or ""
    if prompt_type == "simple":
        return prompt_payload.get("content") or prompt_payload.get("text") or ""
    if "content" in prompt_payload:
        return prompt_payload["content"]
    if "text" in prompt_payload:
        return prompt_payload["text"]
    if "template" in prompt_payload:
        return prompt_payload["template"]
    if "body" in prompt_payload:
        return prompt_payload["body"]

    # Fallback: serialise entire object for visibility
    return json.dumps(prompt_payload, ensure_ascii=False, indent=2)


def apply_prompt_body(prompt_payload: Dict[str, Any], new_body: str) -> Dict[str, Any]:
    """
    Replace the editable portion of the prompt payload with the supplied body.
    """
    prompt_type = (prompt_payload or {}).get("type", "").lower()
    updated = dict(prompt_payload)

    if prompt_type == "multipart":
        updated["prefix"] = new_body
        return updated
    if prompt_type == "simple":
        if "content" in updated:
            updated["content"] = new_body
        elif "text" in updated:
            updated["text"] = new_body
        else:
            updated["content"] = new_body
        return updated

    for candidate_field in ("content", "text", "template", "body", "prefix"):
        if candidate_field in updated:
            updated[candidate_field] = new_body
            return updated

    # If no known field exists, add a generic 'content'
    updated["content"] = new_body
    return updated


class PromptRefiner:
    """
    Wraps LLM interactions required to iteratively improve prompt bodies.
    """

    def __init__(self, completion_client: FoundationaLLMCompletionClient) -> None:
        self.completion_client = completion_client

    def improve_prompt(
        self,
        *,
        agent_name: str,
        prompt_name: str,
        current_body: str,
        optimization_brief: str,
        context_summary: str,
        previous_evaluation_feedback: Optional[str],
        iteration_index: int,
    ) -> PromptImprovementResult:
        """
        Request an improved prompt body from the LLM, capturing reasoning.
        """
        llm_prompt = textwrap.dedent(
            f"""
            You are an expert AI prompt engineer improving prompts used by the agent "{agent_name}".
            You will receive the current prompt body along with a design brief describing
            the problem and the definition of success. You must produce a JSON object
            with the following keys:
              - reasoning: step-by-step explanation of the changes and why they solve the brief.
              - revised_prompt: the updated prompt body that should replace the existing one.
              - confidence: number between 0 and 1 representing your confidence in the revision.
              - suggested_evaluation: optional guidance to refine further testing.
            If the prompt already satisfies the brief, explain why and return the existing prompt.
            Respond ONLY with JSON matching this schema.

            Prompt Name: {prompt_name}
            Prompt Context: {context_summary or "n/a"}
            Optimization Brief:
            {optimization_brief.strip()}

            Current Prompt Body:
            ---
            {current_body.strip()}
            ---

            Previous Evaluation Feedback:
            {previous_evaluation_feedback or "None"}

            Iteration: {iteration_index}
            """
        ).strip()

        try:
            completion_response = self.completion_client.complete(
                prompt=llm_prompt,
            )
        except FoundationaLLMClientError as exc:
            raise

        raw_text = _extract_text_from_completion(completion_response)
        if not raw_text:
            raise FoundationaLLMClientError(
                "Prompt refinement LLM response did not include textual content."
            )

        payload = _ensure_json_payload(raw_text)

        revised_prompt = payload.get("revised_prompt")
        reasoning = payload.get("reasoning", "")
        confidence = payload.get("confidence")
        additional_notes = {
            "suggested_evaluation": payload.get("suggested_evaluation"),
            "raw_json": payload,
        }

        if not isinstance(revised_prompt, str) or not revised_prompt.strip():
            raise FoundationaLLMClientError(
                "LLM did not provide a valid 'revised_prompt' field in the JSON response."
            )

        return PromptImprovementResult(
            revised_body=revised_prompt.strip(),
            reasoning=reasoning.strip(),
            confidence=float(confidence) if isinstance(confidence, (int, float, str)) and str(confidence).strip() else None,
            additional_notes=additional_notes,
            raw_response=completion_response,
        )

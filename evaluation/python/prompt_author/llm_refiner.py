"""
LLM-powered prompt refinement helpers.
"""

from __future__ import annotations

import json
import textwrap
from dataclasses import dataclass, field
from typing import Any, Dict, Optional

from .foundationallm_client import AzureOpenAILLMClient, FoundationaLLMClientError


@dataclass
class PromptImprovementResult:
    revised_body: str
    reasoning: str
    confidence: Optional[float] = None
    additional_notes: Dict[str, Any] = field(default_factory=dict)
    raw_response: Optional[str] = None


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

    def __init__(self, completion_client: AzureOpenAILLMClient) -> None:
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
        system_message = textwrap.dedent(
            """
            You are an expert AI prompt engineer. Your job is to rewrite prompts so that
            they produce reliable, high-quality responses that align with the given goals.
            You must respond ONLY with a valid JSON object. Do not include any text before or after the JSON.
            The JSON must contain exactly these keys:
              - reasoning: A concise, step-by-step explanation of the changes.
              - revised_prompt: The updated prompt text after applying improvements.
              - confidence: A number between 0 and 1 estimating how confident you are that the revision meets the goal.
              - suggested_evaluation: Optional guidance for further testing (can be null or empty string).
            If no changes are required, return the original prompt in revised_prompt and explain why in reasoning.
            """
        ).strip()

        user_message = textwrap.dedent(
            f"""
            Agent Name: {agent_name}
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
            raw_text = self.completion_client.chat_completion(
                messages=[
                    {"role": "system", "content": system_message},
                    {"role": "user", "content": user_message},
                ],
                temperature=0.2,
                max_tokens=1800,
            )
        except FoundationaLLMClientError as exc:
            raise

        if not raw_text:
            raise FoundationaLLMClientError("Prompt refinement LLM returned an empty response.")

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
            raw_response=raw_text,
        )

"""
Core optimisation loop orchestrator.
"""

from __future__ import annotations

from dataclasses import dataclass, field
from pathlib import Path
from typing import Iterable, List, Optional

from .evaluator import EvaluationResult, PromptEvaluator
from .foundationallm_client import FoundationaLLMManagementClient
from .llm_refiner import PromptRefiner, apply_prompt_body, render_prompt_body
from .prompt_catalog import PromptReference
from .storage import OptimizationRunLogger


@dataclass
class OptimizationConfig:
    max_iterations: int = 5
    target_pass_rate: float = 0.9
    suite_name: Optional[str] = None
    quick_mode: bool = False
    test_index: Optional[int] = None
    repeat_test: int = 1
    max_workers: int = 5
    verbose: bool = False
    dry_run: bool = False


@dataclass
class PromptOptimizationOutcome:
    prompt_name: str
    success: bool
    iterations: int
    final_pass_rate: Optional[float]
    final_evaluation_summary: Optional[str]
    reasoning_trail: List[str] = field(default_factory=list)
    history_paths: List[str] = field(default_factory=list)
    error: Optional[str] = None


class PromptOptimizationEngine:
    """
    Coordinates prompt retrieval, improvement, evaluation, and persistence.
    """

    def __init__(
        self,
        *,
        management_client: FoundationaLLMManagementClient,
        refiner: PromptRefiner,
        evaluator: Optional[PromptEvaluator],
        logger: OptimizationRunLogger,
        backup_dir: Path,
        config: OptimizationConfig,
    ) -> None:
        self.management_client = management_client
        self.refiner = refiner
        self.evaluator = evaluator
        self.logger = logger
        self.backup_dir = backup_dir
        self.backup_dir.mkdir(parents=True, exist_ok=True)
        self.config = config

    def optimise_prompts(
        self,
        *,
        agent_name: str,
        prompt_refs: Iterable[PromptReference],
        optimization_brief: str,
    ) -> List[PromptOptimizationOutcome]:
        outcomes: List[PromptOptimizationOutcome] = []
        for prompt_ref in prompt_refs:
            outcome = self._optimise_single_prompt(
                agent_name=agent_name,
                prompt_ref=prompt_ref,
                optimization_brief=optimization_brief,
            )
            outcomes.append(outcome)
        return outcomes

    def _optimise_single_prompt(
        self,
        *,
        agent_name: str,
        prompt_ref: PromptReference,
        optimization_brief: str,
    ) -> PromptOptimizationOutcome:
        prompt_name = prompt_ref.prompt_name
        try:
            prompt_payload = self.management_client.get_prompt(prompt_name)
        except Exception as exc:  # pragma: no cover - network errors
            return PromptOptimizationOutcome(
                prompt_name=prompt_name,
                success=False,
                iterations=0,
                final_pass_rate=None,
                final_evaluation_summary=None,
                error=str(exc),
            )

        original_payload = dict(prompt_payload)
        backup_path = None

        if not self.config.dry_run:
            backup_path = self.management_client.backup_prompt(
                prompt_payload=original_payload,
                agent_name=agent_name,
                output_dir=self.backup_dir,
            )

        reasoning_trail: List[str] = []
        history_paths: List[str] = []
        last_evaluation_summary: Optional[str] = None
        last_pass_rate: Optional[float] = None
        current_payload = dict(original_payload)
        evaluation_feedback = None

        for iteration in range(1, self.config.max_iterations + 1):
            current_body = render_prompt_body(current_payload)
            improvement = self.refiner.improve_prompt(
                agent_name=agent_name,
                prompt_name=prompt_name,
                current_body=current_body,
                optimization_brief=optimization_brief,
                context_summary=", ".join(prompt_ref.contexts),
                previous_evaluation_feedback=evaluation_feedback,
                iteration_index=iteration,
            )

            candidate_payload = apply_prompt_body(current_payload, improvement.revised_body)

            evaluation_result: Optional[EvaluationResult] = None
            if not self.config.dry_run:
                self.management_client.upsert_prompt(candidate_payload)
                if self.evaluator and self.config.suite_name:
                    evaluation_result = self.evaluator.evaluate(
                        agent_name=agent_name,
                        suite_name=self.config.suite_name,
                        quick_mode=self.config.quick_mode,
                        test_index=self.config.test_index,
                        max_workers=self.config.max_workers,
                        repeat_test=self.config.repeat_test,
                        verbose=self.config.verbose,
                    )
                    last_evaluation_summary = evaluation_result.summary
                    last_pass_rate = evaluation_result.pass_rate
                    evaluation_feedback = evaluation_result.summary
                else:
                    last_evaluation_summary = "Evaluation skipped (no suite configured)."
                    last_pass_rate = improvement.confidence
                    evaluation_feedback = "Evaluation skipped."
            else:
                last_evaluation_summary = "Dry run - no evaluation executed."
                last_pass_rate = improvement.confidence
                evaluation_feedback = "Dry run - no evaluation executed."

            record = self.logger.record_iteration(
                iteration_index=iteration,
                prompt_name=prompt_name,
                backup_path=backup_path,
                improvement=improvement,
                evaluation=evaluation_result,
                revised_payload=candidate_payload,
            )

            reasoning_trail.append(improvement.reasoning)
            history_paths.append(str(record.revised_prompt_path))
            current_payload = candidate_payload

            if last_pass_rate is not None and last_pass_rate >= self.config.target_pass_rate:
                return PromptOptimizationOutcome(
                    prompt_name=prompt_name,
                    success=True,
                    iterations=iteration,
                    final_pass_rate=last_pass_rate,
                    final_evaluation_summary=last_evaluation_summary,
                    reasoning_trail=reasoning_trail,
                    history_paths=history_paths,
                )

        # If we reach this point without success, revert to backup if available
        if backup_path and not self.config.dry_run:
            self.management_client.upsert_prompt(original_payload)

        return PromptOptimizationOutcome(
            prompt_name=prompt_name,
            success=False,
            iterations=self.config.max_iterations,
            final_pass_rate=last_pass_rate,
            final_evaluation_summary=last_evaluation_summary,
            reasoning_trail=reasoning_trail,
            history_paths=history_paths,
            error="Convergence target not reached within iteration budget.",
        )

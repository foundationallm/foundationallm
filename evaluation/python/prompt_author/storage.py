"""
Persistence helpers for optimisation history.
"""

from __future__ import annotations

import json
from dataclasses import asdict, dataclass, field
from datetime import datetime
from pathlib import Path
from typing import Any, Dict, List, Optional

from .llm_refiner import PromptImprovementResult
from .evaluator import EvaluationResult


@dataclass
class IterationRecord:
    iteration: int
    prompt_name: str
    reasoning: str
    revised_prompt_path: Path
    backup_path: Optional[Path]
    evaluation_summary: Optional[str]
    evaluation_pass_rate: Optional[float]
    timestamp: str = field(default_factory=lambda: datetime.utcnow().isoformat())
    additional_notes: Dict[str, Any] = field(default_factory=dict)


class OptimizationRunLogger:
    """
    Writes optimisation iteration metadata to disk for auditing.
    """

    def __init__(self, base_dir: Path, agent_name: str) -> None:
        self.base_dir = base_dir
        self.agent_name = agent_name
        self.agent_dir = self.base_dir / agent_name
        self.agent_dir.mkdir(parents=True, exist_ok=True)
        self.iterations: List[IterationRecord] = []

    def record_iteration(
        self,
        *,
        iteration_index: int,
        prompt_name: str,
        backup_path: Optional[Path],
        improvement: PromptImprovementResult,
        evaluation: Optional[EvaluationResult],
        revised_payload: Dict[str, Any],
    ) -> IterationRecord:
        timestamp = datetime.utcnow().strftime("%Y%m%dT%H%M%SZ")
        prompt_dir = self.agent_dir / prompt_name
        prompt_dir.mkdir(parents=True, exist_ok=True)

        revised_path = prompt_dir / f"{prompt_name}-iteration-{iteration_index:02d}-{timestamp}.json"
        with revised_path.open("w", encoding="utf-8") as fh:
            json.dump(revised_payload, fh, ensure_ascii=False, indent=2)

        evaluation_summary = evaluation.summary if evaluation else None
        evaluation_pass_rate = evaluation.pass_rate if evaluation else None

        record = IterationRecord(
            iteration=iteration_index,
            prompt_name=prompt_name,
            reasoning=improvement.reasoning,
            revised_prompt_path=revised_path,
            backup_path=backup_path,
            evaluation_summary=evaluation_summary,
            evaluation_pass_rate=evaluation_pass_rate,
            additional_notes={
                "confidence": improvement.confidence,
                "suggested_evaluation": improvement.additional_notes.get("suggested_evaluation"),
            },
        )

        self.iterations.append(record)
        self._write_manifest()
        return record

    def _write_manifest(self) -> None:
        manifest_path = self.agent_dir / "optimization_history.json"
        data = [self._serialise_record(record) for record in self.iterations]
        with manifest_path.open("w", encoding="utf-8") as fh:
            json.dump(data, fh, ensure_ascii=False, indent=2)

    @staticmethod
    def _serialise_record(record: IterationRecord) -> Dict[str, Any]:
        result = asdict(record)
        result["revised_prompt_path"] = str(record.revised_prompt_path)
        if record.backup_path:
            result["backup_path"] = str(record.backup_path)
        return result

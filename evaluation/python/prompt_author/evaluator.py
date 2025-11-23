"""
Evaluation utilities that wrap the AgentEvals test harness.
"""

from __future__ import annotations

import sys
from dataclasses import dataclass, field
from datetime import datetime
from pathlib import Path
from typing import Any, Dict, Optional


AGENT_TEST_HARNESS_DIR = Path(__file__).resolve().parent.parent / "agent-test-harness"
if str(AGENT_TEST_HARNESS_DIR) not in sys.path:
    sys.path.insert(0, str(AGENT_TEST_HARNESS_DIR))

from test_suite_manager import TestSuiteManager  # type: ignore  # noqa: E402
from validator import TestValidator  # type: ignore  # noqa: E402


@dataclass
class EvaluationResult:
    total_tests: int
    passed_tests: int
    failed_tests: int
    pass_rate: float
    timestamp: str
    suite_name: str
    output_dir: Path
    summary: str
    raw_results: Dict[str, Any] = field(default_factory=dict)
    validation_report: Dict[str, Any] = field(default_factory=dict)

    @property
    def succeeded(self) -> bool:
        return self.total_tests > 0 and self.failed_tests == 0


class PromptEvaluator:
    """
    Runs AgentEvals suites and summarises their results for optimisation loops.
    """

    def __init__(self, output_dir: Path) -> None:
        self.output_dir = output_dir
        self.output_dir.mkdir(parents=True, exist_ok=True)
        self.suite_manager = TestSuiteManager()
        self.validator = TestValidator()

    def evaluate(
        self,
        *,
        agent_name: str,
        suite_name: str,
        quick_mode: bool = False,
        test_index: Optional[int] = None,
        max_workers: int = 5,
        repeat_test: int = 1,
        verbose: bool = False,
    ) -> EvaluationResult:
        timestamp = datetime.utcnow().strftime("%Y%m%d_%H%M%S")

        results = self.suite_manager.run_suite(
            suite_name=suite_name,
            agent_name=agent_name,
            quick_mode=quick_mode,
            test_index=test_index,
            max_workers=max_workers,
            output_dir=str(self.output_dir),
            timestamp=timestamp,
            verbose=verbose,
            repeat_test=repeat_test,
        )

        if results is None:
            raise RuntimeError(f"Evaluation suite '{suite_name}' failed to execute for agent '{agent_name}'.")

        validation = self.validator.validate_results(results, verbose=verbose)

        total = results.get("total_tests", 0)
        failed = results.get("failed_tests", 0)
        passed = results.get("passed_tests", 0)
        pass_rate = (passed / total) if total else 0.0

        summary = (
            f"Evaluation Suite '{suite_name}': {passed}/{total} passed "
            f"({pass_rate:.1%}), {failed} failed."
        )

        failed_tests_list = validation.get("failed_tests_list") or []
        if failed_tests_list:
            failing_questions = [
                f"- [{test.get('Ordinal')}] {test.get('Question', '').strip()[:120]}"
                for test in failed_tests_list[:10]
            ]
            summary += "\nFailing examples:\n" + "\n".join(failing_questions)

        return EvaluationResult(
            total_tests=total,
            passed_tests=passed,
            failed_tests=failed,
            pass_rate=pass_rate,
            timestamp=timestamp,
            suite_name=suite_name,
            output_dir=self.output_dir,
            summary=summary,
            raw_results=results,
            validation_report=validation,
        )

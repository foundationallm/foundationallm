"""
Command-line entry point for prompt authoring and optimisation.
"""

from __future__ import annotations

import argparse
import sys
from pathlib import Path
from typing import List, Optional

from .conversation_context import (
    build_prompt_messages,
    format_prompt_messages,
    persist_prompt_messages,
)
from .evaluator import PromptEvaluator
from .foundationallm_client import (
    AzureOpenAILLMClient,
    FoundationaLLMClientError,
    FoundationaLLMManagementClient,
)
from .llm_refiner import PromptRefiner
from .optimizer import OptimizationConfig, PromptOptimizationEngine
from .prompt_catalog import PromptReference, extract_prompt_references, select_prompts
from .storage import OptimizationRunLogger


def _load_brief(args: argparse.Namespace) -> str:
    if args.brief and args.brief_file:
        raise ValueError("Provide either --brief or --brief-file, not both.")
    if args.brief:
        return args.brief
    if args.brief_file:
        path = Path(args.brief_file)
        if not path.exists():
            raise FileNotFoundError(f"Brief file not found: {path}")
        return path.read_text(encoding="utf-8")
    raise ValueError("A problem brief is required. Supply --brief or --brief-file.")


def _print_prompt_catalog(prompts: List[PromptReference]) -> None:
    for prompt in prompts:
        print(prompt.prompt_name)


def build_arg_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Author and iteratively improve FoundationaLLM agent prompts.",
        formatter_class=argparse.ArgumentDefaultsHelpFormatter,
    )

    parser.add_argument("--agent", required=True, help="Name of the agent whose prompts will be optimised.")
    parser.add_argument(
        "--prompts",
        help="Comma-separated list of prompt names to optimise. Use 'all' to target every prompt.",
    )
    parser.add_argument(
        "--list-prompts",
        action="store_true",
        help="List prompts for the agent and exit without performing optimisation.",
    )
    parser.add_argument(
        "--brief",
        help="Inline description of the problem and success criteria.",
    )
    parser.add_argument(
        "--brief-file",
        help="Path to a file containing the problem brief and success criteria.",
    )
    parser.add_argument(
        "--suite",
        help="AgentEvals suite to execute after each revision (e.g., code-interpreter).",
    )
    parser.add_argument(
        "--max-iterations",
        type=int,
        default=5,
        help="Maximum number of optimisation iterations per prompt.",
    )
    parser.add_argument(
        "--target-pass-rate",
        type=float,
        default=0.9,
        help="Minimum evaluation pass rate required to accept a revision.",
    )
    parser.add_argument("--quick", action="store_true", help="Use quick-mode to limit evaluation suite size.")
    parser.add_argument("--test-index", type=int, help="Run only a specific test index within the suite.")
    parser.add_argument(
        "--repeat-test",
        type=int,
        default=1,
        help="Number of times to repeat each evaluation test.",
    )
    parser.add_argument(
        "--workers",
        type=int,
        default=5,
        help="Number of parallel workers used during evaluation.",
    )
    parser.add_argument("--verbose", action="store_true", help="Enable verbose logging.")
    parser.add_argument(
        "--no-eval",
        action="store_true",
        help="Skip automated evaluation runs (rely solely on LLM reasoning).",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Plan the optimisation without altering prompts or running evaluation.",
    )
    parser.add_argument(
        "--output-dir",
        default="prompt-author-output",
        help="Directory for logs, backups, and evaluation artefacts.",
    )

    return parser


def main(argv: Optional[List[str]] = None) -> int:
    parser = build_arg_parser()
    args = parser.parse_args(argv)

    try:
        management_client = FoundationaLLMManagementClient()
    except FoundationaLLMClientError as exc:
        parser.error(str(exc))
        return 2

    try:
        agent = management_client.get_agent(args.agent)
    except FoundationaLLMClientError as exc:
        parser.error(f"Unable to load agent '{args.agent}': {exc}")
        return 2

    prompt_catalog = extract_prompt_references(agent.resource)
    if not prompt_catalog:
        parser.error(f"No prompts found for agent '{args.agent}'.")
        return 2

    if args.list_prompts:
        _print_prompt_catalog(prompt_catalog)
        return 0

    try:
        brief = _load_brief(args)
    except Exception as exc:
        parser.error(str(exc))
        return 2

    try:
        selected_prompts = select_prompts(prompt_catalog, args.prompts)
    except ValueError as exc:
        parser.error(str(exc))
        return 2

    if not selected_prompts:
        parser.error("No prompts were selected for optimisation.")
        return 2

    base_output_dir = Path(args.output_dir).resolve()
    backup_dir = base_output_dir / "backups"
    history_dir = base_output_dir / "history"
    evaluation_dir = base_output_dir / "evaluations"
    context_dir = base_output_dir / "context"

    try:
        completion_client = AzureOpenAILLMClient()
    except FoundationaLLMClientError as exc:
        parser.error(str(exc))
        return 2

    refiner = PromptRefiner(completion_client=completion_client)
    evaluator = None
    if not args.no_eval and not args.dry_run and args.suite:
        evaluator = PromptEvaluator(output_dir=evaluation_dir)
    elif args.suite and (args.no_eval or args.dry_run):
        print("⚠️  Evaluation suite specified but evaluation disabled; skipping automated tests.")

    logger = OptimizationRunLogger(base_dir=history_dir, agent_name=args.agent)

    # Build aggregated prompt context view
    prompt_messages = build_prompt_messages(
        management_client=management_client,
        prompt_refs=prompt_catalog,
        selected_prompt_names=[prompt.prompt_name for prompt in selected_prompts],
    )
    context_output_path = context_dir / f"{args.agent}-prompt-context.txt"
    persist_prompt_messages(output_path=context_output_path, messages=prompt_messages)

    print("\n=== Prompt Context Overview ===")
    print(format_prompt_messages(prompt_messages))
    print(f"\n📝 Aggregated prompt context saved to: {context_output_path}")

    config = OptimizationConfig(
        max_iterations=args.max_iterations,
        target_pass_rate=args.target_pass_rate,
        suite_name=args.suite if not args.no_eval else None,
        quick_mode=args.quick,
        test_index=args.test_index,
        repeat_test=args.repeat_test,
        max_workers=args.workers,
        verbose=args.verbose,
        dry_run=args.dry_run,
    )

    engine = PromptOptimizationEngine(
        management_client=management_client,
        refiner=refiner,
        evaluator=evaluator,
        logger=logger,
        backup_dir=backup_dir,
        config=config,
    )

    outcomes = engine.optimise_prompts(
        agent_name=args.agent,
        prompt_refs=selected_prompts,
        optimization_brief=brief,
    )

    print("\n=== Optimisation Summary ===")
    exit_code = 0
    for outcome in outcomes:
        status = "✅" if outcome.success else "❌"
        summary_line = (
            f"{status} {outcome.prompt_name}: iterations={outcome.iterations}, "
            f"final_pass_rate={outcome.final_pass_rate if outcome.final_pass_rate is not None else 'n/a'}"
        )
        print(summary_line)
        if outcome.final_evaluation_summary:
            print(f"    Evaluation: {outcome.final_evaluation_summary}")
        if outcome.error:
            print(f"    Error: {outcome.error}")
            exit_code = 1

    return exit_code


if __name__ == "__main__":
    sys.exit(main())

"""
Prompt Authoring and Optimization CLI package.

This package provides a workflow for discovering, authoring, and iteratively
improving agent prompts against the FoundationaLLM platform.

The CLI entry point is intentionally not imported at module load time to avoid
pulling in heavyweight dependencies during lightweight usage (e.g. unit tests).
"""

__all__ = ["__version__"]

__version__ = "0.1.0"

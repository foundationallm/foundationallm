# Prompt Author CLI

The **Prompt Author CLI** provides a command-line workflow for discovering, refining, and validating the prompts used by a FoundationaLLM agent. It wraps the existing AgentEvals harness and the FoundationaLLM management/core APIs to deliver an iterative optimisation loop that:

1. Retrieves an agent’s prompt catalogue.
2. Lets you scope which prompts to modify.
3. Captures a problem brief and target outcome.
4. Uses an LLM to suggest improved prompt text while surfacing its reasoning.
5. Optionally runs AgentEvals suites to confirm the prompt behaves as desired.
6. Saves backups, revision history, evaluation summaries, and writes successful prompts back to the platform.

---

## Installation & Prerequisites

The CLI lives in `evaluation/python/prompt_author`. It reuses the dependencies from the agent test harness:

```bash
python3 -m venv .venv
source .venv/bin/activate
pip install -r evaluation/python/agent-test-harness/requirements.txt
```

Set the required environment variables before running the tool:

| Variable | Description |
| --- | --- |
| `FLLM_MGMT_ENDPOINT` | Management API base URL, including `/instances/{instanceId}/` and trailing slash. |
| `FLLM_MGMT_BEARER_TOKEN` | Bearer token with `api://FoundationaLLM-Management/Data.Manage` scope. |
| `FLLM_ENDPOINT` | Core API base URL. |
| `FLLM_ACCESS_TOKEN` | Access token for the agent completions API. |
| `AZURE_OPENAI_ENDPOINT` | Azure OpenAI endpoint used for LLM-based prompt refinement. |
| `AZURE_OPENAI_API_KEY` | API key for the Azure OpenAI resource. |
| `AZURE_OPENAI_API_VERSION` | *(Optional)* API version (defaults to `2024-02-15-preview`). |
| `AZURE_OPENAI_DEPLOYMENT` | Deployment name (model) used for chat completions. |

### Obtaining the Management Bearer Token

Before running the CLI:

1. Sign in with the Azure CLI:
   ```bash
   az login
   ```
2. Request a management token with the required scope:
   ```bash
   az account get-access-token --scope api://FoundationaLLM-Management/Data.Manage
   ```
3. Copy only the `accessToken` value (without surrounding quotes) from the command output and export it as `FLLM_MGMT_BEARER_TOKEN`.

For automated evaluations, the AgentEvals harness expects any additional dependencies defined in its README (CSV datasets, `.env`, etc.).

---

## Usage

From the repository root:

```bash
python3 -m prompt_author.cli --agent AGENT_NAME --brief "Explain the problem and success criteria."
```

Key switches:

| Switch | Description |
| --- | --- |
| `--agent` | Required. Agent whose prompts will be optimised. |
| `--prompts` | Comma-separated prompt names. Use `all` (default) to optimise every prompt referenced by the agent. |
| `--list-prompts` | Print the agent’s prompt catalogue and exit (no modifications). |
| `--brief` / `--brief-file` | Provide the optimisation brief inline or via a file. Exactly one is required for optimisation runs. |
| `--suite` | AgentEvals suite to execute after each revision (e.g. `code-interpreter`). |
| `--max-iterations` | Maximum refinement iterations per prompt (default: 5). |
| `--target-pass-rate` | Evaluation pass-rate threshold that determines convergence (default: 0.9). |
| `--quick` | Use quick-mode when running evaluations (limits the suite size). |
| `--dry-run` | Perform discovery and LLM refinement without writing prompts or running evaluations. |
| `--yes` | Automatically approve all prompt improvements without interactive confirmation. |
| `--no-eval` | Skip AgentEvals runs even if `--suite` is provided. |
| `--output-dir` | Destination for backups, revision history, and evaluation artefacts (default: `prompt-author-output/`). |

### Example: Optimise Main Prompt with Evaluations

```bash
python3 -m prompt_author.cli \
  --agent SupportAssistant \
  --prompts MainPrompt \
  --brief-file ./briefs/support-upgrade.md \
  --suite conversational \
  --max-iterations 4 \
  --target-pass-rate 0.95 \
  --workers 3
```

### Example: Automated Optimisation (No Human Interaction)

```bash
python3 -m prompt_author.cli \
  --agent SupportAssistant \
  --prompts MainPrompt \
  --brief "Improve response clarity and accuracy" \
  --suite conversational \
  --yes
```

### Example: List Prompts, No Changes

```bash
python3 -m prompt_author.cli --agent SupportAssistant --list-prompts
```

### Output Layout

The CLI creates the following structure inside `--output-dir`:

```
output/
  backups/           # JSON backups of every original prompt
  history/           # Iteration history per prompt (JSON manifest + revised payloads)
  evaluations/       # AgentEvals CSV/JSON artefacts (when enabled)
  context/           # Aggregated prompt transcripts (system/user/tool) for review
```

Each history entry records the LLM’s reasoning, confidence, evaluation summary, and file paths, making it easy to audit or roll back.

---

## Testing

Unit tests for the prompt author package:

```bash
python3 -m pytest evaluation/python/prompt_author/tests
```

Integration testing that exercises LLM refinement and evaluation requires valid tokens, available AgentEvals datasets, and (optionally) mocked services if you want deterministic results.

---

## Troubleshooting

- **Missing environment variables**: the CLI validates management/core API configuration up front. Ensure all required variables are set before running.
- **LLM refinement failures**: verify `FLLM_PROMPT_OPTIMIZER_AGENT` (or `--optimizer-agent`) points to an agent capable of answering the refinement instructions.
- **AgentEvals dataset issues**: the evaluation harness reuses CSVs defined in `evaluation/python/agent-test-harness/test_suites.json`. Ensure the referenced files exist and required dependencies (e.g., `pandas`) are installed.
- **Rollback**: backups are stored per prompt under `backups/{agentName}/{promptName}/`. You can reapply any JSON snapshot using the FoundationaLLM management API or the CLI itself.

---

## Roadmap Ideas

- Add Markdown/HTML summary generation comparing baseline vs revised prompts.
- Provide a dry-run option that emits proposed prompt deltas without LLM calls (useful for code review).
- Surface evaluation metrics in the CLI output (pass/fail counts, top error cases) beyond the textual summary.

Contributions are welcome—feel free to open issues or pull requests with enhancements.***

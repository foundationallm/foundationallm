# FoundationaLLM Agent Evaluations

A comprehensive test framework for FoundationaLLM agents that provides automated test execution, validation, and result management with support for both quick regression testing and comprehensive release validation.

## 🚀 Quick Start

### Prerequisites
- Python 3.10 or later
- FoundationaLLM agent with access token

### Setup (One-time)
1. **Create a local virtual environment:**
   ```powershell
   .\create_venv.ps1
   ```

2. **Activate the virtual environment:**
   ```powershell
   .\.venv\Scripts\Activate.ps1
   ```

3. **Install dependencies:**
   ```powershell
   pip install -r requirements.txt
   ```

4. **Configure environment:**
   ```powershell
   copy sample.env .env
   # Edit .env with your values
   ```

5. **Validate setup:**
   ```powershell
   .\validate_setup.ps1
   ```

### Run Your First Test
```powershell
# Quick test
python run_tests.py --suite who-are-you --agent MAA-02 --quick --report
```

## 📋 Test Framework Features

### 🎯 Test Organization
- **Feature-based test suites**: Organized by capability (code-interpreter, document-analysis, file-operations, etc.)
- **Agent parameterization**: Any agent can run any test suite
- **Flexible execution**: Run individual suites or all suites at once

### 🔍 Validation System
- **Rule-based validation**: Pattern matching, numeric validation, artifact checks
- **LLM-based validation**: Semantic similarity using Azure OpenAI
- **Hybrid approach**: Combines rules and LLM for comprehensive validation
- **Automatic validation**: Integrated into test execution

### 📊 Results Management
- **Multiple output formats**: CSV, JSON, and HTML reports
- **Visual dashboard**: Interactive HTML reports with detailed analysis
- **Performance metrics**: Token usage, duration, artifact counts

### 🧪 Test Generation
- **Automated expansion**: Generate test variations from seed prompts
- **Multiple strategies**: Variations, edge cases, negative tests, combinations
- **LLM-powered**: Uses Azure OpenAI for intelligent test generation
- **Quality control**: Deduplication and validation of generated tests

## 🛠️ Usage Examples

### Basic Test Execution
```powershell
# Run a specific test suite
python run_tests.py --suite who-are-you --agent MAA-02

# Quick mode (first N tests)
python run_tests.py --suite who-are-you --agent MAA-02 --quick

# Specific test by index
python run_tests.py --suite who-are-you --agent MAA-02 --test-index 3

# Repeat each test 3 times for reliability testing
python run_tests.py --suite who-are-you --agent MAA-02 --repeat-test 3
```

### Advanced Testing
```powershell
# Comprehensive validation with LLM
python run_tests.py --suite all --agent MAA-02  --report

# Cross-agent comparison (report generated automatically)
python run_tests.py --suite who-are-you --agents MAA-02,MAA-04,MAA-06 --report

# Repeat tests for reliability analysis
python run_tests.py --suite who-are-you --agent MAA-02 --repeat-test 5
```

### Test Generation
```powershell
# Interactive mode - create new test suite
python generate_tests.py --interactive --suite-name my-custom-tests

# Generate test variations
python generate_tests.py --input seed-tests.csv --output expanded-tests.csv --strategy variations --count 5

# Generate edge cases
python generate_tests.py --input seed-tests.csv --output edge-cases.csv --strategy edge-cases --count 3

# Append to existing suite
python generate_tests.py --input seed.csv --output test-suites/code-interpreter/TestQuestions-code-interpreter.csv --append

# Create test suite in custom directory
python generate_tests.py --test-suites-dir /path/to/custom-test-suites --interactive --suite-name my-custom-tests

# Add tests to existing suite in custom directory
python generate_tests.py --test-suites-dir /path/to/custom-test-suites --interactive --existing-suite code-interpreter
```

### Custom Test-Suites Directory

The `--test-suites-dir` switch allows you to maintain test suites in directories outside the repository (e.g., for local-only test suites that shouldn't be checked into source control). When not provided, the default `test-suites/` directory (relative to the script) is used.

#### Using with run_tests.py
```powershell
# Run tests from a custom test-suites directory
python run_tests.py --test-suites-dir /path/to/custom-test-suites --suite who-are-you --agent MAA-02

# List suites from a custom directory
python run_tests.py --test-suites-dir /path/to/custom-test-suites --list-suites

# Sync test suites in a custom directory
python run_tests.py --test-suites-dir /path/to/custom-test-suites --sync-test-index

# Validate CSV in a custom directory
python run_tests.py --test-suites-dir /path/to/custom-test-suites --validate-csv who-are-you
```

#### Using with generate_tests.py
```powershell
# Create a new test suite in a custom directory
python generate_tests.py --test-suites-dir /path/to/custom-test-suites --interactive --suite-name my-local-tests

# Add tests to an existing suite in a custom directory
python generate_tests.py --test-suites-dir /path/to/custom-test-suites --interactive --existing-suite code-interpreter
```

**Note:** The custom directory must exist and contain a `test_suites.json` file (or one will be created when you add your first suite). All suite CSV files and the configuration file will be created relative to the specified directory.

### Interactive Test Suite Creation
```powershell
# Create a new test suite interactively
python generate_tests.py --interactive --suite-name my-custom-tests

# Add tests to an existing test suite
python generate_tests.py --interactive --existing-suite code-interpreter

# If you try to create a suite that already exists, you'll get options:
python generate_tests.py --interactive --suite-name existing-suite-name
# This will prompt you to:
# 1. Append new tests to the existing suite
# 2. Create a new suite with a different name  
# 3. Cancel the operation

# The interactive mode will guide you through:
# - Creating test questions and expected answers (supports multiline input)
# - Setting up validation rules (contains, excludes, regex, etc.)
# - Choosing validation modes (rule, llm, hybrid)
# - Properly escaping special characters in CSV output
# - Preserving existing tests when adding to existing suites
# - Confirmation prompt when appending to existing suites
# - Smart detection of existing suites with options to append or rename
# - Multiline input support for Questions and Expected Answers (paste tabular data, code, etc.)
```

### Multiline Input Support
The interactive mode supports multiline input for Questions and Expected Answers, making it easy to paste:
- **Tabular data** (like CSV content, database results)
- **Code snippets** (Python, SQL, etc.)
- **Formatted text** (with line breaks and indentation)
- **JSON/XML content**
- **Any multi-line content**

When entering Questions or Expected Answers, you can:
1. Paste multi-line content directly
2. Press **Ctrl+Z** (Windows) or **Ctrl+D** (Unix) when finished
3. The content will be properly captured with newlines preserved

Example of pasting tabular data:
```
Here are the first two rows:

ID	SYMBOL	NAME	PRICE
bitcoin	BTC	Bitcoin	46847.42
ethereum	ETH	Ethereum	2261.91
```

### Report Generation from Existing Results
```powershell
# Generate HTML report from single JSON results file
python run_tests.py --report-from-results results/20251021_201101-MAA-02-code-interpreter-results.json

# Generate HTML report from all JSON files in directory
python run_tests.py --report-from-dir results/

# Generate report with custom output directory
python run_tests.py --report-from-dir results/ --output-dir reports/

# The HTML report includes:
# - Test summary with pass/fail statistics
# - Detailed test results with expandable sections
# - Side-by-side comparison of Agent Answer vs Expected Answer
# - Detailed artifact information (produced vs expected)
# - Agent performance metrics (tokens, duration, artifacts)
# - Interactive interface with collapsible sections
# - Responsive design for mobile and desktop viewing
# - Artifact validation status (count, types, file details)
```

### Utility Commands
```powershell
# List available test suites
python run_tests.py --list-suites

# Validate CSV format
python run_tests.py --validate-csv who-are-you

# Dry run (validate config)
python run_tests.py --suite all --agent MAA-02 --dry-run

# Use custom test-suites directory
python run_tests.py --test-suites-dir /path/to/custom-test-suites --suite who-are-you --agent MAA-02

# Generate HTML report from existing results
python run_tests.py --report-from-results results/20251021_201101-MAA-02-code-interpreter-results.json

# Generate HTML report from all JSON files in directory
python run_tests.py --report-from-dir results/
```

## 📖 Command-Line Reference

### run_tests.py - Test Execution

#### Basic Parameters
| Parameter | Purpose | Values/Examples |
|-----------|---------|-----------------|
| `--suite` | Test suite to run | `who-are-you`, `code-interpreter`, `document-analysis`, `file-operations`, `routing`, `dataframe-tests`, `all` |
| `--agent` | Single agent to test | `MAA-02`, `MAA-04`, `MAA-06`, etc. |
| `--agents` | Multiple agents (comma-separated) | `MAA-02,MAA-04,MAA-06` |
| `--test-suites-dir` | Path to custom test-suites directory | `--test-suites-dir /path/to/custom-test-suites` (default: `test-suites/` relative to script) |

#### Execution Control
| Parameter | Purpose | Values/Examples |
|-----------|---------|-----------------|
| `--quick` | Run only first N tests | `--quick` (uses default limit from test_suites.json) |
| `--test-index` | Run specific test by index | `--test-index 3` (0-based index) |
| `--repeat-test` | Number of times to repeat each test | `--repeat-test 3` (default: 1) |
| `--no-report` | Skip HTML report generation | `--no-report` |

#### Validation Options
| Parameter | Purpose | Values/Examples |
|-----------|---------|-----------------|
| `--strict` | Exit on validation failure | `--strict` |
| `--verbose` | Detailed output | `--verbose` |

#### Report Generation
| Parameter | Purpose | Values/Examples |
|-----------|---------|-----------------|
| `--report` | Generate HTML report | `--report` |
| `--report-from-results` | Generate report from existing JSON | `--report-from-results results/20251021_220653-MAA-02-dataframe-tests-results.json` |
| `--report-from-dir` | Generate report from directory | `--report-from-dir results/` |
| `--output-dir` | Output directory for reports | `--output-dir reports/` |

#### Examples
```powershell
# Basic test execution
python run_tests.py --suite who-are-you --agent MAA-02

# Multi-agent comparison with validation
python run_tests.py --suite who-are-you --agents MAA-02,MAA-04,MAA-06 --report

# Quick test with specific test
python run_tests.py --suite who-are-you --agent MAA-02 --quick --test-index 2

# Repeat tests for reliability analysis
python run_tests.py --suite who-are-you --agent MAA-02 --repeat-test 3 --report

# Generate report from existing results
python run_tests.py --report-from-results results/20251021_220653-MAA-02-dataframe-tests-results.json
```

### generate_tests.py - Test Generation

#### Interactive Mode
| Parameter | Purpose | Values/Examples |
|-----------|---------|-----------------|
| `--interactive` | Enable interactive mode | `--interactive` |
| `--suite-name` | Name for new test suite | `--suite-name my-custom-tests` |
| `--existing-suite` | Add to existing suite | `--existing-suite code-interpreter` |
| `--test-suites-dir` | Path to test-suites directory (default: test-suites/) | `--test-suites-dir /path/to/custom-test-suites` |

#### File-based Generation
| Parameter | Purpose | Values/Examples |
|-----------|---------|-----------------|
| `--input` | Input CSV file | `--input seed-tests.csv` |
| `--output` | Output CSV file | `--output expanded-tests.csv` |
| `--strategy` | Generation strategy | `variations`, `edge-cases`, `negative-tests`, `combinations` |
| `--count` | Number of tests to generate | `--count 5` |
| `--append` | Append to existing file | `--append` |
| `--test-suites-dir` | Path to test-suites directory (default: test-suites/) | `--test-suites-dir /path/to/custom-test-suites` |

#### Generation Strategies
| Strategy | Purpose | Example Output |
|----------|---------|----------------|
| `variations` | Create variations of seed tests | Different phrasings, similar questions |
| `edge-cases` | Generate edge cases | Boundary conditions, extreme inputs |
| `negative-tests` | Create negative test cases | Invalid inputs, error conditions |
| `combinations` | Combine multiple operations | Multi-step workflows, complex scenarios |

#### Variations Strategy Details

The `variations` strategy generates question variations while preserving all original test data:

**Key Features:**
- **Preserves all source columns**: All columns from the input CSV are preserved exactly as they appear
- **Only modifies Question field**: Variations are identical to the original test except for the `Question` column
- **No extra columns**: Does not add `GeneratedFrom` or `GenerationStrategy` columns
- **Preserves validation settings**: Maintains original `ValidationRules` and `ValidationMode` values
- **Handles empty values correctly**: Empty filenames remain empty (no "nan" values)

**How it works:**
1. Reads the input CSV and detects all columns
2. For each seed test, generates N question variations using:
   - LLM-based generation (if Azure OpenAI is configured), or
   - Rule-based generation (fallback with word replacements, prefixes, rephrasing)
3. Creates output rows that are exact copies of the original row, with only the `Question` field changed
4. Output CSV has the same structure and columns as the input CSV

**Example:**
```csv
# Input (TestQuestions-seed.csv)
Question,Filename,Answer,ValidationRules,ValidationMode
Who are you?,,I am a helpful assistant.,"{""llm_validation"": true}",llm

# Output (TestQuestions-variations.csv) with --count 3
Question,Filename,Answer,ValidationRules,ValidationMode
Who are you?,,I am a helpful assistant.,"{""llm_validation"": true}",llm
Can you tell me who are you?,,I am a helpful assistant.,"{""llm_validation"": true}",llm
I would like to know who are you?,,I am a helpful assistant.,"{""llm_validation"": true}",llm
What can you tell me about who are you?,,I am a helpful assistant.,"{""llm_validation"": true}",llm
```

**Usage:**
```powershell
# Generate 9 variations using suite names (recommended)
python generate_tests.py --input-suite code-interpreter --output-suite code-interpreter-variations --strategy variations --count 9

# Or using CSV file paths
python generate_tests.py --input seed-tests.csv --output variations.csv --strategy variations --count 9
```

#### Edge-Cases Strategy Details

The `edge-cases` strategy generates test cases that probe boundary conditions, extreme inputs, and unusual scenarios:

**Key Features:**
- **Tests boundary conditions**: Empty inputs, minimal data, very large inputs
- **Handles special cases**: Special characters, Unicode, encoding issues
- **File format variations**: Unusual file formats, corrupted data, missing files
- **Preserves original structure**: All source columns are preserved (same as variations strategy)
- **LLM-powered or rule-based**: Uses Azure OpenAI for sophisticated edge cases, falls back to simple patterns

**How it works:**
1. For each seed test, generates N edge case scenarios using:
   - **LLM-based generation** (if Azure OpenAI is configured): Creates sophisticated edge cases considering boundary conditions, encoding issues, file format problems, etc.
   - **Rule-based generation** (fallback): Generates simple patterns like "Empty:", "Large:", "Special chars:", "Unicode:", "Minimal:" prefixes
2. Edge cases may modify:
   - The question (to test edge scenarios)
   - The filename (to test with missing/invalid files)
   - The expected answer (to reflect proper edge case handling)
3. Output rows preserve all original columns, with modifications only to relevant fields

**Example:**
```csv
# Input (TestQuestions-seed.csv)
Question,Filename,Answer,ValidationRules,ValidationMode
What text is in sample.txt?,sample.txt,"The file contains important data","{""contains"": [""data""]}",hybrid

# Output (TestQuestions-edge-cases.csv) with --count 3 (LLM-generated)
Question,Filename,Answer,ValidationRules,ValidationMode
What text is in sample.txt?,sample.txt,"The file contains important data","{""contains"": [""data""]}",hybrid
What text is in sample.txt?,,Error: File not found or empty,"{""contains"": [""error"", ""not found""]}",rule
What text is in a 10GB file?,large-file.txt,"Processing very large file...","{""contains"": [""large"", ""processing""]}",rule
What text is in file-with-特殊字符.txt?,file-with-特殊字符.txt,"File contains Unicode characters","{""contains"": [""unicode""]}",rule
```

**Usage:**
```powershell
# Generate edge cases using suite names (recommended)
python generate_tests.py --input-suite document-analysis --output-suite document-analysis-edge-cases --strategy edge-cases --count 5

# Or using CSV file paths
python generate_tests.py --input seed-tests.csv --output edge-cases.csv --strategy edge-cases --count 5
```

**Note:** For best results, configure Azure OpenAI credentials in your `.env` file. The LLM will generate more sophisticated edge cases than the basic rule-based fallback.

#### Negative-Tests Strategy Details

The `negative-tests` strategy generates test cases that should fail or produce errors, testing error handling and validation:

**Key Features:**
- **Invalid inputs**: Tests with malformed or invalid data
- **Missing information**: Tests with required information omitted
- **Contradictory instructions**: Tests with conflicting requirements
- **Unsupported operations**: Tests with operations that aren't supported
- **Error validation**: Expected answers typically indicate errors or clarifications
- **Preserves original structure**: All source columns are preserved
- **Full field modification**: Unlike variations (which only change Question), negative-tests can modify multiple fields

**How it works:**
1. For each seed test, generates N negative test scenarios using:
   - **LLM-based generation** (if Azure OpenAI is configured): Creates sophisticated negative tests considering:
     - Invalid inputs (malformed data, special characters, XSS attempts)
     - Missing required information
     - Contradictory instructions (e.g., "create and delete simultaneously")
     - Unsupported operations
   - **Rule-based generation** (fallback): **Currently not implemented** - if LLM is unavailable, no negative tests will be generated
2. Negative tests typically modify:
   - The question (to include invalid/missing/contradictory elements)
   - The filename (to point to invalid or missing files)
   - The expected answer (to indicate proper error handling or clarification)
   - Validation rules (to check for error messages or appropriate responses)
   - Validation mode (typically set to "rule" for negative tests)
3. Output rows preserve all original columns, with modifications to reflect negative test scenarios

**Example:**
```csv
# Input (TestQuestions-seed.csv)
Question,Filename,Answer,ValidationRules,ValidationMode
Create a PDF with 'Hello World',,A PDF file containing Hello World,"{""artifact_count"": 1, ""artifact_types"": [""File""]}",hybrid

# Output (TestQuestions-negative-tests.csv) with --count 3 (LLM-generated)
Question,Filename,Answer,ValidationRules,ValidationMode
Create a PDF with 'Hello World',,A PDF file containing Hello World,"{""artifact_count"": 1, ""artifact_types"": [""File""]}",hybrid
Create a PDF with 'Hello World' and also delete it,,Error: Contradictory instructions - cannot create and delete simultaneously,"{""contains"": [""error"", ""contradictory""]}",rule
Create a PDF with invalid characters: <script>alert('xss')</script>,,Error: Invalid characters detected in input,"{""contains"": [""error"", ""invalid""]}",rule
Create a PDF with no content specified,,Error: Missing required content for PDF creation,"{""contains"": [""error"", ""missing""]}",rule
```

**Usage:**
```powershell
# Generate negative tests using suite names (recommended)
python generate_tests.py --input-suite code-interpreter --output-suite code-interpreter-negative --strategy negative-tests --count 5

# Or using CSV file paths
python generate_tests.py --input seed-tests.csv --output negative-tests.csv --strategy negative-tests --count 5
```

**Important Notes:**
- **LLM Required**: This strategy requires Azure OpenAI credentials to be configured in your `.env` file. Without the LLM, no negative tests will be generated (the rule-based fallback is not implemented for this strategy).
- **Error-focused**: Expected answers typically indicate errors or clarifications, and validation rules often check for error messages (e.g., `{"contains": ["error", "invalid"]}`).
- **Full field modification**: Unlike the variations strategy which only modifies the Question field, negative-tests can modify Question, Filename, ExpectedAnswer, ValidationRules, and ValidationMode to create comprehensive error scenarios.

#### Interactive Validation Modes
| Mode | Purpose | Validation Rules Asked |
|------|---------|----------------------|
| `rule` | Rule-based validation only | Contains, Excludes, Regex, Min/Max length, Artifacts, Numeric |
| `llm` | LLM-based validation only | Artifacts, Code execution success |
| `hybrid` | Both rule-based and LLM | All validation rules |

#### Interactive Validation Rules
| Rule | Purpose | Examples |
|------|---------|----------|
| **Contains** | Text must contain phrases | `"success", "completed", "error"` |
| **Excludes** | Text must not contain phrases | `"failed", "error", "timeout"` |
| **Regex** | Pattern matching | `"\\d{4}-\\d{2}-\\d{2}"` (date pattern) |
| **Min/Max Length** | Text length constraints | `min: 50, max: 1000` |
| **Artifact Count** | Expected number of artifacts | `1`, `3`, `5` |
| **Artifact Types** | Expected artifact types | `WorkflowExecution, ToolExecution, File` |
| **Numeric Value** | Expected numeric result | `42.5` with tolerance `0.1` |
| **Code Success** | Code execution must succeed | `y` (yes) or `n` (no) |

#### Examples
```powershell
# Interactive test creation
python generate_tests.py --interactive --suite-name my-custom-tests

# Add tests to existing suite
python generate_tests.py --interactive --existing-suite code-interpreter

# Generate test variations using suite names (recommended)
python generate_tests.py --input-suite code-interpreter --output-suite code-interpreter-variations --strategy variations --count 9

# Generate edge cases using suite names
python generate_tests.py --input-suite document-analysis --output-suite document-analysis-edge-cases --strategy edge-cases --count 5

# Generate variations using CSV file paths
python generate_tests.py --input seed-tests.csv --output variations.csv --strategy variations --count 10

# Generate negative tests
python generate_tests.py --input seed-tests.csv --output negative-tests.csv --strategy negative-tests --count 3

# Generate combinations
python generate_tests.py --input seed-tests.csv --output combinations.csv --strategy combinations --count 8
```

### Test Suite Management

#### Utility Commands
| Parameter | Purpose | Values/Examples |
|-----------|---------|-----------------|
| `--list-suites` | List all available test suites | `--list-suites` |
| `--validate-csv` | Validate CSV format for a test suite | `--validate-csv who-are-you` |
| `--dry-run` | Validate configuration without executing tests | `--dry-run` |
| `--test-suites-dir` | Path to custom test-suites directory | `--test-suites-dir /path/to/custom-test-suites` |

#### Test Suite Operations
| Operation | Purpose | Command Example |
|-----------|---------|-----------------|
| **List Suites** | Show all available test suites with descriptions | `python run_tests.py --list-suites` |
| **Validate CSV** | Check CSV format and structure for a specific suite | `python run_tests.py --validate-csv who-are-you` |
| **Dry Run** | Validate configuration without running tests | `python run_tests.py --suite who-are-you --agent MAA-02 --dry-run` |

#### Dry-Run Validation Details
The `--dry-run` switch performs comprehensive configuration validation without executing tests:

**Environment Validation:**
- Virtual environment activation (local `.venv` directory)
- Required environment variables (`FLLM_ACCESS_TOKEN`, `FLLM_ENDPOINT`)
- Output directory creation

**Test Suite Validation:**
- Suite existence in `test-suites/test_suites.json`
- CSV file path configuration
- CSV file existence verification

**Command-Line Validation:**
- Required parameters (`--suite`, `--agent` or `--agents`)
- Parameter conflict detection
- Argument consistency checks

**What Dry-Run Does NOT Validate:**
- CSV content structure or data format
- JSON validation for ValidationRules
- Test data quality (empty questions, malformed data)
- Agent connectivity or test execution

**Use Cases:**
- Quick setup verification before test execution
- Pre-flight validation for expensive test suites
- Configuration debugging and troubleshooting
- CI/CD environment validation

#### Test Suite Configuration
Test suites are configured in `test-suites/test_suites.json` with the following structure:
```json
{
  "suite-name": {
    "csv_file": "test-suites/suite-name/TestQuestions-suite-name.csv",
    "description": "Description of the test suite",
    "quick_mode_limit": 5
  }
}
```

**Custom Test-Suites Directory:**
You can specify a custom location for test suites using the `--test-suites-dir` parameter. This is useful for:
- Using different test suites for different environments
- Sharing test suites across multiple projects
- Organizing test suites in a custom directory structure

When using `--test-suites-dir`, the system expects:
- A directory containing a `test_suites.json` configuration file (or it will create a default one)
- Subdirectories for each test suite (e.g., `who-are-you/`, `code-interpreter/`)
- CSV files following the naming pattern: `TestQuestions-{suite-name}.csv`

**Note:** CSV file paths in `test_suites.json` should be relative to the test-suites directory (e.g., `who-are-you/TestQuestions-who-are-you.csv`), not absolute paths.

#### Examples
```powershell
# List all available test suites
python run_tests.py --list-suites

# Validate CSV format for a specific suite
python run_tests.py --validate-csv who-are-you

# Validate configuration without running tests
python run_tests.py --suite who-are-you --agent MAA-02 --dry-run

# Check if a suite exists and is properly configured
python run_tests.py --suite who-are-you --agent MAA-02 --dry-run --verbose

# Use a custom test-suites directory
python run_tests.py --test-suites-dir /path/to/custom-test-suites --suite who-are-you --agent MAA-02

# List suites from a custom directory
python run_tests.py --test-suites-dir /path/to/custom-test-suites --list-suites

# Sync test suites in a custom directory
python run_tests.py --test-suites-dir /path/to/custom-test-suites --sync-test-index
```

## 📁 Test Data Structure

### Enhanced CSV Format
Test CSV files now support additional columns for validation:

```csv
Question,Filename,ExpectedAnswer,ValidationRules,ValidationMode
"What text is in this file?",sample.txt,"Expected content summary","{\"contains\": [\"key phrase\"], \"excludes\": [\"error\"]}",hybrid
"Create a PDF with 'Hello World'",,"A PDF file containing Hello World","{\"artifact_count\": 1, \"artifact_types\": [\"File\"]}",rule
```

### Validation Rules
- **contains**: Array of strings that must appear in the answer
- **excludes**: Array of strings that must NOT appear
- **regex**: Regular expression pattern to match
- **min_length/max_length**: Character count limits
- **artifact_count**: Expected number of artifacts
- **artifact_types**: Expected artifact types (File, ToolExecution, etc.)
- **code_success**: Boolean for code execution tests
- **numeric_value**: Expected numeric result with tolerance

### Test Suite Organization
```
test-suites/
├── code-interpreter/
│   └── TestQuestions-code-interpreter.csv
├── document-analysis/
│   └── TestQuestions-document-analysis.csv
├── file-operations/
│   └── TestQuestions-file-operations.csv
├── routing/
│   └── TestQuestions-routing.csv
├── conversational/
│   └── TestQuestions-conversational.csv
└── knowledge-retrieval/
    └── TestQuestions-knowledge-retrieval.csv
```

## 🔧 Configuration

### Environment Variables
Required in `.env` file:
```env
FLLM_ACCESS_TOKEN=keya.your-token.ayek
FLLM_ENDPOINT=https://your-endpoint.azurecontainerapps.io/instances/your-instance/
```

Optional for enhanced features:
```env
FLLM_MGMT_ENDPOINT=https://your-mgmt-endpoint/instances/your-instance/
FLLM_MGMT_BEARER_TOKEN=your-bearer-token
AZURE_OPENAI_ENDPOINT=https://your-openai-endpoint.openai.azure.com/
AZURE_OPENAI_API_KEY=your-openai-key
AZURE_OPENAI_DEPLOYMENT=gpt-4
```

### Test Suite Configuration
Edit `test-suites/test_suites.json` to add new test suites:
```json
{
  "my-feature": {
    "csv_file": "test-suites/my-feature/TestQuestions-my-feature.csv",
    "description": "Tests for my custom feature",
    "quick_mode_limit": 5
  }
}
```

## 📊 Results and Reports

### CSV Results
- **Location**: `results/{timestamp}-{agent}-{suite}-results.csv`
- **Key columns**: Question, AgentAnswer, ValidationPassed, ValidationScore, Tokens, Duration

### JSON Results
- **Location**: `results/{timestamp}-{agent}-{suite}-results.json`
- **Content**: Full-fidelity data including artifacts, metadata, and validation details

### HTML Dashboard
- **Location**: `results/summary-{timestamp}.html`
- **Features**: Interactive dashboard with pass/fail rates, failed test details, performance metrics

### Console Output
```
========================================
Test Results Summary
========================================
Total Tests: 25
Passed: 23 (92.0%)
Failed: 2 (8.0%)
Average Validation Score: 87.5
Total Duration: 125.3s
Total Tokens: 45,230
```

## 🚀 Advanced Workflows

### Continuous Integration
```powershell
# CI-friendly execution
python run_tests.py --suite all --agent MAA-02 --strict --no-report

# Save results for artifacts
python run_tests.py --suite all --agent MAA-02 --output-dir ./ci-results
```

### Development Testing
```powershell
# Quick regression test
python run_tests.py --suite who-are-you --agent MAA-02 --quick 
# Debug mode
python run_tests.py --suite who-are-you --agent MAA-02 --verbose
```

### Release Validation
```powershell
# Comprehensive pre-release testing
python run_tests.py --suite all --agent MAA-02  --report --workers 10
```

## 🔍 Troubleshooting

### Common Issues
1. **Virtual environment not activated**: Run `.\.venv\Scripts\Activate.ps1`
2. **Missing environment variables**: Check your `.env` file
3. **Test execution fails**: Use `--verbose` flag for detailed error information
4. **Validation errors**: Check `ValidationDetails` column in results

### Debug Commands
```powershell
# Validate environment
.\validate_setup.ps1

# Check test suite configuration
python run_tests.py --list-suites

# Validate CSV format
python run_tests.py --validate-csv who-are-you

# Dry run to check configuration
python run_tests.py --suite who-are-you --agent MAA-02 --dry-run
```

## 📚 Additional Resources

- **Test Generation**: Use `generate_tests.py` to expand test coverage
- **Validation**: Configure validation rules in CSV files
- **Reports**: Generate HTML dashboards for detailed analysis

For more information, see the inline help:
```powershell
python run_tests.py --help
python generate_tests.py --help
```


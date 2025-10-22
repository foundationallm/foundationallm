# FoundationaLLM Agent Test Harness

A comprehensive test framework for FoundationaLLM agents that provides automated test execution, validation, and result management with support for both quick regression testing and comprehensive release validation.

## 🚀 Quick Start

### Prerequisites
- Python 3.10 or later
- FoundationaLLM agent with access token
- Root virtual environment at `c:\Repos\foundationallm\.venv`

### Setup (One-time)
1. **Activate the root virtual environment:**
   ```powershell
   c:\Repos\foundationallm\.venv\Scripts\Activate.ps1
   ```

2. **Navigate to test harness directory:**
   ```powershell
   cd c:\Repos\foundationallm\evaluation\python\agent-test-harness
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
   .\activate_env.ps1
   ```

### Run Your First Test
```powershell
# Quick test
python run_tests.py --suite code-interpreter --agent MAA-02 --quick

# Full test with validation
python run_tests.py --suite all --agent MAA-02 --validate hybrid --report
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
- **Baseline comparison**: Track changes over time
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
python run_tests.py --suite code-interpreter --agent MAA-02

# Quick mode (first N tests)
python run_tests.py --suite code-interpreter --agent MAA-02 --quick

# Specific test by index
python run_tests.py --suite code-interpreter --agent MAA-02 --test-index 3
```

### Advanced Testing
```powershell
# Comprehensive validation with LLM
python run_tests.py --suite all --agent MAA-02 --validate hybrid --report

# Cross-agent comparison
python run_tests.py --suite code-interpreter --agents MAA-02,MAA-04,MAA-02 --compare

# Baseline comparison
python run_tests.py --suite all --agent MAA-02 --baseline results/baseline-MAA-02.json

# Save baseline
python run_tests.py --suite all --agent MAA-02 --save-baseline
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
python generate_tests.py --input seed.csv --output test-data/code-interpreter/TestQuestions-code-interpreter.csv --append
```

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
python run_tests.py --validate-csv code-interpreter

# Dry run (validate config)
python run_tests.py --suite all --agent MAA-02 --dry-run

# Generate HTML report from existing results
python run_tests.py --report-from-results results/20251021_201101-MAA-02-code-interpreter-results.json

# Generate HTML report from all JSON files in directory
python run_tests.py --report-from-dir results/
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
test-data/
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
Edit `test_suites.json` to add new test suites:
```json
{
  "my-feature": {
    "csv_file": "test-data/my-feature/TestQuestions-my-feature.csv",
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
python run_tests.py --suite code-interpreter --agent MAA-02 --quick --validate rule

# Debug mode
python run_tests.py --suite code-interpreter --agent MAA-02 --verbose
```

### Release Validation
```powershell
# Comprehensive pre-release testing
python run_tests.py --suite all --agent MAA-02 --validate hybrid --report --workers 10

# Compare against previous release
python run_tests.py --suite all --agent MAA-02 --baseline results/baseline-v1.2.3.json
```

## 🔍 Troubleshooting

### Common Issues
1. **Virtual environment not activated**: Run `c:\Repos\foundationallm\.venv\Scripts\Activate.ps1`
2. **Missing environment variables**: Check your `.env` file
3. **Test execution fails**: Use `--verbose` flag for detailed error information
4. **Validation errors**: Check `ValidationDetails` column in results

### Debug Commands
```powershell
# Validate environment
.\activate_env.ps1

# Check test suite configuration
python run_tests.py --list-suites

# Validate CSV format
python run_tests.py --validate-csv code-interpreter

# Dry run to check configuration
python run_tests.py --suite code-interpreter --agent MAA-02 --dry-run
```

## 📚 Additional Resources

- **Test Generation**: Use `generate_tests.py` to expand test coverage
- **Validation**: Configure validation rules in CSV files
- **Reports**: Generate HTML dashboards for detailed analysis
- **Baselines**: Save and compare results over time

For more information, see the inline help:
```powershell
python run_tests.py --help
python generate_tests.py --help
```


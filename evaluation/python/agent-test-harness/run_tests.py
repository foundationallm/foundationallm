#!/usr/bin/env python3
"""
Test Runner for FoundationaLLM Agent Tests

This script provides a command-line interface for running agent tests with
automatic environment setup, validation, and result management.
"""

import argparse
import os
import sys
import subprocess
import json
from datetime import datetime
from pathlib import Path

# Add the current directory to Python path for imports
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from test_suite_manager import TestSuiteManager
from validator import TestValidator


def validate_environment():
    """Validate that required environment variables are set"""
    required_vars = ['FLLM_ACCESS_TOKEN', 'FLLM_ENDPOINT']
    missing_vars = []
    
    for var in required_vars:
        if not os.getenv(var):
            missing_vars.append(var)
    
    if missing_vars:
        print("Error: Missing required environment variables:")
        for var in missing_vars:
            print(f"  - {var}")
        print("\nPlease set these in your .env file or environment.")
        sys.exit(1)


def main():
    parser = argparse.ArgumentParser(
        description="Run FoundationaLLM Agent Tests",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  # Quick regression test
  python run_tests.py --suite code-interpreter --agent MAA-06 --quick
  
  # Comprehensive validation with LLM
  python run_tests.py --suite all --agent MAA-06 --report
  
  # Cross-agent comparison (report generated automatically)
  python run_tests.py --suite code-interpreter --agents MAA-02,MAA-04,MAA-06 --report
  
  # Repeat each test 3 times for reliability testing
  python run_tests.py --suite code-interpreter --agent MAA-02 --repeat-test 3
  
  # Generate HTML report from existing results
  python run_tests.py --report-from-results results/20251021_201101-MAA-02-code-interpreter-results.json
  
  # Generate HTML report from all JSON files in directory
  python run_tests.py --report-from-dir results/
        """
    )
    
    # Test selection
    parser.add_argument('--suite',
                       help='Test suite to run (e.g., code-interpreter, document-analysis, all)')
    parser.add_argument('--agent', 
                       help='Agent name to test (e.g., MAA-06)')
    parser.add_argument('--agents',
                       help='Comma-separated list of agents to test (e.g., MAA-02,MAA-04,MAA-06)')
    
    # Test execution options
    parser.add_argument('--quick', action='store_true',
                       help='Run only first N tests (quick mode)')
    parser.add_argument('--test-index', type=int,
                       help='Run specific test by index (0-based)')
    parser.add_argument('--workers', type=int, default=5,
                       help='Number of parallel workers (default: 5)')
    parser.add_argument('--repeat-test', type=int, default=1,
                       help='Number of times to repeat each test (default: 1)')
    parser.add_argument('--dry-run', action='store_true',
                       help='Validate configuration without executing tests')
    parser.add_argument('--single-conversation', action='store_true',
                       help='Run entire suite within a single ordered conversation session')
    
    # Validation options
    parser.add_argument('--strict', action='store_true',
                       help='Exit with error code if any validation fails')
    
    # Output options
    parser.add_argument('--output-dir', default='results',
                       help='Output directory for results (default: results)')
    parser.add_argument('--report', action='store_true',
                       help='Generate HTML report')
    parser.add_argument('--no-report', action='store_true',
                       help='Skip HTML report generation')
    parser.add_argument('--verbose', action='store_true',
                       help='Verbose output for debugging')
    
    # Utility options
    parser.add_argument('--test-suites-dir',
                       help='Path to test-suites directory (default: test-suites/ relative to script)')
    parser.add_argument('--validate-csv',
                       help='Validate CSV format for specified feature')
    parser.add_argument('--list-suites', action='store_true',
                       help='List available test suites')
    parser.add_argument('--sync-test-index', action='store_true',
                       help='Synchronize test suites in test-suites folder with test_suites.json')
    
    # Report generation from existing results
    parser.add_argument('--report-from-results',
                       help='Generate HTML report from existing JSON results file')
    parser.add_argument('--report-from-dir',
                       help='Generate HTML report from all JSON files in directory')
    
    args = parser.parse_args()
    
    # Resolve test-suites directory path
    test_suites_dir = None
    if args.test_suites_dir:
        test_suites_dir = Path(args.test_suites_dir).resolve()
        if not test_suites_dir.exists():
            print(f"Error: Test-suites directory not found: {test_suites_dir}")
            sys.exit(1)
        if not test_suites_dir.is_dir():
            print(f"Error: Path is not a directory: {test_suites_dir}")
            sys.exit(1)
    
    # Handle utility commands first
    if args.list_suites:
        suite_manager = TestSuiteManager(test_suites_dir=test_suites_dir)
        suites = suite_manager.list_suites()
        print("Available test suites:")
        for name, config in suites.items():
            print(f"  {name}: {config.get('description', 'No description')}")
        return
    
    if args.validate_csv:
        suite_manager = TestSuiteManager(test_suites_dir=test_suites_dir)
        try:
            suite_manager.validate_csv(args.validate_csv)
            print(f"✓ CSV format for {args.validate_csv} is valid")
        except Exception as e:
            print(f"✗ CSV validation failed: {e}")
            sys.exit(1)
        return
    
    if args.sync_test_index:
        suite_manager = TestSuiteManager(test_suites_dir=test_suites_dir)
        try:
            suite_manager.sync_test_index()
        except Exception as e:
            print(f"Error syncing test index: {e}")
            if args.verbose:
                import traceback
                traceback.print_exc()
            sys.exit(1)
        return
    
    # Handle report generation from existing results
    if args.report_from_results or args.report_from_dir:
        from html_reporter import HTMLReporter
        reporter = HTMLReporter()
        
        if args.report_from_results:
            # Generate report from single JSON file
            if not os.path.exists(args.report_from_results):
                print(f"Error: Results file not found: {args.report_from_results}")
                sys.exit(1)
            
            try:
                with open(args.report_from_results, 'r', encoding='utf-8') as f:
                    results = json.load(f)
                
                # Convert single result to multi-agent format if needed
                if 'agent_name' in results:
                    # Single agent result - convert to multi-agent format
                    agent_name = results['agent_name']
                    all_results = {agent_name: results}
                else:
                    # Already in multi-agent format
                    all_results = results
                
                timestamp = results.get('timestamp', datetime.now().strftime("%Y%m%d_%H%M%S"))
                suite_name = results.get('suite_name', 'unknown')
                report_path = os.path.join(args.output_dir, f"summary-{timestamp}-{suite_name}.html")
                reporter.generate_report(all_results, report_path, timestamp)
                print(f"📊 HTML report generated: {report_path}")
                
            except Exception as e:
                print(f"Error loading results file: {e}")
                sys.exit(1)
        
        elif args.report_from_dir:
            # Generate report from all JSON files in directory
            if not os.path.exists(args.report_from_dir):
                print(f"Error: Directory not found: {args.report_from_dir}")
                sys.exit(1)
            
            try:
                all_results = {}
                json_files = [f for f in os.listdir(args.report_from_dir) if f.endswith('.json')]
                
                if not json_files:
                    print(f"Error: No JSON files found in {args.report_from_dir}")
                    sys.exit(1)
                
                print(f"Found {len(json_files)} JSON files in {args.report_from_dir}")
                
                for json_file in json_files:
                    file_path = os.path.join(args.report_from_dir, json_file)
                    try:
                        with open(file_path, 'r', encoding='utf-8') as f:
                            results = json.load(f)
                        
                        # Extract agent name from results
                        if 'agent_name' in results:
                            agent_name = results['agent_name']
                            all_results[agent_name] = results
                            print(f"  Loaded results for {agent_name}")
                        else:
                            print(f"  Warning: No agent_name found in {json_file}")
                            
                    except Exception as e:
                        print(f"  Warning: Could not load {json_file}: {e}")
                
                if not all_results:
                    print("Error: No valid results loaded")
                    sys.exit(1)
                
                timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
                # Get suite name from first agent's results
                suite_name = 'unknown'
                if all_results:
                    first_agent = list(all_results.values())[0]
                    if isinstance(first_agent, dict) and 'results' in first_agent:
                        suite_name = first_agent.get('suite_name', 'unknown')
                report_path = os.path.join(args.output_dir, f"summary-{timestamp}-{suite_name}.html")
                reporter.generate_report(all_results, report_path, timestamp)
                print(f"📊 HTML report generated: {report_path}")
                
            except Exception as e:
                print(f"Error processing directory: {e}")
                sys.exit(1)
        
        return
    
    # Validate arguments
    if not args.report_from_results and not args.report_from_dir:
        # Only validate suite and agent requirements for test execution
        if not args.suite:
            print("Error: Must specify --suite for test execution")
            sys.exit(1)
        
        if not args.agent and not args.agents:
            print("Error: Must specify either --agent or --agents")
            sys.exit(1)
        
        if args.agent and args.agents:
            print("Error: Cannot specify both --agent and --agents")
            sys.exit(1)
    
    # Validate environment variables
    validate_environment()
    
    # Create output directory
    os.makedirs(args.output_dir, exist_ok=True)
    
    # Initialize components
    suite_manager = TestSuiteManager(test_suites_dir=test_suites_dir)
    validator = TestValidator()
    
    # Determine agents to test
    if args.agents:
        agents = [agent.strip() for agent in args.agents.split(',')]
    else:
        agents = [args.agent]
    
    # Generate timestamp for results
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    
    try:
        if args.dry_run:
            print("Dry run mode - validating configuration...")
            for agent in agents:
                suite_manager.validate_suite(args.suite, agent)
            print("✓ Configuration is valid")
            return
        
        # Run tests for each agent
        all_results = {}
        for agent in agents:
            print(f"\n{'='*60}")
            print(f"Running tests for agent: {agent}")
            print(f"Suite: {args.suite}")
            print(f"{'='*60}")
            
            # Run the test suite
            results = suite_manager.run_suite(
                suite_name=args.suite,
                agent_name=agent,
                quick_mode=args.quick,
                test_index=args.test_index,
                max_workers=args.workers,
                output_dir=args.output_dir,
                timestamp=timestamp,
                verbose=args.verbose,
                repeat_test=args.repeat_test,
                single_conversation=args.single_conversation
            )
            
            if results is None:
                print(f"Failed to run tests for agent {agent}")
                continue
            
            all_results[agent] = results
            
            # Validate results using test-specific validation modes
            validation_results = validator.validate_results(
                results, 
                verbose=args.verbose
            )
            
            # Check for validation failures
            failed_tests = validation_results.get('failed_tests_list', [])
            if failed_tests and args.strict:
                print(f"\n✗ {len(failed_tests)} tests failed validation")
                sys.exit(1)
        
        # Generate reports
        if not args.no_report and (args.report or len(agents) > 1):
            from html_reporter import HTMLReporter
            reporter = HTMLReporter()
            
            # Get suite name from first agent's results
            suite_name = 'unknown'
            if all_results:
                first_agent = list(all_results.values())[0]
                if isinstance(first_agent, dict) and 'results' in first_agent:
                    suite_name = first_agent.get('suite_name', 'unknown')
            
            report_path = os.path.join(args.output_dir, f"summary-{timestamp}-{suite_name}.html")
            reporter.generate_report(all_results, report_path, timestamp)
            print(f"\n📊 HTML report generated: {report_path}")
        
        print(f"\n✅ Test execution completed successfully")
        
    except KeyboardInterrupt:
        print("\n⚠️ Test execution interrupted by user")
        sys.exit(1)
    except Exception as e:
        print(f"\n❌ Error during test execution: {e}")
        if args.verbose:
            import traceback
            traceback.print_exc()
        sys.exit(1)


if __name__ == "__main__":
    main()

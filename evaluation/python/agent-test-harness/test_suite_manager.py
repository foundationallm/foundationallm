"""
Test Suite Manager for FoundationaLLM Agent Tests

Manages test suites organized by feature/capability and provides
orchestration for running tests against different agents.
"""

import os
import json
import pandas as pd
from pathlib import Path
from typing import Dict, List, Optional, Any
import sys

# Import the existing test harness
from test_harness import execute_tests, process_question, execute_tests_from_dataframe


class TestSuiteManager:
    """Manages test suites and their execution"""
    
    def __init__(self, config_file: str = "test-suites/test_suites.json"):
        self.config_file = config_file
        self.config = self._load_config()
        self.base_dir = Path(__file__).parent
    
    def _load_config(self) -> Dict[str, Any]:
        """Load test suite configuration"""
        config_path = Path(__file__).parent / self.config_file
        if not config_path.exists():
            # Create default configuration
            default_config = {
                "code-interpreter": {
                    "csv_file": "test-suites/code-interpreter/TestQuestions-code-interpreter.csv",
                    "description": "Tests for code execution and file generation",
                    "quick_mode_limit": 5
                },
                "document-analysis": {
                    "csv_file": "test-suites/document-analysis/TestQuestions-document-analysis.csv",
                    "description": "Tests for document analysis and processing",
                    "quick_mode_limit": 3
                },
                "file-operations": {
                    "csv_file": "test-suites/file-operations/TestQuestions-file-operations.csv",
                    "description": "Tests for file creation, conversion, and manipulation",
                    "quick_mode_limit": 4
                },
                "routing": {
                    "csv_file": "test-suites/routing/TestQuestions-routing.csv",
                    "description": "Tests for agent routing and task delegation",
                    "quick_mode_limit": 5
                },
                "conversational": {
                    "csv_file": "test-suites/conversational/TestQuestions-conversational.csv",
                    "description": "Tests for general conversation and Q&A",
                    "quick_mode_limit": 3
                },
                "knowledge-retrieval": {
                    "csv_file": "test-suites/knowledge-retrieval/TestQuestions-knowledge-retrieval.csv",
                    "description": "Tests for knowledge base queries and retrieval",
                    "quick_mode_limit": 4
                }
            }
            self._save_config(default_config)
            return default_config
        
        with open(config_path, 'r') as f:
            return json.load(f)
    
    def _save_config(self, config: Dict[str, Any]):
        """Save test suite configuration"""
        config_path = Path(__file__).parent / self.config_file
        with open(config_path, 'w') as f:
            json.dump(config, f, indent=2)
    
    def list_suites(self) -> Dict[str, Any]:
        """List all available test suites"""
        return self.config
    
    def get_suite_config(self, suite_name: str) -> Optional[Dict[str, Any]]:
        """Get configuration for a specific test suite"""
        return self.config.get(suite_name)
    
    def validate_suite(self, suite_name: str, agent_name: str) -> bool:
        """Validate that a test suite is properly configured"""
        if suite_name == "all":
            # Validate all suites
            for name in self.config.keys():
                if not self._validate_single_suite(name):
                    return False
            return True
        else:
            return self._validate_single_suite(suite_name)
    
    def _validate_single_suite(self, suite_name: str) -> bool:
        """Validate a single test suite"""
        config = self.get_suite_config(suite_name)
        if not config:
            print(f"Error: Test suite '{suite_name}' not found")
            return False
        
        csv_file = config.get('csv_file')
        if not csv_file:
            print(f"Error: No CSV file specified for suite '{suite_name}'")
            return False
        
        csv_path = self.base_dir / csv_file
        if not csv_path.exists():
            print(f"Error: CSV file not found: {csv_path}")
            return False
        
        return True
    
    def validate_csv(self, feature: str) -> bool:
        """Validate CSV format for a specific feature"""
        config = self.get_suite_config(feature)
        if not config:
            raise ValueError(f"Feature '{feature}' not found in configuration")
        
        csv_file = config.get('csv_file')
        csv_path = self.base_dir / csv_file
        
        if not csv_path.exists():
            raise FileNotFoundError(f"CSV file not found: {csv_path}")
        
        # Read and validate CSV structure
        try:
            df = pd.read_csv(csv_path)
            required_columns = ['Question']
            optional_columns = ['Filename', 'ExpectedAnswer', 'ValidationRules', 'ValidationMode']
            
            # Check required columns
            missing_required = [col for col in required_columns if col not in df.columns]
            if missing_required:
                raise ValueError(f"Missing required columns: {missing_required}")
            
            # Check for empty questions
            empty_questions = df['Question'].isna() | (df['Question'].str.strip() == '')
            if empty_questions.any():
                empty_indices = df[empty_questions].index.tolist()
                raise ValueError(f"Empty questions found at rows: {empty_indices}")
            
            # Validate ValidationRules JSON if present
            if 'ValidationRules' in df.columns:
                for idx, rules in df['ValidationRules'].items():
                    if pd.notna(rules) and rules.strip():
                        try:
                            json.loads(rules)
                        except json.JSONDecodeError as e:
                            raise ValueError(f"Invalid JSON in ValidationRules at row {idx}: {e}")
            
            return True
            
        except Exception as e:
            raise ValueError(f"CSV validation failed: {e}")
    
    def run_suite(self, suite_name: str, agent_name: str, 
                   quick_mode: bool = False, test_index: Optional[int] = None,
                   max_workers: int = 5, output_dir: str = 'results', 
                   timestamp: str = '', verbose: bool = False, 
                   repeat_test: int = 1) -> Optional[Dict[str, Any]]:
        """Run a test suite for a specific agent"""
        
        if suite_name == "all":
            # Run all test suites
            all_results = {}
            for name in self.config.keys():
                if name != "all":  # Avoid infinite recursion
                    result = self.run_suite(
                        name, agent_name, quick_mode, test_index,
                        max_workers, output_dir, timestamp, verbose, repeat_test
                    )
                    if result:
                        all_results[name] = result
            return all_results
        
        # Get suite configuration
        config = self.get_suite_config(suite_name)
        if not config:
            print(f"Error: Test suite '{suite_name}' not found")
            return None
        
        csv_file = config.get('csv_file')
        csv_path = self.base_dir / csv_file
        
        if not csv_path.exists():
            print(f"Error: CSV file not found: {csv_path}")
            return None
        
        # Prepare test execution
        print(f"Loading test suite: {suite_name}")
        print(f"CSV file: {csv_path}")
        print(f"Agent: {agent_name}")
        
        try:
            # Read CSV file
            df = pd.read_csv(csv_path)
            
            # Apply quick mode limit
            if quick_mode:
                limit = config.get('quick_mode_limit', 5)
                df = df.head(limit)
                print(f"Quick mode: Running first {len(df)} tests")
            
            # Apply test index filter
            if test_index is not None:
                if test_index >= len(df):
                    print(f"Error: Test index {test_index} out of range (max: {len(df)-1})")
                    return None
                df = df.iloc[[test_index]]
                print(f"Running specific test at index {test_index}")
            
            # Run the tests
            if repeat_test > 1:
                print(f"Executing {len(df)} tests, repeating each test {repeat_test} times...")
                # Create expanded dataframe with repeated tests
                repeated_rows = []
                for _, row in df.iterrows():
                    for i in range(repeat_test):
                        # Create a copy of the row with a repeat indicator
                        repeated_row = row.copy()
                        repeated_row['_repeat_index'] = i + 1
                        repeated_row['_original_index'] = row.name
                        repeated_rows.append(repeated_row)
                
                # Create new dataframe with repeated tests
                expanded_df = pd.DataFrame(repeated_rows)
                results_df = execute_tests_from_dataframe(expanded_df, agent_name, max_workers)
            elif test_index is not None or quick_mode:
                # Use dataframe when filtering is applied (test_index or quick_mode)
                print(f"Executing {len(df)} tests...")
                results_df = execute_tests_from_dataframe(df, agent_name, max_workers)
            else:
                # Use CSV file when no filtering is applied
                print(f"Executing {len(df)} tests...")
                results_df = execute_tests(csv_path, agent_name, max_workers, output_dir=output_dir)
            
            if results_df is None:
                print("Test execution failed")
                return None
            
            # Convert results to dictionary format
            results = {
                'suite_name': suite_name,
                'agent_name': agent_name,
                'timestamp': timestamp,
                'total_tests': len(results_df),
                'passed_tests': len(results_df[results_df.get('ErrorOccured', 1) == 0]),
                'failed_tests': len(results_df[results_df.get('ErrorOccured', 1) == 1]),
                'results': results_df.to_dict('records')
            }
            
            # Save results
            self._save_results(results, output_dir, timestamp, suite_name, agent_name)
            
            return results
                    
        except Exception as e:
            print(f"Error running test suite: {e}")
            if verbose:
                import traceback
                traceback.print_exc()
            return None
    
    def _save_results(self, results: Dict[str, Any], output_dir: str, 
                     timestamp: str, suite_name: str, agent_name: str):
        """Save test results to files"""
        import pandas as pd
        import json
        
        # Create output directory
        os.makedirs(output_dir, exist_ok=True)
        
        # Save CSV results
        csv_filename = f"{timestamp}-{agent_name}-{suite_name}-results.csv"
        csv_path = os.path.join(output_dir, csv_filename)
        
        df = pd.DataFrame(results['results'])
        df.to_csv(csv_path, index=False)
        print(f"📄 CSV results saved: {csv_path}")
        
        # Save JSON results
        json_filename = f"{timestamp}-{agent_name}-{suite_name}-results.json"
        json_path = os.path.join(output_dir, json_filename)
        
        with open(json_path, 'w', encoding='utf-8') as f:
            json.dump(results, f, ensure_ascii=False, indent=2)
        print(f"📄 JSON results saved: {json_path}")
    
    def add_suite(self, name: str, csv_file: str, description: str = "", 
                  quick_mode_limit: int = 5):
        """Add a new test suite"""
        self.config[name] = {
            "csv_file": csv_file,
            "description": description,
            "quick_mode_limit": quick_mode_limit
        }
        self._save_config(self.config)
        print(f"Added test suite: {name}")
    
    def remove_suite(self, name: str):
        """Remove a test suite"""
        if name in self.config:
            del self.config[name]
            self._save_config(self.config)
            print(f"Removed test suite: {name}")
        else:
            print(f"Test suite '{name}' not found")

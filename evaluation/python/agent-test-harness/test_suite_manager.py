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
from test_harness import execute_tests, execute_tests_from_dataframe, execute_tests_single_conversation


class TestSuiteManager:
    """Manages test suites and their execution"""
    
    def __init__(self, config_file: str = "test-suites/test_suites.json", test_suites_dir: Optional[Path] = None):
        self.base_dir = Path(__file__).parent
        
        # Determine test-suites directory
        if test_suites_dir is not None:
            self.test_suites_dir = Path(test_suites_dir).resolve()
            # Config file is in the test-suites directory
            self.config_file = self.test_suites_dir / "test_suites.json"
        else:
            # Default: use test-suites/ relative to script
            self.test_suites_dir = self.base_dir / "test-suites"
            self.config_file = self.base_dir / config_file
        
        self.config = self._load_config()
    
    def _load_config(self) -> Dict[str, Any]:
        """Load test suite configuration"""
        # self.config_file is already a Path object
        config_path = self.config_file if isinstance(self.config_file, Path) else self.base_dir / self.config_file
        
        if not config_path.exists():
            # Create default configuration
            # Use relative paths from test_suites_dir
            default_config = {}
            suite_names = ["code-interpreter", "document-analysis", "file-operations", 
                          "routing", "conversational", "knowledge-retrieval"]
            for suite_name in suite_names:
                # Paths relative to test_suites_dir
                csv_path = f"{suite_name}/TestQuestions-{suite_name}.csv"
                default_config[suite_name] = {
                    "csv_file": csv_path,
                    "description": f"Tests for {suite_name}",
                    "quick_mode_limit": 5 if suite_name in ["code-interpreter", "routing"] else (3 if suite_name in ["document-analysis", "conversational"] else 4)
                }
            self._save_config(default_config)
            return default_config
        
        with open(config_path, 'r') as f:
            config = json.load(f)
            # Normalize CSV paths: strip 'test-suites/' prefix if present
            # since paths should be relative to test_suites_dir
            for suite_name, suite_config in config.items():
                csv_file = suite_config.get('csv_file', '')
                if csv_file.startswith('test-suites/'):
                    suite_config['csv_file'] = csv_file[len('test-suites/'):]
            return config
    
    def _save_config(self, config: Dict[str, Any]):
        """Save test suite configuration"""
        # self.config_file is already a Path object
        config_path = self.config_file if isinstance(self.config_file, Path) else self.base_dir / self.config_file
        # Ensure parent directory exists
        config_path.parent.mkdir(parents=True, exist_ok=True)
        # Sort keys alphabetically
        sorted_config = dict(sorted(config.items()))
        with open(config_path, 'w') as f:
            json.dump(sorted_config, f, indent=2)
    
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
        
        # CSV paths are relative to test_suites_dir
        csv_path = self.test_suites_dir / csv_file
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
        # CSV paths are relative to test_suites_dir
        csv_path = self.test_suites_dir / csv_file
        
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
                   repeat_test: int = 1, single_conversation: bool = False) -> Optional[Dict[str, Any]]:
        """Run a test suite for a specific agent"""
        
        if suite_name == "all":
            # Run all test suites
            all_results = {}
            for name in self.config.keys():
                if name != "all":  # Avoid infinite recursion
                    result = self.run_suite(
                        name, agent_name, quick_mode, test_index,
                        max_workers, output_dir, timestamp, verbose, repeat_test,
                        single_conversation
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
        # CSV paths are relative to test_suites_dir
        csv_path = self.test_suites_dir / csv_file
        
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
                # Store original index before resetting
                df = df.iloc[[test_index]].copy()
                df['_original_test_index'] = test_index
                df = df.reset_index(drop=True)
                print(f"Running specific test at index {test_index}")
            
            # Run the tests
            if single_conversation and repeat_test > 1:
                print("Warning: --repeat-test is not supported with --single-conversation. Ignoring repeat setting.")
            if single_conversation and max_workers != 1:
                print("Info: --single-conversation forces sequential execution. Ignoring --workers value.")
            
            if single_conversation:
                print(f"Executing {len(df)} tests in single conversation mode...")
                results_df = execute_tests_single_conversation(df, agent_name)
            elif repeat_test > 1:
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
            conversation_session_id = ''
            if single_conversation and not results_df.empty and 'ConversationSessionId' in results_df.columns:
                conversation_session_id = results_df['ConversationSessionId'].iloc[0]

            results = {
                'suite_name': suite_name,
                'agent_name': agent_name,
                'timestamp': timestamp,
                'total_tests': len(results_df),
                'passed_tests': len(results_df[results_df.get('ErrorOccured', 1) == 0]),
                'failed_tests': len(results_df[results_df.get('ErrorOccured', 1) == 1]),
                'results': results_df.to_dict('records'),
                'conversation_mode': single_conversation,
                'conversation_session_id': conversation_session_id
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
    
    def sync_test_index(self):
        """Synchronize test suites in test-suites folder with test_suites.json"""
        if not self.test_suites_dir.exists():
            print(f"Error: test-suites directory not found: {self.test_suites_dir}")
            return
        
        # Track changes
        added_count = 0
        removed_count = 0
        updated_count = 0
        
        # Step 1: Scan test-suites folder for subdirectories
        found_suites = {}
        for item in self.test_suites_dir.iterdir():
            # Skip files (like test_suites.json) and special directories
            if item.is_dir() and item.name != "__pycache__" and item.name != "test_suites.json":
                suite_name = item.name
                csv_file = item / f"TestQuestions-{suite_name}.csv"
                
                if csv_file.exists():
                    # Path relative to test_suites_dir
                    csv_file_rel = f"{suite_name}/TestQuestions-{suite_name}.csv"
                    found_suites[suite_name] = {
                        'csv_file': csv_file_rel,
                        'path': csv_file
                    }
        
        # Step 2: Add missing suites from folder to JSON
        for suite_name, suite_info in found_suites.items():
            if suite_name not in self.config:
                # Add new entry
                self.config[suite_name] = {
                    "csv_file": suite_info['csv_file'],
                    "description": f"Tests for {suite_name}",
                    "quick_mode_limit": 5
                }
                print(f"Added new test suite: {suite_name}")
                added_count += 1
            else:
                # Verify path matches
                expected_path = self.test_suites_dir / suite_info['csv_file']
                current_path = self.test_suites_dir / self.config[suite_name].get('csv_file', '')
                
                if expected_path != current_path and expected_path.exists():
                    # Update path if it's different but the expected path exists
                    self.config[suite_name]['csv_file'] = suite_info['csv_file']
                    print(f"Updated path for test suite: {suite_name}")
                    updated_count += 1
        
        # Step 3: Check JSON entries for non-existent paths
        suites_to_remove = []
        for suite_name, suite_config in list(self.config.items()):
            csv_file = suite_config.get('csv_file', '')
            # CSV paths are relative to test_suites_dir
            csv_path = self.test_suites_dir / csv_file
            
            if not csv_path.exists():
                print(f"\nTest suite '{suite_name}' in JSON points to non-existent path: {csv_path}")
                response = input("Remove this entry from test_suites.json? (y/n): ").strip().lower()
                if response == 'y':
                    suites_to_remove.append(suite_name)
        
        # Remove confirmed entries
        for suite_name in suites_to_remove:
            del self.config[suite_name]
            print(f"Removed test suite entry: {suite_name}")
            removed_count += 1
        
        # Step 4: Save updated config (already sorted by _save_config)
        if added_count > 0 or removed_count > 0 or updated_count > 0:
            self._save_config(self.config)
            print(f"\nSync complete:")
            if added_count > 0:
                print(f"  Added {added_count} new test suite(s)")
            if removed_count > 0:
                print(f"  Removed {removed_count} test suite(s)")
            if updated_count > 0:
                print(f"  Updated {updated_count} test suite(s)")
        else:
            print("No changes needed - test suites are in sync")

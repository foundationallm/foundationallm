#!/usr/bin/env python3
"""
Test Generator for FoundationaLLM Agent Tests

Generates expanded test coverage from seed prompts using LLM-based generation.
"""

import argparse
import csv
import json
import os
import sys
import re
from typing import Dict, List, Any, Optional
from pathlib import Path
import pandas as pd
from openai import AzureOpenAI


class TestGenerator:
    """Generates test variations using LLM-based expansion"""
    
    def __init__(self, test_suites_dir: Optional[Path] = None):
        self.base_dir = Path(__file__).parent
        
        # Determine test-suites directory
        if test_suites_dir is not None:
            self.test_suites_dir = Path(test_suites_dir).resolve()
        else:
            # Default: use test-suites/ relative to script
            self.test_suites_dir = self.base_dir / "test-suites"
        
        self.llm_client = self._init_llm_client()
    
    def _init_llm_client(self):
        """Initialize Azure OpenAI client"""
        try:
            endpoint = os.getenv('AZURE_OPENAI_ENDPOINT')
            api_key = os.getenv('AZURE_OPENAI_API_KEY')
            api_version = os.getenv('AZURE_OPENAI_API_VERSION', '2024-02-15-preview')
            
            if endpoint and api_key:
                return AzureOpenAI(
                    azure_endpoint=endpoint,
                    api_key=api_key,
                    api_version=api_version
                )
            else:
                print("Warning: Azure OpenAI credentials not found. Test generation will be limited.")
                return None
        except Exception as e:
            print(f"Warning: Failed to initialize Azure OpenAI client: {e}")
            return None
    
    def generate_tests(self, input_file: str, output_file: str, 
                      strategy: str = 'variations', count: int = 5,
                      deduplicate: bool = False, append: bool = False) -> bool:
        """Generate expanded tests from seed file"""
        
        try:
            # Read input CSV
            df = pd.read_csv(input_file)
            print(f"Loaded {len(df)} seed tests from {input_file}")
            
            # Generate expanded tests
            expanded_tests = []
            
            for _, row in df.iterrows():
                seed_question = row['Question']
                seed_filename = row.get('Filename', '')
                seed_expected = row.get('ExpectedAnswer', '')
                expansion_strategy = row.get('ExpansionStrategy', strategy)
                expansion_count = row.get('ExpansionCount', count)
                
                print(f"Generating {expansion_count} {expansion_strategy} for: {seed_question[:50]}...")
                
                # Generate variations
                variations = self._generate_variations(
                    seed_question, seed_filename, seed_expected,
                    expansion_strategy, expansion_count
                )
                
                # Add original test
                original_test = {
                    'Question': seed_question,
                    'Filename': seed_filename,
                    'ExpectedAnswer': seed_expected,
                    'ValidationRules': row.get('ValidationRules', '{}'),
                    'ValidationMode': row.get('ValidationMode', 'hybrid'),
                    'GeneratedFrom': 'Original',
                    'GenerationStrategy': 'seed'
                }
                expanded_tests.append(original_test)
                
                # Add generated variations
                expanded_tests.extend(variations)
            
            # Deduplicate if requested
            if deduplicate:
                expanded_tests = self._deduplicate_tests(expanded_tests)
                print(f"After deduplication: {len(expanded_tests)} tests")
            
            # Save results
            if append and os.path.exists(output_file):
                # Append to existing file
                existing_df = pd.read_csv(output_file)
                new_df = pd.DataFrame(expanded_tests)
                combined_df = pd.concat([existing_df, new_df], ignore_index=True)
                combined_df.to_csv(output_file, index=False)
                print(f"Appended {len(expanded_tests)} tests to {output_file}")
            else:
                # Create new file
                output_df = pd.DataFrame(expanded_tests)
                output_df.to_csv(output_file, index=False)
                print(f"Generated {len(expanded_tests)} tests in {output_file}")
            
            return True
            
        except Exception as e:
            print(f"Error generating tests: {e}")
            return False
    
    def _generate_variations(self, seed_question: str, seed_filename: str, 
                           seed_expected: str, strategy: str, count: int) -> List[Dict[str, Any]]:
        """Generate test variations using LLM"""
        
        if not self.llm_client:
            # Fallback to simple rule-based generation
            return self._generate_rule_based_variations(seed_question, seed_filename, seed_expected, strategy, count)
        
        try:
            # Create generation prompt based on strategy
            prompt = self._create_generation_prompt(seed_question, seed_filename, seed_expected, strategy, count)
            
            response = self.llm_client.chat.completions.create(
                model=os.getenv('AZURE_OPENAI_DEPLOYMENT', 'gpt-4'),
                messages=[
                    {"role": "system", "content": "You are an expert test case generator for AI agents. Generate high-quality test variations that maintain the original intent while testing different aspects."},
                    {"role": "user", "content": prompt}
                ],
                temperature=0.7,
                max_tokens=2000
            )
            
            # Parse LLM response
            llm_response = response.choices[0].message.content.strip()
            variations = self._parse_llm_response(llm_response, seed_question, seed_filename, seed_expected, strategy)
            
            return variations[:count]  # Limit to requested count
            
        except Exception as e:
            print(f"Error in LLM generation: {e}")
            # Fallback to rule-based generation
            return self._generate_rule_based_variations(seed_question, seed_filename, seed_expected, strategy, count)
    
    def _create_generation_prompt(self, seed_question: str, seed_filename: str, 
                                seed_expected: str, strategy: str, count: int) -> str:
        """Create prompt for LLM-based test generation"""
        
        if strategy == 'variations':
            return f"""
Generate {count} different ways to ask the same question. Each variation should:
- Maintain the same core intent as the original
- Use different wording and phrasing
- Be natural and conversational
- Test the agent's robustness to different phrasings

Original Question: {seed_question}
Original File: {seed_filename}
Expected Answer: {seed_expected}

Generate {count} variations in this format:
Question: [variation]
Filename: {seed_filename}
ExpectedAnswer: {seed_expected}
ValidationRules: [inferred rules]
ValidationMode: hybrid
GeneratedFrom: {seed_question}
GenerationStrategy: variations
"""
        
        elif strategy == 'edge-cases':
            return f"""
Generate {count} edge case scenarios for this test. Consider:
- Boundary conditions
- Empty or minimal inputs
- Very large inputs
- Special characters or encoding issues
- Unusual file formats or corrupted data

Original Question: {seed_question}
Original File: {seed_filename}
Expected Answer: {seed_expected}

Generate {count} edge cases in this format:
Question: [edge case question]
Filename: [modified filename or empty]
ExpectedAnswer: [expected response for edge case]
ValidationRules: [rules for edge case]
ValidationMode: rule
GeneratedFrom: {seed_question}
GenerationStrategy: edge-cases
"""
        
        elif strategy == 'negative-tests':
            return f"""
Generate {count} negative test scenarios that should fail or produce errors:
- Invalid inputs
- Missing required information
- Contradictory instructions
- Unsupported operations

Original Question: {seed_question}
Original File: {seed_filename}
Expected Answer: {seed_expected}

Generate {count} negative tests in this format:
Question: [negative test question]
Filename: [invalid or missing file]
ExpectedAnswer: [error message or clarification]
ValidationRules: [rules for negative test]
ValidationMode: rule
GeneratedFrom: {seed_question}
GenerationStrategy: negative-tests
"""
        
        else:  # combinations
            return f"""
Generate {count} test combinations that combine multiple operations or parameters:
- Multiple files
- Multiple operations in sequence
- Different input formats
- Complex scenarios

Original Question: {seed_question}
Original File: {seed_filename}
Expected Answer: {seed_expected}

Generate {count} combinations in this format:
Question: [combination question]
Filename: [multiple files or different file]
ExpectedAnswer: [expected response for combination]
ValidationRules: [rules for combination]
ValidationMode: hybrid
GeneratedFrom: {seed_question}
GenerationStrategy: combinations
"""
    
    def _parse_llm_response(self, response: str, seed_question: str, seed_filename: str, 
                           seed_expected: str, strategy: str) -> List[Dict[str, Any]]:
        """Parse LLM response into test variations"""
        
        variations = []
        lines = response.strip().split('\n')
        
        current_variation = {}
        for line in lines:
            line = line.strip()
            if line.startswith('Question:'):
                if current_variation:
                    variations.append(current_variation)
                current_variation = {
                    'Question': line.replace('Question:', '').strip(),
                    'Filename': seed_filename,
                    'ExpectedAnswer': seed_expected,
                    'ValidationRules': '{}',
                    'ValidationMode': 'hybrid',
                    'GeneratedFrom': seed_question,
                    'GenerationStrategy': strategy
                }
            elif line.startswith('Filename:'):
                current_variation['Filename'] = line.replace('Filename:', '').strip()
            elif line.startswith('ExpectedAnswer:'):
                current_variation['ExpectedAnswer'] = line.replace('ExpectedAnswer:', '').strip()
            elif line.startswith('ValidationRules:'):
                current_variation['ValidationRules'] = line.replace('ValidationRules:', '').strip()
            elif line.startswith('ValidationMode:'):
                current_variation['ValidationMode'] = line.replace('ValidationMode:', '').strip()
        
        # Add the last variation
        if current_variation:
            variations.append(current_variation)
        
        return variations
    
    def _generate_rule_based_variations(self, seed_question: str, seed_filename: str, 
                                      seed_expected: str, strategy: str, count: int) -> List[Dict[str, Any]]:
        """Generate variations using simple rule-based patterns"""
        
        variations = []
        
        if strategy == 'variations':
            # Simple paraphrasing patterns
            patterns = [
                seed_question.replace('Create', 'Generate'),
                seed_question.replace('Write', 'Develop'),
                seed_question.replace('Make', 'Build'),
                seed_question.replace('Show', 'Display'),
                seed_question.replace('Tell', 'Explain')
            ]
            
            for i, pattern in enumerate(patterns[:count]):
                if pattern != seed_question:  # Only add if different
                    variations.append({
                        'Question': pattern,
                        'Filename': seed_filename,
                        'ExpectedAnswer': seed_expected,
                        'ValidationRules': '{}',
                        'ValidationMode': 'hybrid',
                        'GeneratedFrom': seed_question,
                        'GenerationStrategy': 'variations'
                    })
        
        elif strategy == 'edge-cases':
            # Generate edge cases
            edge_cases = [
                f"Empty: {seed_question}",
                f"Large: {seed_question}",
                f"Special chars: {seed_question}",
                f"Unicode: {seed_question}",
                f"Minimal: {seed_question}"
            ]
            
            for i, edge_case in enumerate(edge_cases[:count]):
                variations.append({
                    'Question': edge_case,
                    'Filename': '',
                    'ExpectedAnswer': 'Error or edge case handling',
                    'ValidationRules': '{"contains": ["error", "edge"]}',
                    'ValidationMode': 'rule',
                    'GeneratedFrom': seed_question,
                    'GenerationStrategy': 'edge-cases'
                })
        
        return variations
    
    def _deduplicate_tests(self, tests: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
        """Remove duplicate tests based on question similarity"""
        
        # Simple deduplication based on question similarity
        unique_tests = []
        seen_questions = set()
        
        for test in tests:
            question = test['Question'].lower().strip()
            if question not in seen_questions:
                unique_tests.append(test)
                seen_questions.add(question)
        
        return unique_tests
    
    def validate_generated_tests(self, test_file: str) -> bool:
        """Validate generated test file format"""
        
        try:
            df = pd.read_csv(test_file)
            
            # Check required columns
            required_columns = ['Question', 'Filename', 'ExpectedAnswer', 'ValidationRules', 'ValidationMode']
            missing_columns = [col for col in required_columns if col not in df.columns]
            
            if missing_columns:
                print(f"Missing columns: {missing_columns}")
                return False
            
            # Check for empty questions
            empty_questions = df['Question'].isna() | (df['Question'].str.strip() == '')
            if empty_questions.any():
                print(f"Empty questions found at rows: {df[empty_questions].index.tolist()}")
                return False
            
            # Validate JSON in ValidationRules
            for idx, rules in df['ValidationRules'].items():
                if pd.notna(rules) and rules.strip():
                    try:
                        json.loads(rules)
                    except json.JSONDecodeError as e:
                        print(f"Invalid JSON in ValidationRules at row {idx}: {e}")
                        return False
            
            print(f"✓ Generated test file is valid: {len(df)} tests")
            return True
            
        except Exception as e:
            print(f"Error validating test file: {e}")
            return False
    
    def create_interactive_suite(self, suite_name: str, output_dir: Optional[str] = None) -> bool:
        """Create a new test suite interactively"""
        try:
            # Use test_suites_dir if output_dir not provided
            if output_dir is None:
                output_dir = str(self.test_suites_dir)
            
            # Create suite directory
            suite_dir = os.path.join(output_dir, suite_name)
            os.makedirs(suite_dir, exist_ok=True)
            
            # Create CSV file path
            csv_file = os.path.join(suite_dir, f"TestQuestions-{suite_name}.csv")
            
            print(f"\n🎯 Creating new test suite: {suite_name}")
            print(f"📁 Directory: {suite_dir}")
            print(f"📄 CSV file: {csv_file}")
            print("\n" + "="*60)
            
            # Collect tests interactively
            tests = []
            test_count = 0
            
            while True:
                test_count += 1
                print(f"\n📝 Test #{test_count}")
                print("-" * 40)
                
                # Get test details
                question = self._get_user_input("Question: ", required=True, multiline=True)
                filename = self._get_user_input("Filename (optional, press Enter to skip): ", required=False)
                answer = self._get_user_input("Expected Answer: ", required=True, multiline=True)
                
                # Get validation rules
                print("\n🔍 Validation Configuration:")
                validation_mode = self._get_validation_mode()
                validation_rules = self._get_validation_rules(validation_mode)
                
                # Create test record
                test = {
                    'Question': question,
                    'Filename': filename or '',
                    'Answer': answer,
                    'ValidationRules': validation_rules,  # Store as dict, not JSON string
                    'ValidationMode': validation_mode
                }
                
                tests.append(test)
                
                # Ask if user wants to add more tests
                print(f"\n✅ Test #{test_count} added successfully!")
                continue_adding = self._get_user_input("Add another test? (y/n): ", required=True).lower().strip()
                
                if continue_adding not in ['y', 'yes']:
                    break
            
            # Save tests to CSV
            self._save_tests_to_csv(tests, csv_file)
            
            # Update test_suites.json
            self._update_test_suites_config(suite_name, csv_file)
            
            print(f"\n🎉 Test suite '{suite_name}' created successfully!")
            print(f"📊 Total tests: {len(tests)}")
            print(f"📄 CSV file: {csv_file}")
            print(f"🔧 Configuration updated in test_suites.json")
            
            return True
            
        except Exception as e:
            print(f"❌ Error creating test suite: {e}")
            return False
    
    def add_to_existing_suite(self, suite_name: str) -> bool:
        """Add tests to an existing test suite interactively"""
        try:
            # Load test suite configuration
            config_file = self.test_suites_dir / "test_suites.json"
            if not config_file.exists():
                print(f"❌ Error: {config_file} not found")
                return False
            
            with open(config_file, 'r') as f:
                config = json.load(f)
            
            if suite_name not in config:
                print(f"❌ Error: Test suite '{suite_name}' not found in configuration")
                print(f"Available suites: {', '.join(config.keys())}")
                return False
            
            suite_config = config[suite_name]
            csv_file_path = suite_config['csv_file']
            # Resolve CSV path relative to test_suites_dir
            csv_file = self.test_suites_dir / csv_file_path
            
            if not csv_file.exists():
                print(f"❌ Error: CSV file not found: {csv_file}")
                return False
            
            csv_file = str(csv_file)
            
            print(f"\n🎯 Adding tests to existing suite: {suite_name}")
            print(f"📁 CSV file: {csv_file}")
            print(f"📝 Description: {suite_config.get('description', 'No description')}")
            print(f"⚠️  This will APPEND new tests to the existing test suite")
            print(f"📊 Current tests will be preserved and new tests will be added")
            print("\n" + "="*60)
            
            # Load existing tests
            existing_tests = self._load_existing_tests(csv_file)
            print(f"📊 Found {len(existing_tests)} existing tests")
            
            # Ask for confirmation to proceed
            print(f"\n❓ Do you want to continue adding tests to '{suite_name}'?")
            confirm = self._get_user_input("This will append new tests to the existing suite (y/n): ", required=True).lower().strip()
            
            if confirm not in ['y', 'yes']:
                print("❌ Operation cancelled by user")
                return False
            
            # Collect new tests interactively
            new_tests = []
            test_count = 0
            
            while True:
                test_count += 1
                print(f"\n📝 New Test #{test_count}")
                print("-" * 40)
                
                # Get test details
                question = self._get_user_input("Question: ", required=True, multiline=True)
                filename = self._get_user_input("Filename (optional, press Enter to skip): ", required=False)
                answer = self._get_user_input("Expected Answer: ", required=True, multiline=True)
                
                # Get validation rules
                print("\n🔍 Validation Configuration:")
                validation_mode = self._get_validation_mode()
                validation_rules = self._get_validation_rules(validation_mode)
                
                # Create test record
                test = {
                    'Question': question,
                    'Filename': filename or '',
                    'Answer': answer,
                    'ValidationRules': validation_rules,
                    'ValidationMode': validation_mode
                }
                
                new_tests.append(test)
                
                # Ask if user wants to add more tests
                print(f"\n✅ Test #{test_count} added successfully!")
                continue_adding = self._get_user_input("Add another test? (y/n): ", required=True).lower().strip()
                
                if continue_adding not in ['y', 'yes']:
                    break
            
            # Combine existing and new tests
            all_tests = existing_tests + new_tests
            
            # Save all tests to CSV
            self._save_tests_to_csv(all_tests, csv_file)
            
            print(f"\n🎉 Successfully added {len(new_tests)} tests to '{suite_name}'!")
            print(f"📊 Total tests in suite: {len(all_tests)}")
            print(f"📄 Updated CSV file: {csv_file}")
            
            return True
            
        except Exception as e:
            print(f"❌ Error adding tests to suite: {e}")
            return False
    
    def _load_existing_tests(self, csv_file: str) -> List[Dict[str, Any]]:
        """Load existing tests from CSV file"""
        try:
            import pandas as pd
            df = pd.read_csv(csv_file)
            
            tests = []
            for _, row in df.iterrows():
                # Parse validation rules JSON
                try:
                    validation_rules = json.loads(row['ValidationRules']) if pd.notna(row['ValidationRules']) else {}
                except (json.JSONDecodeError, TypeError):
                    validation_rules = {}
                
                test = {
                    'Question': row['Question'],
                    'Filename': row.get('Filename', ''),
                    'Answer': row['Answer'],
                    'ValidationRules': validation_rules,
                    'ValidationMode': row.get('ValidationMode', 'hybrid')
                }
                tests.append(test)
            
            return tests
            
        except Exception as e:
            print(f"⚠️ Warning: Could not load existing tests: {e}")
            return []
    
    def suite_exists(self, suite_name: str) -> bool:
        """Check if a test suite already exists"""
        try:
            # Load test suites configuration
            config_file = self.test_suites_dir / "test_suites.json"
            with open(config_file, 'r') as f:
                suites = json.load(f)
            
            return suite_name in suites
        except (FileNotFoundError, json.JSONDecodeError):
            return False
    
    def _get_user_input(self, prompt: str, required: bool = True, multiline: bool = False) -> str:
        """Get user input with validation"""
        while True:
            try:
                if multiline:
                    print(f"{prompt}")
                    print("💡 Tip: Paste multi-line content and press Ctrl+Z (Windows) or Ctrl+D (Unix) when done")
                    lines = []
                    while True:
                        try:
                            line = input()
                            lines.append(line)
                        except EOFError:
                            break
                    value = '\n'.join(lines).strip()
                else:
                    value = input(prompt).strip()
                
                if required and not value:
                    print("❌ This field is required. Please enter a value.")
                    continue
                return value
            except KeyboardInterrupt:
                print("\n\n⚠️ Operation cancelled by user")
                sys.exit(0)
            except EOFError:
                print("\n\n⚠️ End of input reached")
                sys.exit(0)
    
    def _get_validation_rules(self, validation_mode: str) -> Dict[str, Any]:
        """Get validation rules interactively"""
        print("\n📋 Validation Rules (press Enter to skip each rule):")
        
        rules = {}
        
        # Add LLM validation parameter based on mode
        if validation_mode in ['llm', 'hybrid']:
            rules['llm_validation'] = True
        
        # Skip rule-based validation rules for LLM mode
        if validation_mode != 'llm':
            # Contains rule
            contains = self._get_user_input("  Contains (comma-separated list): ", required=False)
            if contains:
                rules['contains'] = [item.strip() for item in contains.split(',') if item.strip()]
            
            # Excludes rule
            excludes = self._get_user_input("  Excludes (comma-separated list): ", required=False)
            if excludes:
                rules['excludes'] = [item.strip() for item in excludes.split(',') if item.strip()]
            
            # Regex rule
            regex = self._get_user_input("  Regex pattern: ", required=False)
            if regex:
                rules['regex'] = regex
            
            # Length rules
            min_length = self._get_user_input("  Minimum length: ", required=False)
            if min_length and min_length.isdigit():
                rules['min_length'] = int(min_length)
            
            max_length = self._get_user_input("  Maximum length: ", required=False)
            if max_length and max_length.isdigit():
                rules['max_length'] = int(max_length)
        
        # Artifact rules
        artifact_count = self._get_user_input("  Expected artifact count: ", required=False)
        if artifact_count and artifact_count.isdigit():
            rules['artifact_count'] = int(artifact_count)
        
        artifact_types = self._get_user_input("  Expected artifact types (WorkflowExecution, ToolExecution, File, etc.): ", required=False)
        if artifact_types:
            rules['artifact_types'] = [item.strip() for item in artifact_types.split(',') if item.strip()]
        
        # Numeric rules
        numeric_value = self._get_user_input("  Expected numeric value: ", required=False)
        if numeric_value:
            try:
                rules['numeric_value'] = float(numeric_value)
                tolerance = self._get_user_input("  Numeric tolerance (default 0): ", required=False)
                if tolerance:
                    rules['numeric_tolerance'] = float(tolerance)
                else:
                    rules['numeric_tolerance'] = 0
            except ValueError:
                print("⚠️ Invalid numeric value, skipping...")
        
        # Code execution rule
        code_success = self._get_user_input("  Require code execution success? (y/n): ", required=False)
        if code_success and code_success.lower() in ['y', 'yes']:
            rules['code_success'] = True
        
        return rules
    
    def _get_validation_mode(self) -> str:
        """Get validation mode"""
        print("\n🔍 Validation Mode:")
        print("  1. rule - Rule-based validation only")
        print("  2. llm - LLM-based validation only")
        print("  3. hybrid - Both rule-based and LLM validation")
        
        while True:
            choice = self._get_user_input("Choose validation mode (1-3, default: 3): ", required=False)
            if not choice:
                return 'hybrid'
            if choice == '1':
                return 'rule'
            elif choice == '2':
                return 'llm'
            elif choice == '3':
                return 'hybrid'
            else:
                print("❌ Invalid choice. Please enter 1, 2, or 3.")
    
    def _save_tests_to_csv(self, tests: List[Dict[str, Any]], csv_file: str):
        """Save tests to CSV with proper escaping"""
        try:
            with open(csv_file, 'w', newline='', encoding='utf-8') as f:
                writer = csv.writer(f)
                
                # Write header
                writer.writerow(['Question', 'Filename', 'Answer', 'ValidationRules', 'ValidationMode'])
                
                # Write tests
                for test in tests:
                    # Handle validation rules - they should be a dict, not a JSON string
                    if isinstance(test['ValidationRules'], dict):
                        validation_rules_json = json.dumps(test['ValidationRules'])
                    else:
                        # If it's already a string, use it as-is
                        validation_rules_json = test['ValidationRules']
                    
                    writer.writerow([
                        test['Question'],
                        test['Filename'],
                        test['Answer'],
                        validation_rules_json,
                        test['ValidationMode']
                    ])
            
            print(f"💾 Tests saved to: {csv_file}")
            
        except Exception as e:
            print(f"❌ Error saving CSV file: {e}")
            raise
    
    def _update_test_suites_config(self, suite_name: str, csv_file: str):
        """Update test_suites.json with new suite"""
        try:
            config_file = self.test_suites_dir / "test_suites.json"
            
            # Load existing config
            if config_file.exists():
                with open(config_file, 'r') as f:
                    config = json.load(f)
            else:
                config = {}
            
            # Convert absolute csv_file path to relative path from test_suites_dir
            csv_path = Path(csv_file)
            if csv_path.is_absolute():
                try:
                    csv_file_rel = str(csv_path.relative_to(self.test_suites_dir))
                except ValueError:
                    # If not relative to test_suites_dir, use as-is
                    csv_file_rel = csv_file
            else:
                csv_file_rel = csv_file
            
            # Add new suite
            config[suite_name] = {
                "csv_file": csv_file_rel,
                "description": f"Tests for {suite_name}",
                "quick_mode_limit": 5
            }
            
            # Save updated config (sort keys alphabetically)
            sorted_config = dict(sorted(config.items()))
            # Ensure parent directory exists
            config_file.parent.mkdir(parents=True, exist_ok=True)
            with open(config_file, 'w') as f:
                json.dump(sorted_config, f, indent=2)
            
            print(f"🔧 Updated {config_file} with new suite: {suite_name}")
            
        except Exception as e:
            print(f"⚠️ Warning: Could not update test_suites.json: {e}")
            print(f"   You may need to manually add the suite to the configuration.")


def main():
    parser = argparse.ArgumentParser(
        description="Generate expanded test cases from seed prompts",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  # Interactive mode - create new test suite
  python generate_tests.py --interactive --suite-name my-custom-tests
  
  # Interactive mode - add tests to existing suite
  python generate_tests.py --interactive --existing-suite code-interpreter
  
  # Generate variations from seed tests
  python generate_tests.py --input seed-tests.csv --output expanded-tests.csv --strategy variations --count 5
  
  # Generate edge cases
  python generate_tests.py --input seed-tests.csv --output edge-cases.csv --strategy edge-cases --count 3
  
  # Append to existing test suite
  python generate_tests.py --input seed.csv --output test-suites/code-interpreter/TestQuestions-code-interpreter.csv --append
  
  # Generate with deduplication
  python generate_tests.py --input seed.csv --output expanded.csv --deduplicate
        """
    )
    
    parser.add_argument('--input',
                       help='Input CSV file with seed tests')
    parser.add_argument('--output',
                       help='Output CSV file for generated tests')
    
    # Interactive mode
    parser.add_argument('--interactive', action='store_true',
                       help='Create a new test suite interactively')
    parser.add_argument('--suite-name',
                       help='Name for the new test suite (required with --interactive)')
    parser.add_argument('--existing-suite',
                       help='Add tests to an existing test suite (interactive mode)')
    parser.add_argument('--strategy', choices=['variations', 'edge-cases', 'negative-tests', 'combinations'], 
                       default='variations',
                       help='Generation strategy (default: variations)')
    parser.add_argument('--count', type=int, default=5,
                       help='Number of variations to generate per seed (default: 5)')
    parser.add_argument('--deduplicate', action='store_true',
                       help='Remove duplicate tests')
    parser.add_argument('--append', action='store_true',
                       help='Append to existing output file')
    parser.add_argument('--validate', action='store_true',
                       help='Validate generated tests')
    parser.add_argument('--dry-run', action='store_true',
                       help='Show what would be generated without creating files')
    parser.add_argument('--test-suites-dir',
                       help='Path to test-suites directory (default: test-suites/ relative to script)')
    
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
    
    # Handle interactive mode
    if args.interactive:
        if not args.suite_name and not args.existing_suite:
            print("Error: Must specify either --suite-name (for new suite) or --existing-suite (to add to existing suite)")
            sys.exit(1)
        
        if args.suite_name and args.existing_suite:
            print("Error: Cannot specify both --suite-name and --existing-suite")
            sys.exit(1)
        
        # Initialize generator
        generator = TestGenerator(test_suites_dir=test_suites_dir)
        
        if args.existing_suite:
            # Add tests to existing suite
            success = generator.add_to_existing_suite(args.existing_suite)
        else:
            # Check if suite already exists when creating new one
            if generator.suite_exists(args.suite_name):
                csv_path = generator.test_suites_dir / args.suite_name / f"TestQuestions-{args.suite_name}.csv"
                print(f"\n⚠️  Test suite '{args.suite_name}' already exists!")
                print(f"📁 CSV file: {csv_path}")
                print(f"📊 This suite already has tests in it.")
                print(f"\n❓ What would you like to do?")
                print(f"  1. Append new tests to existing suite '{args.suite_name}'")
                print(f"  2. Create a new suite with a different name")
                print(f"  3. Cancel")
                
                choice = generator._get_user_input("Choose an option (1-3): ", required=True).strip()
                
                if choice == "1":
                    # Switch to append mode
                    success = generator.add_to_existing_suite(args.suite_name)
                    if success:
                        print(f"\n🎉 Successfully added tests to existing suite '{args.suite_name}'!")
                    else:
                        print(f"\n❌ Failed to add tests to suite '{args.suite_name}'")
                        sys.exit(1)
                elif choice == "2":
                    # Get new suite name
                    new_name = generator._get_user_input("Enter new suite name: ", required=True).strip()
                    if generator.suite_exists(new_name):
                        print(f"❌ Suite '{new_name}' also exists! Please choose a different name.")
                        sys.exit(1)
                    success = generator.create_interactive_suite(new_name)
                    if success:
                        print(f"\n🎉 Test suite '{new_name}' created successfully!")
                    else:
                        print(f"\n❌ Failed to create test suite '{new_name}'")
                        sys.exit(1)
                else:
                    print("❌ Operation cancelled by user")
                    sys.exit(0)
            else:
                # Suite doesn't exist, create new one
                success = generator.create_interactive_suite(args.suite_name)
                if success:
                    print(f"\n🎉 Test suite '{args.suite_name}' created successfully!")
                else:
                    print(f"\n❌ Failed to create test suite '{args.suite_name}'")
                    sys.exit(1)
        
        
        return
    
    # Validate arguments for non-interactive mode
    if not args.input:
        print("Error: --input is required for non-interactive mode")
        sys.exit(1)
    
    if not args.output:
        print("Error: --output is required for non-interactive mode")
        sys.exit(1)
    
    # Validate input file
    if not os.path.exists(args.input):
        print(f"Error: Input file not found: {args.input}")
        sys.exit(1)
    
    # Initialize generator
    generator = TestGenerator(test_suites_dir=test_suites_dir)
    
    if args.dry_run:
        print("Dry run mode - would generate tests with:")
        print(f"  Input: {args.input}")
        print(f"  Output: {args.output}")
        print(f"  Strategy: {args.strategy}")
        print(f"  Count: {args.count}")
        print(f"  Deduplicate: {args.deduplicate}")
        print(f"  Append: {args.append}")
        return
    
    # Generate tests
    success = generator.generate_tests(
        input_file=args.input,
        output_file=args.output,
        strategy=args.strategy,
        count=args.count,
        deduplicate=args.deduplicate,
        append=args.append
    )
    
    if not success:
        print("Test generation failed")
        sys.exit(1)
    
    # Validate if requested
    if args.validate:
        if generator.validate_generated_tests(args.output):
            print("✓ Generated tests are valid")
        else:
            print("✗ Generated tests have validation errors")
            sys.exit(1)
    
    print("✅ Test generation completed successfully")


if __name__ == "__main__":
    main()

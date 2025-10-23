"""
Test Validator for FoundationaLLM Agent Tests

Provides rule-based, LLM-based, and hybrid validation of test results.
"""

import json
import re
import os
from typing import Dict, List, Any, Optional, Tuple
import pandas as pd
from openai import AzureOpenAI


class TestValidator:
    """Validates test results using multiple strategies"""
    
    def __init__(self):
        self.llm_client = None
        self._init_llm_client()
    
    def _init_llm_client(self):
        """Initialize Azure OpenAI client for LLM validation"""
        try:
            endpoint = os.getenv('AZURE_OPENAI_ENDPOINT')
            api_key = os.getenv('AZURE_OPENAI_API_KEY')
            api_version = os.getenv('AZURE_OPENAI_API_VERSION', '2024-02-15-preview')
            
            if endpoint and api_key:
                self.llm_client = AzureOpenAI(
                    azure_endpoint=endpoint,
                    api_key=api_key,
                    api_version=api_version
                )
            else:
                print("Warning: Azure OpenAI credentials not found. LLM validation will be disabled.")
        except Exception as e:
            print(f"Warning: Failed to initialize Azure OpenAI client: {e}")
            self.llm_client = None
    
    def validate_results(self, results: Dict[str, Any], mode: str = 'hybrid', 
                        verbose: bool = False) -> Dict[str, Any]:
        """Validate test results using specified mode"""
        
        if mode not in ['rule', 'llm', 'hybrid']:
            raise ValueError(f"Invalid validation mode: {mode}")
        
        test_results = results.get('results', [])
        validation_results = {
            'total_tests': len(test_results),
            'passed_tests': 0,
            'failed_tests': 0,
            'validation_mode': mode,
            'failed_tests_list': [],
            'validation_details': []
        }
        
        for i, test_result in enumerate(test_results):
            validation_result = self._validate_single_test(test_result, mode, verbose)
            validation_results['validation_details'].append(validation_result)
            
            if validation_result['passed']:
                validation_results['passed_tests'] += 1
            else:
                validation_results['failed_tests'] += 1
                validation_results['failed_tests_list'].append({
                    'index': i,
                    'question': test_result.get('Question', ''),
                    'reason': validation_result['reason']
                })
        
        return validation_results
    
    def _validate_single_test(self, test_result: Dict[str, Any], 
                             mode: str, verbose: bool) -> Dict[str, Any]:
        """Validate a single test result"""
        
        validation_result = {
            'passed': False,
            'score': 0,
            'reason': '',
            'details': {}
        }
        
        # Extract test data
        question = test_result.get('Question', '')
        agent_answer = test_result.get('AgentAnswer', '')
        expected_answer = test_result.get('ExpectedAnswer', '')
        validation_rules = test_result.get('ValidationRules', '{}')
        validation_mode = test_result.get('ValidationMode', 'hybrid')
        error_occurred = test_result.get('ErrorOccured', 1)
        code_tool_failed = test_result.get('CodeToolFailed', False)
        produced_files_count = test_result.get('ProducedFilesCount', 0)
        
        # Check for execution errors first
        if error_occurred == 1:
            validation_result['reason'] = 'Test execution failed'
            return validation_result
        
        # Parse validation rules
        try:
            rules = json.loads(validation_rules) if validation_rules else {}
        except json.JSONDecodeError:
            rules = {}
        
        # Determine validation strategy
        if mode == 'rule' or (mode == 'hybrid' and validation_mode in ['rule', 'hybrid']):
            rule_result = self._validate_with_rules(
                agent_answer, expected_answer, rules, test_result
            )
            validation_result.update(rule_result)
            
            # If rule validation passes and we're in hybrid mode, also do LLM validation
            if mode == 'hybrid' and rule_result['passed'] and validation_mode in ['llm', 'hybrid']:
                llm_result = self._validate_with_llm(question, agent_answer, expected_answer)
                if llm_result['score'] < 70:  # Threshold for LLM validation
                    validation_result['passed'] = False
                    validation_result['reason'] = f"LLM validation failed: {llm_result['reason']}"
                    validation_result['score'] = llm_result['score']
        
        elif mode == 'llm' or validation_mode == 'llm':
            llm_result = self._validate_with_llm(question, agent_answer, expected_answer)
            validation_result.update(llm_result)
        
        return validation_result
    
    def _validate_with_rules(self, agent_answer: str, expected_answer: str, 
                           rules: Dict[str, Any], test_result: Dict[str, Any]) -> Dict[str, Any]:
        """Validate using rule-based checks"""
        
        result = {
            'passed': True,
            'score': 100,
            'reason': '',
            'details': {}
        }
        
        # Check contains rules
        if 'contains' in rules:
            contains_list = rules['contains']
            if isinstance(contains_list, list):
                for required_text in contains_list:
                    if required_text.lower() not in agent_answer.lower():
                        result['passed'] = False
                        result['reason'] = f"Missing required text: '{required_text}'"
                        result['score'] = 0
                        return result
            else:
                result['passed'] = False
                result['reason'] = f"Invalid contains rule: expected list, got {type(contains_list)}"
                result['score'] = 0
                return result
        
        # Check excludes rules
        if 'excludes' in rules:
            excludes_list = rules['excludes']
            if isinstance(excludes_list, list):
                for forbidden_text in excludes_list:
                    if forbidden_text.lower() in agent_answer.lower():
                        result['passed'] = False
                        result['reason'] = f"Contains forbidden text: '{forbidden_text}'"
                        result['score'] = 0
                        return result
            else:
                result['passed'] = False
                result['reason'] = f"Invalid excludes rule: expected list, got {type(excludes_list)}"
                result['score'] = 0
                return result
        
        # Check regex rules
        if 'regex' in rules:
            pattern = rules['regex']
            if not re.search(pattern, agent_answer):
                result['passed'] = False
                result['reason'] = f"Does not match regex pattern: '{pattern}'"
                result['score'] = 0
                return result
        
        # Check length rules
        if 'min_length' in rules:
            min_len = rules['min_length']
            if len(agent_answer) < min_len:
                result['passed'] = False
                result['reason'] = f"Answer too short: {len(agent_answer)} < {min_len}"
                result['score'] = 0
                return result
        
        if 'max_length' in rules:
            max_len = rules['max_length']
            if len(agent_answer) > max_len:
                result['passed'] = False
                result['reason'] = f"Answer too long: {len(agent_answer)} > {max_len}"
                result['score'] = 0
                return result
        
        # Check artifact rules
        if 'artifact_count' in rules:
            expected_count = rules['artifact_count']
            # Count actual artifacts from ArtifactsSummary
            try:
                artifacts_summary = test_result.get('ArtifactsSummary', '[]')
                import json
                artifacts = json.loads(artifacts_summary) if isinstance(artifacts_summary, str) else artifacts_summary
                actual_count = len(artifacts) if isinstance(artifacts, list) else 0
            except (json.JSONDecodeError, TypeError):
                actual_count = 0
            
            if actual_count != expected_count:
                result['passed'] = False
                result['reason'] = f"Wrong artifact count: {actual_count} != {expected_count}"
                result['score'] = 0
                return result
        
        if 'artifact_types' in rules:
            expected_types = rules['artifact_types']
            # This would need to be implemented based on artifact metadata
            pass
        
        # Check code execution success
        if 'code_success' in rules and rules['code_success']:
            if test_result.get('CodeToolFailed', False):
                result['passed'] = False
                result['reason'] = "Code execution failed"
                result['score'] = 0
                return result
        
        # Check numeric validation
        if 'numeric_value' in rules:
            expected_value = rules['numeric_value']
            tolerance = rules.get('numeric_tolerance', 0)
            
            # Try to extract numeric value from answer
            numeric_match = re.search(r'-?\d+(?:\.\d+)?', agent_answer)
            if numeric_match:
                actual_value = float(numeric_match.group())
                if abs(actual_value - expected_value) > tolerance:
                    result['passed'] = False
                    result['reason'] = f"Numeric value mismatch: {actual_value} != {expected_value} (±{tolerance})"
                    result['score'] = 0
                    return result
            else:
                result['passed'] = False
                result['reason'] = "No numeric value found in answer"
                result['score'] = 0
                return result
        
        return result
    
    def _validate_with_llm(self, question: str, agent_answer: str, 
                          expected_answer: str) -> Dict[str, Any]:
        """Validate using LLM-based semantic analysis"""
        
        if not self.llm_client:
            return {
                'passed': False,
                'score': 0,
                'reason': 'LLM validation not available',
                'details': {}
            }
        
        try:
            # Create validation prompt
            prompt = f"""
You are evaluating an AI agent's response to a user question. Please assess the quality and correctness of the response.

Question: {question}

Expected Answer: {expected_answer}

Agent's Answer: {agent_answer}

Please evaluate the agent's answer on the following criteria:
1. Correctness: Does the answer correctly address the question?
2. Completeness: Is the answer complete and comprehensive?
3. Relevance: Is the answer relevant to the question asked?
4. Clarity: Is the answer clear and well-structured?

Provide a score from 0-100 and a brief explanation of your assessment.
Format your response as JSON:
{{"score": <number>, "reason": "<explanation>", "correctness": <0-100>, "completeness": <0-100>, "relevance": <0-100>, "clarity": <0-100>}}
"""
            
            response = self.llm_client.chat.completions.create(
                model=os.getenv('AZURE_OPENAI_DEPLOYMENT', 'gpt-4'),
                messages=[
                    {"role": "system", "content": "You are an expert evaluator of AI agent responses. Provide objective, detailed assessments."},
                    {"role": "user", "content": prompt}
                ],
                temperature=0.1,
                max_tokens=500
            )
            
            # Parse LLM response
            llm_response = response.choices[0].message.content.strip()
            
            # Try to extract JSON from response
            json_match = re.search(r'\{.*\}', llm_response, re.DOTALL)
            if json_match:
                evaluation = json.loads(json_match.group())
                score = evaluation.get('score', 0)
                
                return {
                    'passed': score >= 70,  # Threshold for passing
                    'score': score,
                    'reason': evaluation.get('reason', 'LLM evaluation completed'),
                    'details': evaluation
                }
            else:
                # Fallback if JSON parsing fails
                return {
                    'passed': False,
                    'score': 0,
                    'reason': 'Failed to parse LLM evaluation response',
                    'details': {'raw_response': llm_response}
                }
                
        except Exception as e:
            return {
                'passed': False,
                'score': 0,
                'reason': f'LLM validation error: {str(e)}',
                'details': {}
            }
    
    def validate_csv_format(self, csv_path: str) -> Tuple[bool, List[str]]:
        """Validate CSV file format for test questions"""
        errors = []
        
        try:
            df = pd.read_csv(csv_path)
            
            # Check required columns
            required_columns = ['Question']
            missing_columns = [col for col in required_columns if col not in df.columns]
            if missing_columns:
                errors.append(f"Missing required columns: {missing_columns}")
            
            # Check for empty questions
            if 'Question' in df.columns:
                empty_questions = df['Question'].isna() | (df['Question'].str.strip() == '')
                if empty_questions.any():
                    empty_indices = df[empty_questions].index.tolist()
                    errors.append(f"Empty questions found at rows: {empty_indices}")
            
            # Validate ValidationRules JSON
            if 'ValidationRules' in df.columns:
                for idx, rules in df['ValidationRules'].items():
                    if pd.notna(rules) and rules.strip():
                        try:
                            json.loads(rules)
                        except json.JSONDecodeError as e:
                            errors.append(f"Invalid JSON in ValidationRules at row {idx}: {e}")
            
            # Validate ValidationMode values
            if 'ValidationMode' in df.columns:
                valid_modes = ['rule', 'llm', 'hybrid']
                invalid_modes = df[~df['ValidationMode'].isin(valid_modes + ['', pd.NA])]
                if not invalid_modes.empty:
                    invalid_indices = invalid_modes.index.tolist()
                    errors.append(f"Invalid ValidationMode values at rows {invalid_indices}. Must be one of: {valid_modes}")
            
            return len(errors) == 0, errors
            
        except Exception as e:
            errors.append(f"Error reading CSV file: {e}")
            return False, errors

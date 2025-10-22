"""
HTML Report Generator for FoundationaLLM Agent Tests

Generates comprehensive HTML reports with visualizations and detailed test results.
"""

import json
import os
from datetime import datetime
from typing import Dict, List, Any
import pandas as pd


class HTMLReporter:
    """Generates HTML reports for test results"""
    
    def __init__(self):
        self.template_dir = os.path.dirname(__file__)
    
    def generate_report(self, results: Dict[str, Any], output_path: str, timestamp: str):
        """Generate HTML report from test results"""
        
        # Process results data
        report_data = self._process_results(results)
        
        # Generate HTML content
        html_content = self._generate_html(report_data, timestamp)
        
        # Write to file
        with open(output_path, 'w', encoding='utf-8') as f:
            f.write(html_content)
        
        print(f"📊 HTML report generated: {output_path}")
    
    def _process_results(self, results: Dict[str, Any]) -> Dict[str, Any]:
        """Process raw results into report data"""
        
        report_data = {
            'timestamp': datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
            'agents': {},
            'summary': {
                'total_agents': 0,
                'total_tests': 0,
                'total_passed': 0,
                'total_failed': 0,
                'overall_pass_rate': 0,
                'total_duration': 0
            }
        }
        
        for agent_name, agent_results in results.items():
            if isinstance(agent_results, dict) and 'results' in agent_results:
                # Single agent results
                agent_data = self._process_agent_results(agent_name, agent_results)
                report_data['agents'][agent_name] = agent_data
                
                # Update summary
                report_data['summary']['total_agents'] += 1
                report_data['summary']['total_tests'] += agent_data['total_tests']
                report_data['summary']['total_passed'] += agent_data['passed_tests']
                report_data['summary']['total_failed'] += agent_data['failed_tests']
                report_data['summary']['total_duration'] += agent_data['total_duration']
            else:
                # Multi-suite results
                for suite_name, suite_results in agent_results.items():
                    if isinstance(suite_results, dict) and 'results' in suite_results:
                        suite_data = self._process_agent_results(f"{agent_name}-{suite_name}", suite_results)
                        report_data['agents'][f"{agent_name}-{suite_name}"] = suite_data
                        
                        # Update summary
                        report_data['summary']['total_agents'] += 1
                        report_data['summary']['total_tests'] += suite_data['total_tests']
                        report_data['summary']['total_passed'] += suite_data['passed_tests']
                        report_data['summary']['total_failed'] += suite_data['failed_tests']
                        report_data['summary']['total_duration'] += suite_data['total_duration']
        
        # Calculate overall pass rate
        if report_data['summary']['total_tests'] > 0:
            report_data['summary']['overall_pass_rate'] = (
                report_data['summary']['total_passed'] / report_data['summary']['total_tests'] * 100
            )
        
        # Set suite name from the first agent's results
        if report_data['agents']:
            first_agent = list(report_data['agents'].values())[0]
            report_data['suite_name'] = first_agent.get('suite_name', 'unknown')
        else:
            report_data['suite_name'] = 'unknown'
        
        return report_data
    
    def _parse_artifacts(self, artifacts_summary: str) -> List[Dict[str, Any]]:
        """Parse all artifacts from ArtifactsSummary JSON string"""
        try:
            import json
            artifacts = json.loads(artifacts_summary)
            return artifacts if isinstance(artifacts, list) else []
        except (json.JSONDecodeError, TypeError):
            return []
    
    def _parse_expected_artifacts(self, answer: str, validation_rules: str) -> Dict[str, Any]:
        """Parse expected artifacts from validation rules"""
        try:
            import json
            rules = json.loads(validation_rules)
            return {
                'count': rules.get('artifact_count', 0),
                'types': rules.get('artifact_types', []),
                'description': f"Expected {rules.get('artifact_count', 0)} artifacts of types: {', '.join(rules.get('artifact_types', []))}"
            }
        except (json.JSONDecodeError, TypeError):
            return {
                'count': 0,
                'types': [],
                'description': 'No artifact expectations defined'
            }
    
    def _process_agent_results(self, agent_name: str, results: Dict[str, Any]) -> Dict[str, Any]:
        """Process results for a single agent"""
        
        test_results = results.get('results', [])
        total_tests = len(test_results)
        
        # Count passed/failed tests
        passed_tests = 0
        failed_tests = 0
        error_tests = 0
        total_duration = 0
        
        # Analyze test results
        test_analysis = []
        for i, test in enumerate(test_results):
            error_occurred = test.get('ErrorOccured', 1)
            validation_passed = test.get('ValidationPassed', -1)
            
            if error_occurred == 1:
                error_tests += 1
                status = 'error'
            elif validation_passed == 1:
                passed_tests += 1
                status = 'passed'
            elif validation_passed == 0:
                failed_tests += 1
                status = 'failed'
            else:
                # No validation performed
                if error_occurred == 0:
                    passed_tests += 1
                    status = 'passed'
                else:
                    failed_tests += 1
                    status = 'failed'
            
            # Parse artifact information
            artifacts_produced = self._parse_artifacts(test.get('ArtifactsSummary', '[]'))
            artifacts_expected = self._parse_expected_artifacts(test.get('Answer', ''), test.get('ValidationRules', '{}'))
            
            # Calculate duration
            test_duration = test.get('CompletionRequestDuration', 0)
            total_duration += test_duration
            
            test_analysis.append({
                'index': f"{agent_name}-{i}",
                'question': test.get('Question', ''),
                'status': status,
                'agent_answer': test.get('AgentAnswer', ''),
                'expected_answer': test.get('Answer', ''),
                'validation_details': test.get('ValidationDetails', ''),
                'validation_score': test.get('ValidationScore', 0),
                'validation_mode': test.get('ValidationMode', ''),
                'tokens': test.get('Tokens', 0),
                'duration': test.get('CompletionRequestDuration', 0),
                'artifacts_count': len(artifacts_produced),
                'artifacts_produced': artifacts_produced,
                'artifacts_expected': artifacts_expected,
                'code_failed': test.get('CodeToolFailed', False)
            })
        
        return {
            'agent_name': agent_name,
            'total_tests': total_tests,
            'passed_tests': passed_tests,
            'failed_tests': failed_tests,
            'error_tests': error_tests,
            'pass_rate': (passed_tests / total_tests * 100) if total_tests > 0 else 0,
            'total_duration': total_duration,
            'test_analysis': test_analysis,
            'suite_name': results.get('suite_name', 'unknown'),
            'timestamp': results.get('timestamp', '')
        }
    
    def _generate_html(self, report_data: Dict[str, Any], timestamp: str) -> str:
        """Generate HTML content"""
        
        return f"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>FoundationaLLM Test Report - {timestamp}</title>
    <style>
        {self._get_css_styles()}
    </style>
</head>
<body>
    <div class="container">
        <header class="header">
            <h1>🧪 FoundationaLLM Agent Test Report</h1>
            <div class="timestamp">Generated: {report_data['timestamp']}</div>
        </header>
        
        {self._generate_summary_section(report_data)}
        {self._generate_agents_section(report_data)}
        {self._generate_footer()}
    </div>
    
    <script>
{self._get_javascript()}
    </script>
</body>
</html>
        """
    
    def _get_css_styles(self) -> str:
        """Get CSS styles for the report"""
        return """
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            line-height: 1.6;
            color: #333;
            background-color: #f5f5f5;
        }
        
        .container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 20px;
        }
        
        .header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            border-radius: 10px;
            margin-bottom: 30px;
            text-align: center;
        }
        
        .header h1 {
            font-size: 2.5em;
            margin-bottom: 10px;
        }
        
        .timestamp {
            opacity: 0.9;
            font-size: 1.1em;
        }
        
        .summary {
            background: white;
            padding: 30px;
            border-radius: 10px;
            margin-bottom: 30px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        
        .summary h2 {
            color: #667eea;
            margin-bottom: 20px;
            font-size: 1.8em;
        }
        
        .suite-info {
            margin-bottom: 25px;
            padding: 15px;
            background: #f8f9fa;
            border-radius: 8px;
            border-left: 4px solid #667eea;
        }
        
        .suite-info h3 {
            color: #495057;
            margin: 0;
            font-size: 1.2em;
            font-weight: 600;
        }
        
        .stats-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 20px;
            margin-bottom: 30px;
        }
        
        .stat-card {
            background: #f8f9fa;
            padding: 20px;
            border-radius: 8px;
            text-align: center;
            border-left: 4px solid #667eea;
        }
        
        .stat-card.passed {
            border-left-color: #28a745;
        }
        
        .stat-card.failed {
            border-left-color: #dc3545;
        }
        
        .stat-card.error {
            border-left-color: #ffc107;
        }
        
        .stat-number {
            font-size: 2.5em;
            font-weight: bold;
            margin-bottom: 5px;
        }
        
        .stat-label {
            color: #666;
            font-size: 0.9em;
            text-transform: uppercase;
            letter-spacing: 1px;
        }
        
        .agent-section {
            background: white;
            margin-bottom: 30px;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            overflow: hidden;
        }
        
        .agent-header {
            background: #667eea;
            color: white;
            padding: 20px 30px;
            cursor: pointer;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }
        
        .agent-header:hover {
            background: #5a6fd8;
        }
        
        .agent-name {
            font-size: 1.5em;
            font-weight: bold;
        }
        
        .agent-stats {
            display: flex;
            gap: 20px;
        }
        
        .agent-stat {
            text-align: center;
        }
        
        .agent-stat-number {
            font-size: 1.5em;
            font-weight: bold;
        }
        
        .agent-stat-label {
            font-size: 0.8em;
            opacity: 0.9;
        }
        
        .agent-content {
            padding: 30px;
            display: none;
        }
        
        .agent-content.expanded {
            display: block;
        }
        
        .test-results {
            margin-top: 20px;
        }
        
        .test-item {
            border: 1px solid #e0e0e0;
            border-radius: 8px;
            margin-bottom: 15px;
            overflow: hidden;
        }
        
        .test-header {
            padding: 15px 20px;
            cursor: pointer;
            display: flex;
            justify-content: space-between;
            align-items: center;
            background: #f8f9fa;
        }
        
        .test-header:hover {
            background: #e9ecef;
        }
        
        .test-question {
            font-weight: 500;
            flex: 1;
            margin-right: 20px;
        }
        
        .test-status {
            padding: 5px 10px;
            border-radius: 20px;
            font-size: 0.8em;
            font-weight: bold;
            text-transform: uppercase;
        }
        
        .test-status.passed {
            background: #d4edda;
            color: #155724;
        }
        
        .test-status.failed {
            background: #f8d7da;
            color: #721c24;
        }
        
        .test-status.error {
            background: #fff3cd;
            color: #856404;
        }
        
        .test-details {
            padding: 20px;
            display: none;
            background: white;
        }
        
        .test-details.expanded {
            display: block;
        }
        
        .test-answer {
            margin-bottom: 15px;
        }
        
        .test-answer h4 {
            color: #667eea;
            margin-bottom: 10px;
        }
        
        .test-answer-content {
            background: #f8f9fa;
            padding: 15px;
            border-radius: 5px;
            border-left: 4px solid #667eea;
            white-space: pre-wrap;
            max-height: 200px;
            overflow-y: auto;
        }
        
        .answers-comparison {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 20px;
            margin-bottom: 20px;
        }
        
        .answer-column {
            background: #f8f9fa;
            padding: 15px;
            border-radius: 8px;
            border: 1px solid #e0e0e0;
        }
        
        .answer-column h4 {
            margin-bottom: 10px;
            color: #667eea;
            font-size: 1.1em;
        }
        
        .answer-column .test-answer-content {
            background: white;
            border-left: 4px solid #667eea;
            margin: 0;
        }
        
        .artifacts-section {
            margin: 20px 0;
            padding: 15px;
            background: #f8f9fa;
            border-radius: 8px;
            border: 1px solid #e0e0e0;
        }
        
        .artifacts-section h4 {
            color: #667eea;
            margin-bottom: 15px;
            font-size: 1.2em;
        }
        
        .artifacts-section h5 {
            color: #495057;
            margin-bottom: 10px;
            font-size: 1em;
        }
        
        .artifacts-list {
            list-style: none;
            padding: 0;
            margin: 0;
        }
        
        .artifact-item {
            background: white;
            padding: 10px;
            margin: 5px 0;
            border-radius: 5px;
            border-left: 4px solid #28a745;
        }
        
        .artifact-filename {
            color: #6c757d;
            font-style: italic;
        }
        
        .artifact-type {
            color: #007bff;
            font-weight: bold;
        }
        
        .artifact-size {
            color: #6c757d;
            font-size: 0.9em;
        }
        
        .artifact-expectation {
            background: white;
            padding: 10px;
            border-radius: 5px;
            border-left: 4px solid #ffc107;
            margin: 0;
        }
        
        .artifact-status {
            padding: 10px;
            border-radius: 5px;
            margin-top: 10px;
            font-weight: bold;
        }
        
        .artifact-status.success {
            background: #d4edda;
            color: #155724;
            border-left: 4px solid #28a745;
        }
        
        .artifact-status.warning {
            background: #fff3cd;
            color: #856404;
            border-left: 4px solid #ffc107;
        }
        
        .artifact-status.error {
            background: #f8d7da;
            color: #721c24;
            border-left: 4px solid #dc3545;
        }
        
        .artifact-type-group {
            margin-bottom: 15px;
            border: 1px solid #e0e0e0;
            border-radius: 6px;
            padding: 10px;
            background: #f8f9fa;
        }
        
        .artifact-type-header {
            color: #667eea;
            font-size: 1.1em;
            display: block;
            margin-bottom: 8px;
            padding-bottom: 5px;
            border-bottom: 1px solid #e0e0e0;
        }
        
        .artifact-type-items {
            margin: 0;
            padding-left: 20px;
        }
        
        .artifact-item {
            margin-bottom: 8px;
            padding: 8px;
            background: white;
            border-radius: 4px;
            border-left: 3px solid #667eea;
        }
        
        .artifact-filename {
            color: #6c757d;
            font-size: 0.9em;
            font-style: italic;
        }
        
        .artifact-result {
            color: #28a745;
            font-size: 0.9em;
            font-family: monospace;
            background: #f8f9fa;
            padding: 2px 4px;
            border-radius: 3px;
        }
        
        .artifact-error {
            color: #dc3545;
            font-size: 0.9em;
            font-family: monospace;
            background: #f8f9fa;
            padding: 2px 4px;
            border-radius: 3px;
        }
        
        .artifact-source {
            color: #6c757d;
            font-size: 0.9em;
        }
        
        .llm-validation-section {
            margin: 20px 0;
            padding: 15px;
            background: #f8f9fa;
            border-radius: 8px;
            border: 1px solid #e0e0e0;
        }
        
        .llm-validation-section h4 {
            color: #667eea;
            margin-bottom: 15px;
            font-size: 1.2em;
        }
        
        .validation-score {
            padding: 10px 15px;
            border-radius: 6px;
            margin-bottom: 10px;
            font-size: 1.1em;
        }
        
        .validation-score.success {
            background: #d4edda;
            color: #155724;
            border-left: 4px solid #28a745;
        }
        
        .validation-score.warning {
            background: #fff3cd;
            color: #856404;
            border-left: 4px solid #ffc107;
        }
        
        .validation-score.error {
            background: #f8d7da;
            color: #721c24;
            border-left: 4px solid #dc3545;
        }
        
        .validation-details {
            margin-top: 15px;
        }
        
        .validation-details h5 {
            color: #495057;
            margin-bottom: 8px;
            font-size: 1em;
        }
        
        .validation-details-content {
            background: white;
            padding: 10px;
            border-radius: 4px;
            border-left: 3px solid #667eea;
            font-family: monospace;
            font-size: 0.9em;
        }
        
        .test-metrics {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
            gap: 15px;
            margin-top: 15px;
        }
        
        .metric {
            text-align: center;
            padding: 10px;
            background: #f8f9fa;
            border-radius: 5px;
        }
        
        .metric-value {
            font-size: 1.2em;
            font-weight: bold;
            color: #667eea;
        }
        
        .metric-label {
            font-size: 0.8em;
            color: #666;
            text-transform: uppercase;
        }
        
        .footer {
            text-align: center;
            padding: 30px;
            color: #666;
            background: white;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        
        .expand-icon {
            transition: transform 0.3s ease;
        }
        
        .expand-icon.expanded {
            transform: rotate(180deg);
        }
        
        @media (max-width: 768px) {
            .container {
                padding: 10px;
            }
            
            .header h1 {
                font-size: 2em;
            }
            
            .stats-grid {
                grid-template-columns: 1fr;
            }
            
            .agent-stats {
                flex-direction: column;
                gap: 10px;
            }
            
            .answers-comparison {
                grid-template-columns: 1fr;
                gap: 15px;
            }
            
            .artifacts-section {
                padding: 10px;
            }
        }
        """
    
    def _generate_summary_section(self, report_data: Dict[str, Any]) -> str:
        """Generate summary section HTML"""
        summary = report_data['summary']
        
        return f"""
        <section class="summary">
            <h2>📊 Test Summary</h2>
            <div class="suite-info">
                <h3>📁 Test Suite: {report_data.get('suite_name', 'Unknown')}</h3>
            </div>
            <div class="stats-grid">
                <div class="stat-card">
                    <div class="stat-number">{summary['total_agents']}</div>
                    <div class="stat-label">Agents Tested</div>
                </div>
                <div class="stat-card">
                    <div class="stat-number">{summary['total_tests']}</div>
                    <div class="stat-label">Total Tests</div>
                </div>
                <div class="stat-card passed">
                    <div class="stat-number">{summary['total_passed']}</div>
                    <div class="stat-label">Passed</div>
                </div>
                <div class="stat-card failed">
                    <div class="stat-number">{summary['total_failed']}</div>
                    <div class="stat-label">Failed</div>
                </div>
                <div class="stat-card">
                    <div class="stat-number">{summary['overall_pass_rate']:.1f}%</div>
                    <div class="stat-label">Pass Rate</div>
                </div>
                <div class="stat-card">
                    <div class="stat-number">{summary.get('total_duration', 0):.1f}s</div>
                    <div class="stat-label">Total Duration</div>
                </div>
            </div>
        </section>
        """
    
    def _generate_agents_section(self, report_data: Dict[str, Any]) -> str:
        """Generate agents section HTML"""
        agents_html = ""
        
        for agent_name, agent_data in report_data['agents'].items():
            agents_html += f"""
            <section class="agent-section">
                <div class="agent-header" onclick="toggleAgent('{agent_name}')">
                    <div class="agent-name">{agent_data['agent_name']}</div>
                    <div class="agent-stats">
                        <div class="agent-stat">
                            <div class="agent-stat-number">{agent_data['total_tests']}</div>
                            <div class="agent-stat-label">Tests</div>
                        </div>
                        <div class="agent-stat">
                            <div class="agent-stat-number">{agent_data['passed_tests']}</div>
                            <div class="agent-stat-label">Passed</div>
                        </div>
                        <div class="agent-stat">
                            <div class="agent-stat-number">{agent_data['failed_tests']}</div>
                            <div class="agent-stat-label">Failed</div>
                        </div>
                        <div class="agent-stat">
                            <div class="agent-stat-number">{agent_data['pass_rate']:.1f}%</div>
                            <div class="agent-stat-label">Pass Rate</div>
                        </div>
                        <div class="agent-stat">
                            <div class="agent-stat-number">{agent_data.get('total_duration', 0):.1f}s</div>
                            <div class="agent-stat-label">Duration</div>
                        </div>
                    </div>
                    <div class="expand-icon" id="icon-{agent_name}">▼</div>
                </div>
                <div class="agent-content" id="content-{agent_name}">
                    <div class="test-results">
                        {self._generate_test_results_html(agent_data['test_analysis'])}
                    </div>
                </div>
            </section>
            """
        
        return agents_html
    
    def _generate_test_results_html(self, test_analysis: List[Dict[str, Any]]) -> str:
        """Generate HTML for test results"""
        tests_html = ""
        
        for test in test_analysis:
            status_class = test['status']
            status_text = test['status'].upper()
            
            # Use a string with no indentation to avoid nesting issues
            tests_html += f"""<div class="test-item">
<div class="test-header" onclick="toggleTest('test-{test['index']}')">
<div class="test-question">{test['question'][:100]}{'...' if len(test['question']) > 100 else ''}</div>
<div class="test-status {status_class}">{status_text}</div>
</div>
<div class="test-details" id="test-{test['index']}">
{self._generate_llm_validation_section(test)}
<div class="answers-comparison">
<div class="answer-column">
<h4>🤖 Agent Answer</h4>
<div class="test-answer-content">{test['agent_answer'] or 'No answer provided'}</div>
</div>
<div class="answer-column">
<h4>🎯 Expected Answer</h4>
<div class="test-answer-content">{test['expected_answer'] or 'No expected answer defined'}</div>
</div>
</div>
{self._generate_artifacts_section(test)}
{f'<div class="test-answer"><h4>Validation Details:</h4><div class="test-answer-content">{test["validation_details"]}</div></div>' if test['validation_details'] else ''}
<div class="test-metrics">
<div class="metric">
<div class="metric-value">{test['tokens']}</div>
<div class="metric-label">Tokens</div>
</div>
<div class="metric">
<div class="metric-value">{test['duration']:.2f}s</div>
<div class="metric-label">Duration</div>
</div>
<div class="metric">
<div class="metric-value">{test['artifacts_count']}</div>
<div class="metric-label">Artifacts</div>
</div>
<div class="metric">
<div class="metric-value">{'❌' if test['code_failed'] else '✅'}</div>
<div class="metric-label">Code Success</div>
</div>
</div>
</div>
</div>"""
        
        return tests_html
    
    def _generate_artifacts_section(self, test: Dict[str, Any]) -> str:
        """Generate HTML for artifacts section"""
        artifacts_produced = test.get('artifacts_produced', [])
        artifacts_expected = test.get('artifacts_expected', {})
        
        if not artifacts_produced and artifacts_expected.get('count', 0) == 0:
            return ""
        
        artifacts_html = '<div class="artifacts-section">'
        artifacts_html += '<h4>📁 Artifacts</h4>'
        
        # Produced artifacts
        if artifacts_produced:
            artifacts_html += '<div class="artifacts-produced">'
            artifacts_html += '<h5>✅ Produced Artifacts:</h5>'
            artifacts_html += '<ul class="artifacts-list">'
            
            # Group artifacts by type for better display
            artifact_types = {}
            for artifact in artifacts_produced:
                artifact_type = artifact.get('type', 'Unknown')
                if artifact_type not in artifact_types:
                    artifact_types[artifact_type] = []
                artifact_types[artifact_type].append(artifact)
            
            for artifact_type, type_artifacts in artifact_types.items():
                artifacts_html += f'<li class="artifact-type-group">'
                artifacts_html += f'<strong class="artifact-type-header">{artifact_type} ({len(type_artifacts)})</strong>'
                artifacts_html += '<ul class="artifact-type-items">'
                
                for artifact in type_artifacts:
                    artifacts_html += f'<li class="artifact-item artifact-{artifact_type.lower()}">'
                    artifacts_html += f'<strong>{artifact.get("title", "Untitled")}</strong>'
                    
                    # Add type-specific details
                    if artifact_type == 'File':
                        if artifact.get('filepath'):
                            artifacts_html += f' <span class="artifact-filename">({artifact["filepath"]})</span>'
                    elif artifact_type == 'ToolExecution':
                        if artifact.get('tool_result'):
                            result_preview = artifact['tool_result'][:100] + '...' if len(artifact['tool_result']) > 100 else artifact['tool_result']
                            artifacts_html += f' <span class="artifact-result">Result: {result_preview}</span>'
                        if artifact.get('tool_error'):
                            artifacts_html += f' <span class="artifact-error">Error: {artifact["tool_error"]}</span>'
                    elif artifact_type == 'WorkflowExecution':
                        if artifact.get('source'):
                            artifacts_html += f' <span class="artifact-source">Source: {artifact["source"]}</span>'
                    
                    artifacts_html += '</li>'
                
                artifacts_html += '</ul>'
                artifacts_html += '</li>'
            
            artifacts_html += '</ul>'
            artifacts_html += '</div>'
        else:
            artifacts_html += '<div class="artifacts-produced">'
            artifacts_html += '<h5>❌ No Artifacts Produced</h5>'
            artifacts_html += '</div>'
        
        # Expected artifacts
        if artifacts_expected.get('count', 0) > 0:
            artifacts_html += '<div class="artifacts-expected">'
            artifacts_html += '<h5>🎯 Expected Artifacts:</h5>'
            artifacts_html += f'<p class="artifact-expectation">{artifacts_expected["description"]}</p>'
            artifacts_html += '</div>'
        
        # Artifact validation
        if artifacts_produced and artifacts_expected.get('count', 0) > 0:
            produced_count = len(artifacts_produced)
            expected_count = artifacts_expected.get('count', 0)
            expected_types = artifacts_expected.get('types', [])
            
            # Count validation
            count_status = ""
            if produced_count == expected_count:
                count_status = '<div class="artifact-status success">✅ Artifact count matches expectation</div>'
            elif produced_count > expected_count:
                count_status = f'<div class="artifact-status warning">⚠️ Produced {produced_count} artifacts, expected {expected_count}</div>'
            else:
                count_status = f'<div class="artifact-status error">❌ Produced {produced_count} artifacts, expected {expected_count}</div>'
            
            # Type validation
            produced_types = list(set([a.get('type', 'Unknown') for a in artifacts_produced]))
            type_status = ""
            if expected_types:
                missing_types = set(expected_types) - set(produced_types)
                extra_types = set(produced_types) - set(expected_types)
                
                if not missing_types and not extra_types:
                    type_status = '<div class="artifact-status success">✅ All expected artifact types present</div>'
                else:
                    type_status = '<div class="artifact-status warning">⚠️ Artifact type mismatch</div>'
                    if missing_types:
                        type_status += f'<div class="artifact-status error">❌ Missing types: {", ".join(missing_types)}</div>'
                    if extra_types:
                        type_status += f'<div class="artifact-status warning">⚠️ Extra types: {", ".join(extra_types)}</div>'
            
            artifacts_html += count_status
            artifacts_html += type_status
        
        artifacts_html += '</div>'
        return artifacts_html
    
    def _generate_llm_validation_section(self, test: Dict[str, Any]) -> str:
        """Generate HTML for LLM validation results section"""
        validation_mode = test.get('validation_mode', '')
        validation_score = test.get('validation_score', 0)
        validation_details = test.get('validation_details', '')
        
        # Only show LLM validation section if LLM validation is enabled
        if validation_mode not in ['llm', 'hybrid']:
            return ""
        
        validation_html = '<div class="llm-validation-section">'
        validation_html += '<h4>🧠 LLM Validation Results</h4>'
        
        # Validation score
        score_class = "success" if validation_score >= 80 else "warning" if validation_score >= 60 else "error"
        validation_html += f'<div class="validation-score {score_class}">'
        validation_html += f'<strong>Similarity Score: {validation_score}%</strong>'
        if validation_score >= 80:
            validation_html += ' ✅ High similarity'
        elif validation_score >= 60:
            validation_html += ' ⚠️ Moderate similarity'
        else:
            validation_html += ' ❌ Low similarity'
        validation_html += '</div>'
        
        # Validation details (if any)
        if validation_details:
            validation_html += '<div class="validation-details">'
            validation_html += '<h5>📝 Validation Details:</h5>'
            validation_html += f'<div class="validation-details-content">{validation_details}</div>'
            validation_html += '</div>'
        
        validation_html += '</div>'
        return validation_html
    
    def _generate_footer(self) -> str:
        """Generate footer HTML"""
        return """
        <footer class="footer">
            <p>Generated by FoundationaLLM Test Harness</p>
            <p>For more information, see the project documentation</p>
        </footer>
        """
    
    def _get_javascript(self) -> str:
        """Get JavaScript for interactive features"""
        return """function toggleAgent(agentName) {
    const content = document.getElementById('content-' + agentName);
    const icon = document.getElementById('icon-' + agentName);
    
    if (content.classList.contains('expanded')) {
        content.classList.remove('expanded');
        icon.classList.remove('expanded');
    } else {
        content.classList.add('expanded');
        icon.classList.add('expanded');
    }
}

function toggleTest(testId) {
    const content = document.getElementById(testId);
    
    if (content.classList.contains('expanded')) {
        content.classList.remove('expanded');
    } else {
        content.classList.add('expanded');
    }
}

// Auto-expand failed tests
document.addEventListener('DOMContentLoaded', function() {
    const failedTests = document.querySelectorAll('.test-status.failed, .test-status.error');
    failedTests.forEach(function(testStatus) {
        const testItem = testStatus.closest('.test-item');
        const testHeader = testItem.querySelector('.test-header');
        if (testHeader) {
            testHeader.click();
        }
    });
});"""

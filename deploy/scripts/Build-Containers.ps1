#! /usr/bin/pwsh

Param(
	# Optional parameters
	[parameter(Mandatory = $false)][string]$branch = "main"
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

$workflows = 
'agent-factory-api-release.yml',
'agent-hub-api-release.yml',
'chat-ui-release.yml',
'core-api-release.yml',
'core-job-release.yml',
'data-source-hub-api-release.yml',
'gatekeeper-api-release.yml',
'gatekeeper-integration-api-release.yml',
'langchain-api-release.yml',
'management-api-release.yml',
'management-ui-release.yml',
'prompt-hub-api-release.yml',
'semantic-kernel-api-release.yml',
'vectorization-api-release.yml',
'vectorization-job-release.yml'

Write-Host "Running FLLM Workflows from the ${branch} branch."

foreach ($workflow in $workflows) {
	
	Write-Host "Running workflow ${workflow}"
	gh workflow run $workflow --ref $branch

	if ($LASTEXITCODE -ne 0) {
		Write-Error("Failed to run workflow ${workflow}")
		exit 1
	}
}
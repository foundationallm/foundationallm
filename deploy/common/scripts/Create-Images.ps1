#! /usr/bin/pwsh

<#
.SYNOPSIS
	Script to run FLLM workflows for creating images.

.DESCRIPTION
	This script runs a series of FLLM workflows for creating images. It iterates through a list of workflows and executes each one using the GitHub CLI (gh). If any workflow fails, an error is thrown and the script exits with a non-zero exit code.

.PARAMETER branch
	Specifies the branch to run the workflows from. The default value is "main".

.EXAMPLE
	.\Create-Images.ps1 -branch "develop"
	Runs the FLLM workflows from the "develop" branch.

#>

Param(
	# Optional parameters
	[parameter(Mandatory = $false)][string]$branch = "main"
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

$workflows =
'orchestration-api-release.yml',
'agent-hub-api-release.yml',
'authorization-api-release.yml',
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
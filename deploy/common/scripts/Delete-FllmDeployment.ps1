#! /usr/bin/pwsh

<#
.SYNOPSIS
This script is used to delete resource groups and their associated resources in
Azure.

.DESCRIPTION
The Delete-FllmStdDeployment.ps1 script takes the subscription ID and AZD
environment name as parameters. It finds the resource groups belonging to the
environment and checks if each resource group exists before deleting the
resources within it. The script also deletes and purges Key Vaults, OpenAI
resources, and App Configurations within each resource group. After deleting the
resources, the script deletes the resource group itself. It then checks the
status of the deletion until all resource groups are confirmed to be deleted.

.PARAMETER azdEnvName
The name of the AZD environment.

.PARAMETER SubscriptionId
The ID of the Azure subscription.

.EXAMPLE
PS> .\Delete-FllmStdDeployment.ps1 -SubscriptionId "12345678-1234-1234-1234-1234567890AB" -azdEnvName "TestEnvironment"
Deletes the resource groups and their associated resources in Azure for the specified subscription ID and AZD environment name.

#>
param (
	[parameter(Mandatory = $true)][string]$azdEnvName,
	[parameter(Mandatory = $true)][string]$subscriptionId
)

$TranscriptName = $($MyInvocation.MyCommand.Name) -replace ".ps1", ".transcript.txt"
Start-Transcript -path .\$TranscriptName -Force

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

. ./Function-Library.ps1

function Remove-AppConfig {
	param(
		[string]$acName,
		[string]$rg,
		[bool]$purge,
		[string]$location
	)

	Invoke-CliCommand "Delete App Configuration $acName" {
		az appconfig delete `
			--name $acName `
			--resource-group $rg `
			--yes
	}

	if ($purge) {
		Invoke-CliCommand "Purge App Configuration $acName" {
			az appconfig purge `
				--name $acName `
				--location $location `
				--yes
		}
	}
}

function Remove-KeyVault {
	param(
		[string]$keyVaultName,
		[bool]$purge,
		[string]$location,
		[string]$resourceGroup
	)

	Invoke-CliCommand "Delete KeyVault $keyVaultName" {
		az keyvault delete `
			--name $keyVaultName `
			--resource-group $resourceGroup
	}

	if ($purge) {
		Invoke-CliCommand "Purge KeyVault $keyVaultName" {
			az keyvault purge `
				--name $keyVaultName `
				--location $location
		}
	}
}

function Remove-OpenAI {
	param(
		[string]$openAiName,
		[string]$resourceGroup,
		[string]$location
	)

	Invoke-CliCommand "Delete OpenAI resource $openAiName" {
		az cognitiveservices account delete `
			--name $openAiName `
			--resource-group $resourceGroup
	}

	Invoke-CliCommand "Purge OpenAI resource $openAiName" {
		az cognitiveservices account purge `
			--name $openAiName `
			--resource-group $resourceGroup `
			--location $location
	}
}

# Function to delete and purge Key Vaults, OpenAI, and App Configurations
function Remove-Resources {
	param([string]$rg)

	$script:keyVaults = $null
	Invoke-CliCommand "Find KeyVaults in $($rg)" {
		$script:keyVaults = az keyvault list `
			--resource-group $rg `
			--query "[].{name:name,location:location,purgeProtection:properties.enablePurgeProtection}" `
			--output json | ConvertFrom-Json -AsHashtable
	}

	if ($null -ne $script:keyVaults) {
		foreach ($kv in $script:keyVaults) {
			$purgeKeyVault = !$kv.purgeProtection
			Remove-KeyVault `
				-keyVaultName $kv.name `
				-purge $purgeKeyVault `
				-location $kv.location `
				-resourceGroup $rg
		}
	}

	$script:openAiResources = $null
	Invoke-CliCommand "Find OpenAI resources in $($rg)" {
		$script:openAiResources = az cognitiveservices account list `
			--resource-group $rg `
			--query "[?kind=='OpenAI'].{name:name,location:location}" `
			--output json | ConvertFrom-Json -AsHashtable
	}

	# If openAI resources are found, delete and purge them
	if ($null -ne $script:openAiResources) {
		foreach ($oai in $script:openAiResources) {
			Remove-OpenAI `
				-openAiName $oai.name `
				-resourceGroup $rg `
				-location $oai.location
		}
	}

	# Find App Configurations in the specified resource group using Invoke-CliCommand
	$script:appConfigs = $null
	Invoke-CliCommand "Find App Configurations in $($rg)" {
		$script:appConfigs = az appconfig list `
			--resource-group $rg `
			--query "[].{name:name,location:location,purgeProtection:enablePurgeProtection}" `
			--output json | ConvertFrom-Json -AsHashtable
	}

	# If app configurations are found, delete and purge them
	if ($null -ne $script:appConfigs) {
		foreach ($ac in $script:appConfigs) {
			$purgeAppConfig = !$ac.purgeProtection
			Remove-AppConfig `
				-acName $ac.name `
				-rg $rg `
				-purge $purgeAppConfig `
				-location $ac.location
		}
	}
}

# Set the subscription context
Invoke-CliCommand "Set the subscription context" {
	az account set --subscription $subscriptionId
}

# Find all Resource Groups with the specified AZD environment tag
$resourceGroups = Get-ResourceGroups `
	-subscriptionId $subscriptionId `
	-azdEnvName $azdEnvName

# Loop through each resource group, delete resources, and then delete the
# resource group
foreach ($rg in $resourceGroups.GetEnumerator()) {
	Write-Host -ForegroundColor Blue "Processing resource group: $($rg.Key)"
	Remove-Resources -rg $rg.Key
}

while ($resourceGroups.Count -gt 0) {
	foreach ($rg in $resourceGroups.GetEnumerator()) {
		$rgExists = 'false'
		Invoke-CliCommand "Check if resource group $($rg.Key) exists" {
			$script:rgExists = az group exists --name $rg.Key --output tsv
		}

		if ($rgExists -eq "true") {
			# This command is allowed to fail because the RG might already be
			# deleted from a previous iteration even if the check for existence was
			# true. So we do not use Invoke-CliCommand.
			Write-Host "--Deleting resource group $($rg.Key)..." -ForegroundColor Yellow
			az group delete --name $rg.Key --no-wait --yes
		}
	}

	Write-Host "Waiting 30 seconds before checking the status of the deletion." -ForegroundColor Cyan
	Start-Sleep -Seconds 30

	$resourceGroups = Get-ResourceGroups `
		-subscriptionId $subscriptionId `
		-azdEnvName $azdEnvName
}

Stop-Transcript

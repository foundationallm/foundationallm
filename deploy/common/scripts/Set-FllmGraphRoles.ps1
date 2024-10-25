# /usr/bin/pwsh
<#
.SYNOPSIS
Assigns Microsoft Graph roles to two managed identities within a specified resource group.

.DESCRIPTION
This script first looks up the object IDs for managed identities containing 'core-api' and 'management-api' in their names within a specified resource group. It then assigns Microsoft Graph roles to these managed identities using the Microsoft Graph API.

.PARAMETER resourceGroupName
The name of the resource group where the managed identities are located.

.EXAMPLE
Set-FllmGraphRoles.ps1 -resourceGroupName "my-resource-group"
Looks up the object IDs for 'core-api' and 'management-api' identities in "my-resource-group" and assigns Microsoft Graph roles to them.

.NOTES
This script can be used to assign Microsoft Graph roles to managed identities in the FLLM environment.
Ensure that the Azure CLI is installed and authenticated before running this script.
#>

Param(
	[parameter(Mandatory = $true)][string]$resourceGroupName
)

# Set Debugging and Error Handling
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function Invoke-AndRequireSuccess {
	param (
		[Parameter(Mandatory = $true, Position = 0)]
		[string]$Message,

		[Parameter(Mandatory = $true, Position = 1)]
		[ScriptBlock]$ScriptBlock
	)

	Write-Host "${message}..." -ForegroundColor Yellow
	$result = & $ScriptBlock

	if ($LASTEXITCODE -ne 0) {
		throw "Failed ${message} (code: ${LASTEXITCODE})"
	}

	return $result
}

# Function to get the object ID for a managed identity by partial name match
function Get-ManagedIdentityObjectId {
	param (
		[Parameter(Mandatory = $true)][string]$resourceGroupName,
		[Parameter(Mandatory = $true)][string]$identityNameContains
	)

	$identity = az identity list `
		--resource-group $resourceGroupName `
		--query "[?contains(name, '$identityNameContains')].{Name:name, ObjectId:principalId}" `
		--output json | ConvertFrom-Json

	if ($identity) {
		return $identity.ObjectId
	}
 else {
		throw "Managed Identity with '$identityNameContains' in the name not found."
	}
}

# Get the object IDs for 'core-api' and 'management-api' managed identities
$coreApiPrincipalId = Get-ManagedIdentityObjectId -resourceGroupName $resourceGroupName -identityNameContains 'core-api'
$managementApiPrincipalId = Get-ManagedIdentityObjectId -resourceGroupName $resourceGroupName -identityNameContains 'management-api'

# Function to assign Microsoft Graph roles to a principal
function Assign-MSGraph-Roles {
	param (
		[Parameter(Mandatory = $true)][string]$principalId
	)

	Push-Location $($MyInvocation.InvocationName | Split-Path)

	$msGraphId = (az ad sp show --id '00000003-0000-0000-c000-000000000000' --output tsv --query 'id')

	$msGraphRoleIds = New-Object -TypeName psobject -Property @{
		'Group.Read.All'       = '5b567255-7703-4780-807c-7be8301ae99b';
		'User.Read.All'        = 'df021288-bdef-4463-88db-98f22de89214';
		'Application.Read.All' = '9a5d68dd-52b0-4cc2-bd40-abcf44ac3a30';
		'Directory.Read.All'   = '7ab1d382-f21e-4acd-a863-ba3e13f7da61';
	}

	$existingRoleData = (
		az rest `
			--method GET `
			--uri "https://graph.microsoft.com/v1.0/servicePrincipals/$($principalId)/appRoleAssignments" `
			--headers "{'Content-Type': 'application/json'}" `
			-o json)

	$existingRoles = $($($existingRoleData | ConvertFrom-Json).value | Select-Object -ExpandProperty appRoleId)

	$msGraphRoleIds.PSObject.Properties | ForEach-Object {

		Invoke-AndRequireSuccess "Assigning Microsoft Graph Role [$($_.Name) | $($_.Value)] to Principal [$($principalId)]" {

			if ($null -ne $existingRoles -and $existingRoles.Contains($_.Value)) {
				Write-Host "Role is already assigned!" -ForegroundColor Blue
				return
			}

			$body = "{'principalId':'$($principalId)','resourceId':'$($msGraphId)','appRoleId':'$($_.Value)'}"

			az rest --method POST `
				--uri "https://graph.microsoft.com/v1.0/servicePrincipals/$($principalId)/appRoleAssignments" `
				--headers 'Content-Type=application/json' `
				--body $body
		}
	}

	Pop-Location
}

# Assign roles to 'core-api' managed identity
Write-Host "**Assigning Microsoft Graph roles to 'core-api' managed identity...**" -ForegroundColor Blue
Assign-MSGraph-Roles -principalId $coreApiPrincipalId

# Assign roles to 'management-api' managed identity
Write-Host "**Assigning Microsoft Graph roles to 'management-api' managed identity...**" -ForegroundColor Blue
Assign-MSGraph-Roles -principalId $managementApiPrincipalId


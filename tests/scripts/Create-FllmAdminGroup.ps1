<#
.SYNOPSIS
    Creates an Azure AD group using the Azure CLI.

.DESCRIPTION
    This script creates a new Azure AD group named 'FLLM-Admins' using the Azure CLI command. 
	It checks for errors during execution and outputs the status of group creation.

.PARAMETER groupName
    Specifies the name of the Azure AD group to be created.

.EXAMPLE
    ./CreateAzureADGroup.ps1
    This example runs the script to create an Azure AD group named 'FLLM-Admins'.
#>

Param(
    [parameter(Mandatory = $false)][string]$groupName = "FLLM-E2E-Admins"
)

# Try block to handle potential errors during the execution
try {
    # Build the command to create the Azure AD group using Azure CLI
    $createGroupCommand = "az ad group create --display-name $groupName --mail-nickname $groupName"

    # Execute the command to create the group
    $output = Invoke-Expression $createGroupCommand

    # If the command executes successfully, output the result
    Write-Host "Azure AD group '$groupName' created successfully."
} 
catch {
    # Catch block to handle and report any errors that occur during the execution
    Write-Host "Failed to create Azure AD group. Error: $($_.Exception.Message)"
}

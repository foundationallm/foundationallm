function New-AgentsArtifacts {
    param (
        [string]$UniqueName,
        [string]$InstanceId,
        [string]$UserGroupObjectId
    )

    $resourceNames = Get-ResourceNames -UniqueName $UniqueName
    $resourceGroupNames = Get-ResourceGroupNames -UniqueName $UniqueName

    $managementAPIEndpointURL = "https://" + (az containerapp ingress show -n $resourceNames.ManagementAPIContainerApp -g $resourceGroupNames.Core --query "fqdn" -o tsv)
    $coreAPIEndpointURL = "https://" + (az containerapp ingress show -n $resourceNames.CoreAPIContainerApp -g $resourceGroupNames.Core --query "fqdn" -o tsv)

    # Initialize global variables for the FoundationaLLM.Core module
    $global:InstanceId = $InstanceId
    $global:ManagementAPIBaseUrl = $managementAPIEndpointURL
    $global:ManagementAPIInstanceRelativeUri = "/instances/$($global:InstanceId)"
    $global:CoreAPIBaseUrl = $coreAPIEndpointURL
    $global:CoreAPIInstanceRelativeUri = "/instances/$($global:InstanceId)"

    $packagePath = "$PSScriptRoot\..\packages\agents"

    $azureOpenAIEndpoint = ((az cognitiveservices account show -g $resourceGroupNames.AIFoundry -n $resourceNames.AIFoundry) | ConvertFrom-Json).properties.endpoint
    $contextAPIEndpoint = "https://" + (az containerapp ingress show -n $resourceNames.ContextAPIContainerApp -g $resourceGroupNames.Core --query "fqdn" -o tsv)

    Deploy-FoundationaLLMPackage `
        -PackageRoot $packagePath `
        -Parameters @{
            AZURE_OPENAI_ENDPOINT = $azureOpenAIEndpoint
            AZURE_AISEARCH_ENDPOINT = "https://$($resourceNames.AISearch).search.windows.net"
            CONTEXTAPI_ENDPOINT = $contextAPIEndpoint
        }
}


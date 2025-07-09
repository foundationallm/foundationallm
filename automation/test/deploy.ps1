Import-Module -Name ".\automation\FoundationaLLM.Core\FoundationaLLM.Core.psm1" -Force

$global:InstanceId = "8ac6074c-bdde-43cb-a140-ec0002d96d2b"
$global:CoreAPIBaseUrl = "https://cacoreapil43jljq2i5ox6.lemondesert-a0804c39.eastus2.azurecontainerapps.io"
$global:CoreAPIInstanceRelativeUri = "/instances/$($global:InstanceId)"
$global:ManagementAPIBaseUrl = "https://camanagementapil43jljq2i5ox6.lemondesert-a0804c39.eastus2.azurecontainerapps.io"
# $global:ManagementAPIBaseUrl = "https://localhost:63267"
$global:ManagementAPIInstanceRelativeUri = "/instances/$($global:InstanceId)"

$agentTemplatePath = ".\automation\test\TestPackage"


Deploy-FoundationaLLMPackage `
    -PackageRoot $agentTemplatePath `
    -Parameters @{
        "AGENT_NAME" = "MAA-01"
    }



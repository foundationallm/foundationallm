#!/usr/bin/env pwsh

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Load utility functions
Push-Location $($MyInvocation.InvocationName | Split-Path)
try {
    . ./utility/Load-Utility-Functions.ps1
}
finally {
    Pop-Location
}

$dnsServerIp = "${env:DNS_RESOLVER_ENDPOINT_IP}"
$outputFolderPath = "../standard-hub/config/vpn" | Get-AbsolutePath
$outputFilePath = Join-Path $outputFolderPath "VpnClientConfiguration.zip"
$resourceGroupName = "${env:RESOURCE_GROUP_NAME}"
$vpnGatewayName = "${env:VPN_GATEWAY_NAME}"
$xmlFilePath = Join-Path $outputFolderPath "AzureVPN" "azurevpnconfig.xml" # Assuming the XML file is inside the extracted folder

# Add the dns-resolver extension to the cli
az extension add --name dns-resolver --allow-preview true --yes --only-show-errors
	
# Get the VPN client configuration package URL
$vpnClientPackageUrl = az network vnet-gateway vpn-client generate `
    --name $vpnGatewayName `
    --resource-group $resourceGroupName `
    --output tsv

# Download the package and extract
Invoke-WebRequest -Uri $vpnClientPackageUrl -OutFile $outputFilePath
Expand-Archive -Path $outputFilePath -DestinationPath $outputFolderPath -Force

# Assuming the XML file is directly inside the expanded folder & Read the XML file
$expandedFolder = (Get-ChildItem -Directory -Path $outputFolderPath)[0].FullName
$xmlFilePath = Join-Path -Path $expandedFolder -ChildPath "azurevpnconfig.xml"
$xmlContent = Get-Content -Path $xmlFilePath
$azureVpnClientConfig = [xml]$xmlContent

# Create a namespace manager
$namespaceManager = New-Object System.Xml.XmlNamespaceManager($azureVpnClientConfig.NameTable)
$namespaceManager.AddNamespace("ns", "http://schemas.datacontract.org/2004/07/")
$namespaceManager.AddNamespace("i", "http://www.w3.org/2001/XMLSchema-instance")

# Remove <clientconfig i:nil="true" /> node if it exists
$nilClientConfigNode = $azureVpnClientConfig.SelectSingleNode("//ns:clientconfig[@i:nil='true']", $namespaceManager)
if ($nilClientConfigNode -ne $null) {
    $nilClientConfigNode.ParentNode.RemoveChild($nilClientConfigNode) | Out-Null
}

# Create DNS servers XML nodes
$dnsServersNode = $azureVpnClientConfig.CreateElement("dnsservers", "http://schemas.datacontract.org/2004/07/")
$dnsServer1 = $azureVpnClientConfig.CreateElement("dnsserver", "http://schemas.datacontract.org/2004/07/")
$dnsServer1.InnerText = $dnsServerIp
$dnsServersNode.AppendChild($dnsServer1) | Out-Null

# Locate the clientconfig node or create it if it doesn't exist
$clientConfigNode = $azureVpnClientConfig.SelectSingleNode("//ns:clientconfig", $namespaceManager)
if ($clientConfigNode -eq $null) {
    $clientConfigNode = $azureVpnClientConfig.CreateElement("clientconfig", "http://schemas.datacontract.org/2004/07/")
    $azureVpnClientConfig.DocumentElement.AppendChild($clientConfigNode) | Out-Null
}

# Append DNS servers to clientconfig
$clientConfigNode.AppendChild($dnsServersNode) | Out-Null

# Save the updated XML file
$azureVpnClientConfig.Save($xmlFilePath)

Write-Host -ForegroundColor Yellow `
"Please Install the Azure VPN client via https://aka.ms/azvpnclientdownload, and 
then import the VPN client configuration package downloaded to $xmlFilePath. Make sure to connect 
to the VPN prior to running the next step of the FLLM Install for Standard."

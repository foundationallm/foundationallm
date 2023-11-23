param addressPrefix string
param location string
param name string
param rulesInbound array = []
param rulesOutbound array = []
param serviceEndpoints array = []
param tags object 
param vnetName string

resource inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = [for rule in rulesInbound :{
  parent: nsg
  name: rule.name
  properties: {
    access: rule.access
    destinationAddressPrefix: rule.?destinationAddressPrefix
    destinationAddressPrefixes: rule.?destinationAddressPrefixes
    destinationPortRange: rule.?destinationPortRange
    destinationPortRanges: rule.?destinationPortRanges
    direction: 'Inbound'
    priority: rule.priority
    protocol: rule.protocol
    sourceAddressPrefix:rule.?sourceAddressPrefix
    sourceAddressPrefixes: rule.?sourceAddressPrefixes
    sourcePortRange: rule.?sourcePortRange
    sourcePortRanges:rule.?sourcePortRanges
  }
}]

resource main 'Microsoft.Network/virtualNetworks/subnets@2023-05-01' =  {
  name: '${vnetName}/${name}'

  properties: {
    addressPrefix: addressPrefix
    privateEndpointNetworkPolicies: 'Enabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
    serviceEndpoints: serviceEndpoints 

    networkSecurityGroup: {
      id: nsg.id
    }
  }
}

resource nsg 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
  location: location
  name: 'nsg-${vnetName}-${name}'
  tags: tags
}

resource outbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = [for rule in rulesOutbound :{
  parent: nsg
  name: rule.name
  properties: {
    access: rule.access
    destinationAddressPrefix: rule.?destinationAddressPrefix
    destinationAddressPrefixes: rule.?destinationAddressPrefixes
    destinationPortRange: rule.?destinationPortRange
    destinationPortRanges: rule.?destinationPortRanges
    direction: 'Outbound'
    priority: rule.priority
    protocol: rule.protocol
    sourceAddressPrefix:rule.?sourceAddressPrefix
    sourceAddressPrefixes: rule.?sourceAddressPrefixes
    sourcePortRange: rule.?sourcePortRange
    sourcePortRanges:rule.?sourcePortRanges
  }
}]

param name string
param rulesInbound array = []
param rulesOutbound array = []

resource inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = [for rule in rulesInbound :{
  name: '${name}/${rule.name}'
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



resource outbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = [for rule in rulesOutbound :{
  name: '${name}/${rule.name}'
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

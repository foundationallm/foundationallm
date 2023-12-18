/** Inputs **/
@description('The NSG location')
param location string

@description('The NSG name suffix')
param resourceSuffix string

@description('The NSG rules')
param rules object = {
  inbound: []
  outbound: []
}

@description('The NSG tags')
param tags object

/** Outputs **/
output id string = main.id

/** Resources **/
@description('The NSG')
resource main 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
  location: location
  name: 'nsg-${resourceSuffix}'
  tags: tags

  properties: {
    securityRules: concat(
      map(rules.?inbound ?? [], rule => {
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
            sourceAddressPrefix: rule.?sourceAddressPrefix
            sourceAddressPrefixes: rule.?sourceAddressPrefixes
            sourcePortRange: rule.?sourcePortRange
            sourcePortRanges: rule.?sourcePortRanges
          }
        }),
      map(rules.?outbound ?? [], rule => {
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
            sourceAddressPrefix: rule.?sourceAddressPrefix
            sourceAddressPrefixes: rule.?sourceAddressPrefixes
            sourcePortRange: rule.?sourcePortRange
            sourcePortRanges: rule.?sourcePortRanges
          }
        })
    )
  }
}

param environmentName string
param location string
param project string
param timestamp string = utcNow()

output vnetId string = main.id

var name = 'vnet-${environmentName}-${location}-net-${project}'

var subnets = [
  {
    name: 'AppGateway'
    addressPrefix: '10.0.0.0/24'
    rules: {
      inbound: [
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '80'
          name: 'allow-internet-http-inbound'
          priority: 128
          protocol: 'Tcp'
          sourceAddressPrefix: 'Internet'
          sourcePortRange: '*'
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '443'
          name: 'allow-internet-https-inbound'
          priority: 132
          protocol: 'Tcp'
          sourceAddressPrefix: 'Internet'
          sourcePortRange: '*'
        }
        {
          access: 'Allow'
          destinationAddressPrefix: '*'
          destinationPortRange: '65200-65535'
          name: 'allow-gatewaymanager-inbound'
          priority: 148
          protocol: 'Tcp'
          sourceAddressPrefix: 'GatewayManager'
          sourcePortRange: '*'
        }
        {
          access: 'Allow'
          destinationAddressPrefix: '*'
          destinationPortRange: '*'
          name: 'allow-loadbalancer-inbound'
          priority: 164
          protocol: '*'
          sourceAddressPrefix: 'AzureLoadBalancer'
          sourcePortRange: '*'
        }
        {
          access: 'Deny'
          destinationAddressPrefix: '*'
          destinationPortRange: '*'
          name: 'deny-all-inbound'
          priority: 4096
          protocol: '*'
          sourceAddressPrefix: '*'
          sourcePortRange: '*'
        }
      ]
    }
  }
  {
    name: 'FLLMBackend'
    addressPrefix: '10.0.16.0/22'
  }
  {
    name: 'FLLMFrontEnd'
    addressPrefix: '10.0.12.0/22'
  }
  {
    name: 'FLLMOpenAI'
    addressPrefix: '10.0.5.0/24'
    rules: {
      inbound: [
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '3443'
          name: 'allow-apim'
          priority: 128
          protocol: 'Tcp'
          sourceAddressPrefix: 'ApiManagement'
          sourcePortRange: '*'
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '6390'
          name: 'allow-lb'
          priority: 192
          protocol: 'Tcp'
          sourceAddressPrefix: 'AzureLoadBalancer'
          sourcePortRange: '*'
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-aks-inbound'
          priority: 256
          protocol: '*'
          sourcePortRange: '*'
          sourceAddressPrefixes: [
            '10.0.16.0/22'
          ]
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '443'
          name: 'allow-apim-inbound'
          priority: 320
          protocol: 'Tcp'
          sourcePortRange: '*'
          sourceAddressPrefixes: [
            '10.0.5.0/24'
          ]
        }
        {
          access: 'Deny'
          destinationAddressPrefix: '*'
          destinationPortRange: '*'
          name: 'deny-all-inbound'
          priority: 4096
          protocol: '*'
          sourceAddressPrefix: '*'
          sourcePortRange: '*'
        }
      ]
      outbound: [
        {
          access: 'Allow'
          destinationAddressPrefix: 'Storage'
          destinationPortRange: '443'
          name: 'allow-storage'
          priority: 128
          protocol: 'Tcp'
          sourceAddressPrefix: 'VirtualNetwork'
          sourcePortRange: '*'
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'SQL'
          destinationPortRange: '1443'
          name: 'allow-sql'
          priority: 192
          protocol: 'Tcp'
          sourceAddressPrefix: 'VirtualNetwork'
          sourcePortRange: '*'
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'AzureKeyVault'
          destinationPortRange: '443'
          name: 'allow-kv'
          priority: 224
          protocol: 'Tcp'
          sourceAddressPrefix: 'VirtualNetwork'
          sourcePortRange: '*'
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-vnet'
          priority: 4068
          protocol: '*'
          sourceAddressPrefix: 'VirtualNetwork'
          sourcePortRange: '*'
        }
      ]
    }
    serviceEndpoints: [
      {
        service: 'Microsoft.CognitiveServices' // TODO: Is this needed?
        locations: [ '*' ]
      }
    ]
  }
  {
    name: 'FLLMServices'
    addressPrefix: '10.0.3.0/24'
    rules: {
      inbound: [
        {
          name: 'deny-all-inbound'
          protocol: '*'
          sourcePortRange: '*'
          destinationPortRange: '*'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
          access: 'Deny'
          priority: 4096
        }
      ]
    }
  }
  {
    name: 'FLLMStorage'
    addressPrefix: '10.0.4.0/24'
    rules: {
      inbound: [
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          direction: 'Inbound'
          name: 'allow-aks-inbound'
          priority: 256
          protocol: '*'
          sourceAddressPrefixes: [ '10.0.16.0/22' ]
          sourcePortRange: '*'
        }
        {
          access: 'Deny'
          destinationAddressPrefix: '*'
          destinationPortRange: '*'
          name: 'deny-all-inbound'
          priority: 4096
          protocol: '*'
          sourceAddressPrefix: '*'
          sourcePortRange: '*'
        }
      ]
      outbound: [
        {
          access: 'Deny'
          destinationAddressPrefix: '*'
          destinationPortRange: '*'
          name: 'deny-all-outbound'
          priority: 4096
          protocol: '*'
          sourceAddressPrefix: '*'
          sourcePortRange: '*'
        }
      ]
    }
  }
  {
    name: 'ops' // TODO: PLEs.  Maybe put these in FLLMServices?
    addressPrefix: '10.0.255.96/27'
    rules: {
      inbound: [
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-rdp-services' // TODO: Don't think we need this rule.
          priority: 256
          protocol: 'Tcp'
          sourcePortRange: '*'
          sourceAddressPrefixes: [ '10.0.0.0/24' ]
        }
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-aks-inbound'
          priority: 264
          protocol: '*'
          sourcePortRange: '*'
          sourceAddressPrefixes: [
            '10.0.12.0/22'
            '10.0.16.0/22'
          ]
        }
        {
          access: 'Deny'
          destinationAddressPrefix: '*'
          destinationPortRange: '*'
          name: 'deny-all-inbound'
          priority: 4096
          protocol: '*'
          sourceAddressPrefix: '*'
          sourcePortRange: '*'
        }
      ]
    }
  }
  {
    name: 'Vectorization'
    addressPrefix: '10.0.6.0/24'
    rules: {
      inbound: [
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-aks-inbound'
          priority: 256
          protocol: '*'
          sourceAddressPrefixes: [ '10.0.16.0/22' ]
          sourcePortRange: '*'
        }
        {
          access: 'Deny'
          destinationAddressPrefix: '*'
          destinationPortRange: '*'
          name: 'deny-all-inbound'
          priority: 4096
          protocol: '*'
          sourceAddressPrefix: '*'
          sourcePortRange: '*'
        }
      ]
      outbound: [
        {
          access: 'Deny'
          destinationAddressPrefix: '*'
          destinationPortRange: '*'
          direction: 'Outbound'
          name: 'deny-all-outbound'
          priority: 4096
          protocol: '*'
          sourceAddressPrefix: '*'
          sourcePortRange: '*'
        }
      ]
    }
  }
]

var tags = {
  Environment: environmentName
  IaC: 'Bicep'
  Project: project
  Purpose: 'Networking'
}

resource main 'Microsoft.Network/virtualNetworks@2023-05-01' = {
  name: name
  location: location
  tags: tags

  properties: {
    enableDdosProtection: false
    addressSpace: {
      addressPrefixes: [ '10.0.0.0/16' ]
    }
    subnets: [for (subnet, i) in subnets: {
      name: subnet.name
      properties: {
        addressPrefix: subnet.addressPrefix
        privateEndpointNetworkPolicies: 'Enabled'
        privateLinkServiceNetworkPolicies: 'Enabled'
        serviceEndpoints: subnet.?serviceEndpoints

        networkSecurityGroup: {
          id: nsg[i].id
        }
      }
    }]
  }
}

resource nsg 'Microsoft.Network/networkSecurityGroups@2023-05-01' = [for subnet in subnets: {
  location: location
  name: 'nsg-${name}-${subnet.name}'
  tags: tags
}]

module nsgRules './modules/nsgRules.bicep' = [for (subnet, i) in subnets: {
  name: 'nsg-${subnet.name}-${timestamp}'
  params: {
    name: nsg[i].name
    rulesInbound: subnet.?rules.?inbound
    rulesOutbound: subnet.?rules.?outbound
  }
}]

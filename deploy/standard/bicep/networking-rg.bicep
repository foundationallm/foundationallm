param environmentName string
param location string
param project string
param timestamp string = utcNow()
param cidrVnet string = '10.220.128.0/21'
output vnetId string = main.id

var name = 'vnet-${environmentName}-${location}-net-${project}'
var cidrAppGateway = cidrSubnet(cidrVnet, 24, 0)
var cidrFllmBackend = cidrSubnet(cidrVnet, 24, 1)
var cidrFllmFrontend = cidrSubnet(cidrVnet, 24, 2)
var cidrFllmOpenAi = cidrSubnet(cidrVnet, 26, 12)

var subnets = [
  {
    name: 'AppGateway'
    addressPrefix: cidrAppGateway
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
    addressPrefix: cidrFllmBackend
  }
  {
    name: 'FLLMFrontEnd'
    addressPrefix: cidrFllmFrontend
  }
  {
    name: 'FLLMOpenAI'
    addressPrefix: cidrFllmOpenAi
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
            cidrFllmBackend
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
            cidrFllmOpenAi
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
    addressPrefix: cidrSubnet(cidrVnet, 26, 13)
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
    addressPrefix: cidrSubnet(cidrVnet, 26, 14)
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
          sourceAddressPrefixes: [ cidrFllmBackend ]
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
    addressPrefix: cidrSubnet(cidrVnet, 26, 15)
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
          sourceAddressPrefixes: [ cidrAppGateway ]
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
            cidrFllmFrontend
            cidrFllmBackend
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
    addressPrefix: cidrSubnet(cidrVnet, 26, 16)
    rules: {
      inbound: [
        {
          access: 'Allow'
          destinationAddressPrefix: 'VirtualNetwork'
          destinationPortRange: '*'
          name: 'allow-aks-inbound'
          priority: 256
          protocol: '*'
          sourceAddressPrefixes: [ cidrFllmBackend ]
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
      addressPrefixes: [ cidrVnet ]
    }
    subnets: [for (subnet, i) in subnets: {
      name: subnet.name
      properties: {
        addressPrefix: subnet.addressPrefix
        privateEndpointNetworkPolicies: 'Enabled'
        privateLinkServiceNetworkPolicies: 'Enabled'
        serviceEndpoints: subnet.?serviceEndpoints

        networkSecurityGroup: {
          id: nsg[i].outputs.id
        }
      }
    }]
  }
}

module nsg 'modules/nsg.bicep' = [for subnet in subnets: {
  name: 'nsg-${subnet.name}-${timestamp}'
  params: {
    location: location
    resourceSuffix: '${name}-${subnet.name}'
    rules: subnet.?rules
    tags: tags
  }
}]

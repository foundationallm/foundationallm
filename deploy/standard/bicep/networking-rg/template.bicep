param environmentName string
param location string
param project string

output vnetId string = main.id

var name = 'vnet-${environmentName}-${location}-net-${project}'
var subnets = [
  {
    name: 'AppGateway'
    addressPrefix: '10.0.0.0/24'
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
  }
  {
    name: 'FLLMStorage'
    addressPrefix: '10.0.4.0/24'
  }
  {
    name: 'ops' // TODO: PLEs?  Maybe put this in FLLMServices?
    addressPrefix: '10.0.255.96/27'
  }
  {
    name: 'Vectorization'
    addressPrefix: '10.0.6.0/24'
  }
]

resource main 'Microsoft.Network/virtualNetworks@2023-05-01' = {
  name: name
  location: location

  properties: {
    enableDdosProtection: false
    addressSpace: {
      addressPrefixes: [ '10.0.0.0/16' ]
    }
  }

  tags: {
    Environment: environmentName
    IaC: 'Bicep'
    Project: project
    Purpose: 'Networking'
  }
}

@batchSize(1)
resource subnet 'Microsoft.Network/virtualNetworks/subnets@2023-05-01' = [for subnet in subnets: {
  name: subnet.name
  parent: main

  properties: {
    addressPrefix: subnet.addressPrefix
    privateEndpointNetworkPolicies: 'Enabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
    serviceEndpoints: subnet.?serviceEndpoints ?? []

    // networkSecurityGroup: {
    //   id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_resource.id
    // }
  }
}]

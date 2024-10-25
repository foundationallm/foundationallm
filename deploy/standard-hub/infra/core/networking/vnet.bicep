param abbrs object
param cidrVnet string
param environmentName string
param location string
param project string
param tags object

// Locals
var cidrDns = cidrSubnet(cidrVnet, 27, 1) // 192.168.100.32/27
var cidrVpnGateway = cidrSubnet(cidrVnet, 27, 0) // 192.168.100.0/27
var vnetName = namer(abbrs.networkVirtualNetworks, environmentName, location, 'net', project)

var subnets = [
  {
    name: 'GatewaySubnet'
    addressPrefix: cidrVpnGateway
  }
  {
    name: 'DNS'
    addressPrefix: cidrDns
    delegations: [
      {
        name: 'Microsoft.Network/dnsResolvers'
        properties: {
          serviceName: 'Microsoft.Network/dnsResolvers'
        }
      }
    ]
  }
]

// Functions
func namer(resourceAbbr string, env string, region string, workloadName string, projectId string) string =>
  '${resourceAbbr}${env}-${region}-${workloadName}-${projectId}'


// Resources
resource main 'Microsoft.Network/virtualNetworks@2023-05-01' = {
  name: vnetName
  location: location
  
  tags: tags

  properties: {
    enableDdosProtection: false
    addressSpace: {
      addressPrefixes: [cidrVnet]
    }
    subnets: [
      for (subnet, i) in subnets: {
        name: subnet.name
        properties: {
          addressPrefix: subnet.addressPrefix
          delegations: subnet.?delegations
          privateEndpointNetworkPolicies: 'Enabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
          serviceEndpoints: subnet.?serviceEndpoints
        }
      }
    ]
  }
}

// Outputs
output vnetId string = main.id
output vnetName string = main.name

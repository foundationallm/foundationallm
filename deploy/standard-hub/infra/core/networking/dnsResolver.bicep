
param abbrs object
param environmentName string
param location string
param project string
param vnetName string

// Locals
var resolverName = namer(abbrs.networkDnsResolvers, environmentName, location, 'net', project)

// Functions
func namer(resourceAbbr string, env string, region string, workloadName string, projectId string) string =>
  '${resourceAbbr}${env}-${region}-${workloadName}-${projectId}'

// Resources
resource resolver 'Microsoft.Network/dnsResolvers@2022-07-01' = {
  name: resolverName
  location: location
  properties: {
    virtualNetwork: {
      id: resourceId(resourceGroup().name, 'Microsoft.Network/virtualNetworks', vnetName)
    }
  }
}

resource inboundEndpoint 'Microsoft.Network/dnsResolvers/inboundEndpoints@2022-07-01' = {
  parent: resolver
  name: resolverName
  location: location
  properties: {
    ipConfigurations: [
      {
        privateIpAllocationMethod: 'Dynamic'
        subnet: {
          id: resourceId(resourceGroup().name, 'Microsoft.Network/virtualNetworks/subnets', vnetName, 'DNS')
        }
      }
    ]
  }
}

// Outputs
output dnsResolverEndpointIp string = inboundEndpoint.properties.ipConfigurations[0].privateIpAddress

/** Inputs **/
param abbrs object
param environmentName string
param location string
param project string
param subnetId string
param tags object
param vpnClientAddressPool string

@allowed([
  'VpnGw1'
  'VpnGw2AZ'
])
param sku string = 'VpnGw1'

@allowed([
  'RouteBased'
  'PolicyBased'
])
param vpnType string = 'RouteBased'


// Locals
var audience = audienceMap[cloud]
var cloud = environment().name
var issuer = 'https://sts.windows.net/${tenantId}/'
var name = namer(abbrs.networkVpnGateways, environmentName, location, 'net', project)
var pipName = namer(abbrs.networkPublicIPAddresses, environmentName, location, 'net', project)
var tenant = uri(environment().authentication.loginEndpoint, tenantId)
var tenantId = subscription().tenantId

var audienceMap = { AzureCloud: '41b23e61-6c1e-4545-b367-cd054e0ed4b4' }

// Function
func namer(resourceAbbr string, env string, region string, workloadName string, projectId string) string =>
  '${resourceAbbr}${env}-${region}-${workloadName}-${projectId}'

// Resources
resource vpnGateway 'Microsoft.Network/virtualNetworkGateways@2021-02-01' = {
  name: name
  location: location
  tags : tags
  properties: {
    ipConfigurations: [
      {
        properties: {
          privateIPAllocationMethod: 'Dynamic'
          subnet: {
            id: subnetId
          }
          publicIPAddress: {
            id: publicIp.id
          }
        }
        name: 'vnetGatewayConfig'
      }
    ]
    sku: {
      name: sku
      tier: sku
    }
    gatewayType: 'Vpn'
    vpnType: vpnType
    vpnClientConfiguration: {
      vpnClientAddressPool: {
        addressPrefixes: [
          vpnClientAddressPool
        ]
      }
      vpnClientProtocols: [
        'OpenVPN'
      ]
      vpnAuthenticationTypes: [
        'AAD'
      ]
      aadTenant: tenant
      aadAudience: audience
      aadIssuer: issuer
    }
  }
}

resource publicIp 'Microsoft.Network/publicIPAddresses@2021-02-01' = {
  name: pipName
  location: location
  sku: {
    name: 'Standard'
  }
  tags: tags
  properties: {
    publicIPAllocationMethod: 'Static'
  }
}

output vpnGatewayName string = vpnGateway.name

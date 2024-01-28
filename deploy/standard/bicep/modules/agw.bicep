/** Inputs **/
@description('Action Group Id for alerts')
param actionGroupId string

@description('Location for all resources')
param location string

@description('Log Analytic Workspace Id to use for diagnostics')
param logAnalyticsWorkspaceId string

@description('Resource suffix for all resources')
param resourceSuffix string

@description('Subnet Id for private endpoint')
param subnetId string

@description('Tags for all resources')
param tags object

@description('Timestamp for nested deployments')
param timestamp string = utcNow()

@description('User Assigned Identity Id to use for the Application Gateway')
param uaiId string

/** Locals **/
@description('Metric alerts for the resource')
var alerts = [
  {
    description: 'Backend health is less than 1 for 5 minutes'
    evaluationFrequency: 'PT1M'
    metricName: 'HealthyHostCount'
    name: 'health'
    operator: 'LessThan'
    severity: 0
    threshold: 1
    timeAggregation: 'Average'
    windowSize: 'PT5M'
  }
  {
    description: 'Failed requests are greater than 10 for 5 minutes'
    evaluationFrequency: 'PT1M'
    metricName: 'FailedRequests'
    name: 'failedrequests'
    operator: 'GreaterThan'
    severity: 1
    threshold: 10
    timeAggregation: 'Total'
    windowSize: 'PT5M'
  }
  {
    description: 'Requests are greater than 1000 for 5 minutes'
    evaluationFrequency: 'PT1M'
    metricName: 'TotalRequests'
    name: 'requests'
    operator: 'GreaterThan'
    severity: 2
    threshold: 1000
    timeAggregation: 'Total'
    windowSize: 'PT5M'
  }
]

@description('The Resource logs to enable')
var logs = [
  'ApplicationGatewayAccessLog'
  'ApplicationGatewayFirewallLog'
  'ApplicationGatewayPerformanceLog'
]

@description('The Resource Name')
var name = '${serviceType}-${resourceSuffix}'

@description('The Resource Service Type token')
var serviceType = 'agw'

/** Outputs **/
@description('The Resource Id')
output id string = main.id

/** Resources **/
resource main 'Microsoft.Network/applicationGateways@2023-05-01' = {
  name: name
  location: location
  tags: tags

  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${uaiId}': {}
    }
  }

  properties: {
    backendSettingsCollection: []
    customErrorConfigurations: []
    enableHttp2: false
    listeners: []
    loadDistributionPolicies: []
    privateLinkConfigurations: []
    redirectConfigurations: []
    rewriteRuleSets: []
    routingRules: []
    sslProfiles: []
    trustedClientCertificates: []
    trustedRootCertificates: []
    urlPathMaps: []

    autoscaleConfiguration: {
      maxCapacity: 3
      minCapacity: 0
    }

    backendAddressPools: [
      {
        name: 'default'
        properties: {
          backendAddresses: []
        }
      }
    ]

    backendHttpSettingsCollection: [
      {
        name: 'default'

        properties: {
          cookieBasedAffinity: 'Disabled'
          pickHostNameFromBackendAddress: false
          port: 80
          protocol: 'Http'
          requestTimeout: 30
        }
      }
    ]

    frontendIPConfigurations: [
      {
        name: 'default'

        properties: {
          privateIPAllocationMethod: 'Dynamic'
          publicIPAddress: {
            id: pip.id
          }
        }
      }
    ]

    frontendPorts: [
      {
        name: 'http'

        properties: {
          port: 80
        }
      }
      {
        name: 'https'

        properties: {
          port: 443
        }
      }
    ]

    gatewayIPConfigurations: [
      {
        name: 'default'
        properties: {
          subnet: {
            id: subnetId
          }
        }
      }
    ]

    httpListeners: [
      {
        name: 'default'
        properties: {
          protocol: 'Http'
          requireServerNameIndication: false

          frontendIPConfiguration: {
            id: resourceId('Microsoft.Network/applicationGateways/frontendIPConfigurations', name, 'default')
          }

          frontendPort: {
            id: resourceId('Microsoft.Network/applicationGateways/frontendPorts', name, 'http')
          }
        }
      }
    ]

    requestRoutingRules: [
      {
        name: 'default'

        properties: {
          priority: 100
          ruleType: 'Basic'

          backendAddressPool: {
            id: resourceId('Microsoft.Network/applicationGateways/backendAddressPools', name, 'default')
          }

          backendHttpSettings: {
            id: resourceId('Microsoft.Network/applicationGateways/backendHttpSettingsCollection', name, 'default')
          }

          httpListener: {
            id: resourceId('Microsoft.Network/applicationGateways/httpListeners', name, 'default')
          }
        }
      }
    ]

    sku: {
      name: 'WAF_v2'
      tier: 'WAF_v2'
    }

    sslPolicy: {
      policyType: 'Predefined'
      policyName: 'AppGwSslPolicy20170401S'
    }

    webApplicationFirewallConfiguration: {
      disabledRuleGroups: []
      enabled: true
      fileUploadLimitInMb: 100
      firewallMode: 'Prevention'
      maxRequestBodySizeInKb: 128
      requestBodyCheck: true
      ruleSetType: 'OWASP'
      ruleSetVersion: '3.2'
    }
  }
}

@description('Diagnostic settings for the resource')
resource diagnostics 'Microsoft.Insights/diagnosticSettings@2017-05-01-preview' = {
  scope: main
  name: 'diag-${serviceType}'
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: [for log in logs: {
      category: log
      enabled: true
    }]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}

resource pip 'Microsoft.Network/publicIPAddresses@2023-05-01' = {
  location: location
  name: 'pip-${name}'
  tags: tags

  properties: {
    idleTimeoutInMinutes: 4
    ipTags: []
    publicIPAddressVersion: 'IPv4'
    publicIPAllocationMethod: 'Static'

    ddosSettings: {
      protectionMode: 'VirtualNetworkInherited'
    }
  }

  sku: {
    name: 'Standard'
    tier: 'Regional'
  }
}

/** Nested Modules **/
@description('Metric alerts for the resource')
module metricAlerts 'utility/metricAlerts.bicep' = {
  name: 'alert-${main.name}-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    alerts: alerts
    metricNamespace: 'Microsoft.Network/ApplicationGateways'
    nameSuffix: name
    serviceId: main.id
    tags: tags
  }
}

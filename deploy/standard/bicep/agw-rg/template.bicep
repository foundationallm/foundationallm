param publicIPAddresses_EUS_FLLM_DEMO_AGW_api_pip_name string = 'EUS-FLLM-DEMO-AGW-api-pip'
param publicIPAddresses_EUS_FLLM_DEMO_AGW_www_pip_name string = 'EUS-FLLM-DEMO-AGW-www-pip'
param applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name string = 'EUS-FLLM-DEMO-AGW-api-agw'
param applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name string = 'EUS-FLLM-DEMO-AGW-www-agw'
param metricAlerts_EUS_FLLM_DEMO_AGW_api_agw_requests_alert_name string = 'EUS-FLLM-DEMO-AGW-api-agw-requests-alert'
param metricAlerts_EUS_FLLM_DEMO_AGW_www_agw_requests_alert_name string = 'EUS-FLLM-DEMO-AGW-www-agw-requests-alert'
param userAssignedIdentities_EUS_FLLM_DEMO_AGW_agw_uai_name string = 'EUS-FLLM-DEMO-AGW-agw-uai'
param metricAlerts_EUS_FLLM_DEMO_AGW_api_agw_backendhealth_alert_name string = 'EUS-FLLM-DEMO-AGW-api-agw-backendhealth-alert'
param metricAlerts_EUS_FLLM_DEMO_AGW_www_agw_backendhealth_alert_name string = 'EUS-FLLM-DEMO-AGW-www-agw-backendhealth-alert'
param metricAlerts_EUS_FLLM_DEMO_AGW_api_agw_failedrequests_alert_name string = 'EUS-FLLM-DEMO-AGW-api-agw-failedrequests-alert'
param metricAlerts_EUS_FLLM_DEMO_AGW_www_agw_failedrequests_alert_name string = 'EUS-FLLM-DEMO-AGW-www-agw-failedrequests-alert'
param actionGroups_EUS_FLLM_DEMO_OPS_ag_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-OPS-rg/providers/Microsoft.Insights/actionGroups/EUS-FLLM-DEMO-OPS-ag'
param virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-NET-rg/providers/Microsoft.Network/virtualNetworks/EUS-FLLM-DEMO-NET-vnet'

resource userAssignedIdentities_EUS_FLLM_DEMO_AGW_agw_uai_name_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: userAssignedIdentities_EUS_FLLM_DEMO_AGW_agw_uai_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'FoundationaLLM-Platform'
  }
}

resource publicIPAddresses_EUS_FLLM_DEMO_AGW_api_pip_name_resource 'Microsoft.Network/publicIPAddresses@2023-05-01' = {
  name: publicIPAddresses_EUS_FLLM_DEMO_AGW_api_pip_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'FoundationaLLM-Platform'
  }
  sku: {
    name: 'Standard'
    tier: 'Regional'
  }
  properties: {
    ipAddress: '172.191.1.103'
    publicIPAddressVersion: 'IPv4'
    publicIPAllocationMethod: 'Static'
    idleTimeoutInMinutes: 4
    ipTags: []
    ddosSettings: {
      protectionMode: 'VirtualNetworkInherited'
    }
  }
}

resource publicIPAddresses_EUS_FLLM_DEMO_AGW_www_pip_name_resource 'Microsoft.Network/publicIPAddresses@2023-05-01' = {
  name: publicIPAddresses_EUS_FLLM_DEMO_AGW_www_pip_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'FoundationaLLM-Platform'
  }
  sku: {
    name: 'Standard'
    tier: 'Regional'
  }
  properties: {
    ipAddress: '172.191.56.153'
    publicIPAddressVersion: 'IPv4'
    publicIPAllocationMethod: 'Static'
    idleTimeoutInMinutes: 4
    ipTags: []
    ddosSettings: {
      protectionMode: 'VirtualNetworkInherited'
    }
  }
}

resource metricAlerts_EUS_FLLM_DEMO_AGW_api_agw_backendhealth_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_AGW_api_agw_backendhealth_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    description: 'Backend health is less than 1 for 5 minutes'
    severity: 0
    enabled: true
    scopes: [
      applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          threshold: 1
          name: 'Metric1'
          metricNamespace: 'Microsoft.Network/ApplicationGateways'
          metricName: 'HealthyHostCount'
          operator: 'LessThan'
          timeAggregation: 'Average'
          skipMetricValidation: false
          criterionType: 'StaticThresholdCriterion'
        }
      ]
      'odata.type': 'Microsoft.Azure.Monitor.MultipleResourceMultipleMetricCriteria'
    }
    autoMitigate: true
    actions: [
      {
        actionGroupId: actionGroups_EUS_FLLM_DEMO_OPS_ag_externalid
        webHookProperties: {}
      }
    ]
  }
}

resource metricAlerts_EUS_FLLM_DEMO_AGW_api_agw_failedrequests_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_AGW_api_agw_failedrequests_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    description: 'Failed requests are greater than 10 for 5 minutes'
    severity: 1
    enabled: true
    scopes: [
      applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          threshold: 10
          name: 'Metric1'
          metricNamespace: 'Microsoft.Network/ApplicationGateways'
          metricName: 'FailedRequests'
          operator: 'GreaterThan'
          timeAggregation: 'Total'
          skipMetricValidation: false
          criterionType: 'StaticThresholdCriterion'
        }
      ]
      'odata.type': 'Microsoft.Azure.Monitor.MultipleResourceMultipleMetricCriteria'
    }
    autoMitigate: true
    actions: [
      {
        actionGroupId: actionGroups_EUS_FLLM_DEMO_OPS_ag_externalid
        webHookProperties: {}
      }
    ]
  }
}

resource metricAlerts_EUS_FLLM_DEMO_AGW_api_agw_requests_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_AGW_api_agw_requests_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    description: 'Requests are greater than 1000 for 5 minutes'
    severity: 2
    enabled: true
    scopes: [
      applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          threshold: 1000
          name: 'Metric1'
          metricNamespace: 'Microsoft.Network/ApplicationGateways'
          metricName: 'TotalRequests'
          operator: 'GreaterThan'
          timeAggregation: 'Total'
          skipMetricValidation: false
          criterionType: 'StaticThresholdCriterion'
        }
      ]
      'odata.type': 'Microsoft.Azure.Monitor.MultipleResourceMultipleMetricCriteria'
    }
    autoMitigate: true
    actions: [
      {
        actionGroupId: actionGroups_EUS_FLLM_DEMO_OPS_ag_externalid
        webHookProperties: {}
      }
    ]
  }
}

resource metricAlerts_EUS_FLLM_DEMO_AGW_www_agw_backendhealth_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_AGW_www_agw_backendhealth_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    description: 'Backend health is less than 1 for 5 minutes'
    severity: 0
    enabled: true
    scopes: [
      applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          threshold: 1
          name: 'Metric1'
          metricNamespace: 'Microsoft.Network/ApplicationGateways'
          metricName: 'HealthyHostCount'
          operator: 'LessThan'
          timeAggregation: 'Average'
          skipMetricValidation: false
          criterionType: 'StaticThresholdCriterion'
        }
      ]
      'odata.type': 'Microsoft.Azure.Monitor.MultipleResourceMultipleMetricCriteria'
    }
    autoMitigate: true
    actions: [
      {
        actionGroupId: actionGroups_EUS_FLLM_DEMO_OPS_ag_externalid
        webHookProperties: {}
      }
    ]
  }
}

resource metricAlerts_EUS_FLLM_DEMO_AGW_www_agw_failedrequests_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_AGW_www_agw_failedrequests_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    description: 'Failed requests are greater than 10 for 5 minutes'
    severity: 1
    enabled: true
    scopes: [
      applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          threshold: 10
          name: 'Metric1'
          metricNamespace: 'Microsoft.Network/ApplicationGateways'
          metricName: 'FailedRequests'
          operator: 'GreaterThan'
          timeAggregation: 'Total'
          skipMetricValidation: false
          criterionType: 'StaticThresholdCriterion'
        }
      ]
      'odata.type': 'Microsoft.Azure.Monitor.MultipleResourceMultipleMetricCriteria'
    }
    autoMitigate: true
    actions: [
      {
        actionGroupId: actionGroups_EUS_FLLM_DEMO_OPS_ag_externalid
        webHookProperties: {}
      }
    ]
  }
}

resource metricAlerts_EUS_FLLM_DEMO_AGW_www_agw_requests_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_AGW_www_agw_requests_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    description: 'Requests are greater than 1000 for 5 minutes'
    severity: 2
    enabled: true
    scopes: [
      applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          threshold: 1000
          name: 'Metric1'
          metricNamespace: 'Microsoft.Network/ApplicationGateways'
          metricName: 'TotalRequests'
          operator: 'GreaterThan'
          timeAggregation: 'Total'
          skipMetricValidation: false
          criterionType: 'StaticThresholdCriterion'
        }
      ]
      'odata.type': 'Microsoft.Azure.Monitor.MultipleResourceMultipleMetricCriteria'
    }
    autoMitigate: true
    actions: [
      {
        actionGroupId: actionGroups_EUS_FLLM_DEMO_OPS_ag_externalid
        webHookProperties: {}
      }
    ]
  }
}

resource applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource 'Microsoft.Network/applicationGateways@2023-05-01' = {
  name: applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'FoundationaLLM-Platform'
    'managed-by-k8s-ingress': '1.5.3/94b2b229/2023-02-03-21:42T+0000'
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourcegroups/EUS-FLLM-DEMO-AGW-rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/EUS-FLLM-DEMO-AGW-agw-uai': {}
    }
  }
  properties: {
    sku: {
      name: 'WAF_v2'
      tier: 'WAF_v2'
    }
    gatewayIPConfigurations: [
      {
        name: 'default'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id}/gatewayIPConfigurations/default'
        properties: {
          subnet: {
            id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/AppGateway'
          }
        }
      }
    ]
    sslCertificates: [
      {
        name: 'default'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id}/sslCertificates/default'
        properties: {
          keyVaultSecretId: 'https://eus-fllm-demo-ops-kv.vault.azure.net/secrets/api-internal-foundationallm-ai'
        }
      }
    ]
    trustedRootCertificates: []
    trustedClientCertificates: []
    sslProfiles: []
    frontendIPConfigurations: [
      {
        name: 'default'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id}/frontendIPConfigurations/default'
        properties: {
          privateIPAllocationMethod: 'Dynamic'
          publicIPAddress: {
            id: publicIPAddresses_EUS_FLLM_DEMO_AGW_api_pip_name_resource.id
          }
        }
      }
    ]
    frontendPorts: [
      {
        name: 'fp-443'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id}/frontendPorts/fp-443'
        properties: {
          port: 443
        }
      }
    ]
    backendAddressPools: [
      {
        name: 'defaultaddresspool'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id}/backendAddressPools/defaultaddresspool'
        properties: {
          backendAddresses: []
        }
      }
      {
        name: 'pool-default-foundationallm-core-api-80-bp-80'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id}/backendAddressPools/pool-default-foundationallm-core-api-80-bp-80'
        properties: {
          backendAddresses: [
            {
              ipAddress: '10.0.16.117'
            }
          ]
        }
      }
    ]
    loadDistributionPolicies: []
    backendHttpSettingsCollection: [
      {
        name: 'bp-default-foundationallm-core-api-80-80-foundationallm-core-api'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id}/backendHttpSettingsCollection/bp-default-foundationallm-core-api-80-80-foundationallm-core-api'
        properties: {
          port: 80
          protocol: 'Http'
          cookieBasedAffinity: 'Disabled'
          pickHostNameFromBackendAddress: false
          requestTimeout: 600
          probe: {
            id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id}/probes/pb-default-foundationallm-core-api-80-foundationallm-core-api'
          }
        }
      }
      {
        name: 'defaulthttpsetting'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id}/backendHttpSettingsCollection/defaulthttpsetting'
        properties: {
          port: 80
          protocol: 'Http'
          cookieBasedAffinity: 'Disabled'
          pickHostNameFromBackendAddress: false
          requestTimeout: 30
          probe: {
            id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id}/probes/defaultprobe-Http'
          }
        }
      }
    ]
    backendSettingsCollection: []
    httpListeners: [
      {
        name: 'fl-beab5374965cfe9c77c0bd525f7d2e60'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id}/httpListeners/fl-beab5374965cfe9c77c0bd525f7d2e60'
        properties: {
          frontendIPConfiguration: {
            id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id}/frontendIPConfigurations/default'
          }
          frontendPort: {
            id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id}/frontendPorts/fp-443'
          }
          protocol: 'Https'
          sslCertificate: {
            id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id}/sslCertificates/default'
          }
          hostNames: [
            'api.internal.foundationallm.ai'
          ]
          requireServerNameIndication: false
        }
      }
    ]
    listeners: []
    urlPathMaps: []
    requestRoutingRules: [
      {
        name: 'rr-beab5374965cfe9c77c0bd525f7d2e60'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id}/requestRoutingRules/rr-beab5374965cfe9c77c0bd525f7d2e60'
        properties: {
          ruleType: 'Basic'
          priority: 19000
          httpListener: {
            id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id}/httpListeners/fl-beab5374965cfe9c77c0bd525f7d2e60'
          }
          backendAddressPool: {
            id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id}/backendAddressPools/pool-default-foundationallm-core-api-80-bp-80'
          }
          backendHttpSettings: {
            id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id}/backendHttpSettingsCollection/bp-default-foundationallm-core-api-80-80-foundationallm-core-api'
          }
        }
      }
    ]
    routingRules: []
    probes: [
      {
        name: 'defaultprobe-Http'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id}/probes/defaultprobe-Http'
        properties: {
          protocol: 'Http'
          host: 'localhost'
          path: '/'
          interval: 30
          timeout: 30
          unhealthyThreshold: 3
          pickHostNameFromBackendHttpSettings: false
          minServers: 0
          match: {}
        }
      }
      {
        name: 'defaultprobe-Https'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id}/probes/defaultprobe-Https'
        properties: {
          protocol: 'Https'
          host: 'localhost'
          path: '/'
          interval: 30
          timeout: 30
          unhealthyThreshold: 3
          pickHostNameFromBackendHttpSettings: false
          minServers: 0
          match: {}
        }
      }
      {
        name: 'pb-default-foundationallm-core-api-80-foundationallm-core-api'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_name_resource.id}/probes/pb-default-foundationallm-core-api-80-foundationallm-core-api'
        properties: {
          protocol: 'Http'
          host: 'api.internal.foundationallm.ai'
          port: 80
          path: '/status'
          interval: 3
          timeout: 1
          unhealthyThreshold: 3
          pickHostNameFromBackendHttpSettings: false
          minServers: 0
          match: {}
        }
      }
    ]
    rewriteRuleSets: []
    redirectConfigurations: []
    privateLinkConfigurations: []
    sslPolicy: {
      policyType: 'Predefined'
      policyName: 'AppGwSslPolicy20170401S'
    }
    webApplicationFirewallConfiguration: {
      enabled: true
      firewallMode: 'Prevention'
      ruleSetType: 'OWASP'
      ruleSetVersion: '3.1'
      disabledRuleGroups: []
      requestBodyCheck: true
      maxRequestBodySizeInKb: 128
      fileUploadLimitInMb: 100
    }
    enableHttp2: false
    autoscaleConfiguration: {
      minCapacity: 0
      maxCapacity: 3
    }
    customErrorConfigurations: []
  }
}

resource applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource 'Microsoft.Network/applicationGateways@2023-05-01' = {
  name: applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'FoundationaLLM-Platform'
    'managed-by-k8s-ingress': '1.5.3/94b2b229/2023-02-03-21:42T+0000'
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourcegroups/EUS-FLLM-DEMO-AGW-rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/EUS-FLLM-DEMO-AGW-agw-uai': {}
    }
  }
  properties: {
    sku: {
      name: 'WAF_v2'
      tier: 'WAF_v2'
    }
    gatewayIPConfigurations: [
      {
        name: 'default'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id}/gatewayIPConfigurations/default'
        properties: {
          subnet: {
            id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/AppGateway'
          }
        }
      }
    ]
    sslCertificates: [
      {
        name: 'default'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id}/sslCertificates/default'
        properties: {
          keyVaultSecretId: 'https://eus-fllm-demo-ops-kv.vault.azure.net/secrets/www-internal-foundationallm-ai'
        }
      }
    ]
    trustedRootCertificates: []
    trustedClientCertificates: []
    sslProfiles: []
    frontendIPConfigurations: [
      {
        name: 'default'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id}/frontendIPConfigurations/default'
        properties: {
          privateIPAllocationMethod: 'Dynamic'
          publicIPAddress: {
            id: publicIPAddresses_EUS_FLLM_DEMO_AGW_www_pip_name_resource.id
          }
        }
      }
    ]
    frontendPorts: [
      {
        name: 'fp-443'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id}/frontendPorts/fp-443'
        properties: {
          port: 443
        }
      }
    ]
    backendAddressPools: [
      {
        name: 'defaultaddresspool'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id}/backendAddressPools/defaultaddresspool'
        properties: {
          backendAddresses: []
        }
      }
      {
        name: 'pool-default-foundationallm-web-chat-ui-80-bp-3000'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id}/backendAddressPools/pool-default-foundationallm-web-chat-ui-80-bp-3000'
        properties: {
          backendAddresses: [
            {
              ipAddress: '10.0.12.70'
            }
          ]
        }
      }
    ]
    loadDistributionPolicies: []
    backendHttpSettingsCollection: [
      {
        name: 'bp-default-foundationallm-web-chat-ui-80-3000-foundationallm-web-chat-ui'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id}/backendHttpSettingsCollection/bp-default-foundationallm-web-chat-ui-80-3000-foundationallm-web-chat-ui'
        properties: {
          port: 3000
          protocol: 'Http'
          cookieBasedAffinity: 'Disabled'
          pickHostNameFromBackendAddress: false
          requestTimeout: 600
          probe: {
            id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id}/probes/pb-default-foundationallm-web-chat-ui-80-foundationallm-web-chat-ui'
          }
        }
      }
      {
        name: 'defaulthttpsetting'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id}/backendHttpSettingsCollection/defaulthttpsetting'
        properties: {
          port: 80
          protocol: 'Http'
          cookieBasedAffinity: 'Disabled'
          pickHostNameFromBackendAddress: false
          requestTimeout: 30
          probe: {
            id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id}/probes/defaultprobe-Http'
          }
        }
      }
    ]
    backendSettingsCollection: []
    httpListeners: [
      {
        name: 'fl-e67232e2dbb30acc57b90ee296b3692e'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id}/httpListeners/fl-e67232e2dbb30acc57b90ee296b3692e'
        properties: {
          frontendIPConfiguration: {
            id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id}/frontendIPConfigurations/default'
          }
          frontendPort: {
            id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id}/frontendPorts/fp-443'
          }
          protocol: 'Https'
          sslCertificate: {
            id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id}/sslCertificates/default'
          }
          hostNames: [
            'www.internal.foundationallm.ai'
          ]
          requireServerNameIndication: false
        }
      }
    ]
    listeners: []
    urlPathMaps: []
    requestRoutingRules: [
      {
        name: 'rr-e67232e2dbb30acc57b90ee296b3692e'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id}/requestRoutingRules/rr-e67232e2dbb30acc57b90ee296b3692e'
        properties: {
          ruleType: 'Basic'
          priority: 19000
          httpListener: {
            id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id}/httpListeners/fl-e67232e2dbb30acc57b90ee296b3692e'
          }
          backendAddressPool: {
            id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id}/backendAddressPools/pool-default-foundationallm-web-chat-ui-80-bp-3000'
          }
          backendHttpSettings: {
            id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id}/backendHttpSettingsCollection/bp-default-foundationallm-web-chat-ui-80-3000-foundationallm-web-chat-ui'
          }
        }
      }
    ]
    routingRules: []
    probes: [
      {
        name: 'defaultprobe-Http'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id}/probes/defaultprobe-Http'
        properties: {
          protocol: 'Http'
          host: 'localhost'
          path: '/'
          interval: 30
          timeout: 30
          unhealthyThreshold: 3
          pickHostNameFromBackendHttpSettings: false
          minServers: 0
          match: {}
        }
      }
      {
        name: 'defaultprobe-Https'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id}/probes/defaultprobe-Https'
        properties: {
          protocol: 'Https'
          host: 'localhost'
          path: '/'
          interval: 30
          timeout: 30
          unhealthyThreshold: 3
          pickHostNameFromBackendHttpSettings: false
          minServers: 0
          match: {}
        }
      }
      {
        name: 'pb-default-foundationallm-web-chat-ui-80-foundationallm-web-chat-ui'
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_name_resource.id}/probes/pb-default-foundationallm-web-chat-ui-80-foundationallm-web-chat-ui'
        properties: {
          protocol: 'Http'
          host: 'www.internal.foundationallm.ai'
          port: 3000
          path: '/status'
          interval: 3
          timeout: 1
          unhealthyThreshold: 3
          pickHostNameFromBackendHttpSettings: false
          minServers: 0
          match: {}
        }
      }
    ]
    rewriteRuleSets: []
    redirectConfigurations: []
    privateLinkConfigurations: []
    sslPolicy: {
      policyType: 'Predefined'
      policyName: 'AppGwSslPolicy20170401S'
    }
    webApplicationFirewallConfiguration: {
      enabled: true
      firewallMode: 'Prevention'
      ruleSetType: 'OWASP'
      ruleSetVersion: '3.1'
      disabledRuleGroups: []
      requestBodyCheck: true
      maxRequestBodySizeInKb: 128
      fileUploadLimitInMb: 100
    }
    enableHttp2: false
    autoscaleConfiguration: {
      minCapacity: 0
      maxCapacity: 3
    }
    customErrorConfigurations: []
  }
}
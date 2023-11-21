param searchServices_eusfllmdemovecsearch_name string = 'eusfllmdemovecsearch'
param privateEndpoints_EUS_FLLM_DEMO_VEC_search_pe_name string = 'EUS-FLLM-DEMO-VEC-search-pe'
param metricAlerts_EUS_FLLM_DEMO_VEC_search_latency_alert_name string = 'EUS-FLLM-DEMO-VEC-search-latency-alert'
param metricAlerts_EUS_FLLM_DEMO_VEC_search_throttling_alert_name string = 'EUS-FLLM-DEMO-VEC-search-throttling-alert'
param actionGroups_EUS_FLLM_DEMO_OPS_ag_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-OPS-rg/providers/Microsoft.Insights/actionGroups/EUS-FLLM-DEMO-OPS-ag'
param virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-NET-rg/providers/Microsoft.Network/virtualNetworks/EUS-FLLM-DEMO-NET-vnet'
param privateDnsZones_privatelink_search_windows_net_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DNS-rg/providers/Microsoft.Network/privateDnsZones/privatelink.search.windows.net'

resource searchServices_eusfllmdemovecsearch_name_resource 'Microsoft.Search/searchServices@2023-11-01' = {
  name: searchServices_eusfllmdemovecsearch_name
  location: 'East US'
  sku: {
    name: 'standard'
  }
  properties: {
    replicaCount: 1
    partitionCount: 1
    hostingMode: 'default'
    publicNetworkAccess: 'disabled'
    networkRuleSet: {
      ipRules: []
    }
    encryptionWithCmk: {
      enforcement: 'Disabled'
    }
    disableLocalAuth: false
    authOptions: {
      apiKeyOnly: {}
    }
    semanticSearch: 'disabled'
  }
}

resource metricAlerts_EUS_FLLM_DEMO_VEC_search_latency_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_VEC_search_latency_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Vectorization'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    description: 'Service latency greater than 5s for 1 hour'
    severity: 0
    enabled: true
    scopes: [
      searchServices_eusfllmdemovecsearch_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT1H'
    criteria: {
      allOf: [
        {
          threshold: 5
          name: 'Metric1'
          metricNamespace: 'Microsoft.Search/searchServices'
          metricName: 'SearchLatency'
          operator: 'GreaterThan'
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

resource metricAlerts_EUS_FLLM_DEMO_VEC_search_throttling_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_VEC_search_throttling_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Vectorization'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    description: 'Service throttled search queries greater than 25% for 1 hour'
    severity: 0
    enabled: true
    scopes: [
      searchServices_eusfllmdemovecsearch_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT1H'
    criteria: {
      allOf: [
        {
          threshold: 25
          name: 'Metric1'
          metricNamespace: 'Microsoft.Search/searchServices'
          metricName: 'SearchLatency'
          operator: 'GreaterThan'
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

resource privateEndpoints_EUS_FLLM_DEMO_VEC_search_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_VEC_search_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Vectorization'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'EUS-FLLM-DEMO-VEC-search-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_VEC_search_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-VEC-search-connection'
        properties: {
          privateLinkServiceId: searchServices_eusfllmdemovecsearch_name_resource.id
          groupIds: [
            'searchService'
          ]
          privateLinkServiceConnectionState: {
            status: 'Approved'
            description: 'Auto-approved'
            actionsRequired: 'None'
          }
        }
      }
    ]
    manualPrivateLinkServiceConnections: []
    subnet: {
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/Vectorization'
    }
    ipConfigurations: []
    customDnsConfigs: []
  }
}

resource privateEndpoints_EUS_FLLM_DEMO_VEC_search_pe_name_search 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_EUS_FLLM_DEMO_VEC_search_pe_name}/search'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink.search.windows.net'
        properties: {
          privateDnsZoneId: privateDnsZones_privatelink_search_windows_net_externalid
        }
      }
    ]
  }
  dependsOn: [
    privateEndpoints_EUS_FLLM_DEMO_VEC_search_pe_name_resource
  ]
}

resource searchServices_eusfllmdemovecsearch_name_EUS_FLLM_DEMO_VEC_search_pe_ac63a4e7_885b_4458_855d_8501f6018cf8 'Microsoft.Search/searchServices/privateEndpointConnections@2023-11-01' = {
  parent: searchServices_eusfllmdemovecsearch_name_resource
  name: 'EUS-FLLM-DEMO-VEC-search-pe.ac63a4e7-885b-4458-855d-8501f6018cf8'
  properties: {
    privateEndpoint: {
      id: privateEndpoints_EUS_FLLM_DEMO_VEC_search_pe_name_resource.id
    }
    privateLinkServiceConnectionState: {
      status: 'Approved'
      description: 'Auto-approved'
      actionsRequired: 'None'
    }
    provisioningState: 'Succeeded'
    groupId: 'searchService'
  }
}
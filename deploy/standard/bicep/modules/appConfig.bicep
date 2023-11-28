param environmentName string
param location string
param workload string
param project string

var name = 'appconfig-${environmentName}-${location}-${workload}-${project}'

var tags= {
  Environment: environmentName
  IaC: 'Bicep'
  Project: project
  Purpose: 'DevOps'
}

resource main 'Microsoft.AppConfiguration/configurationStores@2023-03-01' = {
  name: name
  location: location
  tags: tags

  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-OPS-rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/EUS-FLLM-DEMO-OPS-mi': {}
    }
  }

  properties: {
    disableLocalAuth: false
    enablePurgeProtection: true
    encryption: {}
    publicNetworkAccess: 'Disabled'
    softDeleteRetentionInDays: 1
  }

  sku: {
    name: 'standard'
  }
}

resource uai 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  location: location
  name: 'uai-${name}'
  tags: tags
}


resource privateEndpoints_EUS_FLLM_DEMO_OPS_appconfig_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_OPS_appconfig_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'DevOps'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'EUS-FLLM-DEMO-OPS-appconfig-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_OPS_appconfig_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-OPS-appconfig-connection'
        properties: {
          privateLinkServiceId: configurationStores_eus_fllm_demo_ops_appconfig_name_resource.id
          groupIds: [
            'configurationStores'
          ]
          privateLinkServiceConnectionState: {
            status: 'Approved'
            description: 'Auto-Approved'
            actionsRequired: 'None'
          }
        }
      }
    ]
    manualPrivateLinkServiceConnections: []
    subnet: {
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/ops'
    }
    ipConfigurations: []
    customDnsConfigs: []
  }
}

resource configurationStores_eus_fllm_demo_ops_appconfig_name_configurationStores_eus_fllm_demo_ops_appconfig_name_connection 'Microsoft.AppConfiguration/configurationStores/privateEndpointConnections@2023-03-01' = {
  parent: configurationStores_eus_fllm_demo_ops_appconfig_name_resource
  name: '${configurationStores_eus_fllm_demo_ops_appconfig_name}-connection'
  properties: {
    privateEndpoint: {
      id: privateEndpoints_EUS_FLLM_DEMO_OPS_appconfig_pe_name_resource.id
    }
    privateLinkServiceConnectionState: {
      status: 'Approved'
      description: 'Auto-Approved'
    }
  }
}

resource metricAlerts_EUS_FLLM_DEMO_OPS_appconfig_latency_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_OPS_appconfig_latency_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'DevOps'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    description: 'Service request latency greater than 1000ms for 1 hour'
    severity: 0
    enabled: true
    scopes: [
      configurationStores_eus_fllm_demo_ops_appconfig_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT1H'
    criteria: {
      allOf: [
        {
          threshold: 1000
          name: 'Metric1'
          metricNamespace: 'Microsoft.AppConfiguration/configurationStores'
          metricName: 'DailyStorageUsage'
          operator: 'GreaterThan'
          timeAggregation: 'Maximum'
          skipMetricValidation: false
          criterionType: 'StaticThresholdCriterion'
        }
      ]
      'odata.type': 'Microsoft.Azure.Monitor.MultipleResourceMultipleMetricCriteria'
    }
    autoMitigate: true
    actions: [
      {
        actionGroupId: actionGroups_EUS_FLLM_DEMO_OPS_ag_name_resource.id
        webHookProperties: {}
      }
    ]
  }
}

resource metricAlerts_EUS_FLLM_DEMO_OPS_appconfig_storageUsage_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_OPS_appconfig_storageUsage_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'DevOps'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    description: 'Service maximum storage usage greater than 75% for 1 hour'
    severity: 0
    enabled: true
    scopes: [
      configurationStores_eus_fllm_demo_ops_appconfig_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT1H'
    criteria: {
      allOf: [
        {
          threshold: 75
          name: 'Metric1'
          metricNamespace: 'Microsoft.AppConfiguration/configurationStores'
          metricName: 'DailyStorageUsage'
          operator: 'GreaterThan'
          timeAggregation: 'Maximum'
          skipMetricValidation: false
          criterionType: 'StaticThresholdCriterion'
        }
      ]
      'odata.type': 'Microsoft.Azure.Monitor.MultipleResourceMultipleMetricCriteria'
    }
    autoMitigate: true
    actions: [
      {
        actionGroupId: actionGroups_EUS_FLLM_DEMO_OPS_ag_name_resource.id
        webHookProperties: {}
      }
    ]
  }
}


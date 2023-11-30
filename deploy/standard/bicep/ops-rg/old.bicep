param registries_EUSFLLMDEMOOPScr_name string = 'EUSFLLMDEMOOPScr'
param privateEndpoints_EFLLMdOPS_grafana_pe_name string = 'EFLLMdOPS-grafana-pe'
param workspaces_EUS_FLLM_DEMO_OPS_la_name string = 'EUS-FLLM-DEMO-OPS-la'
param privateEndpoints_EUS_FLLM_DEMO_OPS_registry_pe_name string = 'EUS-FLLM-DEMO-OPS-registry-pe'
param metricAlerts_EUS_FLLM_DEMO_OPS_ado_vmss_cpu_alert_name string = 'EUS-FLLM-DEMO-OPS-ado-vmss-cpu-alert'
param metricAlerts_EUS_FLLM_DEMO_OPS_tfca_aci_cpu_alert_name string = 'EUS-FLLM-DEMO-OPS-tfca-aci-cpu-alert'
param metricAlerts_EUS_FLLM_DEMO_OPS_tfca_aci_ram_alert_name string = 'EUS-FLLM-DEMO-OPS-tfca-aci-ram-alert'
param metricAlerts_EUS_FLLM_DEMO_OPS_ado_vmss_disk_alert_name string = 'EUS-FLLM-DEMO-OPS-ado-vmss-disk-alert'
param smartdetectoralertrules_failure_anomalies_eus_fllm_demo_ops_ai_name string = 'failure anomalies - eus-fllm-demo-ops-ai'
param virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-NET-rg/providers/Microsoft.Network/virtualNetworks/EUS-FLLM-DEMO-NET-vnet'
param privateDnsZones_privatelink_grafana_azure_com_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DNS-rg/providers/Microsoft.Network/privateDnsZones/privatelink.grafana.azure.com'
param privateDnsZones_privatelink_azurecr_io_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DNS-rg/providers/Microsoft.Network/privateDnsZones/privatelink.azurecr.io'
param privateDnsZones_eastus_privatelink_azurecr_io_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DNS-rg/providers/Microsoft.Network/privateDnsZones/eastus.privatelink.azurecr.io'




resource registries_EUSFLLMDEMOOPScr_name_resource 'Microsoft.ContainerRegistry/registries@2023-08-01-preview' = {
  name: registries_EUSFLLMDEMOOPScr_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'DevOps'
    Workspace: 'foundationallm-ops'
  }
  sku: {
    name: 'Premium'
    tier: 'Premium'
  }
  properties: {
    adminUserEnabled: true
    networkRuleSet: {
      defaultAction: 'Deny'
      ipRules: []
    }
    policies: {
      quarantinePolicy: {
        status: 'disabled'
      }
      trustPolicy: {
        type: 'Notary'
        status: 'enabled'
      }
      retentionPolicy: {
        days: 30
        status: 'enabled'
      }
      exportPolicy: {
        status: 'enabled'
      }
      azureADAuthenticationAsArmPolicy: {
        status: 'enabled'
      }
      softDeletePolicy: {
        retentionDays: 30
        status: 'enabled'
      }
    }
    encryption: {
      status: 'disabled'
    }
    dataEndpointEnabled: false
    publicNetworkAccess: 'Disabled'
    networkRuleBypassOptions: 'AzureServices'
    zoneRedundancy: 'Disabled'
    anonymousPullEnabled: true
  }
}

resource registries_EUSFLLMDEMOOPScr_name_fllmagentpool 'Microsoft.ContainerRegistry/registries/agentPools@2019-06-01-preview' = {
  parent: registries_EUSFLLMDEMOOPScr_name_resource
  name: 'fllmagentpool'
  location: 'eastus'
  properties: {
    count: 2
    tier: 'S1'
    os: 'Linux'
    virtualNetworkSubnetResourceId: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/ado'
  }
}

resource registries_EUSFLLMDEMOOPScr_name_repositories_admin 'Microsoft.ContainerRegistry/registries/scopeMaps@2023-08-01-preview' = {
  parent: registries_EUSFLLMDEMOOPScr_name_resource
  name: '_repositories_admin'
  properties: {
    description: 'Can perform all read, write and delete operations on the registry'
    actions: [
      'repositories/*/metadata/read'
      'repositories/*/metadata/write'
      'repositories/*/content/read'
      'repositories/*/content/write'
      'repositories/*/content/delete'
    ]
  }
}

resource registries_EUSFLLMDEMOOPScr_name_repositories_pull 'Microsoft.ContainerRegistry/registries/scopeMaps@2023-08-01-preview' = {
  parent: registries_EUSFLLMDEMOOPScr_name_resource
  name: '_repositories_pull'
  properties: {
    description: 'Can pull any repository of the registry'
    actions: [
      'repositories/*/content/read'
    ]
  }
}

resource registries_EUSFLLMDEMOOPScr_name_repositories_pull_metadata_read 'Microsoft.ContainerRegistry/registries/scopeMaps@2023-08-01-preview' = {
  parent: registries_EUSFLLMDEMOOPScr_name_resource
  name: '_repositories_pull_metadata_read'
  properties: {
    description: 'Can perform all read operations on the registry'
    actions: [
      'repositories/*/content/read'
      'repositories/*/metadata/read'
    ]
  }
}

resource registries_EUSFLLMDEMOOPScr_name_repositories_push 'Microsoft.ContainerRegistry/registries/scopeMaps@2023-08-01-preview' = {
  parent: registries_EUSFLLMDEMOOPScr_name_resource
  name: '_repositories_push'
  properties: {
    description: 'Can push to any repository of the registry'
    actions: [
      'repositories/*/content/read'
      'repositories/*/content/write'
    ]
  }
}

resource registries_EUSFLLMDEMOOPScr_name_repositories_push_metadata_write 'Microsoft.ContainerRegistry/registries/scopeMaps@2023-08-01-preview' = {
  parent: registries_EUSFLLMDEMOOPScr_name_resource
  name: '_repositories_push_metadata_write'
  properties: {
    description: 'Can perform all read and write operations on the registry'
    actions: [
      'repositories/*/metadata/read'
      'repositories/*/metadata/write'
      'repositories/*/content/read'
      'repositories/*/content/write'
    ]
  }
}




















resource privateEndpoints_EUS_FLLM_DEMO_OPS_registry_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_OPS_registry_pe_name
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
        name: 'EUS-FLLM-DEMO-OPS-registry-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_OPS_registry_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-OPS-registry-connection'
        properties: {
          privateLinkServiceId: registries_EUSFLLMDEMOOPScr_name_resource.id
          groupIds: [
            'registry'
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










resource privateEndpoints_EUS_FLLM_DEMO_OPS_registry_pe_name_registry 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_EUS_FLLM_DEMO_OPS_registry_pe_name}/registry'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink.azurecr.io'
        properties: {
          privateDnsZoneId: privateDnsZones_privatelink_azurecr_io_externalid
        }
      }
      {
        name: 'eastus.privatelink.azurecr.io'
        properties: {
          privateDnsZoneId: privateDnsZones_eastus_privatelink_azurecr_io_externalid
        }
      }
    ]
  }
  dependsOn: [
    privateEndpoints_EUS_FLLM_DEMO_OPS_registry_pe_name_resource
  ]
}







resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ADPRequests 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ADPRequests'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ADPRequests'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ADSecurityAssessmentRecommendation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ADSecurityAssessmentRecommendation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ADSecurityAssessmentRecommendation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ADTDataHistoryOperation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ADTDataHistoryOperation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ADTDataHistoryOperation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ADTDigitalTwinsOperation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ADTDigitalTwinsOperation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ADTDigitalTwinsOperation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ADTEventRoutesOperation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ADTEventRoutesOperation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ADTEventRoutesOperation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ADTModelsOperation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ADTModelsOperation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ADTModelsOperation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ADTQueryOperation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ADTQueryOperation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ADTQueryOperation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ADXCommand 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ADXCommand'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ADXCommand'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ADXIngestionBatching 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ADXIngestionBatching'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ADXIngestionBatching'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ADXJournal 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ADXJournal'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ADXJournal'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ADXQuery 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ADXQuery'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ADXQuery'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ADXTableDetails 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ADXTableDetails'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ADXTableDetails'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ADXTableUsageStatistics 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ADXTableUsageStatistics'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ADXTableUsageStatistics'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AegDataPlaneRequests 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AegDataPlaneRequests'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AegDataPlaneRequests'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AegDeliveryFailureLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AegDeliveryFailureLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AegDeliveryFailureLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AegPublishFailureLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AegPublishFailureLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AegPublishFailureLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AEWAssignmentBlobLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AEWAssignmentBlobLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AEWAssignmentBlobLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AEWAuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AEWAuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AEWAuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AEWComputePipelinesLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AEWComputePipelinesLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AEWComputePipelinesLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AFSAuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AFSAuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AFSAuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AgriFoodApplicationAuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AgriFoodApplicationAuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AgriFoodApplicationAuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AgriFoodFarmManagementLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AgriFoodFarmManagementLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AgriFoodFarmManagementLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AgriFoodFarmOperationLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AgriFoodFarmOperationLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AgriFoodFarmOperationLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AgriFoodInsightLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AgriFoodInsightLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AgriFoodInsightLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AgriFoodJobProcessedLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AgriFoodJobProcessedLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AgriFoodJobProcessedLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AgriFoodModelInferenceLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AgriFoodModelInferenceLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AgriFoodModelInferenceLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AgriFoodProviderAuthLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AgriFoodProviderAuthLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AgriFoodProviderAuthLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AgriFoodSatelliteLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AgriFoodSatelliteLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AgriFoodSatelliteLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AgriFoodSensorManagementLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AgriFoodSensorManagementLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AgriFoodSensorManagementLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AgriFoodWeatherLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AgriFoodWeatherLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AgriFoodWeatherLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AGSGrafanaLoginEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AGSGrafanaLoginEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AGSGrafanaLoginEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AGWAccessLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AGWAccessLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AGWAccessLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AGWFirewallLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AGWFirewallLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AGWFirewallLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AGWPerformanceLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AGWPerformanceLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AGWPerformanceLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AHDSDicomAuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AHDSDicomAuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AHDSDicomAuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AHDSDicomDiagnosticLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AHDSDicomDiagnosticLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AHDSDicomDiagnosticLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AHDSMedTechDiagnosticLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AHDSMedTechDiagnosticLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AHDSMedTechDiagnosticLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AirflowDagProcessingLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AirflowDagProcessingLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AirflowDagProcessingLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AKSAudit 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AKSAudit'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AKSAudit'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AKSAuditAdmin 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AKSAuditAdmin'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AKSAuditAdmin'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AKSControlPlane 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AKSControlPlane'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AKSControlPlane'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_Alert 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'Alert'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'Alert'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AmlComputeClusterEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AmlComputeClusterEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AmlComputeClusterEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AmlComputeClusterNodeEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AmlComputeClusterNodeEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AmlComputeClusterNodeEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AmlComputeCpuGpuUtilization 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AmlComputeCpuGpuUtilization'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AmlComputeCpuGpuUtilization'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AmlComputeInstanceEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AmlComputeInstanceEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AmlComputeInstanceEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AmlComputeJobEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AmlComputeJobEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AmlComputeJobEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AmlDataLabelEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AmlDataLabelEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AmlDataLabelEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AmlDataSetEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AmlDataSetEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AmlDataSetEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AmlDataStoreEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AmlDataStoreEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AmlDataStoreEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AmlDeploymentEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AmlDeploymentEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AmlDeploymentEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AmlEnvironmentEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AmlEnvironmentEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AmlEnvironmentEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AmlInferencingEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AmlInferencingEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AmlInferencingEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AmlModelsEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AmlModelsEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AmlModelsEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AmlOnlineEndpointConsoleLog 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AmlOnlineEndpointConsoleLog'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AmlOnlineEndpointConsoleLog'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AmlOnlineEndpointEventLog 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AmlOnlineEndpointEventLog'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AmlOnlineEndpointEventLog'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AmlOnlineEndpointTrafficLog 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AmlOnlineEndpointTrafficLog'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AmlOnlineEndpointTrafficLog'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AmlPipelineEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AmlPipelineEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AmlPipelineEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AmlRegistryReadEventsLog 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AmlRegistryReadEventsLog'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AmlRegistryReadEventsLog'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AmlRegistryWriteEventsLog 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AmlRegistryWriteEventsLog'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AmlRegistryWriteEventsLog'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AmlRunEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AmlRunEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AmlRunEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AmlRunStatusChangedEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AmlRunStatusChangedEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AmlRunStatusChangedEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AMSKeyDeliveryRequests 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AMSKeyDeliveryRequests'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AMSKeyDeliveryRequests'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AMSLiveEventOperations 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AMSLiveEventOperations'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AMSLiveEventOperations'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AMSMediaAccountHealth 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AMSMediaAccountHealth'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AMSMediaAccountHealth'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AMSStreamingEndpointRequests 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AMSStreamingEndpointRequests'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AMSStreamingEndpointRequests'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ANFFileAccess 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ANFFileAccess'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ANFFileAccess'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ApiManagementGatewayLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ApiManagementGatewayLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ApiManagementGatewayLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ApiManagementWebSocketConnectionLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ApiManagementWebSocketConnectionLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ApiManagementWebSocketConnectionLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppAvailabilityResults 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppAvailabilityResults'
  properties: {
    totalRetentionInDays: 90
    plan: 'Analytics'
    schema: {
      name: 'AppAvailabilityResults'
    }
    retentionInDays: 90
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppBrowserTimings 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppBrowserTimings'
  properties: {
    totalRetentionInDays: 90
    plan: 'Analytics'
    schema: {
      name: 'AppBrowserTimings'
    }
    retentionInDays: 90
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppCenterError 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppCenterError'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AppCenterError'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppDependencies 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppDependencies'
  properties: {
    totalRetentionInDays: 90
    plan: 'Analytics'
    schema: {
      name: 'AppDependencies'
    }
    retentionInDays: 90
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppEnvSpringAppConsoleLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppEnvSpringAppConsoleLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AppEnvSpringAppConsoleLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppEvents'
  properties: {
    totalRetentionInDays: 90
    plan: 'Analytics'
    schema: {
      name: 'AppEvents'
    }
    retentionInDays: 90
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppExceptions 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppExceptions'
  properties: {
    totalRetentionInDays: 90
    plan: 'Analytics'
    schema: {
      name: 'AppExceptions'
    }
    retentionInDays: 90
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppMetrics 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppMetrics'
  properties: {
    totalRetentionInDays: 90
    plan: 'Analytics'
    schema: {
      name: 'AppMetrics'
    }
    retentionInDays: 90
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppPageViews 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppPageViews'
  properties: {
    totalRetentionInDays: 90
    plan: 'Analytics'
    schema: {
      name: 'AppPageViews'
    }
    retentionInDays: 90
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppPerformanceCounters 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppPerformanceCounters'
  properties: {
    totalRetentionInDays: 90
    plan: 'Analytics'
    schema: {
      name: 'AppPerformanceCounters'
    }
    retentionInDays: 90
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppPlatformBuildLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppPlatformBuildLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AppPlatformBuildLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppPlatformContainerEventLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppPlatformContainerEventLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AppPlatformContainerEventLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppPlatformIngressLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppPlatformIngressLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AppPlatformIngressLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppPlatformLogsforSpring 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppPlatformLogsforSpring'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AppPlatformLogsforSpring'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppPlatformSystemLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppPlatformSystemLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AppPlatformSystemLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppRequests 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppRequests'
  properties: {
    totalRetentionInDays: 90
    plan: 'Analytics'
    schema: {
      name: 'AppRequests'
    }
    retentionInDays: 90
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppServiceAntivirusScanAuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppServiceAntivirusScanAuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AppServiceAntivirusScanAuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppServiceAppLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppServiceAppLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AppServiceAppLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppServiceAuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppServiceAuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AppServiceAuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppServiceAuthenticationLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppServiceAuthenticationLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AppServiceAuthenticationLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppServiceConsoleLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppServiceConsoleLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AppServiceConsoleLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppServiceEnvironmentPlatformLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppServiceEnvironmentPlatformLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AppServiceEnvironmentPlatformLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppServiceFileAuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppServiceFileAuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AppServiceFileAuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppServiceHTTPLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppServiceHTTPLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AppServiceHTTPLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppServiceIPSecAuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppServiceIPSecAuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AppServiceIPSecAuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppServicePlatformLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppServicePlatformLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AppServicePlatformLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppServiceServerlessSecurityPluginData 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppServiceServerlessSecurityPluginData'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AppServiceServerlessSecurityPluginData'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppSystemEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppSystemEvents'
  properties: {
    totalRetentionInDays: 90
    plan: 'Analytics'
    schema: {
      name: 'AppSystemEvents'
    }
    retentionInDays: 90
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AppTraces 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AppTraces'
  properties: {
    totalRetentionInDays: 90
    plan: 'Analytics'
    schema: {
      name: 'AppTraces'
    }
    retentionInDays: 90
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ASCAuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ASCAuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ASCAuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ASCDeviceEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ASCDeviceEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ASCDeviceEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ASRJobs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ASRJobs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ASRJobs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ASRReplicatedItems 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ASRReplicatedItems'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ASRReplicatedItems'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ATCExpressRouteCircuitIpfix 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ATCExpressRouteCircuitIpfix'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ATCExpressRouteCircuitIpfix'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AUIEventsAudit 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AUIEventsAudit'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AUIEventsAudit'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AUIEventsOperational 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AUIEventsOperational'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AUIEventsOperational'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AutoscaleEvaluationsLog 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AutoscaleEvaluationsLog'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AutoscaleEvaluationsLog'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AutoscaleScaleActionsLog 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AutoscaleScaleActionsLog'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AutoscaleScaleActionsLog'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AVNMConnectivityConfigurationChange 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AVNMConnectivityConfigurationChange'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AVNMConnectivityConfigurationChange'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AVNMNetworkGroupMembershipChange 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AVNMNetworkGroupMembershipChange'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AVNMNetworkGroupMembershipChange'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AVNMRuleCollectionChange 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AVNMRuleCollectionChange'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AVNMRuleCollectionChange'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AVSSyslog 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AVSSyslog'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AVSSyslog'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZFWApplicationRule 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZFWApplicationRule'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZFWApplicationRule'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZFWApplicationRuleAggregation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZFWApplicationRuleAggregation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZFWApplicationRuleAggregation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZFWDnsQuery 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZFWDnsQuery'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZFWDnsQuery'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZFWFatFlow 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZFWFatFlow'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZFWFatFlow'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZFWFlowTrace 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZFWFlowTrace'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZFWFlowTrace'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZFWIdpsSignature 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZFWIdpsSignature'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZFWIdpsSignature'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZFWInternalFqdnResolutionFailure 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZFWInternalFqdnResolutionFailure'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZFWInternalFqdnResolutionFailure'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZFWNatRule 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZFWNatRule'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZFWNatRule'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZFWNatRuleAggregation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZFWNatRuleAggregation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZFWNatRuleAggregation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZFWNetworkRule 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZFWNetworkRule'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZFWNetworkRule'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZFWNetworkRuleAggregation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZFWNetworkRuleAggregation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZFWNetworkRuleAggregation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZFWThreatIntel 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZFWThreatIntel'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZFWThreatIntel'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZKVAuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZKVAuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZKVAuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZKVPolicyEvaluationDetailsLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZKVPolicyEvaluationDetailsLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZKVPolicyEvaluationDetailsLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZMSApplicationMetricLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZMSApplicationMetricLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZMSApplicationMetricLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZMSArchiveLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZMSArchiveLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZMSArchiveLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZMSAutoscaleLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZMSAutoscaleLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZMSAutoscaleLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZMSCustomerManagedKeyUserLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZMSCustomerManagedKeyUserLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZMSCustomerManagedKeyUserLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZMSHybridConnectionsEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZMSHybridConnectionsEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZMSHybridConnectionsEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZMSKafkaCoordinatorLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZMSKafkaCoordinatorLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZMSKafkaCoordinatorLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZMSKafkaUserErrorLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZMSKafkaUserErrorLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZMSKafkaUserErrorLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZMSOperationalLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZMSOperationalLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZMSOperationalLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZMSRunTimeAuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZMSRunTimeAuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZMSRunTimeAuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AZMSVnetConnectionEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AZMSVnetConnectionEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AZMSVnetConnectionEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AzureActivity 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AzureActivity'
  properties: {
    totalRetentionInDays: 90
    plan: 'Analytics'
    schema: {
      name: 'AzureActivity'
    }
    retentionInDays: 90
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AzureActivityV2 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AzureActivityV2'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AzureActivityV2'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AzureAssessmentRecommendation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AzureAssessmentRecommendation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AzureAssessmentRecommendation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AzureAttestationDiagnostics 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AzureAttestationDiagnostics'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AzureAttestationDiagnostics'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AzureCloudHsmAuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AzureCloudHsmAuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AzureCloudHsmAuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AzureDevOpsAuditing 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AzureDevOpsAuditing'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AzureDevOpsAuditing'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AzureDiagnostics 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AzureDiagnostics'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AzureDiagnostics'
      columns: [
        {
          name: 'ElasticPoolName_s'
          type: 'string'
        }
        {
          name: 'DatabaseName_s'
          type: 'string'
        }
        {
          name: 'error_state_d'
          type: 'real'
        }
        {
          name: 'query_hash_s'
          type: 'string'
        }
        {
          name: 'query_plan_hash_s'
          type: 'string'
        }
        {
          name: 'server_principal_sid_g'
          type: 'guid'
        }
        {
          name: 'obo_middle_tier_app_id_g'
          type: 'guid'
        }
        {
          name: 'PartitionId_s'
          type: 'string'
        }
        {
          name: 'originalEventTimestamp_t'
          type: 'datetime'
        }
        {
          name: 'audit_schema_version_d'
          type: 'real'
        }
        {
          name: 'event_time_t'
          type: 'datetime'
        }
        {
          name: 'sequence_number_d'
          type: 'real'
        }
        {
          name: 'action_id_s'
          type: 'string'
        }
        {
          name: 'action_name_s'
          type: 'string'
        }
        {
          name: 'succeeded_s'
          type: 'string'
        }
        {
          name: 'is_column_permission_s'
          type: 'string'
        }
        {
          name: 'session_id_d'
          type: 'real'
        }
        {
          name: 'server_principal_id_d'
          type: 'real'
        }
        {
          name: 'database_principal_id_d'
          type: 'real'
        }
        {
          name: 'target_server_principal_id_d'
          type: 'real'
        }
        {
          name: 'target_database_principal_id_d'
          type: 'real'
        }
        {
          name: 'object_id_d'
          type: 'real'
        }
        {
          name: 'user_defined_event_id_d'
          type: 'real'
        }
        {
          name: 'transaction_id_d'
          type: 'real'
        }
        {
          name: 'class_type_s'
          type: 'string'
        }
        {
          name: 'class_type_description_s'
          type: 'string'
        }
        {
          name: 'securable_class_type_s'
          type: 'string'
        }
        {
          name: 'duration_milliseconds_d'
          type: 'real'
        }
        {
          name: 'response_rows_d'
          type: 'real'
        }
        {
          name: 'affected_rows_d'
          type: 'real'
        }
        {
          name: 'client_tls_version_d'
          type: 'real'
        }
        {
          name: 'database_transaction_id_d'
          type: 'real'
        }
        {
          name: 'ledger_start_sequence_number_d'
          type: 'real'
        }
        {
          name: 'client_ip_s'
          type: 'string'
        }
        {
          name: 'permission_bitmask_g'
          type: 'guid'
        }
        {
          name: 'sequence_group_id_g'
          type: 'guid'
        }
        {
          name: 'session_server_principal_name_s'
          type: 'string'
        }
        {
          name: 'server_principal_name_s'
          type: 'string'
        }
        {
          name: 'server_principal_sid_s'
          type: 'string'
        }
        {
          name: 'database_principal_name_s'
          type: 'string'
        }
        {
          name: 'target_server_principal_name_s'
          type: 'string'
        }
        {
          name: 'target_server_principal_sid_s'
          type: 'string'
        }
        {
          name: 'target_database_principal_name_s'
          type: 'string'
        }
        {
          name: 'server_instance_name_s'
          type: 'string'
        }
        {
          name: 'database_name_s'
          type: 'string'
        }
        {
          name: 'schema_name_s'
          type: 'string'
        }
        {
          name: 'object_name_s'
          type: 'string'
        }
        {
          name: 'statement_s'
          type: 'string'
        }
        {
          name: 'additional_information_s'
          type: 'string'
        }
        {
          name: 'user_defined_information_s'
          type: 'string'
        }
        {
          name: 'application_name_s'
          type: 'string'
        }
        {
          name: 'connection_id_g'
          type: 'guid'
        }
        {
          name: 'data_sensitivity_information_s'
          type: 'string'
        }
        {
          name: 'host_name_s'
          type: 'string'
        }
        {
          name: 'session_context_s'
          type: 'string'
        }
        {
          name: 'client_tls_version_name_s'
          type: 'string'
        }
        {
          name: 'external_policy_permissions_checked_s'
          type: 'string'
        }
        {
          name: 'obo_middle_tier_app_id_s'
          type: 'string'
        }
        {
          name: 'is_server_level_audit_s'
          type: 'string'
        }
        {
          name: 'is_local_secondary_replica_s'
          type: 'string'
        }
        {
          name: 'event_id_g'
          type: 'guid'
        }
        {
          name: 'LogicalServerName_s'
          type: 'string'
        }
        {
          name: 'backupIntervalInMinutes_s'
          type: 'string'
        }
        {
          name: 'backupRetentionIntervalInHours_s'
          type: 'string'
        }
        {
          name: 'backupStorageRedundancy_s'
          type: 'string'
        }
        {
          name: 'lastError_transportErrorCode_d'
          type: 'real'
        }
        {
          name: 'resultSignature_d'
          type: 'real'
        }
        {
          name: 'Description_s'
          type: 'string'
        }
        {
          name: 'Query_s'
          type: 'string'
        }
        {
          name: 'Documents_d'
          type: 'real'
        }
        {
          name: 'identity_claim_upn_s'
          type: 'string'
        }
        {
          name: 'backendResponseCode_d'
          type: 'real'
        }
        {
          name: 'requestSize_d'
          type: 'real'
        }
        {
          name: 'backendProtocol_s'
          type: 'string'
        }
        {
          name: 'clientTime_d'
          type: 'real'
        }
        {
          name: 'DeploymentVersion_s'
          type: 'string'
        }
        {
          name: 'Level_d'
          type: 'real'
        }
        {
          name: 'isRequestSuccess_b'
          type: 'boolean'
        }
        {
          name: 'method_s'
          type: 'string'
        }
        {
          name: 'url_s'
          type: 'string'
        }
        {
          name: 'responseCode_d'
          type: 'real'
        }
        {
          name: 'responseSize_d'
          type: 'real'
        }
        {
          name: 'cache_s'
          type: 'string'
        }
        {
          name: 'apiId_s'
          type: 'string'
        }
        {
          name: 'clientProtocol_s'
          type: 'string'
        }
        {
          name: 'apiRevision_s'
          type: 'string'
        }
        {
          name: 'clientTlsVersion_s'
          type: 'string'
        }
        {
          name: 'truncated_d'
          type: 'real'
        }
        {
          name: 'backendTime_d'
          type: 'real'
        }
        {
          name: 'operationId_s'
          type: 'string'
        }
        {
          name: 'apimSubscriptionId_s'
          type: 'string'
        }
        {
          name: 'backendId_s'
          type: 'string'
        }
        {
          name: 'backendMethod_s'
          type: 'string'
        }
        {
          name: 'backendUrl_s'
          type: 'string'
        }
        {
          name: 'lastError_elapsed_d'
          type: 'real'
        }
        {
          name: 'lastError_source_s'
          type: 'string'
        }
        {
          name: 'lastError_path_s'
          type: 'string'
        }
        {
          name: 'lastError_reason_s'
          type: 'string'
        }
        {
          name: 'lastError_message_s'
          type: 'string'
        }
        {
          name: 'lastError_section_s'
          type: 'string'
        }
        {
          name: 'errors_s'
          type: 'string'
        }
        {
          name: 'accountName_s'
          type: 'string'
        }
        {
          name: 'sizeKb_d'
          type: 'real'
        }
        {
          name: 'event_s'
          type: 'string'
        }
        {
          name: 'location_s'
          type: 'string'
        }
        {
          name: 'databasename_s'
          type: 'string'
        }
        {
          name: 'collectionname_s'
          type: 'string'
        }
        {
          name: 'partitionkeyrangeid_s'
          type: 'string'
        }
        {
          name: 'useragent_s'
          type: 'string'
        }
        {
          name: 'resourcegroupname_s'
          type: 'string'
        }
        {
          name: 'partialipaddress_s'
          type: 'string'
        }
        {
          name: 'regionname_s'
          type: 'string'
        }
        {
          name: 'authtype_s'
          type: 'string'
        }
        {
          name: 'numberofrowsreturned_s'
          type: 'string'
        }
        {
          name: 'signature_s'
          type: 'string'
        }
        {
          name: 'shapesignature_s'
          type: 'string'
        }
        {
          name: 'queryexecutionstatus_s'
          type: 'string'
        }
        {
          name: 'querytext_s'
          type: 'string'
        }
        {
          name: 'secretProperties_attributes_enabled_b'
          type: 'boolean'
        }
        {
          name: 'clientIp_s'
          type: 'string'
        }
        {
          name: 'clientPort_s'
          type: 'string'
        }
        {
          name: 'ruleSetType_s'
          type: 'string'
        }
        {
          name: 'ruleSetVersion_s'
          type: 'string'
        }
        {
          name: 'ruleId_s'
          type: 'string'
        }
        {
          name: 'Message'
          type: 'string'
        }
        {
          name: 'action_s'
          type: 'string'
        }
        {
          name: 'site_s'
          type: 'string'
        }
        {
          name: 'details_message_s'
          type: 'string'
        }
        {
          name: 'details_data_s'
          type: 'string'
        }
        {
          name: 'details_file_s'
          type: 'string'
        }
        {
          name: 'details_line_s'
          type: 'string'
        }
        {
          name: 'hostname_s'
          type: 'string'
        }
        {
          name: 'policyId_s'
          type: 'string'
        }
        {
          name: 'policyScope_s'
          type: 'string'
        }
        {
          name: 'policyScopeName_s'
          type: 'string'
        }
        {
          name: 'listenerName_s'
          type: 'string'
        }
        {
          name: 'ruleName_s'
          type: 'string'
        }
        {
          name: 'backendPoolName_s'
          type: 'string'
        }
        {
          name: 'backendSettingName_s'
          type: 'string'
        }
        {
          name: 'timeStamp_t'
          type: 'datetime'
        }
        {
          name: 'instanceId_s'
          type: 'string'
        }
        {
          name: 'clientIP_s'
          type: 'string'
        }
        {
          name: 'clientPort_d'
          type: 'real'
        }
        {
          name: 'originalRequestUriWithArgs_s'
          type: 'string'
        }
        {
          name: 'requestQuery_s'
          type: 'string'
        }
        {
          name: 'httpStatus_d'
          type: 'real'
        }
        {
          name: 'httpVersion_s'
          type: 'string'
        }
        {
          name: 'receivedBytes_d'
          type: 'real'
        }
        {
          name: 'sentBytes_d'
          type: 'real'
        }
        {
          name: 'connectionSerialNumber_d'
          type: 'real'
        }
        {
          name: 'noOfConnectionRequests_d'
          type: 'real'
        }
        {
          name: 'clientResponseTime_d'
          type: 'real'
        }
        {
          name: 'timeTaken_d'
          type: 'real'
        }
        {
          name: 'WAFEvaluationTime_s'
          type: 'string'
        }
        {
          name: 'WAFMode_s'
          type: 'string'
        }
        {
          name: 'WAFPolicyID_s'
          type: 'string'
        }
        {
          name: 'transactionId_g'
          type: 'guid'
        }
        {
          name: 'sslEnabled_s'
          type: 'string'
        }
        {
          name: 'sslCipher_s'
          type: 'string'
        }
        {
          name: 'sslProtocol_s'
          type: 'string'
        }
        {
          name: 'sslClientVerify_s'
          type: 'string'
        }
        {
          name: 'sslClientCertificateFingerprint_s'
          type: 'string'
        }
        {
          name: 'sslClientCertificateIssuerName_s'
          type: 'string'
        }
        {
          name: 'serverRouted_s'
          type: 'string'
        }
        {
          name: 'serverStatus_s'
          type: 'string'
        }
        {
          name: 'serverResponseLatency_s'
          type: 'string'
        }
        {
          name: 'upstreamSourcePort_s'
          type: 'string'
        }
        {
          name: 'originalHost_s'
          type: 'string'
        }
        {
          name: 'host_s'
          type: 'string'
        }
        {
          name: 'Tenant_s'
          type: 'string'
        }
        {
          name: 'properties_s'
          type: 'string'
        }
        {
          name: 'AssetIdentity_g'
          type: 'guid'
        }
        {
          name: 'identity_claim_scp_s'
          type: 'string'
        }
        {
          name: 'identity_claim_ipaddr_s'
          type: 'string'
        }
        {
          name: 'identity_claim_unique_name_s'
          type: 'string'
        }
        {
          name: 'identity_claim_amr_s'
          type: 'string'
        }
        {
          name: 'identity_claim_home_oid_g'
          type: 'guid'
        }
        {
          name: 'identity_claim_http_schemas_xmlsoap_org_ws_2005_05_identity_claims_upn_s'
          type: 'string'
        }
        {
          name: 'identity_claim_http_schemas_xmlsoap_org_ws_2005_05_identity_claims_name_s'
          type: 'string'
        }
        {
          name: 'apiKind_s'
          type: 'string'
        }
        {
          name: 'apiKindResourceType_s'
          type: 'string'
        }
        {
          name: 'resourceUri_s'
          type: 'string'
        }
        {
          name: 'resourceDetails_s'
          type: 'string'
        }
        {
          name: 'requestResourceType_s'
          type: 'string'
        }
        {
          name: 'requestResourceId_s'
          type: 'string'
        }
        {
          name: 'collectionRid_s'
          type: 'string'
        }
        {
          name: 'databaseRid_s'
          type: 'string'
        }
        {
          name: 'statusCode_s'
          type: 'string'
        }
        {
          name: 'duration_s'
          type: 'string'
        }
        {
          name: 'userAgent_s'
          type: 'string'
        }
        {
          name: 'clientIpAddress_s'
          type: 'string'
        }
        {
          name: 'requestLength_s'
          type: 'string'
        }
        {
          name: 'responseLength_s'
          type: 'string'
        }
        {
          name: 'resourceTokenPermissionId_s'
          type: 'string'
        }
        {
          name: 'resourceTokenPermissionMode_s'
          type: 'string'
        }
        {
          name: 'resourceTokenUserRid_s'
          type: 'string'
        }
        {
          name: 'region_s'
          type: 'string'
        }
        {
          name: 'partitionId_s'
          type: 'string'
        }
        {
          name: 'aadAppliedRoleAssignmentId_s'
          type: 'string'
        }
        {
          name: 'aadPrincipalId_s'
          type: 'string'
        }
        {
          name: 'authTokenType_s'
          type: 'string'
        }
        {
          name: 'keyType_s'
          type: 'string'
        }
        {
          name: 'connectionMode_s'
          type: 'string'
        }
        {
          name: 'stream_s'
          type: 'string'
        }
        {
          name: 'pod_s'
          type: 'string'
        }
        {
          name: 'containerID_s'
          type: 'string'
        }
        {
          name: 'log_s'
          type: 'string'
        }
        {
          name: 'time_s'
          type: 'string'
        }
        {
          name: 'regionName_s'
          type: 'string'
        }
        {
          name: 'operationType_s'
          type: 'string'
        }
        {
          name: 'databaseName_s'
          type: 'string'
        }
        {
          name: 'collectionName_s'
          type: 'string'
        }
        {
          name: 'partitionKey_s'
          type: 'string'
        }
        {
          name: 'partitionKeyRangeId_s'
          type: 'string'
        }
        {
          name: 'requestCharge_s'
          type: 'string'
        }
        {
          name: 'sqlQueryTextTraceType_s'
          type: 'string'
        }
        {
          name: 'enableDataPlaneRequestsTrace_s'
          type: 'string'
        }
        {
          name: 'enableMongoRequestsTrace_s'
          type: 'string'
        }
        {
          name: 'enableControlPlaneRequestsTrace_s'
          type: 'string'
        }
        {
          name: 'enableCassandraRequestsTrace_s'
          type: 'string'
        }
        {
          name: 'enableGremlinRequestsTrace_s'
          type: 'string'
        }
        {
          name: 'activityId_g'
          type: 'guid'
        }
        {
          name: 'httpstatusCode_s'
          type: 'string'
        }
        {
          name: 'result_s'
          type: 'string'
        }
        {
          name: 'httpMethod_s'
          type: 'string'
        }
        {
          name: 'ipRangeFilter_s'
          type: 'string'
        }
        {
          name: 'enableVirtualNetworkFilter_s'
          type: 'string'
        }
        {
          name: 'virtualNetworkResourceEntries_s'
          type: 'string'
        }
        {
          name: 'enablePrivateEndpointConnection_s'
          type: 'string'
        }
        {
          name: 'privateEndpointConnections_s'
          type: 'string'
        }
        {
          name: 'privateEndpointArmUrl_s'
          type: 'string'
        }
        {
          name: 'defaultConsistencyLevel_s'
          type: 'string'
        }
        {
          name: 'enableAutomaticFailover_s'
          type: 'string'
        }
        {
          name: 'maxStalenessIntervalInSeconds_s'
          type: 'string'
        }
        {
          name: 'maxStalenessPrefix_s'
          type: 'string'
        }
        {
          name: 'enableMultipleWriteLocations_s'
          type: 'string'
        }
        {
          name: 'cors_s'
          type: 'string'
        }
        {
          name: 'identity_claim_xms_mirid_s'
          type: 'string'
        }
        {
          name: 'trustedService_s'
          type: 'string'
        }
        {
          name: 'secretProperties_type_s'
          type: 'string'
        }
        {
          name: 'certificateProperties_subject_s'
          type: 'string'
        }
        {
          name: 'certificateProperties_sha1_s'
          type: 'string'
        }
        {
          name: 'certificateProperties_sha256_s'
          type: 'string'
        }
        {
          name: 'certificateProperties_nbf_t'
          type: 'datetime'
        }
        {
          name: 'certificateProperties_exp_t'
          type: 'datetime'
        }
        {
          name: 'certificatePolicyProperties_keyProperties_type_s'
          type: 'string'
        }
        {
          name: 'certificatePolicyProperties_keyProperties_size_d'
          type: 'real'
        }
        {
          name: 'certificatePolicyProperties_keyProperties_reuse_b'
          type: 'boolean'
        }
        {
          name: 'certificatePolicyProperties_keyProperties_export_b'
          type: 'boolean'
        }
        {
          name: 'certificatePolicyProperties_secretProperties_type_s'
          type: 'string'
        }
        {
          name: 'certificatePolicyProperties_certificateIssuerProperties_name_s'
          type: 'string'
        }
        {
          name: 'keyProperties_type_s'
          type: 'string'
        }
        {
          name: 'keyProperties_size_d'
          type: 'real'
        }
        {
          name: 'keyProperties_operations_s'
          type: 'string'
        }
        {
          name: 'keyProperties_attributes_enabled_b'
          type: 'boolean'
        }
        {
          name: 'keyProperties_attributes_hsmPlatform_s'
          type: 'string'
        }
        {
          name: 'subnetId_s'
          type: 'string'
        }
        {
          name: 'privateEndpointId_s'
          type: 'string'
        }
        {
          name: 'ResultDescription'
          type: 'string'
        }
        {
          name: 'identity_claim_oid_g'
          type: 'guid'
        }
        {
          name: 'identity_claim_appidacr_s'
          type: 'string'
        }
        {
          name: 'identity_claim_xms_az_nwperimid_s'
          type: 'string'
        }
        {
          name: 'isRbacAuthorized_b'
          type: 'boolean'
        }
        {
          name: 'appliedAssignmentId_g'
          type: 'guid'
        }
        {
          name: 'identity_claim_http_schemas_microsoft_com_identity_claims_objectidentifier_g'
          type: 'guid'
        }
        {
          name: 'identity_claim_appid_g'
          type: 'guid'
        }
        {
          name: 'id_s'
          type: 'string'
        }
        {
          name: 'properties_sku_Family_s'
          type: 'string'
        }
        {
          name: 'properties_sku_Name_s'
          type: 'string'
        }
        {
          name: 'properties_tenantId_g'
          type: 'guid'
        }
        {
          name: 'properties_networkAcls_bypass_s'
          type: 'string'
        }
        {
          name: 'properties_networkAcls_defaultAction_s'
          type: 'string'
        }
        {
          name: 'properties_enabledForDeployment_b'
          type: 'boolean'
        }
        {
          name: 'properties_enabledForDiskEncryption_b'
          type: 'boolean'
        }
        {
          name: 'properties_enabledForTemplateDeployment_b'
          type: 'boolean'
        }
        {
          name: 'properties_enableSoftDelete_b'
          type: 'boolean'
        }
        {
          name: 'properties_softDeleteRetentionInDays_d'
          type: 'real'
        }
        {
          name: 'properties_enableRbacAuthorization_b'
          type: 'boolean'
        }
        {
          name: 'properties_enablePurgeProtection_b'
          type: 'boolean'
        }
        {
          name: 'Category'
          type: 'string'
        }
        {
          name: 'OperationName'
          type: 'string'
        }
        {
          name: 'ResultType'
          type: 'string'
        }
        {
          name: 'CorrelationId'
          type: 'string'
        }
        {
          name: 'CallerIPAddress'
          type: 'string'
        }
        {
          name: 'clientInfo_s'
          type: 'string'
        }
        {
          name: 'httpStatusCode_d'
          type: 'real'
        }
        {
          name: 'requestUri_s'
          type: 'string'
        }
        {
          name: 'tlsVersion_s'
          type: 'string'
        }
        {
          name: 'ResourceId'
          type: 'string'
        }
        {
          name: 'OperationVersion'
          type: 'string'
        }
        {
          name: 'ResultSignature'
          type: 'string'
        }
        {
          name: 'DurationMs'
          type: 'long'
        }
        {
          name: 'SubscriptionId'
          type: 'guid'
        }
        {
          name: 'ResourceGroup'
          type: 'string'
        }
        {
          name: 'ResourceProvider'
          type: 'string'
        }
        {
          name: 'Resource'
          type: 'string'
        }
        {
          name: 'ResourceType'
          type: 'string'
        }
        {
          name: '_CustomFieldsCollection'
          type: 'dynamic'
        }
      ]
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AzureLoadTestingOperation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AzureLoadTestingOperation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AzureLoadTestingOperation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AzureMetrics 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AzureMetrics'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AzureMetrics'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_AzureMetricsV2 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'AzureMetricsV2'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'AzureMetricsV2'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_BaiClusterEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'BaiClusterEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'BaiClusterEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_BaiClusterNodeEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'BaiClusterNodeEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'BaiClusterNodeEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_BaiJobEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'BaiJobEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'BaiJobEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_BlockchainApplicationLog 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'BlockchainApplicationLog'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'BlockchainApplicationLog'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_BlockchainProxyLog 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'BlockchainProxyLog'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'BlockchainProxyLog'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_CassandraAudit 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'CassandraAudit'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'CassandraAudit'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_CassandraLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'CassandraLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'CassandraLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_CCFApplicationLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'CCFApplicationLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'CCFApplicationLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_CDBCassandraRequests 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'CDBCassandraRequests'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'CDBCassandraRequests'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_CDBControlPlaneRequests 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'CDBControlPlaneRequests'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'CDBControlPlaneRequests'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_CDBDataPlaneRequests 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'CDBDataPlaneRequests'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'CDBDataPlaneRequests'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_CDBGremlinRequests 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'CDBGremlinRequests'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'CDBGremlinRequests'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_CDBMongoRequests 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'CDBMongoRequests'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'CDBMongoRequests'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_CDBPartitionKeyRUConsumption 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'CDBPartitionKeyRUConsumption'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'CDBPartitionKeyRUConsumption'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_CDBPartitionKeyStatistics 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'CDBPartitionKeyStatistics'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'CDBPartitionKeyStatistics'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_CDBQueryRuntimeStatistics 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'CDBQueryRuntimeStatistics'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'CDBQueryRuntimeStatistics'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ChaosStudioExperimentEventLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ChaosStudioExperimentEventLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ChaosStudioExperimentEventLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_CHSMManagementAuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'CHSMManagementAuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'CHSMManagementAuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_CIEventsAudit 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'CIEventsAudit'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'CIEventsAudit'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_CIEventsOperational 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'CIEventsOperational'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'CIEventsOperational'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_CommonSecurityLog 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'CommonSecurityLog'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'CommonSecurityLog'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ComputerGroup 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ComputerGroup'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ComputerGroup'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ContainerAppConsoleLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ContainerAppConsoleLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ContainerAppConsoleLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ContainerAppSystemLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ContainerAppSystemLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ContainerAppSystemLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ContainerEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ContainerEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ContainerEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ContainerEvent_CL 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ContainerEvent_CL'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ContainerEvent_CL'
      columns: [
        {
          name: 'ContainerGroup_s'
          type: 'string'
        }
        {
          name: 'ContainerGroupInstanceId_g'
          type: 'guid'
        }
        {
          name: 'ContainerID_s'
          type: 'string'
        }
        {
          name: 'ContainerName_s'
          type: 'string'
        }
        {
          name: 'Count_s'
          type: 'string'
        }
        {
          name: 'Location_s'
          type: 'string'
        }
        {
          name: 'Message'
          type: 'string'
        }
        {
          name: 'OSType_s'
          type: 'string'
        }
        {
          name: 'Reason_s'
          type: 'string'
        }
        {
          name: 'ResourceGroup'
          type: 'string'
        }
        {
          name: 'SubscriptionId'
          type: 'guid'
        }
      ]
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ContainerImageInventory 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ContainerImageInventory'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ContainerImageInventory'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ContainerInstanceLog 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ContainerInstanceLog'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ContainerInstanceLog'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ContainerInstanceLog_CL 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ContainerInstanceLog_CL'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ContainerInstanceLog_CL'
      columns: [
        {
          name: 'ContainerGroup_s'
          type: 'string'
        }
        {
          name: 'ContainerID_s'
          type: 'string'
        }
        {
          name: 'ContainerImage_s'
          type: 'string'
        }
        {
          name: 'ContainerName_s'
          type: 'string'
        }
        {
          name: 'Location_s'
          type: 'string'
        }
        {
          name: 'Message'
          type: 'string'
        }
        {
          name: 'OSType_s'
          type: 'string'
        }
        {
          name: 'ResourceGroup'
          type: 'string'
        }
        {
          name: 'Source_s'
          type: 'string'
        }
        {
          name: 'SubscriptionId'
          type: 'guid'
        }
      ]
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ContainerInventory 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ContainerInventory'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ContainerInventory'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ContainerLog 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ContainerLog'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ContainerLog'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ContainerLogV2 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ContainerLogV2'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ContainerLogV2'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ContainerNodeInventory 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ContainerNodeInventory'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ContainerNodeInventory'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ContainerRegistryLoginEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ContainerRegistryLoginEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ContainerRegistryLoginEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ContainerRegistryRepositoryEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ContainerRegistryRepositoryEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ContainerRegistryRepositoryEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ContainerServiceLog 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ContainerServiceLog'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ContainerServiceLog'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_CoreAzureBackup 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'CoreAzureBackup'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'CoreAzureBackup'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksAccounts 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksAccounts'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksAccounts'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksCapsule8Dataplane 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksCapsule8Dataplane'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksCapsule8Dataplane'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksClamAVScan 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksClamAVScan'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksClamAVScan'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksClusterLibraries 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksClusterLibraries'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksClusterLibraries'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksClusters 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksClusters'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksClusters'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksDatabricksSQL 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksDatabricksSQL'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksDatabricksSQL'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksDBFS 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksDBFS'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksDBFS'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksDeltaPipelines 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksDeltaPipelines'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksDeltaPipelines'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksFeatureStore 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksFeatureStore'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksFeatureStore'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksGenie 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksGenie'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksGenie'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksGitCredentials 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksGitCredentials'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksGitCredentials'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksGlobalInitScripts 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksGlobalInitScripts'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksGlobalInitScripts'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksIAMRole 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksIAMRole'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksIAMRole'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksInstancePools 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksInstancePools'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksInstancePools'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksJobs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksJobs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksJobs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksMLflowAcledArtifact 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksMLflowAcledArtifact'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksMLflowAcledArtifact'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksMLflowExperiment 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksMLflowExperiment'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksMLflowExperiment'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksModelRegistry 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksModelRegistry'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksModelRegistry'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksNotebook 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksNotebook'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksNotebook'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksPartnerHub 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksPartnerHub'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksPartnerHub'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksRemoteHistoryService 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksRemoteHistoryService'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksRemoteHistoryService'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksRepos 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksRepos'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksRepos'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksSecrets 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksSecrets'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksSecrets'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksServerlessRealTimeInference 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksServerlessRealTimeInference'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksServerlessRealTimeInference'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksSQL 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksSQL'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksSQL'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksSQLPermissions 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksSQLPermissions'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksSQLPermissions'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksSSH 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksSSH'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksSSH'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksTables 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksTables'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksTables'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksUnityCatalog 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksUnityCatalog'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksUnityCatalog'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksWebTerminal 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksWebTerminal'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksWebTerminal'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DatabricksWorkspace 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DatabricksWorkspace'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DatabricksWorkspace'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DataTransferOperations 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DataTransferOperations'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DataTransferOperations'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DCRLogErrors 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DCRLogErrors'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DCRLogErrors'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DCRLogTroubleshooting 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DCRLogTroubleshooting'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DCRLogTroubleshooting'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DevCenterBillingEventLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DevCenterBillingEventLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DevCenterBillingEventLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DevCenterDiagnosticLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DevCenterDiagnosticLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DevCenterDiagnosticLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DevCenterResourceOperationLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DevCenterResourceOperationLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DevCenterResourceOperationLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DSMAzureBlobStorageLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DSMAzureBlobStorageLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DSMAzureBlobStorageLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DSMDataClassificationLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DSMDataClassificationLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DSMDataClassificationLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_DSMDataLabelingLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'DSMDataLabelingLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'DSMDataLabelingLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_EnrichedMicrosoft365AuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'EnrichedMicrosoft365AuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'EnrichedMicrosoft365AuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ETWEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ETWEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ETWEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_Event 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'Event'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'Event'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ExchangeAssessmentRecommendation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ExchangeAssessmentRecommendation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ExchangeAssessmentRecommendation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ExchangeOnlineAssessmentRecommendation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ExchangeOnlineAssessmentRecommendation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ExchangeOnlineAssessmentRecommendation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_FailedIngestion 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'FailedIngestion'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'FailedIngestion'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_FunctionAppLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'FunctionAppLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'FunctionAppLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightAmbariClusterAlerts 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightAmbariClusterAlerts'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightAmbariClusterAlerts'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightAmbariSystemMetrics 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightAmbariSystemMetrics'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightAmbariSystemMetrics'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightGatewayAuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightGatewayAuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightGatewayAuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightHadoopAndYarnLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightHadoopAndYarnLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightHadoopAndYarnLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightHadoopAndYarnMetrics 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightHadoopAndYarnMetrics'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightHadoopAndYarnMetrics'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightHBaseLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightHBaseLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightHBaseLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightHBaseMetrics 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightHBaseMetrics'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightHBaseMetrics'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightHiveAndLLAPLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightHiveAndLLAPLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightHiveAndLLAPLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightHiveAndLLAPMetrics 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightHiveAndLLAPMetrics'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightHiveAndLLAPMetrics'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightHiveQueryAppStats 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightHiveQueryAppStats'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightHiveQueryAppStats'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightHiveTezAppStats 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightHiveTezAppStats'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightHiveTezAppStats'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightJupyterNotebookEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightJupyterNotebookEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightJupyterNotebookEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightKafkaLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightKafkaLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightKafkaLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightKafkaMetrics 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightKafkaMetrics'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightKafkaMetrics'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightKafkaServerLog 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightKafkaServerLog'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightKafkaServerLog'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightOozieLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightOozieLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightOozieLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightRangerAuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightRangerAuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightRangerAuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightSecurityLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightSecurityLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightSecurityLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightSparkApplicationEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightSparkApplicationEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightSparkApplicationEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightSparkBlockManagerEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightSparkBlockManagerEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightSparkBlockManagerEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightSparkEnvironmentEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightSparkEnvironmentEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightSparkEnvironmentEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightSparkExecutorEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightSparkExecutorEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightSparkExecutorEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightSparkExtraEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightSparkExtraEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightSparkExtraEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightSparkJobEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightSparkJobEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightSparkJobEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightSparkLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightSparkLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightSparkLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightSparkSQLExecutionEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightSparkSQLExecutionEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightSparkSQLExecutionEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightSparkStageEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightSparkStageEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightSparkStageEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightSparkStageTaskAccumulables 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightSparkStageTaskAccumulables'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightSparkStageTaskAccumulables'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightSparkTaskEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightSparkTaskEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightSparkTaskEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightStormLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightStormLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightStormLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightStormMetrics 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightStormMetrics'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightStormMetrics'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HDInsightStormTopologyMetrics 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HDInsightStormTopologyMetrics'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HDInsightStormTopologyMetrics'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_Heartbeat 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'Heartbeat'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'Heartbeat'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_HuntingBookmarks 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'HuntingBookmarks'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'HuntingBookmarks'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_InsightsMetrics 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'InsightsMetrics'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'InsightsMetrics'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_IntuneAuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'IntuneAuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'IntuneAuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_IntuneDeviceComplianceOrg 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'IntuneDeviceComplianceOrg'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'IntuneDeviceComplianceOrg'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_IntuneDevices 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'IntuneDevices'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'IntuneDevices'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_IntuneOperationalLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'IntuneOperationalLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'IntuneOperationalLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_KubeEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'KubeEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'KubeEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_KubeHealth 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'KubeHealth'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'KubeHealth'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_KubeMonAgentEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'KubeMonAgentEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'KubeMonAgentEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_KubeNodeInventory 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'KubeNodeInventory'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'KubeNodeInventory'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_KubePodInventory 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'KubePodInventory'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'KubePodInventory'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_KubePVInventory 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'KubePVInventory'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'KubePVInventory'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_KubeServices 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'KubeServices'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'KubeServices'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_LAQueryLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'LAQueryLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'LAQueryLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_LASummaryLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'LASummaryLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'LASummaryLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_LinuxAuditLog 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'LinuxAuditLog'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'LinuxAuditLog'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_LogicAppWorkflowRuntime 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'LogicAppWorkflowRuntime'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'LogicAppWorkflowRuntime'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_MaliciousIPCommunication 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'MaliciousIPCommunication'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'MaliciousIPCommunication'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_MCCEventLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'MCCEventLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'MCCEventLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_MCVPAuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'MCVPAuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'MCVPAuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_MCVPOperationLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'MCVPOperationLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'MCVPOperationLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_MicrosoftAzureBastionAuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'MicrosoftAzureBastionAuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'MicrosoftAzureBastionAuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_MicrosoftDataShareReceivedSnapshotLog 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'MicrosoftDataShareReceivedSnapshotLog'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'MicrosoftDataShareReceivedSnapshotLog'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_MicrosoftDataShareSentSnapshotLog 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'MicrosoftDataShareSentSnapshotLog'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'MicrosoftDataShareSentSnapshotLog'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_MicrosoftDataShareShareLog 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'MicrosoftDataShareShareLog'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'MicrosoftDataShareShareLog'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_MicrosoftDynamicsTelemetryPerformanceLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'MicrosoftDynamicsTelemetryPerformanceLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'MicrosoftDynamicsTelemetryPerformanceLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_MicrosoftDynamicsTelemetrySystemMetricsLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'MicrosoftDynamicsTelemetrySystemMetricsLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'MicrosoftDynamicsTelemetrySystemMetricsLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_MicrosoftGraphActivityLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'MicrosoftGraphActivityLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'MicrosoftGraphActivityLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_MicrosoftHealthcareApisAuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'MicrosoftHealthcareApisAuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'MicrosoftHealthcareApisAuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_NCBMSecurityLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'NCBMSecurityLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'NCBMSecurityLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_NCBMSystemLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'NCBMSystemLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'NCBMSystemLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_NCCKubernetesLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'NCCKubernetesLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'NCCKubernetesLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_NCCVMOrchestrationLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'NCCVMOrchestrationLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'NCCVMOrchestrationLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_NCSStorageAlerts 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'NCSStorageAlerts'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'NCSStorageAlerts'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_NCSStorageLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'NCSStorageLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'NCSStorageLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_NetworkAccessTraffic 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'NetworkAccessTraffic'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'NetworkAccessTraffic'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_NSPAccessLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'NSPAccessLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'NSPAccessLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_NTAIpDetails 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'NTAIpDetails'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'NTAIpDetails'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_NTANetAnalytics 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'NTANetAnalytics'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'NTANetAnalytics'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_NTATopologyDetails 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'NTATopologyDetails'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'NTATopologyDetails'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_NWConnectionMonitorDestinationListenerResult 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'NWConnectionMonitorDestinationListenerResult'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'NWConnectionMonitorDestinationListenerResult'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_NWConnectionMonitorDNSResult 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'NWConnectionMonitorDNSResult'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'NWConnectionMonitorDNSResult'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_NWConnectionMonitorPathResult 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'NWConnectionMonitorPathResult'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'NWConnectionMonitorPathResult'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_NWConnectionMonitorTestResult 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'NWConnectionMonitorTestResult'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'NWConnectionMonitorTestResult'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_OEPAirFlowTask 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'OEPAirFlowTask'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'OEPAirFlowTask'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_OEPAuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'OEPAuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'OEPAuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_OEPDataplaneLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'OEPDataplaneLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'OEPDataplaneLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_OEPElasticOperator 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'OEPElasticOperator'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'OEPElasticOperator'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_OEPElasticsearch 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'OEPElasticsearch'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'OEPElasticsearch'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_OLPSupplyChainEntityOperations 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'OLPSupplyChainEntityOperations'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'OLPSupplyChainEntityOperations'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_OLPSupplyChainEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'OLPSupplyChainEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'OLPSupplyChainEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_OmsCustomerProfileFact 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'OmsCustomerProfileFact'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'OmsCustomerProfileFact'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_Operation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'Operation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'Operation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_Perf 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'Perf'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'Perf'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_PFTitleAuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'PFTitleAuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'PFTitleAuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_PowerBIAuditTenant 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'PowerBIAuditTenant'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'PowerBIAuditTenant'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_PowerBIDatasetsTenant 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'PowerBIDatasetsTenant'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'PowerBIDatasetsTenant'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_PowerBIDatasetsTenantPreview 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'PowerBIDatasetsTenantPreview'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'PowerBIDatasetsTenantPreview'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_PowerBIDatasetsWorkspace 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'PowerBIDatasetsWorkspace'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'PowerBIDatasetsWorkspace'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_PowerBIDatasetsWorkspacePreview 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'PowerBIDatasetsWorkspacePreview'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'PowerBIDatasetsWorkspacePreview'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_PowerBIReportUsageTenant 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'PowerBIReportUsageTenant'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'PowerBIReportUsageTenant'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_PowerBIReportUsageWorkspace 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'PowerBIReportUsageWorkspace'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'PowerBIReportUsageWorkspace'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ProtectionStatus 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ProtectionStatus'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ProtectionStatus'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_PurviewDataSensitivityLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'PurviewDataSensitivityLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'PurviewDataSensitivityLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_PurviewScanStatusLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'PurviewScanStatusLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'PurviewScanStatusLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_PurviewSecurityLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'PurviewSecurityLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'PurviewSecurityLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_REDConnectionEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'REDConnectionEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'REDConnectionEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ReservedCommonFields 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ReservedCommonFields'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ReservedCommonFields'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ResourceManagementPublicAccessLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ResourceManagementPublicAccessLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ResourceManagementPublicAccessLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SCCMAssessmentRecommendation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SCCMAssessmentRecommendation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SCCMAssessmentRecommendation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SCOMAssessmentRecommendation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SCOMAssessmentRecommendation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SCOMAssessmentRecommendation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SecureScoreControls 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SecureScoreControls'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SecureScoreControls'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SecureScores 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SecureScores'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SecureScores'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SecurityAlert 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SecurityAlert'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SecurityAlert'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SecurityAttackPathData 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SecurityAttackPathData'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SecurityAttackPathData'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SecurityBaseline 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SecurityBaseline'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SecurityBaseline'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SecurityBaselineSummary 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SecurityBaselineSummary'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SecurityBaselineSummary'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SecurityDetection 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SecurityDetection'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SecurityDetection'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SecurityEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SecurityEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SecurityEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SecurityNestedRecommendation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SecurityNestedRecommendation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SecurityNestedRecommendation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SecurityRecommendation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SecurityRecommendation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SecurityRecommendation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SecurityRegulatoryCompliance 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SecurityRegulatoryCompliance'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SecurityRegulatoryCompliance'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ServiceFabricOperationalEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ServiceFabricOperationalEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ServiceFabricOperationalEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ServiceFabricReliableActorEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ServiceFabricReliableActorEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ServiceFabricReliableActorEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ServiceFabricReliableServiceEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ServiceFabricReliableServiceEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ServiceFabricReliableServiceEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ServiceMapComputer_CL 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ServiceMapComputer_CL'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ServiceMapComputer_CL'
      columns: [
        {
          name: 'ResourceId'
          type: 'string'
        }
        {
          name: 'WorkspaceResourceUri_s'
          type: 'string'
        }
        {
          name: 'ResourceName_s'
          type: 'string'
        }
        {
          name: 'DisplayName_s'
          type: 'string'
        }
        {
          name: 'FullDisplayName_s'
          type: 'string'
        }
        {
          name: 'ComputerName_s'
          type: 'string'
        }
        {
          name: 'BootTime_t'
          type: 'datetime'
        }
        {
          name: 'TimeZone_s'
          type: 'string'
        }
        {
          name: 'VirtualizationState_s'
          type: 'string'
        }
        {
          name: 'Ipv4Addresses_s'
          type: 'string'
        }
        {
          name: 'Ipv4SubnetMasks_s'
          type: 'string'
        }
        {
          name: 'Ipv6Addresses_s'
          type: 'string'
        }
        {
          name: 'Ipv4DefaultGateways_s'
          type: 'string'
        }
        {
          name: 'MacAddresses_s'
          type: 'string'
        }
        {
          name: 'DnsNames_s'
          type: 'string'
        }
        {
          name: 'AgentId_g'
          type: 'guid'
        }
        {
          name: 'DependencyAgentVersion_s'
          type: 'string'
        }
        {
          name: 'OperatingSystemFamily_s'
          type: 'string'
        }
        {
          name: 'OperatingSystemFullName_s'
          type: 'string'
        }
        {
          name: 'Bitness_s'
          type: 'string'
        }
        {
          name: 'PhysicalMemory_d'
          type: 'real'
        }
        {
          name: 'Cpus_d'
          type: 'real'
        }
        {
          name: 'CpuSpeed_d'
          type: 'real'
        }
        {
          name: 'VirtualMachineType_s'
          type: 'string'
        }
        {
          name: 'VirtualMachineNativeMachineId_g'
          type: 'guid'
        }
        {
          name: 'VirtualMachineName_g'
          type: 'guid'
        }
        {
          name: 'VirtualMachineHypervisorId_s'
          type: 'string'
        }
        {
          name: 'HostingProvider_s'
          type: 'string'
        }
        {
          name: 'AzureVmId_g'
          type: 'guid'
        }
        {
          name: 'AzureLocation_s'
          type: 'string'
        }
        {
          name: 'AzureName_s'
          type: 'string'
        }
        {
          name: 'AzureVMSize_s'
          type: 'string'
        }
        {
          name: 'AzureUpdateDomain_s'
          type: 'string'
        }
        {
          name: 'AzureFaultDomain_s'
          type: 'string'
        }
        {
          name: 'AzureSubscriptionId_g'
          type: 'guid'
        }
        {
          name: 'AzureResourceGroup_s'
          type: 'string'
        }
        {
          name: 'AzureResourceId_s'
          type: 'string'
        }
        {
          name: 'AzureImagePublisher_s'
          type: 'string'
        }
        {
          name: 'AzureImageOffering_s'
          type: 'string'
        }
        {
          name: 'AzureImageSku_s'
          type: 'string'
        }
        {
          name: 'AzureImageVersion_s'
          type: 'string'
        }
        {
          name: 'AzureVmScaleSetName_s'
          type: 'string'
        }
        {
          name: 'AzureVmScaleSetInstanceId_s'
          type: 'string'
        }
        {
          name: 'AzureVmScaleSetDeployment_g'
          type: 'guid'
        }
        {
          name: 'AzureVmScaleSetResourceId_s'
          type: 'string'
        }
      ]
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_ServiceMapProcess_CL 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'ServiceMapProcess_CL'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'ServiceMapProcess_CL'
      columns: [
        {
          name: 'Services_s'
          type: 'string'
        }
        {
          name: 'ResourceId'
          type: 'string'
        }
        {
          name: 'AgentId_g'
          type: 'guid'
        }
        {
          name: 'ResourceName_s'
          type: 'string'
        }
        {
          name: 'MachineResourceName_s'
          type: 'string'
        }
        {
          name: 'ExecutableName_s'
          type: 'string'
        }
        {
          name: 'DisplayName_s'
          type: 'string'
        }
        {
          name: 'Group_s'
          type: 'string'
        }
        {
          name: 'StartTime_t'
          type: 'datetime'
        }
        {
          name: 'FirstPid_d'
          type: 'real'
        }
        {
          name: 'Description_s'
          type: 'string'
        }
        {
          name: 'CompanyName_s'
          type: 'string'
        }
        {
          name: 'InternalName_s'
          type: 'string'
        }
        {
          name: 'ProductName_s'
          type: 'string'
        }
        {
          name: 'ProductVersion_s'
          type: 'string'
        }
        {
          name: 'FileVersion_s'
          type: 'string'
        }
        {
          name: 'CommandLine_s'
          type: 'string'
        }
        {
          name: 'ExecutablePath_s'
          type: 'string'
        }
        {
          name: 'WorkingDirectory_s'
          type: 'string'
        }
        {
          name: 'UserName_s'
          type: 'string'
        }
        {
          name: 'UserDomain_s'
          type: 'string'
        }
      ]
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SfBAssessmentRecommendation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SfBAssessmentRecommendation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SfBAssessmentRecommendation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SfBOnlineAssessmentRecommendation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SfBOnlineAssessmentRecommendation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SfBOnlineAssessmentRecommendation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SharePointOnlineAssessmentRecommendation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SharePointOnlineAssessmentRecommendation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SharePointOnlineAssessmentRecommendation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SignalRServiceDiagnosticLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SignalRServiceDiagnosticLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SignalRServiceDiagnosticLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SigninLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SigninLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SigninLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SPAssessmentRecommendation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SPAssessmentRecommendation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SPAssessmentRecommendation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SQLAssessmentRecommendation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SQLAssessmentRecommendation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SQLAssessmentRecommendation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SqlAtpStatus 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SqlAtpStatus'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SqlAtpStatus'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SQLSecurityAuditEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SQLSecurityAuditEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SQLSecurityAuditEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SqlVulnerabilityAssessmentResult 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SqlVulnerabilityAssessmentResult'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SqlVulnerabilityAssessmentResult'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SqlVulnerabilityAssessmentScanStatus 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SqlVulnerabilityAssessmentScanStatus'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SqlVulnerabilityAssessmentScanStatus'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_StorageAntimalwareScanResults 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'StorageAntimalwareScanResults'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'StorageAntimalwareScanResults'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_StorageBlobLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'StorageBlobLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'StorageBlobLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_StorageCacheOperationEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'StorageCacheOperationEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'StorageCacheOperationEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_StorageCacheUpgradeEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'StorageCacheUpgradeEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'StorageCacheUpgradeEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_StorageCacheWarningEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'StorageCacheWarningEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'StorageCacheWarningEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_StorageFileLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'StorageFileLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'StorageFileLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_StorageMalwareScanningResults 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'StorageMalwareScanningResults'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'StorageMalwareScanningResults'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_StorageMoverCopyLogsFailed 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'StorageMoverCopyLogsFailed'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'StorageMoverCopyLogsFailed'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_StorageMoverCopyLogsTransferred 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'StorageMoverCopyLogsTransferred'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'StorageMoverCopyLogsTransferred'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_StorageMoverJobRunLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'StorageMoverJobRunLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'StorageMoverJobRunLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_StorageQueueLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'StorageQueueLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'StorageQueueLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_StorageTableLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'StorageTableLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'StorageTableLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SucceededIngestion 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SucceededIngestion'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SucceededIngestion'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseBigDataPoolApplicationsEnded 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseBigDataPoolApplicationsEnded'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseBigDataPoolApplicationsEnded'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseBuiltinSqlPoolRequestsEnded 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseBuiltinSqlPoolRequestsEnded'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseBuiltinSqlPoolRequestsEnded'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseDXCommand 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseDXCommand'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseDXCommand'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseDXFailedIngestion 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseDXFailedIngestion'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseDXFailedIngestion'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseDXIngestionBatching 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseDXIngestionBatching'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseDXIngestionBatching'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseDXQuery 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseDXQuery'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseDXQuery'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseDXSucceededIngestion 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseDXSucceededIngestion'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseDXSucceededIngestion'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseDXTableDetails 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseDXTableDetails'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseDXTableDetails'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseDXTableUsageStatistics 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseDXTableUsageStatistics'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseDXTableUsageStatistics'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseGatewayApiRequests 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseGatewayApiRequests'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseGatewayApiRequests'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseGatewayEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseGatewayEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseGatewayEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseIntegrationActivityRuns 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseIntegrationActivityRuns'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseIntegrationActivityRuns'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseIntegrationActivityRunsEnded 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseIntegrationActivityRunsEnded'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseIntegrationActivityRunsEnded'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseIntegrationPipelineRuns 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseIntegrationPipelineRuns'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseIntegrationPipelineRuns'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseIntegrationPipelineRunsEnded 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseIntegrationPipelineRunsEnded'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseIntegrationPipelineRunsEnded'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseIntegrationTriggerRuns 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseIntegrationTriggerRuns'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseIntegrationTriggerRuns'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseIntegrationTriggerRunsEnded 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseIntegrationTriggerRunsEnded'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseIntegrationTriggerRunsEnded'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseLinkEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseLinkEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseLinkEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseRBACEvents 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseRBACEvents'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseRBACEvents'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseRbacOperations 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseRbacOperations'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseRbacOperations'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseScopePoolScopeJobsEnded 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseScopePoolScopeJobsEnded'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseScopePoolScopeJobsEnded'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseScopePoolScopeJobsStateChange 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseScopePoolScopeJobsStateChange'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseScopePoolScopeJobsStateChange'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseSqlPoolDmsWorkers 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseSqlPoolDmsWorkers'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseSqlPoolDmsWorkers'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseSqlPoolExecRequests 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseSqlPoolExecRequests'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseSqlPoolExecRequests'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseSqlPoolRequestSteps 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseSqlPoolRequestSteps'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseSqlPoolRequestSteps'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseSqlPoolSqlRequests 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseSqlPoolSqlRequests'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseSqlPoolSqlRequests'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SynapseSqlPoolWaits 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SynapseSqlPoolWaits'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SynapseSqlPoolWaits'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_Syslog 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'Syslog'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'Syslog'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_SysmonEvent 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'SysmonEvent'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'SysmonEvent'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_TSIIngress 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'TSIIngress'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'TSIIngress'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_UCClient 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'UCClient'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'UCClient'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_UCClientReadinessStatus 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'UCClientReadinessStatus'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'UCClientReadinessStatus'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_UCClientUpdateStatus 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'UCClientUpdateStatus'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'UCClientUpdateStatus'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_UCDeviceAlert 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'UCDeviceAlert'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'UCDeviceAlert'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_UCDOAggregatedStatus 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'UCDOAggregatedStatus'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'UCDOAggregatedStatus'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_UCDOStatus 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'UCDOStatus'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'UCDOStatus'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_UCServiceUpdateStatus 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'UCServiceUpdateStatus'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'UCServiceUpdateStatus'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_UCUpdateAlert 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'UCUpdateAlert'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'UCUpdateAlert'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_Update 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'Update'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'Update'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_UpdateSummary 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'UpdateSummary'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'UpdateSummary'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_Usage 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'Usage'
  properties: {
    totalRetentionInDays: 90
    plan: 'Analytics'
    schema: {
      name: 'Usage'
    }
    retentionInDays: 90
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_VCoreMongoRequests 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'VCoreMongoRequests'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'VCoreMongoRequests'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_VIAudit 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'VIAudit'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'VIAudit'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_VIIndexing 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'VIIndexing'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'VIIndexing'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_VMBoundPort 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'VMBoundPort'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'VMBoundPort'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_VMComputer 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'VMComputer'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'VMComputer'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_VMConnection 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'VMConnection'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'VMConnection'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_VMProcess 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'VMProcess'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'VMProcess'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_W3CIISLog 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'W3CIISLog'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'W3CIISLog'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_WebPubSubConnectivity 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'WebPubSubConnectivity'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'WebPubSubConnectivity'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_WebPubSubHttpRequest 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'WebPubSubHttpRequest'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'WebPubSubHttpRequest'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_WebPubSubMessaging 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'WebPubSubMessaging'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'WebPubSubMessaging'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_Windows365AuditLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'Windows365AuditLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'Windows365AuditLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_WindowsClientAssessmentRecommendation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'WindowsClientAssessmentRecommendation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'WindowsClientAssessmentRecommendation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_WindowsFirewall 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'WindowsFirewall'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'WindowsFirewall'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_WindowsServerAssessmentRecommendation 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'WindowsServerAssessmentRecommendation'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'WindowsServerAssessmentRecommendation'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_WorkloadDiagnosticLogs 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'WorkloadDiagnosticLogs'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'WorkloadDiagnosticLogs'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_WVDAgentHealthStatus 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'WVDAgentHealthStatus'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'WVDAgentHealthStatus'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_WVDAutoscaleEvaluationPooled 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'WVDAutoscaleEvaluationPooled'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'WVDAutoscaleEvaluationPooled'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_WVDCheckpoints 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'WVDCheckpoints'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'WVDCheckpoints'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_WVDConnectionGraphicsDataPreview 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'WVDConnectionGraphicsDataPreview'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'WVDConnectionGraphicsDataPreview'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_WVDConnectionNetworkData 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'WVDConnectionNetworkData'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'WVDConnectionNetworkData'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_WVDConnections 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'WVDConnections'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'WVDConnections'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_WVDErrors 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'WVDErrors'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'WVDErrors'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_WVDFeeds 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'WVDFeeds'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'WVDFeeds'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_WVDHostRegistrations 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'WVDHostRegistrations'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'WVDHostRegistrations'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_WVDManagement 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'WVDManagement'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'WVDManagement'
    }
    retentionInDays: 30
  }
}

resource workspaces_EUS_FLLM_DEMO_OPS_la_name_WVDSessionHostManagement 'Microsoft.OperationalInsights/workspaces/tables@2021-12-01-preview' = {
  parent: workspaces_EUS_FLLM_DEMO_OPS_la_name_resource
  name: 'WVDSessionHostManagement'
  properties: {
    totalRetentionInDays: 30
    plan: 'Analytics'
    schema: {
      name: 'WVDSessionHostManagement'
    }
    retentionInDays: 30
  }
}






resource smartdetectoralertrules_failure_anomalies_eus_fllm_demo_ops_ai_name_resource 'microsoft.alertsmanagement/smartdetectoralertrules@2021-04-01' = {
  name: smartdetectoralertrules_failure_anomalies_eus_fllm_demo_ops_ai_name
  location: 'global'
  properties: {
    description: 'Failure Anomalies notifies you of an unusual rise in the rate of failed HTTP requests or dependency calls.'
    state: 'Enabled'
    severity: 'Sev3'
    frequency: 'PT1M'
    detector: {
      id: 'FailureAnomaliesDetector'
    }
    scope: [
      components_EUS_FLLM_DEMO_OPS_ai_name_resource.id
    ]
    actionGroups: {
      groupIds: [
        actionGroups_Application_Insights_Smart_Detection_name_resource.id
      ]
    }
  }
}



resource registries_EUSFLLMDEMOOPScr_name_registries_EUSFLLMDEMOOPScr_name_583606f6817a4533a73ae7fb8c881c31 'Microsoft.ContainerRegistry/registries/privateEndpointConnections@2023-08-01-preview' = {
  parent: registries_EUSFLLMDEMOOPScr_name_resource
  name: '${registries_EUSFLLMDEMOOPScr_name}.583606f6817a4533a73ae7fb8c881c31'
  properties: {
    privateEndpoint: {
      id: privateEndpoints_EUS_FLLM_DEMO_OPS_registry_pe_name_resource.id
    }
    privateLinkServiceConnectionState: {
      status: 'Approved'
      description: 'Auto-Approved'
    }
  }
}

resource metricAlerts_EUS_FLLM_DEMO_OPS_ado_vmss_cpu_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_OPS_ado_vmss_cpu_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'DevOps'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    description: 'Alert on VMSS Threshold - Average CPU greater than 75% for 5 minutes'
    severity: 2
    enabled: true
    scopes: [
      virtualMachineScaleSets_EUS_FLLM_DEMO_OPS_ado_vmss_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          threshold: 75
          name: 'Metric1'
          metricNamespace: 'Microsoft.Compute/virtualmachinescalesets'
          metricName: 'Percentage CPU'
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
        actionGroupId: actionGroups_EUS_FLLM_DEMO_OPS_ag_name_resource.id
        webHookProperties: {}
      }
    ]
  }
}

resource metricAlerts_EUS_FLLM_DEMO_OPS_ado_vmss_disk_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_OPS_ado_vmss_disk_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'DevOps'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    description: 'Alert on VMSS Threshold - Average Disk Queue greater than 8 for 5 minutes'
    severity: 2
    enabled: true
    scopes: [
      virtualMachineScaleSets_EUS_FLLM_DEMO_OPS_ado_vmss_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          threshold: 8
          name: 'Metric1'
          metricNamespace: 'Microsoft.Compute/virtualmachinescalesets'
          metricName: 'OS Disk Queue Depth'
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
        actionGroupId: actionGroups_EUS_FLLM_DEMO_OPS_ag_name_resource.id
        webHookProperties: {}
      }
    ]
  }
}







resource metricAlerts_EUS_FLLM_DEMO_OPS_tfca_aci_cpu_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_OPS_tfca_aci_cpu_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'DevOps'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    description: 'Alert on Container Instance CPU Threshold - CPU Utilization over 80% for 5 minutes'
    severity: 2
    enabled: true
    scopes: [
      containerGroups_EUS_FLLM_DEMO_OPS_tfca_aci_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          threshold: 80
          name: 'Metric1'
          metricNamespace: 'Microsoft.ContainerInstance/containergroups'
          metricName: 'CpuUsage'
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
        actionGroupId: actionGroups_EUS_FLLM_DEMO_OPS_ag_name_resource.id
        webHookProperties: {}
      }
    ]
  }
}

resource metricAlerts_EUS_FLLM_DEMO_OPS_tfca_aci_ram_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_OPS_tfca_aci_ram_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'DevOps'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    description: 'Alert on Container Instance Memory Threshold - Memory Utilization over 80% for 5 minutes.'
    severity: 2
    enabled: true
    scopes: [
      containerGroups_EUS_FLLM_DEMO_OPS_tfca_aci_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          threshold: 80
          name: 'Metric1'
          metricNamespace: 'Microsoft.ContainerInstance/containergroups'
          metricName: 'MemoryUsage'
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
        actionGroupId: actionGroups_EUS_FLLM_DEMO_OPS_ag_name_resource.id
        webHookProperties: {}
      }
    ]
  }
}


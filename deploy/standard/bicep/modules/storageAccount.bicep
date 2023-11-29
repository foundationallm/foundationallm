
resource storageAccounts_eusfllmdemoopssa_name_resource 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccounts_eusfllmdemoopssa_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'DevOps'
    Workspace: 'foundationallm-ops'
  }
  sku: {
    name: 'Standard_LRS'
    tier: 'Standard'
  }
  kind: 'StorageV2'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    defaultToOAuthAuthentication: true
    publicNetworkAccess: 'Disabled'
    sasPolicy: {
      sasExpirationPeriod: '00.04:00:00'
      expirationAction: 'Log'
    }
    allowCrossTenantReplication: true
    isNfsV3Enabled: false
    isSftpEnabled: false
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: true
    isHnsEnabled: false
    networkAcls: {
      resourceAccessRules: [
        {
          tenantId: '22179471-b099-4504-bfdb-3f184cdae122'
          resourceId: '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/providers/Microsoft.Security/datascanners/storageDataScanner'
        }
      ]
      bypass: 'Logging, Metrics, AzureServices'
      virtualNetworkRules: []
      ipRules: []
      defaultAction: 'Deny'
    }
    supportsHttpsTrafficOnly: true
    encryption: {
      requireInfrastructureEncryption: true
      services: {
        file: {
          keyType: 'Account'
          enabled: true
        }
        blob: {
          keyType: 'Account'
          enabled: true
        }
      }
      keySource: 'Microsoft.Storage'
    }
    accessTier: 'Hot'
  }
}

resource systemTopics_eusfllmdemoopssa_d63dac3c_9957_4c24_9baf_ffb984d8ffc4_name_resource 'Microsoft.EventGrid/systemTopics@2023-06-01-preview' = {
  name: systemTopics_eusfllmdemoopssa_d63dac3c_9957_4c24_9baf_ffb984d8ffc4_name
  location: 'eastus'
  properties: {
    source: storageAccounts_eusfllmdemoopssa_name_resource.id
    topicType: 'microsoft.storage.storageaccounts'
  }
}


resource privateEndpoints_EUS_FLLM_DEMO_OPS_blob_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_OPS_blob_pe_name
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
        name: 'EUS-FLLM-DEMO-OPS-blob-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_OPS_blob_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-OPS-blob-connection'
        properties: {
          privateLinkServiceId: storageAccounts_eusfllmdemoopssa_name_resource.id
          groupIds: [
            'blob'
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


resource privateEndpoints_EUS_FLLM_DEMO_OPS_dfs_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_OPS_dfs_pe_name
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
        name: 'EUS-FLLM-DEMO-OPS-dfs-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_OPS_dfs_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-OPS-dfs-connection'
        properties: {
          privateLinkServiceId: storageAccounts_eusfllmdemoopssa_name_resource.id
          groupIds: [
            'dfs'
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


resource privateEndpoints_EUS_FLLM_DEMO_OPS_file_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_OPS_file_pe_name
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
        name: 'EUS-FLLM-DEMO-OPS-file-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_OPS_file_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-OPS-file-connection'
        properties: {
          privateLinkServiceId: storageAccounts_eusfllmdemoopssa_name_resource.id
          groupIds: [
            'file'
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

resource privateEndpoints_EUS_FLLM_DEMO_OPS_queue_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_OPS_queue_pe_name
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
        name: 'EUS-FLLM-DEMO-OPS-queue-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_OPS_queue_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-OPS-queue-connection'
        properties: {
          privateLinkServiceId: storageAccounts_eusfllmdemoopssa_name_resource.id
          groupIds: [
            'queue'
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


resource privateEndpoints_EUS_FLLM_DEMO_OPS_table_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_OPS_table_pe_name
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
        name: 'EUS-FLLM-DEMO-OPS-table-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_OPS_table_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-OPS-table-connection'
        properties: {
          privateLinkServiceId: storageAccounts_eusfllmdemoopssa_name_resource.id
          groupIds: [
            'table'
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

resource privateEndpoints_EUS_FLLM_DEMO_OPS_web_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_OPS_web_pe_name
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
        name: 'EUS-FLLM-DEMO-OPS-web-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_OPS_web_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-OPS-web-connection'
        properties: {
          privateLinkServiceId: storageAccounts_eusfllmdemoopssa_name_resource.id
          groupIds: [
            'web'
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
    customDnsConfigs: [
      {
        fqdn: 'eusfllmdemoopssa.z13.web.core.windows.net'
        ipAddresses: [
          '10.0.255.116'
        ]
      }
    ]
  }
}
resource storageAccounts_eusfllmdemoopssa_name_default 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccounts_eusfllmdemoopssa_name_resource
  name: 'default'
  sku: {
    name: 'Standard_LRS'
    tier: 'Standard'
  }
  properties: {
    changeFeed: {
      enabled: false
    }
    restorePolicy: {
      enabled: false
    }
    containerDeleteRetentionPolicy: {
      enabled: true
      days: 30
    }
    cors: {
      corsRules: []
    }
    deleteRetentionPolicy: {
      allowPermanentDelete: false
      enabled: true
      days: 30
    }
    isVersioningEnabled: true
  }
}
resource Microsoft_Storage_storageAccounts_fileServices_storageAccounts_eusfllmdemoopssa_name_default 'Microsoft.Storage/storageAccounts/fileServices@2023-01-01' = {
  parent: storageAccounts_eusfllmdemoopssa_name_resource
  name: 'default'
  sku: {
    name: 'Standard_LRS'
    tier: 'Standard'
  }
  properties: {
    protocolSettings: {
      smb: {}
    }
    cors: {
      corsRules: []
    }
    shareDeleteRetentionPolicy: {
      enabled: true
      days: 30
    }
  }
}

resource storageAccounts_eusfllmdemoopssa_name_storageAccounts_eusfllmdemoopssa_name_06f276cb_ea2c_4963_86e2_e63760986c6a 'Microsoft.Storage/storageAccounts/privateEndpointConnections@2023-01-01' = {
  parent: storageAccounts_eusfllmdemoopssa_name_resource
  name: '${storageAccounts_eusfllmdemoopssa_name}.06f276cb-ea2c-4963-86e2-e63760986c6a'
  properties: {
    provisioningState: 'Succeeded'
    privateEndpoint: {}
    privateLinkServiceConnectionState: {
      status: 'Approved'
      description: 'Auto-Approved'
      actionRequired: 'None'
    }
  }
}

resource storageAccounts_eusfllmdemoopssa_name_storageAccounts_eusfllmdemoopssa_name_1a3b36d6_1924_4d81_aa16_aac50bcdff4f 'Microsoft.Storage/storageAccounts/privateEndpointConnections@2023-01-01' = {
  parent: storageAccounts_eusfllmdemoopssa_name_resource
  name: '${storageAccounts_eusfllmdemoopssa_name}.1a3b36d6-1924-4d81-aa16-aac50bcdff4f'
  properties: {
    provisioningState: 'Succeeded'
    privateEndpoint: {}
    privateLinkServiceConnectionState: {
      status: 'Approved'
      description: 'Auto-Approved'
      actionRequired: 'None'
    }
  }
}

resource storageAccounts_eusfllmdemoopssa_name_storageAccounts_eusfllmdemoopssa_name_46b3022e_e532_4f44_8225_95158ee83971 'Microsoft.Storage/storageAccounts/privateEndpointConnections@2023-01-01' = {
  parent: storageAccounts_eusfllmdemoopssa_name_resource
  name: '${storageAccounts_eusfllmdemoopssa_name}.46b3022e-e532-4f44-8225-95158ee83971'
  properties: {
    provisioningState: 'Succeeded'
    privateEndpoint: {}
    privateLinkServiceConnectionState: {
      status: 'Approved'
      description: 'Auto-Approved'
      actionRequired: 'None'
    }
  }
}
resource storageAccounts_eusfllmdemoopssa_name_storageAccounts_eusfllmdemoopssa_name_8f311195_62ed_4690_a48c_c90755f68154 'Microsoft.Storage/storageAccounts/privateEndpointConnections@2023-01-01' = {
  parent: storageAccounts_eusfllmdemoopssa_name_resource
  name: '${storageAccounts_eusfllmdemoopssa_name}.8f311195-62ed-4690-a48c-c90755f68154'
  properties: {
    provisioningState: 'Succeeded'
    privateEndpoint: {}
    privateLinkServiceConnectionState: {
      status: 'Approved'
      description: 'Auto-Approved'
      actionRequired: 'None'
    }
  }
}

resource storageAccounts_eusfllmdemoopssa_name_storageAccounts_eusfllmdemoopssa_name_bf8e9dc2_9972_4463_a05b_f2d684d8b2ab 'Microsoft.Storage/storageAccounts/privateEndpointConnections@2023-01-01' = {
  parent: storageAccounts_eusfllmdemoopssa_name_resource
  name: '${storageAccounts_eusfllmdemoopssa_name}.bf8e9dc2-9972-4463-a05b-f2d684d8b2ab'
  properties: {
    provisioningState: 'Succeeded'
    privateEndpoint: {}
    privateLinkServiceConnectionState: {
      status: 'Approved'
      description: 'Auto-Approved'
      actionRequired: 'None'
    }
  }
}

resource storageAccounts_eusfllmdemoopssa_name_storageAccounts_eusfllmdemoopssa_name_cc2eaed0_c6b1_4272_8e31_d9575f24647f 'Microsoft.Storage/storageAccounts/privateEndpointConnections@2023-01-01' = {
  parent: storageAccounts_eusfllmdemoopssa_name_resource
  name: '${storageAccounts_eusfllmdemoopssa_name}.cc2eaed0-c6b1-4272-8e31-d9575f24647f'
  properties: {
    provisioningState: 'Succeeded'
    privateEndpoint: {}
    privateLinkServiceConnectionState: {
      status: 'Approved'
      description: 'Auto-Approved'
      actionRequired: 'None'
    }
  }
}

resource Microsoft_Storage_storageAccounts_queueServices_storageAccounts_eusfllmdemoopssa_name_default 'Microsoft.Storage/storageAccounts/queueServices@2023-01-01' = {
  parent: storageAccounts_eusfllmdemoopssa_name_resource
  name: 'default'
  properties: {
    cors: {
      corsRules: []
    }
  }
}

resource Microsoft_Storage_storageAccounts_tableServices_storageAccounts_eusfllmdemoopssa_name_default 'Microsoft.Storage/storageAccounts/tableServices@2023-01-01' = {
  parent: storageAccounts_eusfllmdemoopssa_name_resource
  name: 'default'
  properties: {
    cors: {
      corsRules: []
    }
  }
}

resource metricAlerts_EUS_FLLM_DEMO_OPS_sa_availability_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_OPS_sa_availability_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'DevOps'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    description: 'Alert on Storage Account Threshold - Account availability less than 99% for 5 minutes'
    severity: 1
    enabled: true
    scopes: [
      storageAccounts_eusfllmdemoopssa_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          threshold: 99
          name: 'Metric1'
          metricNamespace: 'Microsoft.Storage/storageaccounts'
          metricName: 'Availability'
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
        actionGroupId: actionGroups_EUS_FLLM_DEMO_OPS_ag_name_resource.id
        webHookProperties: {}
      }
    ]
  }
}


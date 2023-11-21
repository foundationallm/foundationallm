param storageAccounts_efllmdstoragesa_name string = 'efllmdstoragesa'
param privateEndpoints_e_FLLM_d_STORAGE_dfs_pe_name string = 'e-FLLM-d-STORAGE-dfs-pe'
param privateEndpoints_e_FLLM_d_STORAGE_web_pe_name string = 'e-FLLM-d-STORAGE-web-pe'
param privateEndpoints_e_FLLM_d_STORAGE_blob_pe_name string = 'e-FLLM-d-STORAGE-blob-pe'
param privateEndpoints_e_FLLM_d_STORAGE_file_pe_name string = 'e-FLLM-d-STORAGE-file-pe'
param privateEndpoints_e_FLLM_d_STORAGE_queue_pe_name string = 'e-FLLM-d-STORAGE-queue-pe'
param privateEndpoints_e_FLLM_d_STORAGE_table_pe_name string = 'e-FLLM-d-STORAGE-table-pe'
param databaseAccounts_eus_fllm_demo_storage_cdb_name string = 'eus-fllm-demo-storage-cdb'
param privateEndpoints_EUS_FLLM_DEMO_STORAGE_sql_pe_name string = 'EUS-FLLM-DEMO-STORAGE-sql-pe'
param metricAlerts_e_FLLM_d_STORAGE_sa_availability_alert_name string = 'e-FLLM-d-STORAGE-sa-availability-alert'
param metricAlerts_EUS_FLLM_DEMO_STORAGE_cdb_availability_alert_name string = 'EUS-FLLM-DEMO-STORAGE-cdb-availability-alert'
param systemTopics_efllmdstoragesa_e890ac3a_6c93_4b83_8f98_137345ee5922_name string = 'efllmdstoragesa-e890ac3a-6c93-4b83-8f98-137345ee5922'
param actionGroups_EUS_FLLM_DEMO_OPS_ag_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-OPS-rg/providers/Microsoft.Insights/actionGroups/EUS-FLLM-DEMO-OPS-ag'
param virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-NET-rg/providers/Microsoft.Network/virtualNetworks/EUS-FLLM-DEMO-NET-vnet'
param privateDnsZones_privatelink_blob_core_windows_net_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DNS-rg/providers/Microsoft.Network/privateDnsZones/privatelink.blob.core.windows.net'
param privateDnsZones_privatelink_dfs_core_windows_net_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DNS-rg/providers/Microsoft.Network/privateDnsZones/privatelink.dfs.core.windows.net'
param privateDnsZones_privatelink_file_core_windows_net_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DNS-rg/providers/Microsoft.Network/privateDnsZones/privatelink.file.core.windows.net'
param privateDnsZones_privatelink_queue_core_windows_net_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DNS-rg/providers/Microsoft.Network/privateDnsZones/privatelink.queue.core.windows.net'
param privateDnsZones_privatelink_table_core_windows_net_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DNS-rg/providers/Microsoft.Network/privateDnsZones/privatelink.table.core.windows.net'
param privateDnsZones_privatelink_azurewebsites_net_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DNS-rg/providers/Microsoft.Network/privateDnsZones/privatelink.azurewebsites.net'
param privateDnsZones_privatelink_documents_azure_com_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DNS-rg/providers/Microsoft.Network/privateDnsZones/privatelink.documents.azure.com'

resource databaseAccounts_eus_fllm_demo_storage_cdb_name_resource 'Microsoft.DocumentDB/databaseAccounts@2023-09-15' = {
  name: databaseAccounts_eus_fllm_demo_storage_cdb_name
  location: 'East US'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'FoundationaLLM-Platform'
  }
  kind: 'GlobalDocumentDB'
  identity: {
    type: 'None'
  }
  properties: {
    publicNetworkAccess: 'Disabled'
    enableAutomaticFailover: false
    enableMultipleWriteLocations: false
    isVirtualNetworkFilterEnabled: false
    virtualNetworkRules: []
    disableKeyBasedMetadataWriteAccess: false
    enableFreeTier: false
    enableAnalyticalStorage: false
    analyticalStorageConfiguration: {
      schemaType: 'WellDefined'
    }
    createMode: 'Default'
    databaseAccountOfferType: 'Standard'
    defaultIdentity: 'FirstPartyIdentity'
    networkAclBypass: 'None'
    disableLocalAuth: false
    enablePartitionMerge: false
    enableBurstCapacity: false
    minimalTlsVersion: 'Tls12'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
      maxIntervalInSeconds: 5
      maxStalenessPrefix: 100
    }
    locations: [
      {
        locationName: 'East US'
        provisioningState: 'Succeeded'
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    cors: []
    capabilities: []
    ipRules: []
    backupPolicy: {
      type: 'Continuous'
      continuousModeProperties: {
        tier: 'Continuous30Days'
      }
    }
    networkAclBypassResourceIds: []
    capacity: {
      totalThroughputLimit: 5000
    }
    keysMetadata: {}
  }
}

resource storageAccounts_efllmdstoragesa_name_resource 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccounts_efllmdstoragesa_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'FoundationaLLM-Platform'
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

resource databaseAccounts_eus_fllm_demo_storage_cdb_name_database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_storage_cdb_name_resource
  name: 'database'
  properties: {
    resource: {
      id: 'database'
    }
  }
}

resource databaseAccounts_eus_fllm_demo_storage_cdb_name_00000000_0000_0000_0000_000000000001 'Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_storage_cdb_name_resource
  name: '00000000-0000-0000-0000-000000000001'
  properties: {
    roleName: 'Cosmos DB Built-in Data Reader'
    type: 'BuiltInRole'
    assignableScopes: [
      databaseAccounts_eus_fllm_demo_storage_cdb_name_resource.id
    ]
    permissions: [
      {
        dataActions: [
          'Microsoft.DocumentDB/databaseAccounts/readMetadata'
          'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/executeQuery'
          'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/readChangeFeed'
          'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/read'
        ]
        notDataActions: []
      }
    ]
  }
}

resource databaseAccounts_eus_fllm_demo_storage_cdb_name_00000000_0000_0000_0000_000000000002 'Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_storage_cdb_name_resource
  name: '00000000-0000-0000-0000-000000000002'
  properties: {
    roleName: 'Cosmos DB Built-in Data Contributor'
    type: 'BuiltInRole'
    assignableScopes: [
      databaseAccounts_eus_fllm_demo_storage_cdb_name_resource.id
    ]
    permissions: [
      {
        dataActions: [
          'Microsoft.DocumentDB/databaseAccounts/readMetadata'
          'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/*'
          'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/*'
        ]
        notDataActions: []
      }
    ]
  }
}

resource systemTopics_efllmdstoragesa_e890ac3a_6c93_4b83_8f98_137345ee5922_name_resource 'Microsoft.EventGrid/systemTopics@2023-06-01-preview' = {
  name: systemTopics_efllmdstoragesa_e890ac3a_6c93_4b83_8f98_137345ee5922_name
  location: 'eastus'
  properties: {
    source: storageAccounts_efllmdstoragesa_name_resource.id
    topicType: 'microsoft.storage.storageaccounts'
  }
}

resource systemTopics_efllmdstoragesa_e890ac3a_6c93_4b83_8f98_137345ee5922_name_StorageAntimalwareSubscription 'Microsoft.EventGrid/systemTopics/eventSubscriptions@2023-06-01-preview' = {
  parent: systemTopics_efllmdstoragesa_e890ac3a_6c93_4b83_8f98_137345ee5922_name_resource
  name: 'StorageAntimalwareSubscription'
  properties: {
    destination: {
      properties: {
        maxEventsPerBatch: 1
        preferredBatchSizeInKilobytes: 64
        azureActiveDirectoryTenantId: '33e01921-4d64-4f8c-a055-5bdaffd5e33d'
        azureActiveDirectoryApplicationIdOrUri: 'f1f8da5f-609a-401d-85b2-d498116b7265'
      }
      endpointType: 'WebHook'
    }
    filter: {
      includedEventTypes: [
        'Microsoft.Storage.BlobCreated'
      ]
      advancedFilters: [
        {
          values: [
            'BlockBlob'
          ]
          operatorType: 'StringContains'
          key: 'data.blobType'
        }
      ]
    }
    eventDeliverySchema: 'EventGridSchema'
    retryPolicy: {
      maxDeliveryAttempts: 30
      eventTimeToLiveInMinutes: 1440
    }
  }
}

resource metricAlerts_e_FLLM_d_STORAGE_sa_availability_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_e_FLLM_d_STORAGE_sa_availability_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    description: 'Alert on Storage Account Threshold - Account availability less than 99% for 5 minutes'
    severity: 1
    enabled: true
    scopes: [
      storageAccounts_efllmdstoragesa_name_resource.id
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
        actionGroupId: actionGroups_EUS_FLLM_DEMO_OPS_ag_externalid
        webHookProperties: {}
      }
    ]
  }
}

resource metricAlerts_EUS_FLLM_DEMO_STORAGE_cdb_availability_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_STORAGE_cdb_availability_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    description: 'Service availability less than 99% for 1 hour'
    severity: 0
    enabled: true
    scopes: [
      databaseAccounts_eus_fllm_demo_storage_cdb_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT1H'
    criteria: {
      allOf: [
        {
          threshold: 99
          name: 'Metric1'
          metricNamespace: 'Microsoft.DocumentDB/DatabaseAccounts'
          metricName: 'ServiceAvailability'
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

resource privateEndpoints_e_FLLM_d_STORAGE_blob_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_e_FLLM_d_STORAGE_blob_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'e-FLLM-d-STORAGE-blob-connection'
        id: '${privateEndpoints_e_FLLM_d_STORAGE_blob_pe_name_resource.id}/privateLinkServiceConnections/e-FLLM-d-STORAGE-blob-connection'
        properties: {
          privateLinkServiceId: storageAccounts_efllmdstoragesa_name_resource.id
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
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/FLLMStorage'
    }
    ipConfigurations: []
    customDnsConfigs: []
  }
}

resource privateEndpoints_e_FLLM_d_STORAGE_dfs_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_e_FLLM_d_STORAGE_dfs_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'e-FLLM-d-STORAGE-dfs-connection'
        id: '${privateEndpoints_e_FLLM_d_STORAGE_dfs_pe_name_resource.id}/privateLinkServiceConnections/e-FLLM-d-STORAGE-dfs-connection'
        properties: {
          privateLinkServiceId: storageAccounts_efllmdstoragesa_name_resource.id
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
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/FLLMStorage'
    }
    ipConfigurations: []
    customDnsConfigs: []
  }
}

resource privateEndpoints_e_FLLM_d_STORAGE_file_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_e_FLLM_d_STORAGE_file_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'e-FLLM-d-STORAGE-file-connection'
        id: '${privateEndpoints_e_FLLM_d_STORAGE_file_pe_name_resource.id}/privateLinkServiceConnections/e-FLLM-d-STORAGE-file-connection'
        properties: {
          privateLinkServiceId: storageAccounts_efllmdstoragesa_name_resource.id
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
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/FLLMStorage'
    }
    ipConfigurations: []
    customDnsConfigs: []
  }
}

resource privateEndpoints_e_FLLM_d_STORAGE_queue_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_e_FLLM_d_STORAGE_queue_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'e-FLLM-d-STORAGE-queue-connection'
        id: '${privateEndpoints_e_FLLM_d_STORAGE_queue_pe_name_resource.id}/privateLinkServiceConnections/e-FLLM-d-STORAGE-queue-connection'
        properties: {
          privateLinkServiceId: storageAccounts_efllmdstoragesa_name_resource.id
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
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/FLLMStorage'
    }
    ipConfigurations: []
    customDnsConfigs: []
  }
}

resource privateEndpoints_e_FLLM_d_STORAGE_table_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_e_FLLM_d_STORAGE_table_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'e-FLLM-d-STORAGE-table-connection'
        id: '${privateEndpoints_e_FLLM_d_STORAGE_table_pe_name_resource.id}/privateLinkServiceConnections/e-FLLM-d-STORAGE-table-connection'
        properties: {
          privateLinkServiceId: storageAccounts_efllmdstoragesa_name_resource.id
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
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/FLLMStorage'
    }
    ipConfigurations: []
    customDnsConfigs: []
  }
}

resource privateEndpoints_e_FLLM_d_STORAGE_web_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_e_FLLM_d_STORAGE_web_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'e-FLLM-d-STORAGE-web-connection'
        id: '${privateEndpoints_e_FLLM_d_STORAGE_web_pe_name_resource.id}/privateLinkServiceConnections/e-FLLM-d-STORAGE-web-connection'
        properties: {
          privateLinkServiceId: storageAccounts_efllmdstoragesa_name_resource.id
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
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/FLLMStorage'
    }
    ipConfigurations: []
    customDnsConfigs: [
      {
        fqdn: 'efllmdstoragesa.z13.web.core.windows.net'
        ipAddresses: [
          '10.0.4.6'
        ]
      }
    ]
  }
}

resource privateEndpoints_EUS_FLLM_DEMO_STORAGE_sql_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_STORAGE_sql_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'EUS-FLLM-DEMO-STORAGE-sql-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_STORAGE_sql_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-STORAGE-sql-connection'
        properties: {
          privateLinkServiceId: databaseAccounts_eus_fllm_demo_storage_cdb_name_resource.id
          groupIds: [
            'Sql'
          ]
          privateLinkServiceConnectionState: {
            status: 'Approved'
            actionsRequired: 'None'
          }
        }
      }
    ]
    manualPrivateLinkServiceConnections: []
    subnet: {
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/FLLMStorage'
    }
    ipConfigurations: []
    customDnsConfigs: []
  }
}

resource privateEndpoints_e_FLLM_d_STORAGE_blob_pe_name_blob 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_e_FLLM_d_STORAGE_blob_pe_name}/blob'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink.blob.core.windows.net'
        properties: {
          privateDnsZoneId: privateDnsZones_privatelink_blob_core_windows_net_externalid
        }
      }
    ]
  }
  dependsOn: [
    privateEndpoints_e_FLLM_d_STORAGE_blob_pe_name_resource
  ]
}

resource privateEndpoints_e_FLLM_d_STORAGE_dfs_pe_name_dfs 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_e_FLLM_d_STORAGE_dfs_pe_name}/dfs'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink.dfs.core.windows.net'
        properties: {
          privateDnsZoneId: privateDnsZones_privatelink_dfs_core_windows_net_externalid
        }
      }
    ]
  }
  dependsOn: [
    privateEndpoints_e_FLLM_d_STORAGE_dfs_pe_name_resource
  ]
}

resource privateEndpoints_e_FLLM_d_STORAGE_file_pe_name_file 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_e_FLLM_d_STORAGE_file_pe_name}/file'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink.file.core.windows.net'
        properties: {
          privateDnsZoneId: privateDnsZones_privatelink_file_core_windows_net_externalid
        }
      }
    ]
  }
  dependsOn: [
    privateEndpoints_e_FLLM_d_STORAGE_file_pe_name_resource
  ]
}

resource privateEndpoints_e_FLLM_d_STORAGE_queue_pe_name_queue 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_e_FLLM_d_STORAGE_queue_pe_name}/queue'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink.queue.core.windows.net'
        properties: {
          privateDnsZoneId: privateDnsZones_privatelink_queue_core_windows_net_externalid
        }
      }
    ]
  }
  dependsOn: [
    privateEndpoints_e_FLLM_d_STORAGE_queue_pe_name_resource
  ]
}

resource privateEndpoints_EUS_FLLM_DEMO_STORAGE_sql_pe_name_sql 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_EUS_FLLM_DEMO_STORAGE_sql_pe_name}/sql'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink.documents.azure.com'
        properties: {
          privateDnsZoneId: privateDnsZones_privatelink_documents_azure_com_externalid
        }
      }
    ]
  }
  dependsOn: [
    privateEndpoints_EUS_FLLM_DEMO_STORAGE_sql_pe_name_resource
  ]
}

resource privateEndpoints_e_FLLM_d_STORAGE_table_pe_name_table 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_e_FLLM_d_STORAGE_table_pe_name}/table'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink.table.core.windows.net'
        properties: {
          privateDnsZoneId: privateDnsZones_privatelink_table_core_windows_net_externalid
        }
      }
    ]
  }
  dependsOn: [
    privateEndpoints_e_FLLM_d_STORAGE_table_pe_name_resource
  ]
}

resource privateEndpoints_e_FLLM_d_STORAGE_web_pe_name_web 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_e_FLLM_d_STORAGE_web_pe_name}/web'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink.azurewebsites.net'
        properties: {
          privateDnsZoneId: privateDnsZones_privatelink_azurewebsites_net_externalid
        }
      }
    ]
  }
  dependsOn: [
    privateEndpoints_e_FLLM_d_STORAGE_web_pe_name_resource
  ]
}

resource storageAccounts_efllmdstoragesa_name_default 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccounts_efllmdstoragesa_name_resource
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

resource Microsoft_Storage_storageAccounts_fileServices_storageAccounts_efllmdstoragesa_name_default 'Microsoft.Storage/storageAccounts/fileServices@2023-01-01' = {
  parent: storageAccounts_efllmdstoragesa_name_resource
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

resource storageAccounts_efllmdstoragesa_name_storageAccounts_efllmdstoragesa_name_2003a8b1_0274_4082_9b4f_69e113889118 'Microsoft.Storage/storageAccounts/privateEndpointConnections@2023-01-01' = {
  parent: storageAccounts_efllmdstoragesa_name_resource
  name: '${storageAccounts_efllmdstoragesa_name}.2003a8b1-0274-4082-9b4f-69e113889118'
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

resource storageAccounts_efllmdstoragesa_name_storageAccounts_efllmdstoragesa_name_32191dcc_5e9b_427b_9516_fa3bc4920570 'Microsoft.Storage/storageAccounts/privateEndpointConnections@2023-01-01' = {
  parent: storageAccounts_efllmdstoragesa_name_resource
  name: '${storageAccounts_efllmdstoragesa_name}.32191dcc-5e9b-427b-9516-fa3bc4920570'
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

resource storageAccounts_efllmdstoragesa_name_storageAccounts_efllmdstoragesa_name_9372f45d_62c9_4cc5_92ef_11b16de61927 'Microsoft.Storage/storageAccounts/privateEndpointConnections@2023-01-01' = {
  parent: storageAccounts_efllmdstoragesa_name_resource
  name: '${storageAccounts_efllmdstoragesa_name}.9372f45d-62c9-4cc5-92ef-11b16de61927'
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

resource storageAccounts_efllmdstoragesa_name_storageAccounts_efllmdstoragesa_name_9783273b_7c94_4b57_b6fa_0ac74465412e 'Microsoft.Storage/storageAccounts/privateEndpointConnections@2023-01-01' = {
  parent: storageAccounts_efllmdstoragesa_name_resource
  name: '${storageAccounts_efllmdstoragesa_name}.9783273b-7c94-4b57-b6fa-0ac74465412e'
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

resource storageAccounts_efllmdstoragesa_name_storageAccounts_efllmdstoragesa_name_b9c2be2b_5764_4fd0_8b08_999ac179952c 'Microsoft.Storage/storageAccounts/privateEndpointConnections@2023-01-01' = {
  parent: storageAccounts_efllmdstoragesa_name_resource
  name: '${storageAccounts_efllmdstoragesa_name}.b9c2be2b-5764-4fd0-8b08-999ac179952c'
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

resource storageAccounts_efllmdstoragesa_name_storageAccounts_efllmdstoragesa_name_e79faf8b_c93c_46ae_8219_e8de2cd03059 'Microsoft.Storage/storageAccounts/privateEndpointConnections@2023-01-01' = {
  parent: storageAccounts_efllmdstoragesa_name_resource
  name: '${storageAccounts_efllmdstoragesa_name}.e79faf8b-c93c-46ae-8219-e8de2cd03059'
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

resource Microsoft_Storage_storageAccounts_queueServices_storageAccounts_efllmdstoragesa_name_default 'Microsoft.Storage/storageAccounts/queueServices@2023-01-01' = {
  parent: storageAccounts_efllmdstoragesa_name_resource
  name: 'default'
  properties: {
    cors: {
      corsRules: []
    }
  }
}

resource Microsoft_Storage_storageAccounts_tableServices_storageAccounts_efllmdstoragesa_name_default 'Microsoft.Storage/storageAccounts/tableServices@2023-01-01' = {
  parent: storageAccounts_efllmdstoragesa_name_resource
  name: 'default'
  properties: {
    cors: {
      corsRules: []
    }
  }
}

resource databaseAccounts_eus_fllm_demo_storage_cdb_name_EUS_FLLM_DEMO_STORAGE_sql_pe 'Microsoft.DocumentDB/databaseAccounts/privateEndpointConnections@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_storage_cdb_name_resource
  name: 'EUS-FLLM-DEMO-STORAGE-sql-pe'
  properties: {
    provisioningState: 'Succeeded'
    groupId: 'Sql'
    privateEndpoint: {
      id: privateEndpoints_EUS_FLLM_DEMO_STORAGE_sql_pe_name_resource.id
    }
    privateLinkServiceConnectionState: {
      status: 'Approved'
    }
  }
}

resource databaseAccounts_eus_fllm_demo_storage_cdb_name_database_completions 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_storage_cdb_name_database
  name: 'completions'
  properties: {
    resource: {
      id: 'completions'
      indexingPolicy: {
        indexingMode: 'consistent'
        automatic: true
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
      }
      partitionKey: {
        paths: [
          '/sessionId'
        ]
        kind: 'Hash'
        version: 2
      }
      uniqueKeyPolicy: {
        uniqueKeys: []
      }
      conflictResolutionPolicy: {
        mode: 'LastWriterWins'
        conflictResolutionPath: '/_ts'
      }
    }
  }
  dependsOn: [

    databaseAccounts_eus_fllm_demo_storage_cdb_name_resource
  ]
}

resource databaseAccounts_eus_fllm_demo_storage_cdb_name_database_customer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_storage_cdb_name_database
  name: 'customer'
  properties: {
    resource: {
      id: 'customer'
      indexingPolicy: {
        indexingMode: 'consistent'
        automatic: true
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
      }
      partitionKey: {
        paths: [
          '/customerId'
        ]
        kind: 'Hash'
        version: 2
      }
      uniqueKeyPolicy: {
        uniqueKeys: []
      }
      conflictResolutionPolicy: {
        mode: 'LastWriterWins'
        conflictResolutionPath: '/_ts'
      }
    }
  }
  dependsOn: [

    databaseAccounts_eus_fllm_demo_storage_cdb_name_resource
  ]
}

resource databaseAccounts_eus_fllm_demo_storage_cdb_name_database_embedding 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_storage_cdb_name_database
  name: 'embedding'
  properties: {
    resource: {
      id: 'embedding'
      indexingPolicy: {
        indexingMode: 'consistent'
        automatic: true
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
      }
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
        version: 2
      }
      uniqueKeyPolicy: {
        uniqueKeys: []
      }
      conflictResolutionPolicy: {
        mode: 'LastWriterWins'
        conflictResolutionPath: '/_ts'
      }
    }
  }
  dependsOn: [

    databaseAccounts_eus_fllm_demo_storage_cdb_name_resource
  ]
}

resource databaseAccounts_eus_fllm_demo_storage_cdb_name_database_leases 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_storage_cdb_name_database
  name: 'leases'
  properties: {
    resource: {
      id: 'leases'
      indexingPolicy: {
        indexingMode: 'consistent'
        automatic: true
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
      }
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
        version: 2
      }
      uniqueKeyPolicy: {
        uniqueKeys: []
      }
      conflictResolutionPolicy: {
        mode: 'LastWriterWins'
        conflictResolutionPath: '/_ts'
      }
    }
  }
  dependsOn: [

    databaseAccounts_eus_fllm_demo_storage_cdb_name_resource
  ]
}

resource databaseAccounts_eus_fllm_demo_storage_cdb_name_database_product 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_storage_cdb_name_database
  name: 'product'
  properties: {
    resource: {
      id: 'product'
      indexingPolicy: {
        indexingMode: 'consistent'
        automatic: true
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
      }
      partitionKey: {
        paths: [
          '/categoryId'
        ]
        kind: 'Hash'
        version: 2
      }
      uniqueKeyPolicy: {
        uniqueKeys: []
      }
      conflictResolutionPolicy: {
        mode: 'LastWriterWins'
        conflictResolutionPath: '/_ts'
      }
    }
  }
  dependsOn: [

    databaseAccounts_eus_fllm_demo_storage_cdb_name_resource
  ]
}

resource storageAccounts_efllmdstoragesa_name_default_agents 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: storageAccounts_efllmdstoragesa_name_default
  name: 'agents'
  properties: {
    immutableStorageWithVersioning: {
      enabled: false
    }
    defaultEncryptionScope: '$account-encryption-key'
    denyEncryptionScopeOverride: false
    publicAccess: 'None'
  }
  dependsOn: [

    storageAccounts_efllmdstoragesa_name_resource
  ]
}

resource storageAccounts_efllmdstoragesa_name_default_data_sources 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: storageAccounts_efllmdstoragesa_name_default
  name: 'data-sources'
  properties: {
    immutableStorageWithVersioning: {
      enabled: false
    }
    defaultEncryptionScope: '$account-encryption-key'
    denyEncryptionScopeOverride: false
    publicAccess: 'None'
  }
  dependsOn: [

    storageAccounts_efllmdstoragesa_name_resource
  ]
}

resource storageAccounts_efllmdstoragesa_name_default_demos_source 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: storageAccounts_efllmdstoragesa_name_default
  name: 'demos-source'
  properties: {
    immutableStorageWithVersioning: {
      enabled: false
    }
    defaultEncryptionScope: '$account-encryption-key'
    denyEncryptionScopeOverride: false
    publicAccess: 'None'
  }
  dependsOn: [

    storageAccounts_efllmdstoragesa_name_resource
  ]
}

resource storageAccounts_efllmdstoragesa_name_default_foundationallm_source 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: storageAccounts_efllmdstoragesa_name_default
  name: 'foundationallm-source'
  properties: {
    immutableStorageWithVersioning: {
      enabled: false
    }
    defaultEncryptionScope: '$account-encryption-key'
    denyEncryptionScopeOverride: false
    publicAccess: 'None'
  }
  dependsOn: [

    storageAccounts_efllmdstoragesa_name_resource
  ]
}

resource storageAccounts_efllmdstoragesa_name_default_hai_source 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: storageAccounts_efllmdstoragesa_name_default
  name: 'hai-source'
  properties: {
    immutableStorageWithVersioning: {
      enabled: false
    }
    defaultEncryptionScope: '$account-encryption-key'
    denyEncryptionScopeOverride: false
    publicAccess: 'None'
  }
  dependsOn: [

    storageAccounts_efllmdstoragesa_name_resource
  ]
}

resource storageAccounts_efllmdstoragesa_name_default_prompts 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: storageAccounts_efllmdstoragesa_name_default
  name: 'prompts'
  properties: {
    immutableStorageWithVersioning: {
      enabled: false
    }
    defaultEncryptionScope: '$account-encryption-key'
    denyEncryptionScopeOverride: false
    publicAccess: 'None'
  }
  dependsOn: [

    storageAccounts_efllmdstoragesa_name_resource
  ]
}

resource storageAccounts_efllmdstoragesa_name_default_sdzwa_source 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: storageAccounts_efllmdstoragesa_name_default
  name: 'sdzwa-source'
  properties: {
    immutableStorageWithVersioning: {
      enabled: false
    }
    defaultEncryptionScope: '$account-encryption-key'
    denyEncryptionScopeOverride: false
    publicAccess: 'None'
  }
  dependsOn: [

    storageAccounts_efllmdstoragesa_name_resource
  ]
}

resource storageAccounts_efllmdstoragesa_name_default_solliance_source 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: storageAccounts_efllmdstoragesa_name_default
  name: 'solliance-source'
  properties: {
    immutableStorageWithVersioning: {
      enabled: false
    }
    defaultEncryptionScope: '$account-encryption-key'
    denyEncryptionScopeOverride: false
    publicAccess: 'None'
  }
  dependsOn: [

    storageAccounts_efllmdstoragesa_name_resource
  ]
}

resource databaseAccounts_eus_fllm_demo_storage_cdb_name_database_completions_default 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/throughputSettings@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_storage_cdb_name_database_completions
  name: 'default'
  properties: {
    resource: {
      throughput: 100
      autoscaleSettings: {
        maxThroughput: 1000
      }
    }
  }
  dependsOn: [

    databaseAccounts_eus_fllm_demo_storage_cdb_name_database
    databaseAccounts_eus_fllm_demo_storage_cdb_name_resource
  ]
}

resource databaseAccounts_eus_fllm_demo_storage_cdb_name_database_customer_default 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/throughputSettings@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_storage_cdb_name_database_customer
  name: 'default'
  properties: {
    resource: {
      throughput: 100
      autoscaleSettings: {
        maxThroughput: 1000
      }
    }
  }
  dependsOn: [

    databaseAccounts_eus_fllm_demo_storage_cdb_name_database
    databaseAccounts_eus_fllm_demo_storage_cdb_name_resource
  ]
}

resource databaseAccounts_eus_fllm_demo_storage_cdb_name_database_embedding_default 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/throughputSettings@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_storage_cdb_name_database_embedding
  name: 'default'
  properties: {
    resource: {
      throughput: 100
      autoscaleSettings: {
        maxThroughput: 1000
      }
    }
  }
  dependsOn: [

    databaseAccounts_eus_fllm_demo_storage_cdb_name_database
    databaseAccounts_eus_fllm_demo_storage_cdb_name_resource
  ]
}

resource databaseAccounts_eus_fllm_demo_storage_cdb_name_database_leases_default 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/throughputSettings@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_storage_cdb_name_database_leases
  name: 'default'
  properties: {
    resource: {
      throughput: 100
      autoscaleSettings: {
        maxThroughput: 1000
      }
    }
  }
  dependsOn: [

    databaseAccounts_eus_fllm_demo_storage_cdb_name_database
    databaseAccounts_eus_fllm_demo_storage_cdb_name_resource
  ]
}

resource databaseAccounts_eus_fllm_demo_storage_cdb_name_database_product_default 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/throughputSettings@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_storage_cdb_name_database_product
  name: 'default'
  properties: {
    resource: {
      throughput: 100
      autoscaleSettings: {
        maxThroughput: 1000
      }
    }
  }
  dependsOn: [

    databaseAccounts_eus_fllm_demo_storage_cdb_name_database
    databaseAccounts_eus_fllm_demo_storage_cdb_name_resource
  ]
}
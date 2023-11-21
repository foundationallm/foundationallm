@secure()
param vulnerabilityAssessments_Default_storageContainerPath string
param servers_eusfllmdemodatamssql_name string = 'eusfllmdemodatamssql'
param storageAccounts_stcocotemp_name string = 'stcocotemp'
param storageAccounts_eusfllmdemodatasa_name string = 'eusfllmdemodatasa'
param privateEndpoints_cocorahs_st_temp_kab_name string = 'cocorahs-st-temp-kab'
param databaseAccounts_eus_fllm_demo_data_cdb_name string = 'eus-fllm-demo-data-cdb'
param privateEndpoints_EUS_FLLM_DEMO_DATA_dfs_pe_name string = 'EUS-FLLM-DEMO-DATA-dfs-pe'
param privateEndpoints_EUS_FLLM_DEMO_DATA_sql_pe_name string = 'EUS-FLLM-DEMO-DATA-sql-pe'
param privateEndpoints_EUS_FLLM_DEMO_DATA_web_pe_name string = 'EUS-FLLM-DEMO-DATA-web-pe'
param privateEndpoints_EUS_FLLM_DEMO_DATA_blob_pe_name string = 'EUS-FLLM-DEMO-DATA-blob-pe'
param privateEndpoints_EUS_FLLM_DEMO_DATA_file_pe_name string = 'EUS-FLLM-DEMO-DATA-file-pe'
param privateEndpoints_EUS_FLLM_DEMO_DATA_queue_pe_name string = 'EUS-FLLM-DEMO-DATA-queue-pe'
param privateEndpoints_EUS_FLLM_DEMO_DATA_table_pe_name string = 'EUS-FLLM-DEMO-DATA-table-pe'
param metricAlerts_EUS_FLLM_DEMO_DATA_mssql_cpu_alert_name string = 'EUS-FLLM-DEMO-DATA-mssql-cpu-alert'
param privateEndpoints_EUS_FLLM_DEMO_DATA_sqlServer_pe_name string = 'EUS-FLLM-DEMO-DATA-sqlServer-pe'
param metricAlerts_EUS_FLLM_DEMO_DATA_sa_availability_alert_name string = 'EUS-FLLM-DEMO-DATA-sa-availability-alert'
param metricAlerts_EUS_FLLM_DEMO_DATA_cdb_availability_alert_name string = 'EUS-FLLM-DEMO-DATA-cdb-availability-alert'
param systemTopics_stcocotemp_f0d1fd7b_3864_45c9_9a81_6649ae2c59dd_name string = 'stcocotemp-f0d1fd7b-3864-45c9-9a81-6649ae2c59dd'
param systemTopics_eusfllmdemodatasa_aadfcca5_584f_459b_8f07_4cc521f6fef4_name string = 'eusfllmdemodatasa-aadfcca5-584f-459b-8f07-4cc521f6fef4'
param actionGroups_EUS_FLLM_DEMO_OPS_ag_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-OPS-rg/providers/Microsoft.Insights/actionGroups/EUS-FLLM-DEMO-OPS-ag'
param virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-NET-rg/providers/Microsoft.Network/virtualNetworks/EUS-FLLM-DEMO-NET-vnet'
param privateDnsZones_privatelink_blob_core_windows_net_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/eus-fllm-demo-dns-rg/providers/Microsoft.Network/privateDnsZones/privatelink.blob.core.windows.net'
param privateDnsZones_privatelink_dfs_core_windows_net_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DNS-rg/providers/Microsoft.Network/privateDnsZones/privatelink.dfs.core.windows.net'
param privateDnsZones_privatelink_file_core_windows_net_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DNS-rg/providers/Microsoft.Network/privateDnsZones/privatelink.file.core.windows.net'
param privateDnsZones_privatelink_queue_core_windows_net_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DNS-rg/providers/Microsoft.Network/privateDnsZones/privatelink.queue.core.windows.net'
param privateDnsZones_privatelink_documents_azure_com_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DNS-rg/providers/Microsoft.Network/privateDnsZones/privatelink.documents.azure.com'
param privateDnsZones_privatelink_database_windows_net_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DNS-rg/providers/Microsoft.Network/privateDnsZones/privatelink.database.windows.net'
param privateDnsZones_privatelink_table_core_windows_net_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DNS-rg/providers/Microsoft.Network/privateDnsZones/privatelink.table.core.windows.net'
param privateDnsZones_privatelink_azurewebsites_net_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DNS-rg/providers/Microsoft.Network/privateDnsZones/privatelink.azurewebsites.net'

resource databaseAccounts_eus_fllm_demo_data_cdb_name_resource 'Microsoft.DocumentDB/databaseAccounts@2023-09-15' = {
  name: databaseAccounts_eus_fllm_demo_data_cdb_name
  location: 'East US'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'foundationallm-customer'
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

resource servers_eusfllmdemodatamssql_name_resource 'Microsoft.Sql/servers@2023-02-01-preview' = {
  name: servers_eusfllmdemodatamssql_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'foundationallm-customer'
  }
  kind: 'v12.0'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    administratorLogin: 'CloudSA01e4a324'
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'Group'
      login: 'FoundationaLLM SQL Admins'
      sid: '73d59f98-857b-45e7-950b-5ee30d289bc8'
      tenantId: '22179471-b099-4504-bfdb-3f184cdae122'
      azureADOnlyAuthentication: false
    }
    restrictOutboundNetworkAccess: 'Disabled'
  }
}

resource storageAccounts_eusfllmdemodatasa_name_resource 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccounts_eusfllmdemodatasa_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'foundationallm-customer'
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

resource storageAccounts_stcocotemp_name_resource 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccounts_stcocotemp_name
  location: 'eastus'
  sku: {
    name: 'Standard_LRS'
    tier: 'Standard'
  }
  kind: 'StorageV2'
  properties: {
    dnsEndpointType: 'Standard'
    defaultToOAuthAuthentication: false
    publicNetworkAccess: 'Enabled'
    allowCrossTenantReplication: false
    routingPreference: {
      routingChoice: 'MicrosoftRouting'
      publishMicrosoftEndpoints: true
      publishInternetEndpoints: true
    }
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: true
    networkAcls: {
      resourceAccessRules: [
        {
          tenantId: '22179471-b099-4504-bfdb-3f184cdae122'
          resourceId: '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/providers/Microsoft.Security/datascanners/storageDataScanner'
        }
      ]
      bypass: 'AzureServices'
      virtualNetworkRules: []
      ipRules: []
      defaultAction: 'Allow'
    }
    supportsHttpsTrafficOnly: true
    encryption: {
      requireInfrastructureEncryption: false
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

resource databaseAccounts_eus_fllm_demo_data_cdb_name_database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_data_cdb_name_resource
  name: 'database'
  properties: {
    resource: {
      id: 'database'
    }
  }
}

resource databaseAccounts_eus_fllm_demo_data_cdb_name_00000000_0000_0000_0000_000000000001 'Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_data_cdb_name_resource
  name: '00000000-0000-0000-0000-000000000001'
  properties: {
    roleName: 'Cosmos DB Built-in Data Reader'
    type: 'BuiltInRole'
    assignableScopes: [
      databaseAccounts_eus_fllm_demo_data_cdb_name_resource.id
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

resource databaseAccounts_eus_fllm_demo_data_cdb_name_00000000_0000_0000_0000_000000000002 'Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_data_cdb_name_resource
  name: '00000000-0000-0000-0000-000000000002'
  properties: {
    roleName: 'Cosmos DB Built-in Data Contributor'
    type: 'BuiltInRole'
    assignableScopes: [
      databaseAccounts_eus_fllm_demo_data_cdb_name_resource.id
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

resource systemTopics_eusfllmdemodatasa_aadfcca5_584f_459b_8f07_4cc521f6fef4_name_resource 'Microsoft.EventGrid/systemTopics@2023-06-01-preview' = {
  name: systemTopics_eusfllmdemodatasa_aadfcca5_584f_459b_8f07_4cc521f6fef4_name
  location: 'eastus'
  properties: {
    source: storageAccounts_eusfllmdemodatasa_name_resource.id
    topicType: 'microsoft.storage.storageaccounts'
  }
}

resource systemTopics_stcocotemp_f0d1fd7b_3864_45c9_9a81_6649ae2c59dd_name_resource 'Microsoft.EventGrid/systemTopics@2023-06-01-preview' = {
  name: systemTopics_stcocotemp_f0d1fd7b_3864_45c9_9a81_6649ae2c59dd_name
  location: 'eastus'
  properties: {
    source: storageAccounts_stcocotemp_name_resource.id
    topicType: 'microsoft.storage.storageaccounts'
  }
}

resource systemTopics_eusfllmdemodatasa_aadfcca5_584f_459b_8f07_4cc521f6fef4_name_StorageAntimalwareSubscription 'Microsoft.EventGrid/systemTopics/eventSubscriptions@2023-06-01-preview' = {
  parent: systemTopics_eusfllmdemodatasa_aadfcca5_584f_459b_8f07_4cc521f6fef4_name_resource
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

resource systemTopics_stcocotemp_f0d1fd7b_3864_45c9_9a81_6649ae2c59dd_name_StorageAntimalwareSubscription 'Microsoft.EventGrid/systemTopics/eventSubscriptions@2023-06-01-preview' = {
  parent: systemTopics_stcocotemp_f0d1fd7b_3864_45c9_9a81_6649ae2c59dd_name_resource
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

resource metricAlerts_EUS_FLLM_DEMO_DATA_cdb_availability_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_DATA_cdb_availability_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'foundationallm-customer'
  }
  properties: {
    description: 'Service availability less than 99% for 1 hour'
    severity: 0
    enabled: true
    scopes: [
      databaseAccounts_eus_fllm_demo_data_cdb_name_resource.id
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

resource metricAlerts_EUS_FLLM_DEMO_DATA_mssql_cpu_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_DATA_mssql_cpu_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'foundationallm-customer'
  }
  properties: {
    description: 'Service CPU utilization greater than 75% for 1 hour'
    severity: 0
    enabled: true
    scopes: [
      servers_eusfllmdemodatamssql_name_EUS_FLLM_DEMO_DATA_mssql_ep.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT1H'
    criteria: {
      allOf: [
        {
          threshold: 75
          name: 'Metric1'
          metricNamespace: 'Microsoft.Sql/servers/elasticpools'
          metricName: 'cpu_percent'
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

resource metricAlerts_EUS_FLLM_DEMO_DATA_sa_availability_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_DATA_sa_availability_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'foundationallm-customer'
  }
  properties: {
    description: 'Alert on Storage Account Threshold - Account availability less than 99% for 5 minutes'
    severity: 1
    enabled: true
    scopes: [
      storageAccounts_eusfllmdemodatasa_name_resource.id
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

resource privateEndpoints_cocorahs_st_temp_kab_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_cocorahs_st_temp_kab_name
  location: 'eastus'
  properties: {
    privateLinkServiceConnections: [
      {
        name: privateEndpoints_cocorahs_st_temp_kab_name
        id: '${privateEndpoints_cocorahs_st_temp_kab_name_resource.id}/privateLinkServiceConnections/${privateEndpoints_cocorahs_st_temp_kab_name}'
        properties: {
          privateLinkServiceId: storageAccounts_stcocotemp_name_resource.id
          groupIds: [
            'blob'
          ]
          privateLinkServiceConnectionState: {
            status: 'Disconnected'
            description: 'Deleted state'
            actionsRequired: 'None'
          }
        }
      }
    ]
    manualPrivateLinkServiceConnections: []
    customNetworkInterfaceName: '${privateEndpoints_cocorahs_st_temp_kab_name}-nic'
    subnet: {
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/Datasources'
    }
    ipConfigurations: []
    customDnsConfigs: []
  }
}

resource privateEndpoints_EUS_FLLM_DEMO_DATA_blob_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_DATA_blob_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'foundationallm-customer'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'EUS-FLLM-DEMO-DATA-blob-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_DATA_blob_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-DATA-blob-connection'
        properties: {
          privateLinkServiceId: storageAccounts_eusfllmdemodatasa_name_resource.id
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
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/Datasources'
    }
    ipConfigurations: []
    customDnsConfigs: []
  }
}

resource privateEndpoints_EUS_FLLM_DEMO_DATA_dfs_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_DATA_dfs_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'foundationallm-customer'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'EUS-FLLM-DEMO-DATA-dfs-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_DATA_dfs_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-DATA-dfs-connection'
        properties: {
          privateLinkServiceId: storageAccounts_eusfllmdemodatasa_name_resource.id
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
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/Datasources'
    }
    ipConfigurations: []
    customDnsConfigs: []
  }
}

resource privateEndpoints_EUS_FLLM_DEMO_DATA_file_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_DATA_file_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'foundationallm-customer'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'EUS-FLLM-DEMO-DATA-file-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_DATA_file_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-DATA-file-connection'
        properties: {
          privateLinkServiceId: storageAccounts_eusfllmdemodatasa_name_resource.id
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
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/Datasources'
    }
    ipConfigurations: []
    customDnsConfigs: []
  }
}

resource privateEndpoints_EUS_FLLM_DEMO_DATA_queue_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_DATA_queue_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'foundationallm-customer'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'EUS-FLLM-DEMO-DATA-queue-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_DATA_queue_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-DATA-queue-connection'
        properties: {
          privateLinkServiceId: storageAccounts_eusfllmdemodatasa_name_resource.id
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
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/Datasources'
    }
    ipConfigurations: []
    customDnsConfigs: []
  }
}

resource privateEndpoints_EUS_FLLM_DEMO_DATA_sql_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_DATA_sql_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'foundationallm-customer'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'EUS-FLLM-DEMO-DATA-sql-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_DATA_sql_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-DATA-sql-connection'
        properties: {
          privateLinkServiceId: databaseAccounts_eus_fllm_demo_data_cdb_name_resource.id
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
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/Datasources'
    }
    ipConfigurations: []
    customDnsConfigs: []
  }
}

resource privateEndpoints_EUS_FLLM_DEMO_DATA_sqlServer_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_DATA_sqlServer_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'foundationallm-customer'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'EUS-FLLM-DEMO-DATA-sql-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_DATA_sqlServer_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-DATA-sql-connection'
        properties: {
          privateLinkServiceId: servers_eusfllmdemodatamssql_name_resource.id
          groupIds: [
            'SqlServer'
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
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/Datasources'
    }
    ipConfigurations: []
    customDnsConfigs: []
  }
}

resource privateEndpoints_EUS_FLLM_DEMO_DATA_table_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_DATA_table_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'foundationallm-customer'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'EUS-FLLM-DEMO-DATA-table-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_DATA_table_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-DATA-table-connection'
        properties: {
          privateLinkServiceId: storageAccounts_eusfllmdemodatasa_name_resource.id
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
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/Datasources'
    }
    ipConfigurations: []
    customDnsConfigs: []
  }
}

resource privateEndpoints_EUS_FLLM_DEMO_DATA_web_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_DATA_web_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'foundationallm-customer'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'EUS-FLLM-DEMO-DATA-web-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_DATA_web_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-DATA-web-connection'
        properties: {
          privateLinkServiceId: storageAccounts_eusfllmdemodatasa_name_resource.id
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
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/Datasources'
    }
    ipConfigurations: []
    customDnsConfigs: [
      {
        fqdn: 'eusfllmdemodatasa.z13.web.core.windows.net'
        ipAddresses: [
          '10.0.2.11'
        ]
      }
    ]
  }
}

resource privateEndpoints_EUS_FLLM_DEMO_DATA_blob_pe_name_blob 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_EUS_FLLM_DEMO_DATA_blob_pe_name}/blob'
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
    privateEndpoints_EUS_FLLM_DEMO_DATA_blob_pe_name_resource
  ]
}

resource privateEndpoints_cocorahs_st_temp_kab_name_default 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_cocorahs_st_temp_kab_name}/default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink-blob-core-windows-net'
        properties: {
          privateDnsZoneId: privateDnsZones_privatelink_blob_core_windows_net_externalid
        }
      }
    ]
  }
  dependsOn: [
    privateEndpoints_cocorahs_st_temp_kab_name_resource
  ]
}

resource privateEndpoints_EUS_FLLM_DEMO_DATA_dfs_pe_name_dfs 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_EUS_FLLM_DEMO_DATA_dfs_pe_name}/dfs'
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
    privateEndpoints_EUS_FLLM_DEMO_DATA_dfs_pe_name_resource
  ]
}

resource privateEndpoints_EUS_FLLM_DEMO_DATA_file_pe_name_file 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_EUS_FLLM_DEMO_DATA_file_pe_name}/file'
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
    privateEndpoints_EUS_FLLM_DEMO_DATA_file_pe_name_resource
  ]
}

resource privateEndpoints_EUS_FLLM_DEMO_DATA_queue_pe_name_queue 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_EUS_FLLM_DEMO_DATA_queue_pe_name}/queue'
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
    privateEndpoints_EUS_FLLM_DEMO_DATA_queue_pe_name_resource
  ]
}

resource privateEndpoints_EUS_FLLM_DEMO_DATA_sql_pe_name_sql 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_EUS_FLLM_DEMO_DATA_sql_pe_name}/sql'
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
    privateEndpoints_EUS_FLLM_DEMO_DATA_sql_pe_name_resource
  ]
}

resource privateEndpoints_EUS_FLLM_DEMO_DATA_sqlServer_pe_name_sqlServer 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_EUS_FLLM_DEMO_DATA_sqlServer_pe_name}/sqlServer'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink.database.windows.net'
        properties: {
          privateDnsZoneId: privateDnsZones_privatelink_database_windows_net_externalid
        }
      }
    ]
  }
  dependsOn: [
    privateEndpoints_EUS_FLLM_DEMO_DATA_sqlServer_pe_name_resource
  ]
}

resource privateEndpoints_EUS_FLLM_DEMO_DATA_table_pe_name_table 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_EUS_FLLM_DEMO_DATA_table_pe_name}/table'
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
    privateEndpoints_EUS_FLLM_DEMO_DATA_table_pe_name_resource
  ]
}

resource privateEndpoints_EUS_FLLM_DEMO_DATA_web_pe_name_web 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_EUS_FLLM_DEMO_DATA_web_pe_name}/web'
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
    privateEndpoints_EUS_FLLM_DEMO_DATA_web_pe_name_resource
  ]
}

resource servers_eusfllmdemodatamssql_name_ActiveDirectory 'Microsoft.Sql/servers/administrators@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'ActiveDirectory'
  properties: {
    administratorType: 'ActiveDirectory'
    login: 'FoundationaLLM SQL Admins'
    sid: '73d59f98-857b-45e7-950b-5ee30d289bc8'
    tenantId: '22179471-b099-4504-bfdb-3f184cdae122'
  }
}

resource servers_eusfllmdemodatamssql_name_Default 'Microsoft.Sql/servers/advancedThreatProtectionSettings@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'Default'
  properties: {
    state: 'Enabled'
  }
}

resource servers_eusfllmdemodatamssql_name_CreateIndex 'Microsoft.Sql/servers/advisors@2014-04-01' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'CreateIndex'
  properties: {
    autoExecuteValue: 'Disabled'
  }
}

resource servers_eusfllmdemodatamssql_name_DbParameterization 'Microsoft.Sql/servers/advisors@2014-04-01' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'DbParameterization'
  properties: {
    autoExecuteValue: 'Disabled'
  }
}

resource servers_eusfllmdemodatamssql_name_DefragmentIndex 'Microsoft.Sql/servers/advisors@2014-04-01' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'DefragmentIndex'
  properties: {
    autoExecuteValue: 'Disabled'
  }
}

resource servers_eusfllmdemodatamssql_name_DropIndex 'Microsoft.Sql/servers/advisors@2014-04-01' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'DropIndex'
  properties: {
    autoExecuteValue: 'Disabled'
  }
}

resource servers_eusfllmdemodatamssql_name_ForceLastGoodPlan 'Microsoft.Sql/servers/advisors@2014-04-01' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'ForceLastGoodPlan'
  properties: {
    autoExecuteValue: 'Enabled'
  }
}

resource Microsoft_Sql_servers_auditingPolicies_servers_eusfllmdemodatamssql_name_Default 'Microsoft.Sql/servers/auditingPolicies@2014-04-01' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'Default'
  location: 'East US'
  properties: {
    auditingState: 'Disabled'
  }
}

resource Microsoft_Sql_servers_auditingSettings_servers_eusfllmdemodatamssql_name_Default 'Microsoft.Sql/servers/auditingSettings@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'default'
  properties: {
    retentionDays: 0
    auditActionsAndGroups: [
      'SUCCESSFUL_DATABASE_AUTHENTICATION_GROUP'
      'FAILED_DATABASE_AUTHENTICATION_GROUP'
      'BATCH_COMPLETED_GROUP'
    ]
    isAzureMonitorTargetEnabled: true
    isManagedIdentityInUse: false
    state: 'Enabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
}

resource Microsoft_Sql_servers_azureADOnlyAuthentications_servers_eusfllmdemodatamssql_name_Default 'Microsoft.Sql/servers/azureADOnlyAuthentications@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'Default'
  properties: {
    azureADOnlyAuthentication: false
  }
}

resource Microsoft_Sql_servers_connectionPolicies_servers_eusfllmdemodatamssql_name_default 'Microsoft.Sql/servers/connectionPolicies@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'default'
  location: 'eastus'
  properties: {
    connectionType: 'Default'
  }
}

resource servers_eusfllmdemodatamssql_name_master_Default 'Microsoft.Sql/servers/databases/advancedThreatProtectionSettings@2023-02-01-preview' = {
  name: '${servers_eusfllmdemodatamssql_name}/master/Default'
  properties: {
    state: 'Disabled'
  }
  dependsOn: [
    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_auditingPolicies_servers_eusfllmdemodatamssql_name_master_Default 'Microsoft.Sql/servers/databases/auditingPolicies@2014-04-01' = {
  name: '${servers_eusfllmdemodatamssql_name}/master/Default'
  location: 'East US'
  properties: {
    auditingState: 'Disabled'
  }
  dependsOn: [
    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_auditingSettings_servers_eusfllmdemodatamssql_name_master_Default 'Microsoft.Sql/servers/databases/auditingSettings@2023-02-01-preview' = {
  name: '${servers_eusfllmdemodatamssql_name}/master/Default'
  properties: {
    retentionDays: 0
    auditActionsAndGroups: [
      'SUCCESSFUL_DATABASE_AUTHENTICATION_GROUP'
      'FAILED_DATABASE_AUTHENTICATION_GROUP'
      'BATCH_COMPLETED_GROUP'
    ]
    isAzureMonitorTargetEnabled: true
    isManagedIdentityInUse: false
    state: 'Enabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
  dependsOn: [
    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_extendedAuditingSettings_servers_eusfllmdemodatamssql_name_master_Default 'Microsoft.Sql/servers/databases/extendedAuditingSettings@2023-02-01-preview' = {
  name: '${servers_eusfllmdemodatamssql_name}/master/Default'
  properties: {
    retentionDays: 0
    auditActionsAndGroups: [
      'SUCCESSFUL_DATABASE_AUTHENTICATION_GROUP'
      'FAILED_DATABASE_AUTHENTICATION_GROUP'
      'BATCH_COMPLETED_GROUP'
    ]
    isAzureMonitorTargetEnabled: true
    isManagedIdentityInUse: false
    state: 'Enabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
  dependsOn: [
    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_geoBackupPolicies_servers_eusfllmdemodatamssql_name_master_Default 'Microsoft.Sql/servers/databases/geoBackupPolicies@2023-02-01-preview' = {
  name: '${servers_eusfllmdemodatamssql_name}/master/Default'
  properties: {
    state: 'Disabled'
  }
  dependsOn: [
    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource servers_eusfllmdemodatamssql_name_master_Current 'Microsoft.Sql/servers/databases/ledgerDigestUploads@2023-02-01-preview' = {
  name: '${servers_eusfllmdemodatamssql_name}/master/Current'
  properties: {}
  dependsOn: [
    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_securityAlertPolicies_servers_eusfllmdemodatamssql_name_master_Default 'Microsoft.Sql/servers/databases/securityAlertPolicies@2023-02-01-preview' = {
  name: '${servers_eusfllmdemodatamssql_name}/master/Default'
  properties: {
    state: 'Disabled'
    disabledAlerts: [
      ''
    ]
    emailAddresses: [
      ''
    ]
    emailAccountAdmins: false
    retentionDays: 0
  }
  dependsOn: [
    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_transparentDataEncryption_servers_eusfllmdemodatamssql_name_master_Current 'Microsoft.Sql/servers/databases/transparentDataEncryption@2023-02-01-preview' = {
  name: '${servers_eusfllmdemodatamssql_name}/master/Current'
  properties: {
    state: 'Disabled'
  }
  dependsOn: [
    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_vulnerabilityAssessments_servers_eusfllmdemodatamssql_name_master_Default 'Microsoft.Sql/servers/databases/vulnerabilityAssessments@2023-02-01-preview' = {
  name: '${servers_eusfllmdemodatamssql_name}/master/Default'
  properties: {
    recurringScans: {
      isEnabled: false
      emailSubscriptionAdmins: true
      emails: []
    }
  }
  dependsOn: [
    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_devOpsAuditingSettings_servers_eusfllmdemodatamssql_name_Default 'Microsoft.Sql/servers/devOpsAuditingSettings@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'Default'
  properties: {
    isAzureMonitorTargetEnabled: false
    isManagedIdentityInUse: false
    state: 'Disabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
}

resource servers_eusfllmdemodatamssql_name_EUS_FLLM_DEMO_DATA_mssql_ep 'Microsoft.Sql/servers/elasticPools@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'EUS-FLLM-DEMO-DATA-mssql-ep'
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Storage'
    Workspace: 'foundationallm-customer'
  }
  sku: {
    name: 'GP_Gen5'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 4
  }
  kind: 'vcore,pool'
  properties: {
    maxSizeBytes: 137438953472
    minCapacity: -1
    perDatabaseSettings: {
      minCapacity: '0.25'
      maxCapacity: 4
    }
    zoneRedundant: false
    licenseType: 'LicenseIncluded'
    maintenanceConfigurationId: '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/providers/Microsoft.Maintenance/publicMaintenanceConfigurations/SQL_Default'
    availabilityZone: 'NoPreference'
  }
}

resource servers_eusfllmdemodatamssql_name_current 'Microsoft.Sql/servers/encryptionProtector@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'current'
  kind: 'servicemanaged'
  properties: {
    serverKeyName: 'ServiceManaged'
    serverKeyType: 'ServiceManaged'
    autoRotationEnabled: false
  }
}

resource Microsoft_Sql_servers_extendedAuditingSettings_servers_eusfllmdemodatamssql_name_Default 'Microsoft.Sql/servers/extendedAuditingSettings@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'default'
  properties: {
    retentionDays: 0
    auditActionsAndGroups: [
      'SUCCESSFUL_DATABASE_AUTHENTICATION_GROUP'
      'FAILED_DATABASE_AUTHENTICATION_GROUP'
      'BATCH_COMPLETED_GROUP'
    ]
    isAzureMonitorTargetEnabled: true
    isManagedIdentityInUse: false
    state: 'Enabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
}

resource servers_eusfllmdemodatamssql_name_AllowAllWindowsAzureIps 'Microsoft.Sql/servers/firewallRules@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'AllowAllWindowsAzureIps'
}

resource servers_eusfllmdemodatamssql_name_AllowAzureServices 'Microsoft.Sql/servers/firewallRules@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'AllowAzureServices'
}

resource servers_eusfllmdemodatamssql_name_ClientIPAddress_2023_11_2_11_44_41 'Microsoft.Sql/servers/firewallRules@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'ClientIPAddress_2023-11-2_11-44-41'
}

resource servers_eusfllmdemodatamssql_name_ClientIPAddress_2023_11_2_17_0_13 'Microsoft.Sql/servers/firewallRules@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'ClientIPAddress_2023-11-2_17-0-13'
}

resource servers_eusfllmdemodatamssql_name_ServiceManaged 'Microsoft.Sql/servers/keys@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'ServiceManaged'
  kind: 'servicemanaged'
  properties: {
    serverKeyType: 'ServiceManaged'
  }
}

resource Microsoft_Sql_servers_securityAlertPolicies_servers_eusfllmdemodatamssql_name_Default 'Microsoft.Sql/servers/securityAlertPolicies@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'Default'
  properties: {
    state: 'Enabled'
    disabledAlerts: [
      ''
    ]
    emailAddresses: [
      ''
    ]
    emailAccountAdmins: false
    retentionDays: 0
  }
}

resource Microsoft_Sql_servers_sqlVulnerabilityAssessments_servers_eusfllmdemodatamssql_name_Default 'Microsoft.Sql/servers/sqlVulnerabilityAssessments@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'Default'
  properties: {
    state: 'Enabled'
  }
}

resource Microsoft_Sql_servers_vulnerabilityAssessments_servers_eusfllmdemodatamssql_name_Default 'Microsoft.Sql/servers/vulnerabilityAssessments@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'Default'
  properties: {
    recurringScans: {
      isEnabled: false
      emailSubscriptionAdmins: true
    }
    storageContainerPath: vulnerabilityAssessments_Default_storageContainerPath
  }
}

resource storageAccounts_eusfllmdemodatasa_name_default 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccounts_eusfllmdemodatasa_name_resource
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

resource storageAccounts_stcocotemp_name_default 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccounts_stcocotemp_name_resource
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
      days: 7
    }
    cors: {
      corsRules: []
    }
    deleteRetentionPolicy: {
      allowPermanentDelete: false
      enabled: true
      days: 7
    }
    isVersioningEnabled: false
  }
}

resource Microsoft_Storage_storageAccounts_fileServices_storageAccounts_eusfllmdemodatasa_name_default 'Microsoft.Storage/storageAccounts/fileServices@2023-01-01' = {
  parent: storageAccounts_eusfllmdemodatasa_name_resource
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

resource Microsoft_Storage_storageAccounts_fileServices_storageAccounts_stcocotemp_name_default 'Microsoft.Storage/storageAccounts/fileServices@2023-01-01' = {
  parent: storageAccounts_stcocotemp_name_resource
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
      days: 7
    }
  }
}

resource storageAccounts_eusfllmdemodatasa_name_storageAccounts_eusfllmdemodatasa_name_3dc850fe_e54e_4564_a044_31ee976ca9cc 'Microsoft.Storage/storageAccounts/privateEndpointConnections@2023-01-01' = {
  parent: storageAccounts_eusfllmdemodatasa_name_resource
  name: '${storageAccounts_eusfllmdemodatasa_name}.3dc850fe-e54e-4564-a044-31ee976ca9cc'
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

resource storageAccounts_eusfllmdemodatasa_name_storageAccounts_eusfllmdemodatasa_name_4471641e_8d94_4f97_95b9_78a3f3900785 'Microsoft.Storage/storageAccounts/privateEndpointConnections@2023-01-01' = {
  parent: storageAccounts_eusfllmdemodatasa_name_resource
  name: '${storageAccounts_eusfllmdemodatasa_name}.4471641e-8d94-4f97-95b9-78a3f3900785'
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

resource storageAccounts_eusfllmdemodatasa_name_storageAccounts_eusfllmdemodatasa_name_aaf573d8_d745_4b57_83f3_f1a3124f77ac 'Microsoft.Storage/storageAccounts/privateEndpointConnections@2023-01-01' = {
  parent: storageAccounts_eusfllmdemodatasa_name_resource
  name: '${storageAccounts_eusfllmdemodatasa_name}.aaf573d8-d745-4b57-83f3-f1a3124f77ac'
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

resource storageAccounts_eusfllmdemodatasa_name_storageAccounts_eusfllmdemodatasa_name_dc6f9f65_9f36_4d7e_889f_5b9e35ffb8ed 'Microsoft.Storage/storageAccounts/privateEndpointConnections@2023-01-01' = {
  parent: storageAccounts_eusfllmdemodatasa_name_resource
  name: '${storageAccounts_eusfllmdemodatasa_name}.dc6f9f65-9f36-4d7e-889f-5b9e35ffb8ed'
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

resource storageAccounts_eusfllmdemodatasa_name_storageAccounts_eusfllmdemodatasa_name_eb77c460_fd25_4afb_958f_bd0d37678c0f 'Microsoft.Storage/storageAccounts/privateEndpointConnections@2023-01-01' = {
  parent: storageAccounts_eusfllmdemodatasa_name_resource
  name: '${storageAccounts_eusfllmdemodatasa_name}.eb77c460-fd25-4afb-958f-bd0d37678c0f'
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

resource storageAccounts_eusfllmdemodatasa_name_storageAccounts_eusfllmdemodatasa_name_ee4b5428_2b98_48d4_ae9f_e0d56685ceac 'Microsoft.Storage/storageAccounts/privateEndpointConnections@2023-01-01' = {
  parent: storageAccounts_eusfllmdemodatasa_name_resource
  name: '${storageAccounts_eusfllmdemodatasa_name}.ee4b5428-2b98-48d4-ae9f-e0d56685ceac'
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

resource Microsoft_Storage_storageAccounts_queueServices_storageAccounts_eusfllmdemodatasa_name_default 'Microsoft.Storage/storageAccounts/queueServices@2023-01-01' = {
  parent: storageAccounts_eusfllmdemodatasa_name_resource
  name: 'default'
  properties: {
    cors: {
      corsRules: []
    }
  }
}

resource Microsoft_Storage_storageAccounts_queueServices_storageAccounts_stcocotemp_name_default 'Microsoft.Storage/storageAccounts/queueServices@2023-01-01' = {
  parent: storageAccounts_stcocotemp_name_resource
  name: 'default'
  properties: {
    cors: {
      corsRules: []
    }
  }
}

resource Microsoft_Storage_storageAccounts_tableServices_storageAccounts_eusfllmdemodatasa_name_default 'Microsoft.Storage/storageAccounts/tableServices@2023-01-01' = {
  parent: storageAccounts_eusfllmdemodatasa_name_resource
  name: 'default'
  properties: {
    cors: {
      corsRules: []
    }
  }
}

resource Microsoft_Storage_storageAccounts_tableServices_storageAccounts_stcocotemp_name_default 'Microsoft.Storage/storageAccounts/tableServices@2023-01-01' = {
  parent: storageAccounts_stcocotemp_name_resource
  name: 'default'
  properties: {
    cors: {
      corsRules: []
    }
  }
}

resource databaseAccounts_eus_fllm_demo_data_cdb_name_EUS_FLLM_DEMO_DATA_sql_pe 'Microsoft.DocumentDB/databaseAccounts/privateEndpointConnections@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_data_cdb_name_resource
  name: 'EUS-FLLM-DEMO-DATA-sql-pe'
  properties: {
    provisioningState: 'Succeeded'
    groupId: 'Sql'
    privateEndpoint: {
      id: privateEndpoints_EUS_FLLM_DEMO_DATA_sql_pe_name_resource.id
    }
    privateLinkServiceConnectionState: {
      status: 'Approved'
    }
  }
}

resource databaseAccounts_eus_fllm_demo_data_cdb_name_database_completions 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_data_cdb_name_database
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

    databaseAccounts_eus_fllm_demo_data_cdb_name_resource
  ]
}

resource databaseAccounts_eus_fllm_demo_data_cdb_name_database_customer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_data_cdb_name_database
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

    databaseAccounts_eus_fllm_demo_data_cdb_name_resource
  ]
}

resource databaseAccounts_eus_fllm_demo_data_cdb_name_database_embedding 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_data_cdb_name_database
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

    databaseAccounts_eus_fllm_demo_data_cdb_name_resource
  ]
}

resource databaseAccounts_eus_fllm_demo_data_cdb_name_database_leases 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_data_cdb_name_database
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

    databaseAccounts_eus_fllm_demo_data_cdb_name_resource
  ]
}

resource databaseAccounts_eus_fllm_demo_data_cdb_name_database_product 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_data_cdb_name_database
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

    databaseAccounts_eus_fllm_demo_data_cdb_name_resource
  ]
}

resource servers_eusfllmdemodatamssql_name_anomaly_detection 'Microsoft.Sql/servers/databases@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'anomaly-detection'
  location: 'eastus'
  sku: {
    name: 'ElasticPool'
    tier: 'GeneralPurpose'
    capacity: 0
  }
  kind: 'v12.0,user,vcore,pool'
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 34359738368
    elasticPoolId: servers_eusfllmdemodatamssql_name_EUS_FLLM_DEMO_DATA_mssql_ep.id
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    requestedBackupStorageRedundancy: 'Local'
    maintenanceConfigurationId: '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/providers/Microsoft.Maintenance/publicMaintenanceConfigurations/SQL_Default'
    isLedgerOn: false
    availabilityZone: 'NoPreference'
  }
}

resource servers_eusfllmdemodatamssql_name_cocorahsdb 'Microsoft.Sql/servers/databases@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'cocorahsdb'
  location: 'eastus'
  sku: {
    name: 'ElasticPool'
    tier: 'GeneralPurpose'
    capacity: 0
  }
  kind: 'v12.0,user,vcore,pool'
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 268435456000
    elasticPoolId: servers_eusfllmdemodatamssql_name_EUS_FLLM_DEMO_DATA_mssql_ep.id
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    licenseType: 'LicenseIncluded'
    readScale: 'Disabled'
    requestedBackupStorageRedundancy: 'Local'
    maintenanceConfigurationId: '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/providers/Microsoft.Maintenance/publicMaintenanceConfigurations/SQL_Default'
    isLedgerOn: false
    availabilityZone: 'NoPreference'
  }
}

resource servers_eusfllmdemodatamssql_name_anomaly_detection_Default 'Microsoft.Sql/servers/databases/advancedThreatProtectionSettings@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_anomaly_detection
  name: 'Default'
  properties: {
    state: 'Disabled'
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource servers_eusfllmdemodatamssql_name_cocorahsdb_Default 'Microsoft.Sql/servers/databases/advancedThreatProtectionSettings@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_cocorahsdb
  name: 'Default'
  properties: {
    state: 'Disabled'
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource servers_eusfllmdemodatamssql_name_anomaly_detection_CreateIndex 'Microsoft.Sql/servers/databases/advisors@2014-04-01' = {
  parent: servers_eusfllmdemodatamssql_name_anomaly_detection
  name: 'CreateIndex'
  properties: {
    autoExecuteValue: 'Disabled'
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource servers_eusfllmdemodatamssql_name_cocorahsdb_CreateIndex 'Microsoft.Sql/servers/databases/advisors@2014-04-01' = {
  parent: servers_eusfllmdemodatamssql_name_cocorahsdb
  name: 'CreateIndex'
  properties: {
    autoExecuteValue: 'Disabled'
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource servers_eusfllmdemodatamssql_name_anomaly_detection_DbParameterization 'Microsoft.Sql/servers/databases/advisors@2014-04-01' = {
  parent: servers_eusfllmdemodatamssql_name_anomaly_detection
  name: 'DbParameterization'
  properties: {
    autoExecuteValue: 'Disabled'
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource servers_eusfllmdemodatamssql_name_cocorahsdb_DbParameterization 'Microsoft.Sql/servers/databases/advisors@2014-04-01' = {
  parent: servers_eusfllmdemodatamssql_name_cocorahsdb
  name: 'DbParameterization'
  properties: {
    autoExecuteValue: 'Disabled'
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource servers_eusfllmdemodatamssql_name_anomaly_detection_DefragmentIndex 'Microsoft.Sql/servers/databases/advisors@2014-04-01' = {
  parent: servers_eusfllmdemodatamssql_name_anomaly_detection
  name: 'DefragmentIndex'
  properties: {
    autoExecuteValue: 'Disabled'
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource servers_eusfllmdemodatamssql_name_cocorahsdb_DefragmentIndex 'Microsoft.Sql/servers/databases/advisors@2014-04-01' = {
  parent: servers_eusfllmdemodatamssql_name_cocorahsdb
  name: 'DefragmentIndex'
  properties: {
    autoExecuteValue: 'Disabled'
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource servers_eusfllmdemodatamssql_name_anomaly_detection_DropIndex 'Microsoft.Sql/servers/databases/advisors@2014-04-01' = {
  parent: servers_eusfllmdemodatamssql_name_anomaly_detection
  name: 'DropIndex'
  properties: {
    autoExecuteValue: 'Disabled'
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource servers_eusfllmdemodatamssql_name_cocorahsdb_DropIndex 'Microsoft.Sql/servers/databases/advisors@2014-04-01' = {
  parent: servers_eusfllmdemodatamssql_name_cocorahsdb
  name: 'DropIndex'
  properties: {
    autoExecuteValue: 'Disabled'
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource servers_eusfllmdemodatamssql_name_anomaly_detection_ForceLastGoodPlan 'Microsoft.Sql/servers/databases/advisors@2014-04-01' = {
  parent: servers_eusfllmdemodatamssql_name_anomaly_detection
  name: 'ForceLastGoodPlan'
  properties: {
    autoExecuteValue: 'Enabled'
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource servers_eusfllmdemodatamssql_name_cocorahsdb_ForceLastGoodPlan 'Microsoft.Sql/servers/databases/advisors@2014-04-01' = {
  parent: servers_eusfllmdemodatamssql_name_cocorahsdb
  name: 'ForceLastGoodPlan'
  properties: {
    autoExecuteValue: 'Enabled'
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_auditingPolicies_servers_eusfllmdemodatamssql_name_anomaly_detection_Default 'Microsoft.Sql/servers/databases/auditingPolicies@2014-04-01' = {
  parent: servers_eusfllmdemodatamssql_name_anomaly_detection
  name: 'Default'
  location: 'East US'
  properties: {
    auditingState: 'Disabled'
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_auditingPolicies_servers_eusfllmdemodatamssql_name_cocorahsdb_Default 'Microsoft.Sql/servers/databases/auditingPolicies@2014-04-01' = {
  parent: servers_eusfllmdemodatamssql_name_cocorahsdb
  name: 'Default'
  location: 'East US'
  properties: {
    auditingState: 'Disabled'
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_auditingSettings_servers_eusfllmdemodatamssql_name_anomaly_detection_Default 'Microsoft.Sql/servers/databases/auditingSettings@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_anomaly_detection
  name: 'default'
  properties: {
    retentionDays: 0
    auditActionsAndGroups: []
    isStorageSecondaryKeyInUse: false
    isAzureMonitorTargetEnabled: false
    isManagedIdentityInUse: false
    state: 'Disabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_auditingSettings_servers_eusfllmdemodatamssql_name_cocorahsdb_Default 'Microsoft.Sql/servers/databases/auditingSettings@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_cocorahsdb
  name: 'default'
  properties: {
    retentionDays: 0
    auditActionsAndGroups: []
    isStorageSecondaryKeyInUse: false
    isAzureMonitorTargetEnabled: false
    isManagedIdentityInUse: false
    state: 'Disabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_backupLongTermRetentionPolicies_servers_eusfllmdemodatamssql_name_anomaly_detection_default 'Microsoft.Sql/servers/databases/backupLongTermRetentionPolicies@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_anomaly_detection
  name: 'default'
  properties: {
    makeBackupsImmutable: false
    weeklyRetention: 'PT0S'
    monthlyRetention: 'PT0S'
    yearlyRetention: 'PT0S'
    weekOfYear: 0
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_backupLongTermRetentionPolicies_servers_eusfllmdemodatamssql_name_cocorahsdb_default 'Microsoft.Sql/servers/databases/backupLongTermRetentionPolicies@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_cocorahsdb
  name: 'default'
  properties: {
    makeBackupsImmutable: false
    weeklyRetention: 'PT0S'
    monthlyRetention: 'PT0S'
    yearlyRetention: 'PT0S'
    weekOfYear: 0
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_backupShortTermRetentionPolicies_servers_eusfllmdemodatamssql_name_anomaly_detection_default 'Microsoft.Sql/servers/databases/backupShortTermRetentionPolicies@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_anomaly_detection
  name: 'default'
  properties: {
    retentionDays: 7
    diffBackupIntervalInHours: 12
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_backupShortTermRetentionPolicies_servers_eusfllmdemodatamssql_name_cocorahsdb_default 'Microsoft.Sql/servers/databases/backupShortTermRetentionPolicies@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_cocorahsdb
  name: 'default'
  properties: {
    retentionDays: 7
    diffBackupIntervalInHours: 24
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_extendedAuditingSettings_servers_eusfllmdemodatamssql_name_anomaly_detection_Default 'Microsoft.Sql/servers/databases/extendedAuditingSettings@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_anomaly_detection
  name: 'default'
  properties: {
    retentionDays: 0
    auditActionsAndGroups: []
    isStorageSecondaryKeyInUse: false
    isAzureMonitorTargetEnabled: false
    isManagedIdentityInUse: false
    state: 'Disabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_extendedAuditingSettings_servers_eusfllmdemodatamssql_name_cocorahsdb_Default 'Microsoft.Sql/servers/databases/extendedAuditingSettings@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_cocorahsdb
  name: 'default'
  properties: {
    retentionDays: 0
    auditActionsAndGroups: []
    isStorageSecondaryKeyInUse: false
    isAzureMonitorTargetEnabled: false
    isManagedIdentityInUse: false
    state: 'Disabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_geoBackupPolicies_servers_eusfllmdemodatamssql_name_anomaly_detection_Default 'Microsoft.Sql/servers/databases/geoBackupPolicies@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_anomaly_detection
  name: 'Default'
  properties: {
    state: 'Disabled'
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_geoBackupPolicies_servers_eusfllmdemodatamssql_name_cocorahsdb_Default 'Microsoft.Sql/servers/databases/geoBackupPolicies@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_cocorahsdb
  name: 'Default'
  properties: {
    state: 'Disabled'
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource servers_eusfllmdemodatamssql_name_anomaly_detection_Current 'Microsoft.Sql/servers/databases/ledgerDigestUploads@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_anomaly_detection
  name: 'Current'
  properties: {}
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource servers_eusfllmdemodatamssql_name_cocorahsdb_Current 'Microsoft.Sql/servers/databases/ledgerDigestUploads@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_cocorahsdb
  name: 'Current'
  properties: {}
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_securityAlertPolicies_servers_eusfllmdemodatamssql_name_anomaly_detection_Default 'Microsoft.Sql/servers/databases/securityAlertPolicies@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_anomaly_detection
  name: 'Default'
  properties: {
    state: 'Disabled'
    disabledAlerts: [
      ''
    ]
    emailAddresses: [
      ''
    ]
    emailAccountAdmins: false
    retentionDays: 0
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_securityAlertPolicies_servers_eusfllmdemodatamssql_name_cocorahsdb_Default 'Microsoft.Sql/servers/databases/securityAlertPolicies@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_cocorahsdb
  name: 'Default'
  properties: {
    state: 'Disabled'
    disabledAlerts: [
      ''
    ]
    emailAddresses: [
      ''
    ]
    emailAccountAdmins: false
    retentionDays: 0
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_transparentDataEncryption_servers_eusfllmdemodatamssql_name_anomaly_detection_Current 'Microsoft.Sql/servers/databases/transparentDataEncryption@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_anomaly_detection
  name: 'Current'
  properties: {
    state: 'Enabled'
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_transparentDataEncryption_servers_eusfllmdemodatamssql_name_cocorahsdb_Current 'Microsoft.Sql/servers/databases/transparentDataEncryption@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_cocorahsdb
  name: 'Current'
  properties: {
    state: 'Enabled'
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_vulnerabilityAssessments_servers_eusfllmdemodatamssql_name_anomaly_detection_Default 'Microsoft.Sql/servers/databases/vulnerabilityAssessments@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_anomaly_detection
  name: 'Default'
  properties: {
    recurringScans: {
      isEnabled: false
      emailSubscriptionAdmins: true
      emails: []
    }
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_vulnerabilityAssessments_servers_eusfllmdemodatamssql_name_cocorahsdb_Default 'Microsoft.Sql/servers/databases/vulnerabilityAssessments@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_cocorahsdb
  name: 'Default'
  properties: {
    recurringScans: {
      isEnabled: false
      emailSubscriptionAdmins: true
      emails: []
    }
  }
  dependsOn: [

    servers_eusfllmdemodatamssql_name_resource
  ]
}

resource servers_eusfllmdemodatamssql_name_EUS_FLLM_DEMO_DATA_sqlServer_pe_54a090e6_54d9_467b_8adf_46b8154e52ce 'Microsoft.Sql/servers/privateEndpointConnections@2023-02-01-preview' = {
  parent: servers_eusfllmdemodatamssql_name_resource
  name: 'EUS-FLLM-DEMO-DATA-sqlServer-pe-54a090e6-54d9-467b-8adf-46b8154e52ce'
  properties: {
    privateEndpoint: {
      id: privateEndpoints_EUS_FLLM_DEMO_DATA_sqlServer_pe_name_resource.id
    }
    privateLinkServiceConnectionState: {
      status: 'Approved'
      description: 'Auto-approved'
    }
  }
}

resource storageAccounts_stcocotemp_name_default_database 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: storageAccounts_stcocotemp_name_default
  name: 'database'
  properties: {
    immutableStorageWithVersioning: {
      enabled: false
    }
    defaultEncryptionScope: '$account-encryption-key'
    denyEncryptionScopeOverride: false
    publicAccess: 'None'
  }
  dependsOn: [

    storageAccounts_stcocotemp_name_resource
  ]
}

resource databaseAccounts_eus_fllm_demo_data_cdb_name_database_completions_default 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/throughputSettings@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_data_cdb_name_database_completions
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

    databaseAccounts_eus_fllm_demo_data_cdb_name_database
    databaseAccounts_eus_fllm_demo_data_cdb_name_resource
  ]
}

resource databaseAccounts_eus_fllm_demo_data_cdb_name_database_customer_default 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/throughputSettings@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_data_cdb_name_database_customer
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

    databaseAccounts_eus_fllm_demo_data_cdb_name_database
    databaseAccounts_eus_fllm_demo_data_cdb_name_resource
  ]
}

resource databaseAccounts_eus_fllm_demo_data_cdb_name_database_embedding_default 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/throughputSettings@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_data_cdb_name_database_embedding
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

    databaseAccounts_eus_fllm_demo_data_cdb_name_database
    databaseAccounts_eus_fllm_demo_data_cdb_name_resource
  ]
}

resource databaseAccounts_eus_fllm_demo_data_cdb_name_database_leases_default 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/throughputSettings@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_data_cdb_name_database_leases
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

    databaseAccounts_eus_fllm_demo_data_cdb_name_database
    databaseAccounts_eus_fllm_demo_data_cdb_name_resource
  ]
}

resource databaseAccounts_eus_fllm_demo_data_cdb_name_database_product_default 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/throughputSettings@2023-09-15' = {
  parent: databaseAccounts_eus_fllm_demo_data_cdb_name_database_product
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

    databaseAccounts_eus_fllm_demo_data_cdb_name_database
    databaseAccounts_eus_fllm_demo_data_cdb_name_resource
  ]
}
param privateDnsZones_azure_api_net_name string = 'azure-api.net'
param privateDnsZones_scm_azure_api_net_name string = 'scm.azure-api.net'
param privateDnsZones_portal_azure_api_net_name string = 'portal.azure-api.net'
param privateDnsZones_privatelink_azurecr_io_name string = 'privatelink.azurecr.io'
param privateDnsZones_developer_azure_api_net_name string = 'developer.azure-api.net'
param privateDnsZones_privatelink_azconfig_io_name string = 'privatelink.azconfig.io'
param privateDnsZones_management_azure_api_net_name string = 'management.azure-api.net'
param privateDnsZones_privatelink_azure_api_net_name string = 'privatelink.azure-api.net'
param privateDnsZones_privatelink_eastus_azmk8s_io_name string = 'privatelink.eastus.azmk8s.io'
param privateDnsZones_privatelink_openai_azure_com_name string = 'privatelink.openai.azure.com'
param privateDnsZones_eastus_privatelink_azurecr_io_name string = 'eastus.privatelink.azurecr.io'
param privateDnsZones_privatelink_azurewebsites_net_name string = 'privatelink.azurewebsites.net'
param privateDnsZones_privatelink_grafana_azure_com_name string = 'privatelink.grafana.azure.com'
param privateDnsZones_privatelink_monitor_azure_com_name string = 'privatelink.monitor.azure.com'
param privateDnsZones_privatelink_search_windows_net_name string = 'privatelink.search.windows.net'
param privateDnsZones_privatelink_documents_azure_com_name string = 'privatelink.documents.azure.com'
param privateDnsZones_privatelink_vaultcore_azure_net_name string = 'privatelink.vaultcore.azure.net'
param privateDnsZones_privatelink_database_windows_net_name string = 'privatelink.database.windows.net'
param privateDnsZones_privatelink_dfs_core_windows_net_name string = 'privatelink.dfs.core.windows.net'
param privateDnsZones_privatelink_blob_core_windows_net_name string = 'privatelink.blob.core.windows.net'
param privateDnsZones_privatelink_file_core_windows_net_name string = 'privatelink.file.core.windows.net'
param privateDnsZones_privatelink_queue_core_windows_net_name string = 'privatelink.queue.core.windows.net'
param privateDnsZones_privatelink_table_core_windows_net_name string = 'privatelink.table.core.windows.net'
param privateDnsZones_privatelink_cognitiveservices_azure_com_name string = 'privatelink.cognitiveservices.azure.com'
param privateDnsZones_privatelink_eastus_prometheus_monitor_azure_com_name string = 'privatelink.eastus.prometheus.monitor.azure.com'
param virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-NET-rg/providers/Microsoft.Network/virtualNetworks/EUS-FLLM-DEMO-NET-vnet'
param managedClusters_EUS_FLLM_DEMO_APP_BACKEND_aks_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-APP-rg/providers/Microsoft.ContainerService/managedClusters/EUS-FLLM-DEMO-APP-BACKEND-aks'
param managedClusters_EUS_FLLM_DEMO_APP_FRONTEND_aks_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-APP-rg/providers/Microsoft.ContainerService/managedClusters/EUS-FLLM-DEMO-APP-FRONTEND-aks'

resource privateDnsZones_azure_api_net_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_azure_api_net_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 2
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_developer_azure_api_net_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_developer_azure_api_net_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 2
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_eastus_privatelink_azurecr_io_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_eastus_privatelink_azurecr_io_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 1
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_management_azure_api_net_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_management_azure_api_net_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 2
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_portal_azure_api_net_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_portal_azure_api_net_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 2
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_privatelink_azconfig_io_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_privatelink_azconfig_io_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 2
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_privatelink_azure_api_net_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_privatelink_azure_api_net_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 1
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_privatelink_azurecr_io_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_privatelink_azurecr_io_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 3
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_privatelink_azurewebsites_net_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_privatelink_azurewebsites_net_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 1
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_privatelink_blob_core_windows_net_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_privatelink_blob_core_windows_net_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 5
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_privatelink_cognitiveservices_azure_com_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_privatelink_cognitiveservices_azure_com_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 2
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_privatelink_database_windows_net_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_privatelink_database_windows_net_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 2
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_privatelink_dfs_core_windows_net_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_privatelink_dfs_core_windows_net_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 4
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_privatelink_documents_azure_com_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_privatelink_documents_azure_com_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 5
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_privatelink_eastus_azmk8s_io_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_privatelink_eastus_azmk8s_io_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 3
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_privatelink_eastus_prometheus_monitor_azure_com_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_privatelink_eastus_prometheus_monitor_azure_com_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 2
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_privatelink_file_core_windows_net_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_privatelink_file_core_windows_net_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 4
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_privatelink_grafana_azure_com_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_privatelink_grafana_azure_com_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 3
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_privatelink_monitor_azure_com_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_privatelink_monitor_azure_com_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 10
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_privatelink_openai_azure_com_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_privatelink_openai_azure_com_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 3
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_privatelink_queue_core_windows_net_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_privatelink_queue_core_windows_net_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 4
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_privatelink_search_windows_net_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_privatelink_search_windows_net_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 2
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_privatelink_table_core_windows_net_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_privatelink_table_core_windows_net_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 4
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_privatelink_vaultcore_azure_net_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_privatelink_vaultcore_azure_net_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 3
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_scm_azure_api_net_name_resource 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  name: privateDnsZones_scm_azure_api_net_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    maxNumberOfRecordSets: 25000
    maxNumberOfVirtualNetworkLinks: 1000
    maxNumberOfVirtualNetworkLinksWithRegistration: 100
    numberOfRecordSets: 2
    numberOfVirtualNetworkLinks: 0
    numberOfVirtualNetworkLinksWithRegistration: 0
    provisioningState: 'Succeeded'
  }
}

resource privateDnsZones_privatelink_monitor_azure_com_name_api 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_monitor_azure_com_name_resource
  name: 'api'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OPS-azuremonitor-pe with resource guid a207dfd1-678c-4469-bbcd-88949d1ff211'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.255.100'
      }
    ]
  }
}

resource privateDnsZones_privatelink_monitor_azure_com_name_diagservices_query 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_monitor_azure_com_name_resource
  name: 'diagservices-query'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OPS-azuremonitor-pe with resource guid a207dfd1-678c-4469-bbcd-88949d1ff211'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.255.104'
      }
    ]
  }
}

resource privateDnsZones_privatelink_monitor_azure_com_name_eastus_livediagnostics 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_monitor_azure_com_name_resource
  name: 'eastus.livediagnostics'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OPS-azuremonitor-pe with resource guid a207dfd1-678c-4469-bbcd-88949d1ff211'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.255.112'
      }
    ]
  }
}

resource privateDnsZones_privatelink_monitor_azure_com_name_eastus_8_in_ai 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_monitor_azure_com_name_resource
  name: 'eastus-8.in.ai'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OPS-azuremonitor-pe with resource guid a207dfd1-678c-4469-bbcd-88949d1ff211'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.255.111'
      }
    ]
  }
}

resource privateDnsZones_privatelink_grafana_azure_com_name_efllmdopsgd_dcebg7hmdzcddvdm_eus 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_grafana_azure_com_name_resource
  name: 'efllmdopsgd-dcebg7hmdzcddvdm.eus'
  properties: {
    metadata: {
      creator: 'created by private endpoint EFLLMdOPS-grafana-pe with resource guid 8531c366-d133-48c3-8b1a-d010ce11a5c6'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.255.124'
      }
    ]
  }
}

resource privateDnsZones_privatelink_blob_core_windows_net_name_efllmdstoragesa 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_blob_core_windows_net_name_resource
  name: 'efllmdstoragesa'
  properties: {
    metadata: {
      creator: 'created by private endpoint e-FLLM-d-STORAGE-blob-pe with resource guid 2ef46ac9-7f04-4880-8f88-04077ded29c3'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.4.5'
      }
    ]
  }
}

resource privateDnsZones_privatelink_dfs_core_windows_net_name_efllmdstoragesa 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_dfs_core_windows_net_name_resource
  name: 'efllmdstoragesa'
  properties: {
    metadata: {
      creator: 'created by private endpoint e-FLLM-d-STORAGE-dfs-pe with resource guid a8050389-a216-400e-9d26-fbae6762207c'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.4.7'
      }
    ]
  }
}

resource privateDnsZones_privatelink_file_core_windows_net_name_efllmdstoragesa 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_file_core_windows_net_name_resource
  name: 'efllmdstoragesa'
  properties: {
    metadata: {
      creator: 'created by private endpoint e-FLLM-d-STORAGE-file-pe with resource guid 5d502f11-23f3-42c0-a837-550bb75b41d3'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.4.11'
      }
    ]
  }
}

resource privateDnsZones_privatelink_queue_core_windows_net_name_efllmdstoragesa 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_queue_core_windows_net_name_resource
  name: 'efllmdstoragesa'
  properties: {
    metadata: {
      creator: 'created by private endpoint e-FLLM-d-STORAGE-queue-pe with resource guid f77dbfc5-79e0-4a56-b16b-d9aac8e22239'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.4.4'
      }
    ]
  }
}

resource privateDnsZones_privatelink_table_core_windows_net_name_efllmdstoragesa 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_table_core_windows_net_name_resource
  name: 'efllmdstoragesa'
  properties: {
    metadata: {
      creator: 'created by private endpoint e-FLLM-d-STORAGE-table-pe with resource guid 22496208-0c15-439d-bf00-9e895a9aa23d'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.4.10'
      }
    ]
  }
}

resource privateDnsZones_privatelink_eastus_azmk8s_io_name_eus_fllm_demo_app_backend_aks 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_eastus_azmk8s_io_name_resource
  name: 'eus-fllm-demo-app-backend-aks'
  properties: {
    metadata: {
      'own-aks-cluster': managedClusters_EUS_FLLM_DEMO_APP_BACKEND_aks_externalid
      'owned-by': 'aksrp'
    }
    ttl: 300
    aRecords: [
      {
        ipv4Address: '10.0.16.4'
      }
    ]
  }
}

resource privateDnsZones_privatelink_eastus_azmk8s_io_name_eus_fllm_demo_app_frontend_aks 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_eastus_azmk8s_io_name_resource
  name: 'eus-fllm-demo-app-frontend-aks'
  properties: {
    metadata: {
      'own-aks-cluster': managedClusters_EUS_FLLM_DEMO_APP_FRONTEND_aks_externalid
      'owned-by': 'aksrp'
    }
    ttl: 300
    aRecords: [
      {
        ipv4Address: '10.0.12.4'
      }
    ]
  }
}

resource privateDnsZones_privatelink_documents_azure_com_name_eus_fllm_demo_data_cdb 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_documents_azure_com_name_resource
  name: 'eus-fllm-demo-data-cdb'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-DATA-sql-pe with resource guid be913a23-5777-44f2-a2e4-45420c8cb353'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.2.8'
      }
    ]
  }
}

resource privateDnsZones_privatelink_documents_azure_com_name_eus_fllm_demo_data_cdb_eastus 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_documents_azure_com_name_resource
  name: 'eus-fllm-demo-data-cdb-eastus'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-DATA-sql-pe with resource guid be913a23-5777-44f2-a2e4-45420c8cb353'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.2.9'
      }
    ]
  }
}

resource privateDnsZones_privatelink_database_windows_net_name_eusfllmdemodatamssql 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_database_windows_net_name_resource
  name: 'eusfllmdemodatamssql'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-DATA-sqlServer-pe with resource guid 015fc55a-eb9c-4f35-8bfa-4c9cb32c1531'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.2.7'
      }
    ]
  }
}

resource privateDnsZones_privatelink_blob_core_windows_net_name_eusfllmdemodatasa 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_blob_core_windows_net_name_resource
  name: 'eusfllmdemodatasa'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-DATA-blob-pe with resource guid 91c1e7b1-def4-496a-abc6-68a2cd6f3ac5'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.2.4'
      }
    ]
  }
}

resource privateDnsZones_privatelink_dfs_core_windows_net_name_eusfllmdemodatasa 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_dfs_core_windows_net_name_resource
  name: 'eusfllmdemodatasa'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-DATA-dfs-pe with resource guid 4f66af1b-4280-4273-a7d9-c206f4899f37'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.2.10'
      }
    ]
  }
}

resource privateDnsZones_privatelink_file_core_windows_net_name_eusfllmdemodatasa 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_file_core_windows_net_name_resource
  name: 'eusfllmdemodatasa'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-DATA-file-pe with resource guid f21bec96-07c4-4832-913e-276bcf8d4965'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.2.6'
      }
    ]
  }
}

resource privateDnsZones_privatelink_queue_core_windows_net_name_eusfllmdemodatasa 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_queue_core_windows_net_name_resource
  name: 'eusfllmdemodatasa'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-DATA-queue-pe with resource guid 308ff307-aacb-4356-8513-79af4a3ceab9'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.2.5'
      }
    ]
  }
}

resource privateDnsZones_privatelink_table_core_windows_net_name_eusfllmdemodatasa 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_table_core_windows_net_name_resource
  name: 'eusfllmdemodatasa'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-DATA-table-pe with resource guid fcb039eb-13a2-4476-a805-d36ffa9485a8'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.2.12'
      }
    ]
  }
}

resource privateDnsZones_privatelink_openai_azure_com_name_eus_fllm_demo_oai_0 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_openai_azure_com_name_resource
  name: 'eus-fllm-demo-oai-0'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OAI-0-openai-pe with resource guid 00dddfb6-4018-4472-834f-6186e9cccac2'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.5.9'
      }
    ]
  }
}

resource privateDnsZones_privatelink_openai_azure_com_name_eus_fllm_demo_oai_1 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_openai_azure_com_name_resource
  name: 'eus-fllm-demo-oai-1'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OAI-1-openai-pe with resource guid a7d17ac1-3964-4b7f-b484-42236527d105'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.5.8'
      }
    ]
  }
}

resource privateDnsZones_azure_api_net_name_eus_fllm_demo_oai_apim 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_azure_api_net_name_resource
  name: 'eus-fllm-demo-oai-apim'
  properties: {
    metadata: {}
    ttl: 0
    aRecords: [
      {
        ipv4Address: '10.0.5.4'
      }
    ]
  }
}

resource privateDnsZones_developer_azure_api_net_name_eus_fllm_demo_oai_apim 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_developer_azure_api_net_name_resource
  name: 'eus-fllm-demo-oai-apim'
  properties: {
    metadata: {}
    ttl: 0
    aRecords: [
      {
        ipv4Address: '10.0.5.4'
      }
    ]
  }
}

resource privateDnsZones_management_azure_api_net_name_eus_fllm_demo_oai_apim 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_management_azure_api_net_name_resource
  name: 'eus-fllm-demo-oai-apim'
  properties: {
    metadata: {}
    ttl: 0
    aRecords: [
      {
        ipv4Address: '10.0.5.4'
      }
    ]
  }
}

resource privateDnsZones_portal_azure_api_net_name_eus_fllm_demo_oai_apim 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_portal_azure_api_net_name_resource
  name: 'eus-fllm-demo-oai-apim'
  properties: {
    metadata: {}
    ttl: 0
    aRecords: [
      {
        ipv4Address: '10.0.5.4'
      }
    ]
  }
}

resource privateDnsZones_scm_azure_api_net_name_eus_fllm_demo_oai_apim 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_scm_azure_api_net_name_resource
  name: 'eus-fllm-demo-oai-apim'
  properties: {
    metadata: {}
    ttl: 0
    aRecords: [
      {
        ipv4Address: '10.0.5.4'
      }
    ]
  }
}

resource privateDnsZones_privatelink_cognitiveservices_azure_com_name_eus_fllm_demo_oai_content_safety 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_cognitiveservices_azure_com_name_resource
  name: 'eus-fllm-demo-oai-content-safety'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OAI-content-safety-pe with resource guid fcef19f3-241e-4911-855e-44f70af0f3c2'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.5.10'
      }
    ]
  }
}

resource privateDnsZones_privatelink_vaultcore_azure_net_name_eus_fllm_demo_oai_kv 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_vaultcore_azure_net_name_resource
  name: 'eus-fllm-demo-oai-kv'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OAI-kv-pe with resource guid 4b59ba30-d22e-49e4-9ac8-abc3770a06d7'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.5.5'
      }
    ]
  }
}

resource privateDnsZones_privatelink_eastus_prometheus_monitor_azure_com_name_eus_fllm_demo_ops_amw_nzsj 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_eastus_prometheus_monitor_azure_com_name_resource
  name: 'eus-fllm-demo-ops-amw-nzsj'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OPS-prometheusMetrics-pe with resource guid 6d22f8db-4631-48e9-84ec-4a791eefc160'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.255.115'
      }
    ]
  }
}

resource privateDnsZones_privatelink_azconfig_io_name_eus_fllm_demo_ops_appconfig 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_azconfig_io_name_resource
  name: 'eus-fllm-demo-ops-appconfig'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OPS-appconfig-pe with resource guid 48204bbe-0db5-4c9a-b972-6b90774c4195'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.255.114'
      }
    ]
  }
}

resource privateDnsZones_privatelink_azurecr_io_name_eusfllmdemoopscr 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_azurecr_io_name_resource
  name: 'eusfllmdemoopscr'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OPS-registry-pe with resource guid ffb366e0-47d5-4ca2-ba85-0d8389efe824'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.255.123'
      }
    ]
  }
}

resource privateDnsZones_privatelink_azurecr_io_name_eusfllmdemoopscr_eastus_data 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_azurecr_io_name_resource
  name: 'eusfllmdemoopscr.eastus.data'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OPS-registry-pe with resource guid ffb366e0-47d5-4ca2-ba85-0d8389efe824'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.255.122'
      }
    ]
  }
}

resource privateDnsZones_privatelink_vaultcore_azure_net_name_eus_fllm_demo_ops_kv 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_vaultcore_azure_net_name_resource
  name: 'eus-fllm-demo-ops-kv'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OPS-kv-pe with resource guid 548f6f88-72b6-41df-80a4-16ee06b18baf'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.255.113'
      }
    ]
  }
}

resource privateDnsZones_privatelink_blob_core_windows_net_name_eusfllmdemoopssa 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_blob_core_windows_net_name_resource
  name: 'eusfllmdemoopssa'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OPS-blob-pe with resource guid 4d3848d5-a154-46dc-90e8-ddbe124e5a21'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.255.117'
      }
    ]
  }
}

resource privateDnsZones_privatelink_dfs_core_windows_net_name_eusfllmdemoopssa 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_dfs_core_windows_net_name_resource
  name: 'eusfllmdemoopssa'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OPS-dfs-pe with resource guid 00eabd05-c09a-41bd-8d34-4d699d0f03fe'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.255.120'
      }
    ]
  }
}

resource privateDnsZones_privatelink_file_core_windows_net_name_eusfllmdemoopssa 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_file_core_windows_net_name_resource
  name: 'eusfllmdemoopssa'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OPS-file-pe with resource guid 4d94d450-7fc0-45ec-898c-09242a33d679'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.255.119'
      }
    ]
  }
}

resource privateDnsZones_privatelink_queue_core_windows_net_name_eusfllmdemoopssa 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_queue_core_windows_net_name_resource
  name: 'eusfllmdemoopssa'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OPS-queue-pe with resource guid 18430433-2689-4c47-86c0-657772f1891f'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.255.118'
      }
    ]
  }
}

resource privateDnsZones_privatelink_table_core_windows_net_name_eusfllmdemoopssa 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_table_core_windows_net_name_resource
  name: 'eusfllmdemoopssa'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OPS-table-pe with resource guid 79f67797-41d2-4ede-a943-84135f0cf8b7'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.255.121'
      }
    ]
  }
}

resource privateDnsZones_privatelink_documents_azure_com_name_eus_fllm_demo_storage_cdb 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_documents_azure_com_name_resource
  name: 'eus-fllm-demo-storage-cdb'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-STORAGE-sql-pe with resource guid 2cbfaa3d-f0b5-4a5c-ae3a-ea2581742fe3'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.4.8'
      }
    ]
  }
}

resource privateDnsZones_privatelink_documents_azure_com_name_eus_fllm_demo_storage_cdb_eastus 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_documents_azure_com_name_resource
  name: 'eus-fllm-demo-storage-cdb-eastus'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-STORAGE-sql-pe with resource guid 2cbfaa3d-f0b5-4a5c-ae3a-ea2581742fe3'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.4.9'
      }
    ]
  }
}

resource privateDnsZones_privatelink_search_windows_net_name_eusfllmdemovecsearch 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_search_windows_net_name_resource
  name: 'eusfllmdemovecsearch'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-VEC-search-pe with resource guid ac63a4e7-885b-4458-855d-8501f6018cf8'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.6.4'
      }
    ]
  }
}

resource privateDnsZones_privatelink_monitor_azure_com_name_global_handler_control 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_monitor_azure_com_name_resource
  name: 'global.handler.control'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OPS-azuremonitor-pe with resource guid a207dfd1-678c-4469-bbcd-88949d1ff211'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.255.107'
      }
    ]
  }
}

resource privateDnsZones_privatelink_monitor_azure_com_name_global_in_ai 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_monitor_azure_com_name_resource
  name: 'global.in.ai'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OPS-azuremonitor-pe with resource guid a207dfd1-678c-4469-bbcd-88949d1ff211'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.255.101'
      }
    ]
  }
}

resource privateDnsZones_privatelink_monitor_azure_com_name_live 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_monitor_azure_com_name_resource
  name: 'live'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OPS-azuremonitor-pe with resource guid a207dfd1-678c-4469-bbcd-88949d1ff211'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.255.103'
      }
    ]
  }
}

resource privateDnsZones_privatelink_monitor_azure_com_name_profiler 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_monitor_azure_com_name_resource
  name: 'profiler'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OPS-azuremonitor-pe with resource guid a207dfd1-678c-4469-bbcd-88949d1ff211'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.255.102'
      }
    ]
  }
}

resource privateDnsZones_privatelink_blob_core_windows_net_name_scadvisorcontentpl 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_blob_core_windows_net_name_resource
  name: 'scadvisorcontentpl'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OPS-azuremonitor-pe with resource guid a207dfd1-678c-4469-bbcd-88949d1ff211'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.255.106'
      }
    ]
  }
}

resource privateDnsZones_privatelink_monitor_azure_com_name_snapshot 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_monitor_azure_com_name_resource
  name: 'snapshot'
  properties: {
    metadata: {
      creator: 'created by private endpoint EUS-FLLM-DEMO-OPS-azuremonitor-pe with resource guid a207dfd1-678c-4469-bbcd-88949d1ff211'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.255.105'
      }
    ]
  }
}

resource privateDnsZones_privatelink_grafana_azure_com_name_sso_eus 'Microsoft.Network/privateDnsZones/A@2018-09-01' = {
  parent: privateDnsZones_privatelink_grafana_azure_com_name_resource
  name: 'sso.eus'
  properties: {
    metadata: {
      creator: 'created by private endpoint EFLLMdOPS-grafana-pe with resource guid 8531c366-d133-48c3-8b1a-d010ce11a5c6'
    }
    ttl: 10
    aRecords: [
      {
        ipv4Address: '10.0.255.124'
      }
    ]
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_azure_api_net_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_azure_api_net_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_developer_azure_api_net_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_developer_azure_api_net_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_eastus_privatelink_azurecr_io_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_eastus_privatelink_azurecr_io_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_management_azure_api_net_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_management_azure_api_net_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_portal_azure_api_net_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_portal_azure_api_net_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_privatelink_azconfig_io_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_privatelink_azconfig_io_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_privatelink_azure_api_net_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_privatelink_azure_api_net_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_privatelink_azurecr_io_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_privatelink_azurecr_io_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_privatelink_azurewebsites_net_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_privatelink_azurewebsites_net_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_privatelink_blob_core_windows_net_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_privatelink_blob_core_windows_net_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_privatelink_cognitiveservices_azure_com_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_privatelink_cognitiveservices_azure_com_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_privatelink_database_windows_net_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_privatelink_database_windows_net_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_privatelink_dfs_core_windows_net_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_privatelink_dfs_core_windows_net_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_privatelink_documents_azure_com_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_privatelink_documents_azure_com_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_privatelink_eastus_azmk8s_io_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_privatelink_eastus_azmk8s_io_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_privatelink_eastus_prometheus_monitor_azure_com_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_privatelink_eastus_prometheus_monitor_azure_com_name_resource
  name: '@'
  properties: {
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_privatelink_file_core_windows_net_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_privatelink_file_core_windows_net_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_privatelink_grafana_azure_com_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_privatelink_grafana_azure_com_name_resource
  name: '@'
  properties: {
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_privatelink_monitor_azure_com_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_privatelink_monitor_azure_com_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_privatelink_openai_azure_com_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_privatelink_openai_azure_com_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_privatelink_queue_core_windows_net_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_privatelink_queue_core_windows_net_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_privatelink_search_windows_net_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_privatelink_search_windows_net_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_privatelink_table_core_windows_net_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_privatelink_table_core_windows_net_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_privatelink_vaultcore_azure_net_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_privatelink_vaultcore_azure_net_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_scm_azure_api_net_name 'Microsoft.Network/privateDnsZones/SOA@2018-09-01' = {
  parent: privateDnsZones_scm_azure_api_net_name_resource
  name: '@'
  properties: {
    metadata: {}
    ttl: 3600
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
  }
}

resource privateDnsZones_azure_api_net_name_privateDnsZones_azure_api_net_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_azure_api_net_name_resource
  name: '${privateDnsZones_azure_api_net_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_developer_azure_api_net_name_privateDnsZones_developer_azure_api_net_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_developer_azure_api_net_name_resource
  name: '${privateDnsZones_developer_azure_api_net_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_eastus_privatelink_azurecr_io_name_privateDnsZones_eastus_privatelink_azurecr_io_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_eastus_privatelink_azurecr_io_name_resource
  name: '${privateDnsZones_eastus_privatelink_azurecr_io_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_management_azure_api_net_name_privateDnsZones_management_azure_api_net_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_management_azure_api_net_name_resource
  name: '${privateDnsZones_management_azure_api_net_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_portal_azure_api_net_name_privateDnsZones_portal_azure_api_net_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_portal_azure_api_net_name_resource
  name: '${privateDnsZones_portal_azure_api_net_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_privatelink_azconfig_io_name_privateDnsZones_privatelink_azconfig_io_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_privatelink_azconfig_io_name_resource
  name: '${privateDnsZones_privatelink_azconfig_io_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_privatelink_azure_api_net_name_privateDnsZones_privatelink_azure_api_net_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_privatelink_azure_api_net_name_resource
  name: '${privateDnsZones_privatelink_azure_api_net_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_privatelink_azurecr_io_name_privateDnsZones_privatelink_azurecr_io_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_privatelink_azurecr_io_name_resource
  name: '${privateDnsZones_privatelink_azurecr_io_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_privatelink_azurewebsites_net_name_privateDnsZones_privatelink_azurewebsites_net_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_privatelink_azurewebsites_net_name_resource
  name: '${privateDnsZones_privatelink_azurewebsites_net_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_privatelink_blob_core_windows_net_name_privateDnsZones_privatelink_blob_core_windows_net_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_privatelink_blob_core_windows_net_name_resource
  name: '${privateDnsZones_privatelink_blob_core_windows_net_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_privatelink_cognitiveservices_azure_com_name_privateDnsZones_privatelink_cognitiveservices_azure_com_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_privatelink_cognitiveservices_azure_com_name_resource
  name: '${privateDnsZones_privatelink_cognitiveservices_azure_com_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_privatelink_database_windows_net_name_privateDnsZones_privatelink_database_windows_net_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_privatelink_database_windows_net_name_resource
  name: '${privateDnsZones_privatelink_database_windows_net_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_privatelink_dfs_core_windows_net_name_privateDnsZones_privatelink_dfs_core_windows_net_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_privatelink_dfs_core_windows_net_name_resource
  name: '${privateDnsZones_privatelink_dfs_core_windows_net_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_privatelink_documents_azure_com_name_privateDnsZones_privatelink_documents_azure_com_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_privatelink_documents_azure_com_name_resource
  name: '${privateDnsZones_privatelink_documents_azure_com_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_privatelink_eastus_azmk8s_io_name_privateDnsZones_privatelink_eastus_azmk8s_io_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_privatelink_eastus_azmk8s_io_name_resource
  name: '${privateDnsZones_privatelink_eastus_azmk8s_io_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_privatelink_eastus_prometheus_monitor_azure_com_name_privateDnsZones_privatelink_eastus_prometheus_monitor_azure_com_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_privatelink_eastus_prometheus_monitor_azure_com_name_resource
  name: '${privateDnsZones_privatelink_eastus_prometheus_monitor_azure_com_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_privatelink_file_core_windows_net_name_privateDnsZones_privatelink_file_core_windows_net_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_privatelink_file_core_windows_net_name_resource
  name: '${privateDnsZones_privatelink_file_core_windows_net_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_privatelink_grafana_azure_com_name_privateDnsZones_privatelink_grafana_azure_com_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_privatelink_grafana_azure_com_name_resource
  name: '${privateDnsZones_privatelink_grafana_azure_com_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_privatelink_monitor_azure_com_name_privateDnsZones_privatelink_monitor_azure_com_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_privatelink_monitor_azure_com_name_resource
  name: '${privateDnsZones_privatelink_monitor_azure_com_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_privatelink_openai_azure_com_name_privateDnsZones_privatelink_openai_azure_com_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_privatelink_openai_azure_com_name_resource
  name: '${privateDnsZones_privatelink_openai_azure_com_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_privatelink_queue_core_windows_net_name_privateDnsZones_privatelink_queue_core_windows_net_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_privatelink_queue_core_windows_net_name_resource
  name: '${privateDnsZones_privatelink_queue_core_windows_net_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_privatelink_search_windows_net_name_privateDnsZones_privatelink_search_windows_net_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_privatelink_search_windows_net_name_resource
  name: '${privateDnsZones_privatelink_search_windows_net_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_privatelink_table_core_windows_net_name_privateDnsZones_privatelink_table_core_windows_net_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_privatelink_table_core_windows_net_name_resource
  name: '${privateDnsZones_privatelink_table_core_windows_net_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_privatelink_vaultcore_azure_net_name_privateDnsZones_privatelink_vaultcore_azure_net_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_privatelink_vaultcore_azure_net_name_resource
  name: '${privateDnsZones_privatelink_vaultcore_azure_net_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}

resource privateDnsZones_scm_azure_api_net_name_privateDnsZones_scm_azure_api_net_name 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: privateDnsZones_scm_azure_api_net_name_resource
  name: '${privateDnsZones_scm_azure_api_net_name}'
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid
    }
  }
}
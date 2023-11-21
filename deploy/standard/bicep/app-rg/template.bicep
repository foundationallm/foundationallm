param privateEndpoints_EUS_FLLM_DEMO_APP_BACKEND_aks_pe_name string = 'EUS-FLLM-DEMO-APP-BACKEND-aks-pe'
param privateEndpoints_EUS_FLLM_DEMO_APP_FRONTEND_aks_pe_name string = 'EUS-FLLM-DEMO-APP-FRONTEND-aks-pe'
param metricAlerts_EUS_FLLM_DEMO_APP_BACKEND_aks_cpu_alert_name string = 'EUS-FLLM-DEMO-APP-BACKEND-aks-cpu-alert'
param metricAlerts_EUS_FLLM_DEMO_APP_BACKEND_aks_mem_alert_name string = 'EUS-FLLM-DEMO-APP-BACKEND-aks-mem-alert'
param managedClusters_EUS_FLLM_DEMO_APP_BACKEND_aks_name string = 'EUS-FLLM-DEMO-APP-BACKEND-aks'
param metricAlerts_EUS_FLLM_DEMO_APP_FRONTEND_aks_cpu_alert_name string = 'EUS-FLLM-DEMO-APP-FRONTEND-aks-cpu-alert'
param metricAlerts_EUS_FLLM_DEMO_APP_FRONTEND_aks_mem_alert_name string = 'EUS-FLLM-DEMO-APP-FRONTEND-aks-mem-alert'
param managedClusters_EUS_FLLM_DEMO_APP_FRONTEND_aks_name string = 'EUS-FLLM-DEMO-APP-FRONTEND-aks'
param userAssignedIdentities_EUS_FLLM_DEMO_APP_chat_ui_mi_name string = 'EUS-FLLM-DEMO-APP-chat-ui-mi'
param userAssignedIdentities_EUS_FLLM_DEMO_APP_core_api_mi_name string = 'EUS-FLLM-DEMO-APP-core-api-mi'
param dataCollectionRules_MSCI_eastus_eus_fllm_demo_app_backend_aks_name string = 'MSCI-eastus-eus-fllm-demo-app-backend-aks'
param userAssignedIdentities_EUS_FLLM_DEMO_APP_BACKEND_aks_mi_name string = 'EUS-FLLM-DEMO-APP-BACKEND-aks-mi'
param dataCollectionRules_MSProm_eastus_EUS_FLLM_DEMO_APP_BACKEND_aks_name string = 'MSProm-eastus-EUS-FLLM-DEMO-APP-BACKEND-aks'
param userAssignedIdentities_EUS_FLLM_DEMO_APP_FRONTEND_aks_mi_name string = 'EUS-FLLM-DEMO-APP-FRONTEND-aks-mi'
param dataCollectionRules_MSProm_eastus_EUS_FLLM_DEMO_APP_FRONTEND_aks_name string = 'MSProm-eastus-EUS-FLLM-DEMO-APP-FRONTEND-aks'
param userAssignedIdentities_EUS_FLLM_DEMO_APP_agent_hub_api_mi_name string = 'EUS-FLLM-DEMO-APP-agent-hub-api-mi'
param userAssignedIdentities_EUS_FLLM_DEMO_APP_langchain_api_mi_name string = 'EUS-FLLM-DEMO-APP-langchain-api-mi'
param userAssignedIdentities_EUS_FLLM_DEMO_APP_gatekeeper_api_mi_name string = 'EUS-FLLM-DEMO-APP-gatekeeper-api-mi'
param userAssignedIdentities_EUS_FLLM_DEMO_APP_prompt_hub_api_mi_name string = 'EUS-FLLM-DEMO-APP-prompt-hub-api-mi'
param metricAlerts_CPU_Usage_Percentage_EUS_FLLM_DEMO_APP_FRONTEND_aks_name string = 'CPU Usage Percentage - EUS-FLLM-DEMO-APP-FRONTEND-aks'
param dataCollectionEndpoints_MSProm_eastus_eus_fllm_demo_app_backend_aks_name string = 'MSProm-eastus-eus-fllm-demo-app-backend-aks'
param dataCollectionEndpoints_MSProm_eastus_EUS_FLLM_DEMO_APP_FRONTEND_aks_name string = 'MSProm-eastus-EUS-FLLM-DEMO-APP-FRONTEND-aks'
param userAssignedIdentities_EUS_FLLM_DEMO_APP_agent_factory_api_mi_name string = 'EUS-FLLM-DEMO-APP-agent-factory-api-mi'
param userAssignedIdentities_EUS_FLLM_DEMO_APP_data_source_hub_api_mi_name string = 'EUS-FLLM-DEMO-APP-data-source-hub-api-mi'
param userAssignedIdentities_EUS_FLLM_DEMO_APP_semantic_kernel_api_mi_name string = 'EUS-FLLM-DEMO-APP-semantic-kernel-api-mi'
param metricAlerts_Memory_Working_Set_Percentage_EUS_FLLM_DEMO_APP_FRONTEND_aks_name string = 'Memory Working Set Percentage - EUS-FLLM-DEMO-APP-FRONTEND-aks'
param prometheusRuleGroups_NodeRecordingRulesRuleGroup_EUS_FLLM_DEMO_APP_BACKEND_aks_name string = 'NodeRecordingRulesRuleGroup-EUS-FLLM-DEMO-APP-BACKEND-aks'
param prometheusRuleGroups_NodeRecordingRulesRuleGroup_EUS_FLLM_DEMO_APP_FRONTEND_aks_name string = 'NodeRecordingRulesRuleGroup-EUS-FLLM-DEMO-APP-FRONTEND-aks'
param prometheusRuleGroups_NodeRecordingRulesRuleGroup_Win_EUS_FLLM_DEMO_APP_BACKEND_aks_name string = 'NodeRecordingRulesRuleGroup-Win-EUS-FLLM-DEMO-APP-BACKEND-aks'
param prometheusRuleGroups_NodeRecordingRulesRuleGroup_Win_EUS_FLLM_DEMO_APP_FRONTEND_aks_name string = 'NodeRecordingRulesRuleGroup-Win-EUS-FLLM-DEMO-APP-FRONTEND-aks'
param prometheusRuleGroups_KubernetesRecordingRulesRuleGroup_EUS_FLLM_DEMO_APP_BACKEND_aks_name string = 'KubernetesRecordingRulesRuleGroup-EUS-FLLM-DEMO-APP-BACKEND-aks'
param prometheusRuleGroups_KubernetesRecordingRulesRuleGroup_EUS_FLLM_DEMO_APP_FRONTEND_aks_name string = 'KubernetesRecordingRulesRuleGroup-EUS-FLLM-DEMO-APP-FRONTEND-aks'
param prometheusRuleGroups_NodeAndKubernetesRecordingRulesRuleGroup_Win_EUS_FLLM_DEMO_APP_BACKEND_aks_name string = 'NodeAndKubernetesRecordingRulesRuleGroup-Win-EUS-FLLM-DEMO-APP-BACKEND-aks'
param prometheusRuleGroups_NodeAndKubernetesRecordingRulesRuleGroup_Win_EUS_FLLM_DEMO_APP_FRONTEND_aks_name string = 'NodeAndKubernetesRecordingRulesRuleGroup-Win-EUS-FLLM-DEMO-APP-FRONTEND-aks'
param accounts_eus_fllm_demo_ops_amw_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/eus-fllm-demo-ops-rg/providers/microsoft.monitor/accounts/eus-fllm-demo-ops-amw'
param virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-NET-rg/providers/Microsoft.Network/virtualNetworks/EUS-FLLM-DEMO-NET-vnet'
param applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-AGW-rg/providers/Microsoft.Network/applicationGateways/EUS-FLLM-DEMO-AGW-api-agw'
param workspaces_EUS_FLLM_DEMO_OPS_la_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-OPS-rg/providers/Microsoft.OperationalInsights/workspaces/EUS-FLLM-DEMO-OPS-la'
param publicIPAddresses_3bb8b2a2_472e_4d28_915a_f14d2beca423_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-APP-BACKEND-aks-mrg/providers/Microsoft.Network/publicIPAddresses/3bb8b2a2-472e-4d28-915a-f14d2beca423'
param privateDnsZones_privatelink_eastus_azmk8s_io_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DNS-rg/providers/Microsoft.Network/privateDnsZones/privatelink.eastus.azmk8s.io'
param userAssignedIdentities_EUS_FLLM_DEMO_APP_BACKEND_aks_agentpool_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-APP-BACKEND-aks-mrg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/EUS-FLLM-DEMO-APP-BACKEND-aks-agentpool'
param applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-AGW-rg/providers/Microsoft.Network/applicationGateways/EUS-FLLM-DEMO-AGW-www-agw'
param publicIPAddresses_c299afba_4567_422e_94d5_09428a36fbbe_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-APP-FRONTEND-aks-mrg/providers/Microsoft.Network/publicIPAddresses/c299afba-4567-422e-94d5-09428a36fbbe'
param userAssignedIdentities_EUS_FLLM_DEMO_APP_FRONTEND_aks_agentpool_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-APP-FRONTEND-aks-mrg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/EUS-FLLM-DEMO-APP-FRONTEND-aks-agentpool'
param actiongroups_EUS_FLLM_DEMO_OPS_ag_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-OPS-rg/providers/Microsoft.Insights/actiongroups/EUS-FLLM-DEMO-OPS-ag'

resource managedClusters_EUS_FLLM_DEMO_APP_BACKEND_aks_name_resource 'Microsoft.ContainerService/managedClusters@2023-01-02-preview' = {
  name: managedClusters_EUS_FLLM_DEMO_APP_BACKEND_aks_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Application'
    Workspace: 'FoundationaLLM-Platform'
  }
  sku: {
    name: 'Basic'
    tier: 'Paid'
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-APP-rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/EUS-FLLM-DEMO-APP-BACKEND-aks-mi': {}
    }
  }
  properties: {
    kubernetesVersion: '1.26.6'
    fqdnSubdomain: managedClusters_EUS_FLLM_DEMO_APP_BACKEND_aks_name
    agentPoolProfiles: [
      {
        name: 'system'
        count: 2
        vmSize: 'Standard_DS2_v2'
        osDiskSizeGB: 1024
        osDiskType: 'Managed'
        kubeletDiskType: 'OS'
        vnetSubnetID: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/FLLMBackend'
        maxPods: 30
        type: 'VirtualMachineScaleSets'
        maxCount: 6
        minCount: 2
        enableAutoScaling: true
        scaleDownMode: 'Delete'
        powerState: {
          code: 'Running'
        }
        orchestratorVersion: '1.26.6'
        enableNodePublicIP: false
        enableCustomCATrust: false
        tags: {
          Environment: 'DEMO'
          Project: 'FLLM'
          Purpose: 'Application'
          Workspace: 'FoundationaLLM-Platform'
        }
        nodeTaints: [
          'CriticalAddonsOnly=true:NoSchedule'
        ]
        mode: 'System'
        enableEncryptionAtHost: false
        enableUltraSSD: false
        osType: 'Linux'
        osSKU: 'Ubuntu'
        upgradeSettings: {
          maxSurge: '200'
        }
        enableFIPS: false
      }
      {
        name: 'user'
        count: 2
        vmSize: 'Standard_DS2_v2'
        osDiskSizeGB: 1024
        osDiskType: 'Managed'
        kubeletDiskType: 'OS'
        vnetSubnetID: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/FLLMBackend'
        maxPods: 30
        type: 'VirtualMachineScaleSets'
        maxCount: 3
        minCount: 1
        enableAutoScaling: true
        scaleDownMode: 'Delete'
        powerState: {
          code: 'Running'
        }
        orchestratorVersion: '1.26.6'
        enableNodePublicIP: false
        enableCustomCATrust: false
        tags: {
          Environment: 'DEMO'
          Project: 'FLLM'
          Purpose: 'Application'
          Workspace: 'FoundationaLLM-Platform'
        }
        mode: 'User'
        enableEncryptionAtHost: false
        enableUltraSSD: false
        osType: 'Linux'
        osSKU: 'Ubuntu'
        upgradeSettings: {
          maxSurge: '200'
        }
        enableFIPS: false
      }
    ]
    windowsProfile: {
      adminUsername: 'azureuser'
      enableCSIProxy: true
    }
    servicePrincipalProfile: {
      clientId: 'msi'
    }
    addonProfiles: {
      azureKeyvaultSecretsProvider: {
        enabled: true
        config: {
          enableSecretRotation: 'true'
          rotationPollInterval: '2m'
        }
      }
      azurepolicy: {
        enabled: true
        config: {
          version: 'v2'
        }
      }
      ingressApplicationGateway: {
        enabled: true
        config: {
          applicationGatewayId: applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_externalid
          effectiveApplicationGatewayId: applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_externalid
        }
      }
      omsagent: {
        enabled: true
        config: {
          logAnalyticsWorkspaceResourceID: workspaces_EUS_FLLM_DEMO_OPS_la_externalid
          useAADAuth: 'true'
        }
      }
    }
    nodeResourceGroup: '${managedClusters_EUS_FLLM_DEMO_APP_BACKEND_aks_name}-mrg'
    enableRBAC: true
    networkProfile: {
      networkPlugin: 'azure'
      networkPolicy: 'azure'
      loadBalancerSku: 'Standard'
      loadBalancerProfile: {
        managedOutboundIPs: {
          count: 1
        }
        effectiveOutboundIPs: [
          {
            id: publicIPAddresses_3bb8b2a2_472e_4d28_915a_f14d2beca423_externalid
          }
        ]
        backendPoolType: 'nodeIPConfiguration'
      }
      serviceCidr: '10.100.0.0/16'
      dnsServiceIP: '10.100.254.1'
      outboundType: 'loadBalancer'
      serviceCidrs: [
        '10.100.0.0/16'
      ]
      ipFamilies: [
        'IPv4'
      ]
    }
    aadProfile: {
      managed: true
      adminGroupObjectIDs: [
        '40319637-db4e-45d3-a329-12543acebaac'
        '73d59f98-857b-45e7-950b-5ee30d289bc8'
      ]
      enableAzureRBAC: true
      tenantID: '22179471-b099-4504-bfdb-3f184cdae122'
    }
    privateLinkResources: [
      {
        name: 'management'
        type: 'Microsoft.ContainerService/managedClusters/privateLinkResources'
        groupId: 'management'
        requiredMembers: [
          'management'
        ]
      }
    ]
    apiServerAccessProfile: {
      enablePrivateCluster: true
      privateDNSZone: privateDnsZones_privatelink_eastus_azmk8s_io_externalid
      enablePrivateClusterPublicFQDN: false
    }
    identityProfile: {
      kubeletidentity: {
        resourceId: userAssignedIdentities_EUS_FLLM_DEMO_APP_BACKEND_aks_agentpool_externalid
        clientId: 'ce00c8d9-2e79-4791-9f96-e8da1e834e79'
        objectId: '7c36ecd3-df3b-482f-aa03-df84543a1403'
      }
    }
    autoScalerProfile: {
      'balance-similar-node-groups': 'false'
      expander: 'random'
      'max-empty-bulk-delete': '10'
      'max-graceful-termination-sec': '600'
      'max-node-provision-time': '15m'
      'max-total-unready-percentage': '45'
      'new-pod-scale-up-delay': '0s'
      'ok-total-unready-count': '3'
      'scale-down-delay-after-add': '10m'
      'scale-down-delay-after-delete': '10s'
      'scale-down-delay-after-failure': '3m'
      'scale-down-unneeded-time': '10m'
      'scale-down-unready-time': '20m'
      'scale-down-utilization-threshold': '0.5'
      'scan-interval': '10s'
      'skip-nodes-with-local-storage': 'false'
      'skip-nodes-with-system-pods': 'true'
    }
    autoUpgradeProfile: {
      upgradeChannel: 'stable'
    }
    disableLocalAccounts: true
    securityProfile: {
      defender: {
        logAnalyticsWorkspaceResourceId: workspaces_EUS_FLLM_DEMO_OPS_la_externalid
        securityMonitoring: {
          enabled: true
        }
      }
      workloadIdentity: {
        enabled: true
      }
      imageCleaner: {
        enabled: false
        intervalHours: 48
      }
    }
    storageProfile: {
      diskCSIDriver: {
        enabled: true
        version: 'v1'
      }
      fileCSIDriver: {
        enabled: true
      }
      snapshotController: {
        enabled: true
      }
    }
    oidcIssuerProfile: {
      enabled: true
    }
    workloadAutoScalerProfile: {}
    azureMonitorProfile: {
      metrics: {
        enabled: true
      }
    }
  }
}

resource managedClusters_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_resource 'Microsoft.ContainerService/managedClusters@2023-01-02-preview' = {
  name: managedClusters_EUS_FLLM_DEMO_APP_FRONTEND_aks_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Application'
    Workspace: 'FoundationaLLM-Platform'
  }
  sku: {
    name: 'Basic'
    tier: 'Paid'
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-APP-rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/EUS-FLLM-DEMO-APP-FRONTEND-aks-mi': {}
    }
  }
  properties: {
    kubernetesVersion: '1.26.6'
    fqdnSubdomain: managedClusters_EUS_FLLM_DEMO_APP_FRONTEND_aks_name
    agentPoolProfiles: [
      {
        name: 'system'
        count: 2
        vmSize: 'Standard_DS2_v2'
        osDiskSizeGB: 1024
        osDiskType: 'Managed'
        kubeletDiskType: 'OS'
        vnetSubnetID: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/FLLMFrontEnd'
        maxPods: 30
        type: 'VirtualMachineScaleSets'
        maxCount: 6
        minCount: 2
        enableAutoScaling: true
        scaleDownMode: 'Delete'
        powerState: {
          code: 'Running'
        }
        orchestratorVersion: '1.26.6'
        enableNodePublicIP: false
        enableCustomCATrust: false
        tags: {
          Environment: 'DEMO'
          Project: 'FLLM'
          Purpose: 'Application'
          Workspace: 'FoundationaLLM-Platform'
        }
        nodeTaints: [
          'CriticalAddonsOnly=true:NoSchedule'
        ]
        mode: 'System'
        enableEncryptionAtHost: false
        enableUltraSSD: false
        osType: 'Linux'
        osSKU: 'Ubuntu'
        upgradeSettings: {
          maxSurge: '200'
        }
        enableFIPS: false
      }
      {
        name: 'user'
        count: 1
        vmSize: 'Standard_DS2_v2'
        osDiskSizeGB: 1024
        osDiskType: 'Managed'
        kubeletDiskType: 'OS'
        vnetSubnetID: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/FLLMFrontEnd'
        maxPods: 30
        type: 'VirtualMachineScaleSets'
        maxCount: 3
        minCount: 1
        enableAutoScaling: true
        scaleDownMode: 'Delete'
        powerState: {
          code: 'Running'
        }
        orchestratorVersion: '1.26.6'
        enableNodePublicIP: false
        enableCustomCATrust: false
        tags: {
          Environment: 'DEMO'
          Project: 'FLLM'
          Purpose: 'Application'
          Workspace: 'FoundationaLLM-Platform'
        }
        mode: 'User'
        enableEncryptionAtHost: false
        enableUltraSSD: false
        osType: 'Linux'
        osSKU: 'Ubuntu'
        upgradeSettings: {
          maxSurge: '200'
        }
        enableFIPS: false
      }
    ]
    windowsProfile: {
      adminUsername: 'azureuser'
      enableCSIProxy: true
    }
    servicePrincipalProfile: {
      clientId: 'msi'
    }
    addonProfiles: {
      azureKeyvaultSecretsProvider: {
        enabled: true
        config: {
          enableSecretRotation: 'true'
          rotationPollInterval: '2m'
        }
      }
      azurepolicy: {
        enabled: true
        config: {
          version: 'v2'
        }
      }
      ingressApplicationGateway: {
        enabled: true
        config: {
          applicationGatewayId: applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_externalid
          effectiveApplicationGatewayId: applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_externalid
        }
      }
      omsagent: {
        enabled: true
        config: {
          logAnalyticsWorkspaceResourceID: workspaces_EUS_FLLM_DEMO_OPS_la_externalid
          useAADAuth: 'true'
        }
      }
    }
    nodeResourceGroup: '${managedClusters_EUS_FLLM_DEMO_APP_FRONTEND_aks_name}-mrg'
    enableRBAC: true
    networkProfile: {
      networkPlugin: 'azure'
      networkPolicy: 'azure'
      loadBalancerSku: 'Standard'
      loadBalancerProfile: {
        managedOutboundIPs: {
          count: 1
        }
        effectiveOutboundIPs: [
          {
            id: publicIPAddresses_c299afba_4567_422e_94d5_09428a36fbbe_externalid
          }
        ]
        backendPoolType: 'nodeIPConfiguration'
      }
      serviceCidr: '10.100.0.0/16'
      dnsServiceIP: '10.100.254.1'
      outboundType: 'loadBalancer'
      serviceCidrs: [
        '10.100.0.0/16'
      ]
      ipFamilies: [
        'IPv4'
      ]
    }
    aadProfile: {
      managed: true
      adminGroupObjectIDs: [
        '40319637-db4e-45d3-a329-12543acebaac'
        '73d59f98-857b-45e7-950b-5ee30d289bc8'
      ]
      enableAzureRBAC: true
      tenantID: '22179471-b099-4504-bfdb-3f184cdae122'
    }
    privateLinkResources: [
      {
        name: 'management'
        type: 'Microsoft.ContainerService/managedClusters/privateLinkResources'
        groupId: 'management'
        requiredMembers: [
          'management'
        ]
      }
    ]
    apiServerAccessProfile: {
      enablePrivateCluster: true
      privateDNSZone: privateDnsZones_privatelink_eastus_azmk8s_io_externalid
      enablePrivateClusterPublicFQDN: false
    }
    identityProfile: {
      kubeletidentity: {
        resourceId: userAssignedIdentities_EUS_FLLM_DEMO_APP_FRONTEND_aks_agentpool_externalid
        clientId: '22941ffe-9a77-4859-871b-5e76b83dcd80'
        objectId: '1b7b600c-b55b-4698-92ec-134ddaf73122'
      }
    }
    autoScalerProfile: {
      'balance-similar-node-groups': 'false'
      expander: 'random'
      'max-empty-bulk-delete': '10'
      'max-graceful-termination-sec': '600'
      'max-node-provision-time': '15m'
      'max-total-unready-percentage': '45'
      'new-pod-scale-up-delay': '0s'
      'ok-total-unready-count': '3'
      'scale-down-delay-after-add': '10m'
      'scale-down-delay-after-delete': '10s'
      'scale-down-delay-after-failure': '3m'
      'scale-down-unneeded-time': '10m'
      'scale-down-unready-time': '20m'
      'scale-down-utilization-threshold': '0.5'
      'scan-interval': '10s'
      'skip-nodes-with-local-storage': 'false'
      'skip-nodes-with-system-pods': 'true'
    }
    autoUpgradeProfile: {
      upgradeChannel: 'stable'
    }
    disableLocalAccounts: true
    securityProfile: {
      defender: {
        logAnalyticsWorkspaceResourceId: workspaces_EUS_FLLM_DEMO_OPS_la_externalid
        securityMonitoring: {
          enabled: true
        }
      }
      workloadIdentity: {
        enabled: true
      }
      imageCleaner: {
        enabled: false
        intervalHours: 48
      }
    }
    storageProfile: {
      diskCSIDriver: {
        enabled: true
        version: 'v1'
      }
      fileCSIDriver: {
        enabled: true
      }
      snapshotController: {
        enabled: true
      }
    }
    oidcIssuerProfile: {
      enabled: true
    }
    workloadAutoScalerProfile: {}
    azureMonitorProfile: {
      metrics: {
        enabled: true
      }
    }
  }
}

resource dataCollectionEndpoints_MSProm_eastus_eus_fllm_demo_app_backend_aks_name_resource 'Microsoft.Insights/dataCollectionEndpoints@2022-06-01' = {
  name: dataCollectionEndpoints_MSProm_eastus_eus_fllm_demo_app_backend_aks_name
  location: 'eastus'
  kind: 'Linux'
  properties: {
    immutableId: 'dce-880a486a92a349abbd4c0d817c3c45db'
    configurationAccess: {}
    logsIngestion: {}
    metricsIngestion: {}
    networkAcls: {
      publicNetworkAccess: 'Enabled'
    }
  }
}

resource dataCollectionEndpoints_MSProm_eastus_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_resource 'Microsoft.Insights/dataCollectionEndpoints@2022-06-01' = {
  name: dataCollectionEndpoints_MSProm_eastus_EUS_FLLM_DEMO_APP_FRONTEND_aks_name
  location: 'eastus'
  kind: 'Linux'
  properties: {
    immutableId: 'dce-f60d43fcea224c1c9cf52517db61a174'
    configurationAccess: {}
    logsIngestion: {}
    metricsIngestion: {}
    networkAcls: {
      publicNetworkAccess: 'Enabled'
    }
  }
}

resource dataCollectionRules_MSCI_eastus_eus_fllm_demo_app_backend_aks_name_resource 'Microsoft.Insights/dataCollectionRules@2022-06-01' = {
  name: dataCollectionRules_MSCI_eastus_eus_fllm_demo_app_backend_aks_name
  location: 'eastus'
  kind: 'Linux'
  properties: {
    dataSources: {
      syslog: []
      extensions: [
        {
          streams: [
            'Microsoft-ContainerInsights-Group-Default'
          ]
          extensionName: 'ContainerInsights'
          extensionSettings: {}
          name: 'ContainerInsightsExtension'
        }
      ]
    }
    destinations: {
      logAnalytics: [
        {
          workspaceResourceId: workspaces_EUS_FLLM_DEMO_OPS_la_externalid
          name: 'ciworkspace'
        }
      ]
    }
    dataFlows: [
      {
        streams: [
          'Microsoft-ContainerInsights-Group-Default'
        ]
        destinations: [
          'ciworkspace'
        ]
      }
    ]
  }
}

resource userAssignedIdentities_EUS_FLLM_DEMO_APP_agent_factory_api_mi_name_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: userAssignedIdentities_EUS_FLLM_DEMO_APP_agent_factory_api_mi_name
  location: 'eastus'
}

resource userAssignedIdentities_EUS_FLLM_DEMO_APP_agent_hub_api_mi_name_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: userAssignedIdentities_EUS_FLLM_DEMO_APP_agent_hub_api_mi_name
  location: 'eastus'
}

resource userAssignedIdentities_EUS_FLLM_DEMO_APP_BACKEND_aks_mi_name_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: userAssignedIdentities_EUS_FLLM_DEMO_APP_BACKEND_aks_mi_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Application'
    Workspace: 'FoundationaLLM-Platform'
  }
}

resource userAssignedIdentities_EUS_FLLM_DEMO_APP_chat_ui_mi_name_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: userAssignedIdentities_EUS_FLLM_DEMO_APP_chat_ui_mi_name
  location: 'eastus'
}

resource userAssignedIdentities_EUS_FLLM_DEMO_APP_core_api_mi_name_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: userAssignedIdentities_EUS_FLLM_DEMO_APP_core_api_mi_name
  location: 'eastus'
}

resource userAssignedIdentities_EUS_FLLM_DEMO_APP_data_source_hub_api_mi_name_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: userAssignedIdentities_EUS_FLLM_DEMO_APP_data_source_hub_api_mi_name
  location: 'eastus'
}

resource userAssignedIdentities_EUS_FLLM_DEMO_APP_FRONTEND_aks_mi_name_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: userAssignedIdentities_EUS_FLLM_DEMO_APP_FRONTEND_aks_mi_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Application'
    Workspace: 'FoundationaLLM-Platform'
  }
}

resource userAssignedIdentities_EUS_FLLM_DEMO_APP_gatekeeper_api_mi_name_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: userAssignedIdentities_EUS_FLLM_DEMO_APP_gatekeeper_api_mi_name
  location: 'eastus'
}

resource userAssignedIdentities_EUS_FLLM_DEMO_APP_langchain_api_mi_name_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: userAssignedIdentities_EUS_FLLM_DEMO_APP_langchain_api_mi_name
  location: 'eastus'
}

resource userAssignedIdentities_EUS_FLLM_DEMO_APP_prompt_hub_api_mi_name_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: userAssignedIdentities_EUS_FLLM_DEMO_APP_prompt_hub_api_mi_name
  location: 'eastus'
}

resource userAssignedIdentities_EUS_FLLM_DEMO_APP_semantic_kernel_api_mi_name_resource 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: userAssignedIdentities_EUS_FLLM_DEMO_APP_semantic_kernel_api_mi_name
  location: 'eastus'
}

resource prometheusRuleGroups_KubernetesRecordingRulesRuleGroup_EUS_FLLM_DEMO_APP_BACKEND_aks_name_resource 'Microsoft.AlertsManagement/prometheusRuleGroups@2023-03-01' = {
  name: prometheusRuleGroups_KubernetesRecordingRulesRuleGroup_EUS_FLLM_DEMO_APP_BACKEND_aks_name
  location: 'eastus'
  properties: {
    enabled: true
    description: 'Kubernetes Recording Rules Rule Group'
    clusterName: 'EUS-FLLM-DEMO-APP-BACKEND-aks'
    scopes: [
      accounts_eus_fllm_demo_ops_amw_externalid
      managedClusters_EUS_FLLM_DEMO_APP_BACKEND_aks_name_resource.id
    ]
    rules: [
      {
        record: 'node_namespace_pod_container:container_cpu_usage_seconds_total:sum_irate'
        enabled: true
        expression: 'sum by (cluster, namespace, pod, container) (  irate(container_cpu_usage_seconds_total{job="cadvisor", image!=""}[5m])) * on (cluster, namespace, pod) group_left(node) topk by (cluster, namespace, pod) (  1, max by(cluster, namespace, pod, node) (kube_pod_info{node!=""}))\n'
        labels: {}
      }
      {
        record: 'node_namespace_pod_container:container_memory_working_set_bytes'
        enabled: true
        expression: 'container_memory_working_set_bytes{job="cadvisor", image!=""}* on (namespace, pod) group_left(node) topk by(namespace, pod) (1,  max by(namespace, pod, node) (kube_pod_info{node!=""}))\n'
        labels: {}
      }
      {
        record: 'node_namespace_pod_container:container_memory_rss'
        enabled: true
        expression: 'container_memory_rss{job="cadvisor", image!=""}* on (namespace, pod) group_left(node) topk by(namespace, pod) (1,  max by(namespace, pod, node) (kube_pod_info{node!=""}))\n'
        labels: {}
      }
      {
        record: 'node_namespace_pod_container:container_memory_cache'
        enabled: true
        expression: 'container_memory_cache{job="cadvisor", image!=""}* on (namespace, pod) group_left(node) topk by(namespace, pod) (1,  max by(namespace, pod, node) (kube_pod_info{node!=""}))\n'
        labels: {}
      }
      {
        record: 'node_namespace_pod_container:container_memory_swap'
        enabled: true
        expression: 'container_memory_swap{job="cadvisor", image!=""}* on (namespace, pod) group_left(node) topk by(namespace, pod) (1,  max by(namespace, pod, node) (kube_pod_info{node!=""}))\n'
        labels: {}
      }
      {
        record: 'cluster:namespace:pod_memory:active:kube_pod_container_resource_requests'
        enabled: true
        expression: 'kube_pod_container_resource_requests{resource="memory",job="kube-state-metrics"}  * on (namespace, pod, cluster)group_left() max by (namespace, pod, cluster) (  (kube_pod_status_phase{phase=~"Pending|Running"} == 1))\n'
        labels: {}
      }
      {
        record: 'namespace_memory:kube_pod_container_resource_requests:sum'
        enabled: true
        expression: 'sum by (namespace, cluster) (    sum by (namespace, pod, cluster) (        max by (namespace, pod, container, cluster) (          kube_pod_container_resource_requests{resource="memory",job="kube-state-metrics"}        ) * on(namespace, pod, cluster) group_left() max by (namespace, pod, cluster) (          kube_pod_status_phase{phase=~"Pending|Running"} == 1        )    ))\n'
        labels: {}
      }
      {
        record: 'cluster:namespace:pod_cpu:active:kube_pod_container_resource_requests'
        enabled: true
        expression: 'kube_pod_container_resource_requests{resource="cpu",job="kube-state-metrics"}  * on (namespace, pod, cluster)group_left() max by (namespace, pod, cluster) (  (kube_pod_status_phase{phase=~"Pending|Running"} == 1))\n'
        labels: {}
      }
      {
        record: 'namespace_cpu:kube_pod_container_resource_requests:sum'
        enabled: true
        expression: 'sum by (namespace, cluster) (    sum by (namespace, pod, cluster) (        max by (namespace, pod, container, cluster) (          kube_pod_container_resource_requests{resource="cpu",job="kube-state-metrics"}        ) * on(namespace, pod, cluster) group_left() max by (namespace, pod, cluster) (          kube_pod_status_phase{phase=~"Pending|Running"} == 1        )    ))\n'
        labels: {}
      }
      {
        record: 'cluster:namespace:pod_memory:active:kube_pod_container_resource_limits'
        enabled: true
        expression: 'kube_pod_container_resource_limits{resource="memory",job="kube-state-metrics"}  * on (namespace, pod, cluster)group_left() max by (namespace, pod, cluster) (  (kube_pod_status_phase{phase=~"Pending|Running"} == 1))\n'
        labels: {}
      }
      {
        record: 'namespace_memory:kube_pod_container_resource_limits:sum'
        enabled: true
        expression: 'sum by (namespace, cluster) (    sum by (namespace, pod, cluster) (        max by (namespace, pod, container, cluster) (          kube_pod_container_resource_limits{resource="memory",job="kube-state-metrics"}        ) * on(namespace, pod, cluster) group_left() max by (namespace, pod, cluster) (          kube_pod_status_phase{phase=~"Pending|Running"} == 1        )    ))\n'
        labels: {}
      }
      {
        record: 'cluster:namespace:pod_cpu:active:kube_pod_container_resource_limits'
        enabled: true
        expression: 'kube_pod_container_resource_limits{resource="cpu",job="kube-state-metrics"}  * on (namespace, pod, cluster)group_left() max by (namespace, pod, cluster) ( (kube_pod_status_phase{phase=~"Pending|Running"} == 1) )\n'
        labels: {}
      }
      {
        record: 'namespace_cpu:kube_pod_container_resource_limits:sum'
        enabled: true
        expression: 'sum by (namespace, cluster) (    sum by (namespace, pod, cluster) (        max by (namespace, pod, container, cluster) (          kube_pod_container_resource_limits{resource="cpu",job="kube-state-metrics"}        ) * on(namespace, pod, cluster) group_left() max by (namespace, pod, cluster) (          kube_pod_status_phase{phase=~"Pending|Running"} == 1        )    ))\n'
        labels: {}
      }
      {
        record: 'namespace_workload_pod:kube_pod_owner:relabel'
        enabled: true
        expression: 'max by (cluster, namespace, workload, pod) (  label_replace(    label_replace(      kube_pod_owner{job="kube-state-metrics", owner_kind="ReplicaSet"},      "replicaset", "$1", "owner_name", "(.*)"    ) * on(replicaset, namespace) group_left(owner_name) topk by(replicaset, namespace) (      1, max by (replicaset, namespace, owner_name) (        kube_replicaset_owner{job="kube-state-metrics"}      )    ),    "workload", "$1", "owner_name", "(.*)"  ))\n'
        labels: {
          workload_type: 'deployment'
        }
      }
      {
        record: 'namespace_workload_pod:kube_pod_owner:relabel'
        enabled: true
        expression: 'max by (cluster, namespace, workload, pod) (  label_replace(    kube_pod_owner{job="kube-state-metrics", owner_kind="DaemonSet"},    "workload", "$1", "owner_name", "(.*)"  ))\n'
        labels: {
          workload_type: 'daemonset'
        }
      }
      {
        record: 'namespace_workload_pod:kube_pod_owner:relabel'
        enabled: true
        expression: 'max by (cluster, namespace, workload, pod) (  label_replace(    kube_pod_owner{job="kube-state-metrics", owner_kind="StatefulSet"},    "workload", "$1", "owner_name", "(.*)"  ))\n'
        labels: {
          workload_type: 'statefulset'
        }
      }
      {
        record: 'namespace_workload_pod:kube_pod_owner:relabel'
        enabled: true
        expression: 'max by (cluster, namespace, workload, pod) (  label_replace(    kube_pod_owner{job="kube-state-metrics", owner_kind="Job"},    "workload", "$1", "owner_name", "(.*)"  ))\n'
        labels: {
          workload_type: 'job'
        }
      }
      {
        record: ':node_memory_MemAvailable_bytes:sum'
        enabled: true
        expression: 'sum(  node_memory_MemAvailable_bytes{job="node"} or  (    node_memory_Buffers_bytes{job="node"} +    node_memory_Cached_bytes{job="node"} +    node_memory_MemFree_bytes{job="node"} +    node_memory_Slab_bytes{job="node"}  )) by (cluster)\n'
        labels: {}
      }
      {
        record: 'cluster:node_cpu:ratio_rate5m'
        enabled: true
        expression: 'sum(rate(node_cpu_seconds_total{job="node",mode!="idle",mode!="iowait",mode!="steal"}[5m])) by (cluster) /count(sum(node_cpu_seconds_total{job="node"}) by (cluster, instance, cpu)) by (cluster)\n'
        labels: {}
      }
    ]
    interval: 'PT1M'
  }
}

resource prometheusRuleGroups_KubernetesRecordingRulesRuleGroup_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_resource 'Microsoft.AlertsManagement/prometheusRuleGroups@2023-03-01' = {
  name: prometheusRuleGroups_KubernetesRecordingRulesRuleGroup_EUS_FLLM_DEMO_APP_FRONTEND_aks_name
  location: 'eastus'
  properties: {
    enabled: true
    description: 'Kubernetes Recording Rules Rule Group'
    clusterName: 'EUS-FLLM-DEMO-APP-FRONTEND-aks'
    scopes: [
      accounts_eus_fllm_demo_ops_amw_externalid
      managedClusters_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_resource.id
    ]
    rules: [
      {
        record: 'node_namespace_pod_container:container_cpu_usage_seconds_total:sum_irate'
        enabled: true
        expression: 'sum by (cluster, namespace, pod, container) (  irate(container_cpu_usage_seconds_total{job="cadvisor", image!=""}[5m])) * on (cluster, namespace, pod) group_left(node) topk by (cluster, namespace, pod) (  1, max by(cluster, namespace, pod, node) (kube_pod_info{node!=""}))\n'
        labels: {}
      }
      {
        record: 'node_namespace_pod_container:container_memory_working_set_bytes'
        enabled: true
        expression: 'container_memory_working_set_bytes{job="cadvisor", image!=""}* on (namespace, pod) group_left(node) topk by(namespace, pod) (1,  max by(namespace, pod, node) (kube_pod_info{node!=""}))\n'
        labels: {}
      }
      {
        record: 'node_namespace_pod_container:container_memory_rss'
        enabled: true
        expression: 'container_memory_rss{job="cadvisor", image!=""}* on (namespace, pod) group_left(node) topk by(namespace, pod) (1,  max by(namespace, pod, node) (kube_pod_info{node!=""}))\n'
        labels: {}
      }
      {
        record: 'node_namespace_pod_container:container_memory_cache'
        enabled: true
        expression: 'container_memory_cache{job="cadvisor", image!=""}* on (namespace, pod) group_left(node) topk by(namespace, pod) (1,  max by(namespace, pod, node) (kube_pod_info{node!=""}))\n'
        labels: {}
      }
      {
        record: 'node_namespace_pod_container:container_memory_swap'
        enabled: true
        expression: 'container_memory_swap{job="cadvisor", image!=""}* on (namespace, pod) group_left(node) topk by(namespace, pod) (1,  max by(namespace, pod, node) (kube_pod_info{node!=""}))\n'
        labels: {}
      }
      {
        record: 'cluster:namespace:pod_memory:active:kube_pod_container_resource_requests'
        enabled: true
        expression: 'kube_pod_container_resource_requests{resource="memory",job="kube-state-metrics"}  * on (namespace, pod, cluster)group_left() max by (namespace, pod, cluster) (  (kube_pod_status_phase{phase=~"Pending|Running"} == 1))\n'
        labels: {}
      }
      {
        record: 'namespace_memory:kube_pod_container_resource_requests:sum'
        enabled: true
        expression: 'sum by (namespace, cluster) (    sum by (namespace, pod, cluster) (        max by (namespace, pod, container, cluster) (          kube_pod_container_resource_requests{resource="memory",job="kube-state-metrics"}        ) * on(namespace, pod, cluster) group_left() max by (namespace, pod, cluster) (          kube_pod_status_phase{phase=~"Pending|Running"} == 1        )    ))\n'
        labels: {}
      }
      {
        record: 'cluster:namespace:pod_cpu:active:kube_pod_container_resource_requests'
        enabled: true
        expression: 'kube_pod_container_resource_requests{resource="cpu",job="kube-state-metrics"}  * on (namespace, pod, cluster)group_left() max by (namespace, pod, cluster) (  (kube_pod_status_phase{phase=~"Pending|Running"} == 1))\n'
        labels: {}
      }
      {
        record: 'namespace_cpu:kube_pod_container_resource_requests:sum'
        enabled: true
        expression: 'sum by (namespace, cluster) (    sum by (namespace, pod, cluster) (        max by (namespace, pod, container, cluster) (          kube_pod_container_resource_requests{resource="cpu",job="kube-state-metrics"}        ) * on(namespace, pod, cluster) group_left() max by (namespace, pod, cluster) (          kube_pod_status_phase{phase=~"Pending|Running"} == 1        )    ))\n'
        labels: {}
      }
      {
        record: 'cluster:namespace:pod_memory:active:kube_pod_container_resource_limits'
        enabled: true
        expression: 'kube_pod_container_resource_limits{resource="memory",job="kube-state-metrics"}  * on (namespace, pod, cluster)group_left() max by (namespace, pod, cluster) (  (kube_pod_status_phase{phase=~"Pending|Running"} == 1))\n'
        labels: {}
      }
      {
        record: 'namespace_memory:kube_pod_container_resource_limits:sum'
        enabled: true
        expression: 'sum by (namespace, cluster) (    sum by (namespace, pod, cluster) (        max by (namespace, pod, container, cluster) (          kube_pod_container_resource_limits{resource="memory",job="kube-state-metrics"}        ) * on(namespace, pod, cluster) group_left() max by (namespace, pod, cluster) (          kube_pod_status_phase{phase=~"Pending|Running"} == 1        )    ))\n'
        labels: {}
      }
      {
        record: 'cluster:namespace:pod_cpu:active:kube_pod_container_resource_limits'
        enabled: true
        expression: 'kube_pod_container_resource_limits{resource="cpu",job="kube-state-metrics"}  * on (namespace, pod, cluster)group_left() max by (namespace, pod, cluster) ( (kube_pod_status_phase{phase=~"Pending|Running"} == 1) )\n'
        labels: {}
      }
      {
        record: 'namespace_cpu:kube_pod_container_resource_limits:sum'
        enabled: true
        expression: 'sum by (namespace, cluster) (    sum by (namespace, pod, cluster) (        max by (namespace, pod, container, cluster) (          kube_pod_container_resource_limits{resource="cpu",job="kube-state-metrics"}        ) * on(namespace, pod, cluster) group_left() max by (namespace, pod, cluster) (          kube_pod_status_phase{phase=~"Pending|Running"} == 1        )    ))\n'
        labels: {}
      }
      {
        record: 'namespace_workload_pod:kube_pod_owner:relabel'
        enabled: true
        expression: 'max by (cluster, namespace, workload, pod) (  label_replace(    label_replace(      kube_pod_owner{job="kube-state-metrics", owner_kind="ReplicaSet"},      "replicaset", "$1", "owner_name", "(.*)"    ) * on(replicaset, namespace) group_left(owner_name) topk by(replicaset, namespace) (      1, max by (replicaset, namespace, owner_name) (        kube_replicaset_owner{job="kube-state-metrics"}      )    ),    "workload", "$1", "owner_name", "(.*)"  ))\n'
        labels: {
          workload_type: 'deployment'
        }
      }
      {
        record: 'namespace_workload_pod:kube_pod_owner:relabel'
        enabled: true
        expression: 'max by (cluster, namespace, workload, pod) (  label_replace(    kube_pod_owner{job="kube-state-metrics", owner_kind="DaemonSet"},    "workload", "$1", "owner_name", "(.*)"  ))\n'
        labels: {
          workload_type: 'daemonset'
        }
      }
      {
        record: 'namespace_workload_pod:kube_pod_owner:relabel'
        enabled: true
        expression: 'max by (cluster, namespace, workload, pod) (  label_replace(    kube_pod_owner{job="kube-state-metrics", owner_kind="StatefulSet"},    "workload", "$1", "owner_name", "(.*)"  ))\n'
        labels: {
          workload_type: 'statefulset'
        }
      }
      {
        record: 'namespace_workload_pod:kube_pod_owner:relabel'
        enabled: true
        expression: 'max by (cluster, namespace, workload, pod) (  label_replace(    kube_pod_owner{job="kube-state-metrics", owner_kind="Job"},    "workload", "$1", "owner_name", "(.*)"  ))\n'
        labels: {
          workload_type: 'job'
        }
      }
      {
        record: ':node_memory_MemAvailable_bytes:sum'
        enabled: true
        expression: 'sum(  node_memory_MemAvailable_bytes{job="node"} or  (    node_memory_Buffers_bytes{job="node"} +    node_memory_Cached_bytes{job="node"} +    node_memory_MemFree_bytes{job="node"} +    node_memory_Slab_bytes{job="node"}  )) by (cluster)\n'
        labels: {}
      }
      {
        record: 'cluster:node_cpu:ratio_rate5m'
        enabled: true
        expression: 'sum(rate(node_cpu_seconds_total{job="node",mode!="idle",mode!="iowait",mode!="steal"}[5m])) by (cluster) /count(sum(node_cpu_seconds_total{job="node"}) by (cluster, instance, cpu)) by (cluster)\n'
        labels: {}
      }
    ]
    interval: 'PT1M'
  }
}

resource prometheusRuleGroups_NodeAndKubernetesRecordingRulesRuleGroup_Win_EUS_FLLM_DEMO_APP_BACKEND_aks_name_resource 'Microsoft.AlertsManagement/prometheusRuleGroups@2023-03-01' = {
  name: prometheusRuleGroups_NodeAndKubernetesRecordingRulesRuleGroup_Win_EUS_FLLM_DEMO_APP_BACKEND_aks_name
  location: 'eastus'
  properties: {
    enabled: true
    description: 'Node and Kubernetes Recording Rules Rule Group for Windows Nodes'
    clusterName: 'EUS-FLLM-DEMO-APP-BACKEND-aks'
    scopes: [
      accounts_eus_fllm_demo_ops_amw_externalid
      managedClusters_EUS_FLLM_DEMO_APP_BACKEND_aks_name_resource.id
    ]
    rules: [
      {
        record: 'node:windows_node_filesystem_usage:'
        enabled: true
        expression: 'max by (instance,volume)((windows_logical_disk_size_bytes{job="windows-exporter"} - windows_logical_disk_free_bytes{job="windows-exporter"}) / windows_logical_disk_size_bytes{job="windows-exporter"})\n'
        labels: {}
      }
      {
        record: 'node:windows_node_filesystem_avail:'
        enabled: true
        expression: 'max by (instance, volume) (windows_logical_disk_free_bytes{job="windows-exporter"} / windows_logical_disk_size_bytes{job="windows-exporter"})\n'
        labels: {}
      }
      {
        record: ':windows_node_net_utilisation:sum_irate'
        enabled: true
        expression: 'sum(irate(windows_net_bytes_total{job="windows-exporter"}[5m]))\n'
        labels: {}
      }
      {
        record: 'node:windows_node_net_utilisation:sum_irate'
        enabled: true
        expression: 'sum by (instance) ((irate(windows_net_bytes_total{job="windows-exporter"}[5m])))\n'
        labels: {}
      }
      {
        record: ':windows_node_net_saturation:sum_irate'
        enabled: true
        expression: 'sum(irate(windows_net_packets_received_discarded_total{job="windows-exporter"}[5m])) + sum(irate(windows_net_packets_outbound_discarded_total{job="windows-exporter"}[5m]))\n'
        labels: {}
      }
      {
        record: 'node:windows_node_net_saturation:sum_irate'
        enabled: true
        expression: 'sum by (instance) ((irate(windows_net_packets_received_discarded_total{job="windows-exporter"}[5m]) + irate(windows_net_packets_outbound_discarded_total{job="windows-exporter"}[5m])))\n'
        labels: {}
      }
      {
        record: 'windows_pod_container_available'
        enabled: true
        expression: 'windows_container_available{job="windows-exporter", container_id != ""} * on(container_id) group_left(container, pod, namespace) max(kube_pod_container_info{job="kube-state-metrics", container_id != ""}) by(container, container_id, pod, namespace)\n'
        labels: {}
      }
      {
        record: 'windows_container_total_runtime'
        enabled: true
        expression: 'windows_container_cpu_usage_seconds_total{job="windows-exporter", container_id != ""} * on(container_id) group_left(container, pod, namespace) max(kube_pod_container_info{job="kube-state-metrics", container_id != ""}) by(container, container_id, pod, namespace)\n'
        labels: {}
      }
      {
        record: 'windows_container_memory_usage'
        enabled: true
        expression: 'windows_container_memory_usage_commit_bytes{job="windows-exporter", container_id != ""} * on(container_id) group_left(container, pod, namespace) max(kube_pod_container_info{job="kube-state-metrics", container_id != ""}) by(container, container_id, pod, namespace)\n'
        labels: {}
      }
      {
        record: 'windows_container_private_working_set_usage'
        enabled: true
        expression: 'windows_container_memory_usage_private_working_set_bytes{job="windows-exporter", container_id != ""} * on(container_id) group_left(container, pod, namespace) max(kube_pod_container_info{job="kube-state-metrics", container_id != ""}) by(container, container_id, pod, namespace)\n'
        labels: {}
      }
      {
        record: 'windows_container_network_received_bytes_total'
        enabled: true
        expression: 'windows_container_network_receive_bytes_total{job="windows-exporter", container_id != ""} * on(container_id) group_left(container, pod, namespace) max(kube_pod_container_info{job="kube-state-metrics", container_id != ""}) by(container, container_id, pod, namespace)\n'
        labels: {}
      }
      {
        record: 'windows_container_network_transmitted_bytes_total'
        enabled: true
        expression: 'windows_container_network_transmit_bytes_total{job="windows-exporter", container_id != ""} * on(container_id) group_left(container, pod, namespace) max(kube_pod_container_info{job="kube-state-metrics", container_id != ""}) by(container, container_id, pod, namespace)\n'
        labels: {}
      }
      {
        record: 'kube_pod_windows_container_resource_memory_request'
        enabled: true
        expression: 'max by (namespace, pod, container) (kube_pod_container_resource_requests{resource="memory",job="kube-state-metrics"}) * on(container,pod,namespace) (windows_pod_container_available)\n'
        labels: {}
      }
      {
        record: 'kube_pod_windows_container_resource_memory_limit'
        enabled: true
        expression: 'kube_pod_container_resource_limits{resource="memory",job="kube-state-metrics"} * on(container,pod,namespace) (windows_pod_container_available)\n'
        labels: {}
      }
      {
        record: 'kube_pod_windows_container_resource_cpu_cores_request'
        enabled: true
        expression: 'max by (namespace, pod, container) ( kube_pod_container_resource_requests{resource="cpu",job="kube-state-metrics"}) * on(container,pod,namespace) (windows_pod_container_available)\n'
        labels: {}
      }
      {
        record: 'kube_pod_windows_container_resource_cpu_cores_limit'
        enabled: true
        expression: 'kube_pod_container_resource_limits{resource="cpu",job="kube-state-metrics"} * on(container,pod,namespace) (windows_pod_container_available)\n'
        labels: {}
      }
      {
        record: 'namespace_pod_container:windows_container_cpu_usage_seconds_total:sum_rate'
        enabled: true
        expression: 'sum by (namespace, pod, container) (rate(windows_container_total_runtime{}[5m]))\n'
        labels: {}
      }
    ]
    interval: 'PT1M'
  }
}

resource prometheusRuleGroups_NodeAndKubernetesRecordingRulesRuleGroup_Win_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_resource 'Microsoft.AlertsManagement/prometheusRuleGroups@2023-03-01' = {
  name: prometheusRuleGroups_NodeAndKubernetesRecordingRulesRuleGroup_Win_EUS_FLLM_DEMO_APP_FRONTEND_aks_name
  location: 'eastus'
  properties: {
    enabled: true
    description: 'Node and Kubernetes Recording Rules Rule Group for Windows Nodes'
    clusterName: 'EUS-FLLM-DEMO-APP-FRONTEND-aks'
    scopes: [
      accounts_eus_fllm_demo_ops_amw_externalid
      managedClusters_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_resource.id
    ]
    rules: [
      {
        record: 'node:windows_node_filesystem_usage:'
        enabled: true
        expression: 'max by (instance,volume)((windows_logical_disk_size_bytes{job="windows-exporter"} - windows_logical_disk_free_bytes{job="windows-exporter"}) / windows_logical_disk_size_bytes{job="windows-exporter"})\n'
        labels: {}
      }
      {
        record: 'node:windows_node_filesystem_avail:'
        enabled: true
        expression: 'max by (instance, volume) (windows_logical_disk_free_bytes{job="windows-exporter"} / windows_logical_disk_size_bytes{job="windows-exporter"})\n'
        labels: {}
      }
      {
        record: ':windows_node_net_utilisation:sum_irate'
        enabled: true
        expression: 'sum(irate(windows_net_bytes_total{job="windows-exporter"}[5m]))\n'
        labels: {}
      }
      {
        record: 'node:windows_node_net_utilisation:sum_irate'
        enabled: true
        expression: 'sum by (instance) ((irate(windows_net_bytes_total{job="windows-exporter"}[5m])))\n'
        labels: {}
      }
      {
        record: ':windows_node_net_saturation:sum_irate'
        enabled: true
        expression: 'sum(irate(windows_net_packets_received_discarded_total{job="windows-exporter"}[5m])) + sum(irate(windows_net_packets_outbound_discarded_total{job="windows-exporter"}[5m]))\n'
        labels: {}
      }
      {
        record: 'node:windows_node_net_saturation:sum_irate'
        enabled: true
        expression: 'sum by (instance) ((irate(windows_net_packets_received_discarded_total{job="windows-exporter"}[5m]) + irate(windows_net_packets_outbound_discarded_total{job="windows-exporter"}[5m])))\n'
        labels: {}
      }
      {
        record: 'windows_pod_container_available'
        enabled: true
        expression: 'windows_container_available{job="windows-exporter", container_id != ""} * on(container_id) group_left(container, pod, namespace) max(kube_pod_container_info{job="kube-state-metrics", container_id != ""}) by(container, container_id, pod, namespace)\n'
        labels: {}
      }
      {
        record: 'windows_container_total_runtime'
        enabled: true
        expression: 'windows_container_cpu_usage_seconds_total{job="windows-exporter", container_id != ""} * on(container_id) group_left(container, pod, namespace) max(kube_pod_container_info{job="kube-state-metrics", container_id != ""}) by(container, container_id, pod, namespace)\n'
        labels: {}
      }
      {
        record: 'windows_container_memory_usage'
        enabled: true
        expression: 'windows_container_memory_usage_commit_bytes{job="windows-exporter", container_id != ""} * on(container_id) group_left(container, pod, namespace) max(kube_pod_container_info{job="kube-state-metrics", container_id != ""}) by(container, container_id, pod, namespace)\n'
        labels: {}
      }
      {
        record: 'windows_container_private_working_set_usage'
        enabled: true
        expression: 'windows_container_memory_usage_private_working_set_bytes{job="windows-exporter", container_id != ""} * on(container_id) group_left(container, pod, namespace) max(kube_pod_container_info{job="kube-state-metrics", container_id != ""}) by(container, container_id, pod, namespace)\n'
        labels: {}
      }
      {
        record: 'windows_container_network_received_bytes_total'
        enabled: true
        expression: 'windows_container_network_receive_bytes_total{job="windows-exporter", container_id != ""} * on(container_id) group_left(container, pod, namespace) max(kube_pod_container_info{job="kube-state-metrics", container_id != ""}) by(container, container_id, pod, namespace)\n'
        labels: {}
      }
      {
        record: 'windows_container_network_transmitted_bytes_total'
        enabled: true
        expression: 'windows_container_network_transmit_bytes_total{job="windows-exporter", container_id != ""} * on(container_id) group_left(container, pod, namespace) max(kube_pod_container_info{job="kube-state-metrics", container_id != ""}) by(container, container_id, pod, namespace)\n'
        labels: {}
      }
      {
        record: 'kube_pod_windows_container_resource_memory_request'
        enabled: true
        expression: 'max by (namespace, pod, container) (kube_pod_container_resource_requests{resource="memory",job="kube-state-metrics"}) * on(container,pod,namespace) (windows_pod_container_available)\n'
        labels: {}
      }
      {
        record: 'kube_pod_windows_container_resource_memory_limit'
        enabled: true
        expression: 'kube_pod_container_resource_limits{resource="memory",job="kube-state-metrics"} * on(container,pod,namespace) (windows_pod_container_available)\n'
        labels: {}
      }
      {
        record: 'kube_pod_windows_container_resource_cpu_cores_request'
        enabled: true
        expression: 'max by (namespace, pod, container) ( kube_pod_container_resource_requests{resource="cpu",job="kube-state-metrics"}) * on(container,pod,namespace) (windows_pod_container_available)\n'
        labels: {}
      }
      {
        record: 'kube_pod_windows_container_resource_cpu_cores_limit'
        enabled: true
        expression: 'kube_pod_container_resource_limits{resource="cpu",job="kube-state-metrics"} * on(container,pod,namespace) (windows_pod_container_available)\n'
        labels: {}
      }
      {
        record: 'namespace_pod_container:windows_container_cpu_usage_seconds_total:sum_rate'
        enabled: true
        expression: 'sum by (namespace, pod, container) (rate(windows_container_total_runtime{}[5m]))\n'
        labels: {}
      }
    ]
    interval: 'PT1M'
  }
}

resource prometheusRuleGroups_NodeRecordingRulesRuleGroup_EUS_FLLM_DEMO_APP_BACKEND_aks_name_resource 'Microsoft.AlertsManagement/prometheusRuleGroups@2023-03-01' = {
  name: prometheusRuleGroups_NodeRecordingRulesRuleGroup_EUS_FLLM_DEMO_APP_BACKEND_aks_name
  location: 'eastus'
  properties: {
    enabled: true
    description: 'Node Recording Rules Rule Group'
    clusterName: 'EUS-FLLM-DEMO-APP-BACKEND-aks'
    scopes: [
      accounts_eus_fllm_demo_ops_amw_externalid
      managedClusters_EUS_FLLM_DEMO_APP_BACKEND_aks_name_resource.id
    ]
    rules: [
      {
        record: 'instance:node_num_cpu:sum'
        enabled: true
        expression: 'count without (cpu, mode) (  node_cpu_seconds_total{job="node",mode="idle"})\n'
        labels: {}
      }
      {
        record: 'instance:node_cpu_utilisation:rate5m'
        enabled: true
        expression: '1 - avg without (cpu) (  sum without (mode) (rate(node_cpu_seconds_total{job="node", mode=~"idle|iowait|steal"}[5m])))\n'
        labels: {}
      }
      {
        record: 'instance:node_load1_per_cpu:ratio'
        enabled: true
        expression: '(  node_load1{job="node"}/  instance:node_num_cpu:sum{job="node"})\n'
        labels: {}
      }
      {
        record: 'instance:node_memory_utilisation:ratio'
        enabled: true
        expression: '1 - (  (    node_memory_MemAvailable_bytes{job="node"}    or    (      node_memory_Buffers_bytes{job="node"}      +      node_memory_Cached_bytes{job="node"}      +      node_memory_MemFree_bytes{job="node"}      +      node_memory_Slab_bytes{job="node"}    )  )/  node_memory_MemTotal_bytes{job="node"})\n'
        labels: {}
      }
      {
        record: 'instance:node_vmstat_pgmajfault:rate5m'
        enabled: true
        expression: 'rate(node_vmstat_pgmajfault{job="node"}[5m])\n'
        labels: {}
      }
      {
        record: 'instance_device:node_disk_io_time_seconds:rate5m'
        enabled: true
        expression: 'rate(node_disk_io_time_seconds_total{job="node", device!=""}[5m])\n'
        labels: {}
      }
      {
        record: 'instance_device:node_disk_io_time_weighted_seconds:rate5m'
        enabled: true
        expression: 'rate(node_disk_io_time_weighted_seconds_total{job="node", device!=""}[5m])\n'
        labels: {}
      }
      {
        record: 'instance:node_network_receive_bytes_excluding_lo:rate5m'
        enabled: true
        expression: 'sum without (device) (  rate(node_network_receive_bytes_total{job="node", device!="lo"}[5m]))\n'
        labels: {}
      }
      {
        record: 'instance:node_network_transmit_bytes_excluding_lo:rate5m'
        enabled: true
        expression: 'sum without (device) (  rate(node_network_transmit_bytes_total{job="node", device!="lo"}[5m]))\n'
        labels: {}
      }
      {
        record: 'instance:node_network_receive_drop_excluding_lo:rate5m'
        enabled: true
        expression: 'sum without (device) (  rate(node_network_receive_drop_total{job="node", device!="lo"}[5m]))\n'
        labels: {}
      }
      {
        record: 'instance:node_network_transmit_drop_excluding_lo:rate5m'
        enabled: true
        expression: 'sum without (device) (  rate(node_network_transmit_drop_total{job="node", device!="lo"}[5m]))\n'
        labels: {}
      }
    ]
    interval: 'PT1M'
  }
}

resource prometheusRuleGroups_NodeRecordingRulesRuleGroup_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_resource 'Microsoft.AlertsManagement/prometheusRuleGroups@2023-03-01' = {
  name: prometheusRuleGroups_NodeRecordingRulesRuleGroup_EUS_FLLM_DEMO_APP_FRONTEND_aks_name
  location: 'eastus'
  properties: {
    enabled: true
    description: 'Node Recording Rules Rule Group'
    clusterName: 'EUS-FLLM-DEMO-APP-FRONTEND-aks'
    scopes: [
      accounts_eus_fllm_demo_ops_amw_externalid
      managedClusters_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_resource.id
    ]
    rules: [
      {
        record: 'instance:node_num_cpu:sum'
        enabled: true
        expression: 'count without (cpu, mode) (  node_cpu_seconds_total{job="node",mode="idle"})\n'
        labels: {}
      }
      {
        record: 'instance:node_cpu_utilisation:rate5m'
        enabled: true
        expression: '1 - avg without (cpu) (  sum without (mode) (rate(node_cpu_seconds_total{job="node", mode=~"idle|iowait|steal"}[5m])))\n'
        labels: {}
      }
      {
        record: 'instance:node_load1_per_cpu:ratio'
        enabled: true
        expression: '(  node_load1{job="node"}/  instance:node_num_cpu:sum{job="node"})\n'
        labels: {}
      }
      {
        record: 'instance:node_memory_utilisation:ratio'
        enabled: true
        expression: '1 - (  (    node_memory_MemAvailable_bytes{job="node"}    or    (      node_memory_Buffers_bytes{job="node"}      +      node_memory_Cached_bytes{job="node"}      +      node_memory_MemFree_bytes{job="node"}      +      node_memory_Slab_bytes{job="node"}    )  )/  node_memory_MemTotal_bytes{job="node"})\n'
        labels: {}
      }
      {
        record: 'instance:node_vmstat_pgmajfault:rate5m'
        enabled: true
        expression: 'rate(node_vmstat_pgmajfault{job="node"}[5m])\n'
        labels: {}
      }
      {
        record: 'instance_device:node_disk_io_time_seconds:rate5m'
        enabled: true
        expression: 'rate(node_disk_io_time_seconds_total{job="node", device!=""}[5m])\n'
        labels: {}
      }
      {
        record: 'instance_device:node_disk_io_time_weighted_seconds:rate5m'
        enabled: true
        expression: 'rate(node_disk_io_time_weighted_seconds_total{job="node", device!=""}[5m])\n'
        labels: {}
      }
      {
        record: 'instance:node_network_receive_bytes_excluding_lo:rate5m'
        enabled: true
        expression: 'sum without (device) (  rate(node_network_receive_bytes_total{job="node", device!="lo"}[5m]))\n'
        labels: {}
      }
      {
        record: 'instance:node_network_transmit_bytes_excluding_lo:rate5m'
        enabled: true
        expression: 'sum without (device) (  rate(node_network_transmit_bytes_total{job="node", device!="lo"}[5m]))\n'
        labels: {}
      }
      {
        record: 'instance:node_network_receive_drop_excluding_lo:rate5m'
        enabled: true
        expression: 'sum without (device) (  rate(node_network_receive_drop_total{job="node", device!="lo"}[5m]))\n'
        labels: {}
      }
      {
        record: 'instance:node_network_transmit_drop_excluding_lo:rate5m'
        enabled: true
        expression: 'sum without (device) (  rate(node_network_transmit_drop_total{job="node", device!="lo"}[5m]))\n'
        labels: {}
      }
    ]
    interval: 'PT1M'
  }
}

resource prometheusRuleGroups_NodeRecordingRulesRuleGroup_Win_EUS_FLLM_DEMO_APP_BACKEND_aks_name_resource 'Microsoft.AlertsManagement/prometheusRuleGroups@2023-03-01' = {
  name: prometheusRuleGroups_NodeRecordingRulesRuleGroup_Win_EUS_FLLM_DEMO_APP_BACKEND_aks_name
  location: 'eastus'
  properties: {
    enabled: true
    description: 'Node and Kubernetes Recording Rules Rule Group for Windows Nodes'
    clusterName: 'EUS-FLLM-DEMO-APP-BACKEND-aks'
    scopes: [
      accounts_eus_fllm_demo_ops_amw_externalid
      managedClusters_EUS_FLLM_DEMO_APP_BACKEND_aks_name_resource.id
    ]
    rules: [
      {
        record: 'node:windows_node:sum'
        enabled: true
        expression: 'count (windows_system_system_up_time{job="windows-exporter"})\n'
        labels: {}
      }
      {
        record: 'node:windows_node_num_cpu:sum'
        enabled: true
        expression: 'count by (instance) (sum by (instance, core) (windows_cpu_time_total{job="windows-exporter"}))\n'
        labels: {}
      }
      {
        record: ':windows_node_cpu_utilisation:avg5m'
        enabled: true
        expression: '1 - avg(rate(windows_cpu_time_total{job="windows-exporter",mode="idle"}[5m]))\n'
        labels: {}
      }
      {
        record: 'node:windows_node_cpu_utilisation:avg5m'
        enabled: true
        expression: '1 - avg by (instance) (rate(windows_cpu_time_total{job="windows-exporter",mode="idle"}[5m]))\n'
        labels: {}
      }
      {
        record: ':windows_node_memory_utilisation:'
        enabled: true
        expression: '1 -sum(windows_memory_available_bytes{job="windows-exporter"})/sum(windows_os_visible_memory_bytes{job="windows-exporter"})\n'
        labels: {}
      }
      {
        record: ':windows_node_memory_MemFreeCached_bytes:sum'
        enabled: true
        expression: 'sum(windows_memory_available_bytes{job="windows-exporter"} + windows_memory_cache_bytes{job="windows-exporter"})\n'
        labels: {}
      }
      {
        record: 'node:windows_node_memory_totalCached_bytes:sum'
        enabled: true
        expression: '(windows_memory_cache_bytes{job="windows-exporter"} + windows_memory_modified_page_list_bytes{job="windows-exporter"} + windows_memory_standby_cache_core_bytes{job="windows-exporter"} + windows_memory_standby_cache_normal_priority_bytes{job="windows-exporter"} + windows_memory_standby_cache_reserve_bytes{job="windows-exporter"})\n'
        labels: {}
      }
      {
        record: ':windows_node_memory_MemTotal_bytes:sum'
        enabled: true
        expression: 'sum(windows_os_visible_memory_bytes{job="windows-exporter"})\n'
        labels: {}
      }
      {
        record: 'node:windows_node_memory_bytes_available:sum'
        enabled: true
        expression: 'sum by (instance) ((windows_memory_available_bytes{job="windows-exporter"}))\n'
        labels: {}
      }
      {
        record: 'node:windows_node_memory_bytes_total:sum'
        enabled: true
        expression: 'sum by (instance) (windows_os_visible_memory_bytes{job="windows-exporter"})\n'
        labels: {}
      }
      {
        record: 'node:windows_node_memory_utilisation:ratio'
        enabled: true
        expression: '(node:windows_node_memory_bytes_total:sum - node:windows_node_memory_bytes_available:sum) / scalar(sum(node:windows_node_memory_bytes_total:sum))\n'
        labels: {}
      }
      {
        record: 'node:windows_node_memory_utilisation:'
        enabled: true
        expression: '1 - (node:windows_node_memory_bytes_available:sum / node:windows_node_memory_bytes_total:sum)\n'
        labels: {}
      }
      {
        record: 'node:windows_node_memory_swap_io_pages:irate'
        enabled: true
        expression: 'irate(windows_memory_swap_page_operations_total{job="windows-exporter"}[5m])\n'
        labels: {}
      }
      {
        record: ':windows_node_disk_utilisation:avg_irate'
        enabled: true
        expression: 'avg(irate(windows_logical_disk_read_seconds_total{job="windows-exporter"}[5m]) + irate(windows_logical_disk_write_seconds_total{job="windows-exporter"}[5m]))\n'
        labels: {}
      }
      {
        record: 'node:windows_node_disk_utilisation:avg_irate'
        enabled: true
        expression: 'avg by (instance) ((irate(windows_logical_disk_read_seconds_total{job="windows-exporter"}[5m]) + irate(windows_logical_disk_write_seconds_total{job="windows-exporter"}[5m])))\n'
        labels: {}
      }
    ]
    interval: 'PT1M'
  }
}

resource prometheusRuleGroups_NodeRecordingRulesRuleGroup_Win_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_resource 'Microsoft.AlertsManagement/prometheusRuleGroups@2023-03-01' = {
  name: prometheusRuleGroups_NodeRecordingRulesRuleGroup_Win_EUS_FLLM_DEMO_APP_FRONTEND_aks_name
  location: 'eastus'
  properties: {
    enabled: true
    description: 'Node and Kubernetes Recording Rules Rule Group for Windows Nodes'
    clusterName: 'EUS-FLLM-DEMO-APP-FRONTEND-aks'
    scopes: [
      accounts_eus_fllm_demo_ops_amw_externalid
      managedClusters_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_resource.id
    ]
    rules: [
      {
        record: 'node:windows_node:sum'
        enabled: true
        expression: 'count (windows_system_system_up_time{job="windows-exporter"})\n'
        labels: {}
      }
      {
        record: 'node:windows_node_num_cpu:sum'
        enabled: true
        expression: 'count by (instance) (sum by (instance, core) (windows_cpu_time_total{job="windows-exporter"}))\n'
        labels: {}
      }
      {
        record: ':windows_node_cpu_utilisation:avg5m'
        enabled: true
        expression: '1 - avg(rate(windows_cpu_time_total{job="windows-exporter",mode="idle"}[5m]))\n'
        labels: {}
      }
      {
        record: 'node:windows_node_cpu_utilisation:avg5m'
        enabled: true
        expression: '1 - avg by (instance) (rate(windows_cpu_time_total{job="windows-exporter",mode="idle"}[5m]))\n'
        labels: {}
      }
      {
        record: ':windows_node_memory_utilisation:'
        enabled: true
        expression: '1 -sum(windows_memory_available_bytes{job="windows-exporter"})/sum(windows_os_visible_memory_bytes{job="windows-exporter"})\n'
        labels: {}
      }
      {
        record: ':windows_node_memory_MemFreeCached_bytes:sum'
        enabled: true
        expression: 'sum(windows_memory_available_bytes{job="windows-exporter"} + windows_memory_cache_bytes{job="windows-exporter"})\n'
        labels: {}
      }
      {
        record: 'node:windows_node_memory_totalCached_bytes:sum'
        enabled: true
        expression: '(windows_memory_cache_bytes{job="windows-exporter"} + windows_memory_modified_page_list_bytes{job="windows-exporter"} + windows_memory_standby_cache_core_bytes{job="windows-exporter"} + windows_memory_standby_cache_normal_priority_bytes{job="windows-exporter"} + windows_memory_standby_cache_reserve_bytes{job="windows-exporter"})\n'
        labels: {}
      }
      {
        record: ':windows_node_memory_MemTotal_bytes:sum'
        enabled: true
        expression: 'sum(windows_os_visible_memory_bytes{job="windows-exporter"})\n'
        labels: {}
      }
      {
        record: 'node:windows_node_memory_bytes_available:sum'
        enabled: true
        expression: 'sum by (instance) ((windows_memory_available_bytes{job="windows-exporter"}))\n'
        labels: {}
      }
      {
        record: 'node:windows_node_memory_bytes_total:sum'
        enabled: true
        expression: 'sum by (instance) (windows_os_visible_memory_bytes{job="windows-exporter"})\n'
        labels: {}
      }
      {
        record: 'node:windows_node_memory_utilisation:ratio'
        enabled: true
        expression: '(node:windows_node_memory_bytes_total:sum - node:windows_node_memory_bytes_available:sum) / scalar(sum(node:windows_node_memory_bytes_total:sum))\n'
        labels: {}
      }
      {
        record: 'node:windows_node_memory_utilisation:'
        enabled: true
        expression: '1 - (node:windows_node_memory_bytes_available:sum / node:windows_node_memory_bytes_total:sum)\n'
        labels: {}
      }
      {
        record: 'node:windows_node_memory_swap_io_pages:irate'
        enabled: true
        expression: 'irate(windows_memory_swap_page_operations_total{job="windows-exporter"}[5m])\n'
        labels: {}
      }
      {
        record: ':windows_node_disk_utilisation:avg_irate'
        enabled: true
        expression: 'avg(irate(windows_logical_disk_read_seconds_total{job="windows-exporter"}[5m]) + irate(windows_logical_disk_write_seconds_total{job="windows-exporter"}[5m]))\n'
        labels: {}
      }
      {
        record: 'node:windows_node_disk_utilisation:avg_irate'
        enabled: true
        expression: 'avg by (instance) ((irate(windows_logical_disk_read_seconds_total{job="windows-exporter"}[5m]) + irate(windows_logical_disk_write_seconds_total{job="windows-exporter"}[5m])))\n'
        labels: {}
      }
    ]
    interval: 'PT1M'
  }
}

resource managedClusters_EUS_FLLM_DEMO_APP_BACKEND_aks_name_system 'Microsoft.ContainerService/managedClusters/agentPools@2023-01-02-preview' = {
  parent: managedClusters_EUS_FLLM_DEMO_APP_BACKEND_aks_name_resource
  name: 'system'
  properties: {
    count: 2
    vmSize: 'Standard_DS2_v2'
    osDiskSizeGB: 1024
    osDiskType: 'Managed'
    kubeletDiskType: 'OS'
    vnetSubnetID: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/FLLMBackend'
    maxPods: 30
    type: 'VirtualMachineScaleSets'
    maxCount: 6
    minCount: 2
    enableAutoScaling: true
    scaleDownMode: 'Delete'
    powerState: {
      code: 'Running'
    }
    orchestratorVersion: '1.26.6'
    enableNodePublicIP: false
    enableCustomCATrust: false
    tags: {
      Environment: 'DEMO'
      Project: 'FLLM'
      Purpose: 'Application'
      Workspace: 'FoundationaLLM-Platform'
    }
    nodeTaints: [
      'CriticalAddonsOnly=true:NoSchedule'
    ]
    mode: 'System'
    enableEncryptionAtHost: false
    enableUltraSSD: false
    osType: 'Linux'
    osSKU: 'Ubuntu'
    upgradeSettings: {
      maxSurge: '200'
    }
    enableFIPS: false
  }
}

resource managedClusters_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_system 'Microsoft.ContainerService/managedClusters/agentPools@2023-01-02-preview' = {
  parent: managedClusters_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_resource
  name: 'system'
  properties: {
    count: 2
    vmSize: 'Standard_DS2_v2'
    osDiskSizeGB: 1024
    osDiskType: 'Managed'
    kubeletDiskType: 'OS'
    vnetSubnetID: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/FLLMFrontEnd'
    maxPods: 30
    type: 'VirtualMachineScaleSets'
    maxCount: 6
    minCount: 2
    enableAutoScaling: true
    scaleDownMode: 'Delete'
    powerState: {
      code: 'Running'
    }
    orchestratorVersion: '1.26.6'
    enableNodePublicIP: false
    enableCustomCATrust: false
    tags: {
      Environment: 'DEMO'
      Project: 'FLLM'
      Purpose: 'Application'
      Workspace: 'FoundationaLLM-Platform'
    }
    nodeTaints: [
      'CriticalAddonsOnly=true:NoSchedule'
    ]
    mode: 'System'
    enableEncryptionAtHost: false
    enableUltraSSD: false
    osType: 'Linux'
    osSKU: 'Ubuntu'
    upgradeSettings: {
      maxSurge: '200'
    }
    enableFIPS: false
  }
}

resource managedClusters_EUS_FLLM_DEMO_APP_BACKEND_aks_name_user 'Microsoft.ContainerService/managedClusters/agentPools@2023-01-02-preview' = {
  parent: managedClusters_EUS_FLLM_DEMO_APP_BACKEND_aks_name_resource
  name: 'user'
  properties: {
    count: 2
    vmSize: 'Standard_DS2_v2'
    osDiskSizeGB: 1024
    osDiskType: 'Managed'
    kubeletDiskType: 'OS'
    vnetSubnetID: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/FLLMBackend'
    maxPods: 30
    type: 'VirtualMachineScaleSets'
    maxCount: 3
    minCount: 1
    enableAutoScaling: true
    scaleDownMode: 'Delete'
    powerState: {
      code: 'Running'
    }
    orchestratorVersion: '1.26.6'
    enableNodePublicIP: false
    enableCustomCATrust: false
    tags: {
      Environment: 'DEMO'
      Project: 'FLLM'
      Purpose: 'Application'
      Workspace: 'FoundationaLLM-Platform'
    }
    mode: 'User'
    enableEncryptionAtHost: false
    enableUltraSSD: false
    osType: 'Linux'
    osSKU: 'Ubuntu'
    upgradeSettings: {
      maxSurge: '200'
    }
    enableFIPS: false
  }
}

resource managedClusters_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_user 'Microsoft.ContainerService/managedClusters/agentPools@2023-01-02-preview' = {
  parent: managedClusters_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_resource
  name: 'user'
  properties: {
    count: 1
    vmSize: 'Standard_DS2_v2'
    osDiskSizeGB: 1024
    osDiskType: 'Managed'
    kubeletDiskType: 'OS'
    vnetSubnetID: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/FLLMFrontEnd'
    maxPods: 30
    type: 'VirtualMachineScaleSets'
    maxCount: 3
    minCount: 1
    enableAutoScaling: true
    scaleDownMode: 'Delete'
    powerState: {
      code: 'Running'
    }
    orchestratorVersion: '1.26.6'
    enableNodePublicIP: false
    enableCustomCATrust: false
    tags: {
      Environment: 'DEMO'
      Project: 'FLLM'
      Purpose: 'Application'
      Workspace: 'FoundationaLLM-Platform'
    }
    mode: 'User'
    enableEncryptionAtHost: false
    enableUltraSSD: false
    osType: 'Linux'
    osSKU: 'Ubuntu'
    upgradeSettings: {
      maxSurge: '200'
    }
    enableFIPS: false
  }
}

resource managedClusters_EUS_FLLM_DEMO_APP_BACKEND_aks_name_defender_cloudposture 'Microsoft.ContainerService/managedClusters/trustedAccessRoleBindings@2023-01-02-preview' = {
  parent: managedClusters_EUS_FLLM_DEMO_APP_BACKEND_aks_name_resource
  name: 'defender-cloudposture'
  properties: {
    sourceResourceId: '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/providers/Microsoft.Security/pricings/CloudPosture/securityOperators/DefenderCSPMSecurityOperator'
    roles: [
      'Microsoft.Security/pricings/microsoft-defender-operator'
    ]
  }
}

resource managedClusters_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_defender_cloudposture 'Microsoft.ContainerService/managedClusters/trustedAccessRoleBindings@2023-01-02-preview' = {
  parent: managedClusters_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_resource
  name: 'defender-cloudposture'
  properties: {
    sourceResourceId: '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/providers/Microsoft.Security/pricings/CloudPosture/securityOperators/DefenderCSPMSecurityOperator'
    roles: [
      'Microsoft.Security/pricings/microsoft-defender-operator'
    ]
  }
}

resource dataCollectionRules_MSProm_eastus_EUS_FLLM_DEMO_APP_BACKEND_aks_name_resource 'Microsoft.Insights/dataCollectionRules@2022-06-01' = {
  name: dataCollectionRules_MSProm_eastus_EUS_FLLM_DEMO_APP_BACKEND_aks_name
  location: 'eastus'
  kind: 'Linux'
  properties: {
    description: 'DCR for Azure Monitor Metrics Profile (Managed Prometheus)'
    dataCollectionEndpointId: dataCollectionEndpoints_MSProm_eastus_eus_fllm_demo_app_backend_aks_name_resource.id
    dataSources: {
      prometheusForwarder: [
        {
          streams: [
            'Microsoft-PrometheusMetrics'
          ]
          labelIncludeFilter: {}
          name: 'PrometheusDataSource'
        }
      ]
    }
    destinations: {
      monitoringAccounts: [
        {
          accountResourceId: accounts_eus_fllm_demo_ops_amw_externalid
          name: 'MonitoringAccount1'
        }
      ]
    }
    dataFlows: [
      {
        streams: [
          'Microsoft-PrometheusMetrics'
        ]
        destinations: [
          'MonitoringAccount1'
        ]
      }
    ]
  }
}

resource dataCollectionRules_MSProm_eastus_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_resource 'Microsoft.Insights/dataCollectionRules@2022-06-01' = {
  name: dataCollectionRules_MSProm_eastus_EUS_FLLM_DEMO_APP_FRONTEND_aks_name
  location: 'eastus'
  kind: 'Linux'
  properties: {
    description: 'DCR for Azure Monitor Metrics Profile (Managed Prometheus)'
    dataCollectionEndpointId: dataCollectionEndpoints_MSProm_eastus_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_resource.id
    dataSources: {
      prometheusForwarder: [
        {
          streams: [
            'Microsoft-PrometheusMetrics'
          ]
          name: 'PrometheusDataSource'
        }
      ]
    }
    destinations: {
      monitoringAccounts: [
        {
          accountResourceId: accounts_eus_fllm_demo_ops_amw_externalid
          name: 'MonitoringAccount1'
        }
      ]
    }
    dataFlows: [
      {
        streams: [
          'Microsoft-PrometheusMetrics'
        ]
        destinations: [
          'MonitoringAccount1'
        ]
      }
    ]
  }
}

resource metricAlerts_CPU_Usage_Percentage_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_CPU_Usage_Percentage_EUS_FLLM_DEMO_APP_FRONTEND_aks_name
  location: 'global'
  properties: {
    severity: 3
    enabled: true
    scopes: [
      managedClusters_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_resource.id
    ]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          threshold: 95
          name: 'Metric1'
          metricName: 'node_cpu_usage_percentage'
          operator: 'GreaterThan'
          timeAggregation: 'Average'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
      'odata.type': 'Microsoft.Azure.Monitor.MultipleResourceMultipleMetricCriteria'
    }
    actions: [
      {
        actionGroupId: actiongroups_EUS_FLLM_DEMO_OPS_ag_externalid
      }
    ]
  }
}

resource metricAlerts_EUS_FLLM_DEMO_APP_BACKEND_aks_cpu_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_APP_BACKEND_aks_cpu_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Application'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    description: 'Node CPU utilization greater than 95% for 1 hour'
    severity: 3
    enabled: true
    scopes: [
      managedClusters_EUS_FLLM_DEMO_APP_BACKEND_aks_name_resource.id
    ]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          threshold: 95
          name: 'Metric1'
          metricNamespace: 'Microsoft.ContainerService/managedClusters'
          metricName: 'node_cpu_usage_percentage'
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
        actionGroupId: actiongroups_EUS_FLLM_DEMO_OPS_ag_externalid
        webHookProperties: {}
      }
    ]
  }
}

resource metricAlerts_EUS_FLLM_DEMO_APP_BACKEND_aks_mem_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_APP_BACKEND_aks_mem_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Application'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    description: 'Node memory utilization greater than 95% for 1 hour'
    severity: 3
    enabled: true
    scopes: [
      managedClusters_EUS_FLLM_DEMO_APP_BACKEND_aks_name_resource.id
    ]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          threshold: 100
          name: 'Metric1'
          metricNamespace: 'Microsoft.ContainerService/managedClusters'
          metricName: 'node_memory_working_set_percentage'
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
        actionGroupId: actiongroups_EUS_FLLM_DEMO_OPS_ag_externalid
        webHookProperties: {}
      }
    ]
  }
}

resource metricAlerts_EUS_FLLM_DEMO_APP_FRONTEND_aks_cpu_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_APP_FRONTEND_aks_cpu_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Application'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    description: 'Node CPU utilization greater than 95% for 1 hour'
    severity: 3
    enabled: true
    scopes: [
      managedClusters_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_resource.id
    ]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          threshold: 95
          name: 'Metric1'
          metricNamespace: 'Microsoft.ContainerService/managedClusters'
          metricName: 'node_cpu_usage_percentage'
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
        actionGroupId: actiongroups_EUS_FLLM_DEMO_OPS_ag_externalid
        webHookProperties: {}
      }
    ]
  }
}

resource metricAlerts_EUS_FLLM_DEMO_APP_FRONTEND_aks_mem_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_APP_FRONTEND_aks_mem_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Application'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    description: 'Node memory utilization greater than 95% for 1 hour'
    severity: 3
    enabled: true
    scopes: [
      managedClusters_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_resource.id
    ]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          threshold: 100
          name: 'Metric1'
          metricNamespace: 'Microsoft.ContainerService/managedClusters'
          metricName: 'node_memory_working_set_percentage'
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
        actionGroupId: actiongroups_EUS_FLLM_DEMO_OPS_ag_externalid
        webHookProperties: {}
      }
    ]
  }
}

resource metricAlerts_Memory_Working_Set_Percentage_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_Memory_Working_Set_Percentage_EUS_FLLM_DEMO_APP_FRONTEND_aks_name
  location: 'global'
  properties: {
    severity: 3
    enabled: true
    scopes: [
      managedClusters_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_resource.id
    ]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          threshold: 100
          name: 'Metric1'
          metricName: 'node_memory_working_set_percentage'
          operator: 'GreaterThan'
          timeAggregation: 'Average'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
      'odata.type': 'Microsoft.Azure.Monitor.MultipleResourceMultipleMetricCriteria'
    }
    actions: [
      {
        actionGroupId: actiongroups_EUS_FLLM_DEMO_OPS_ag_externalid
      }
    ]
  }
}

resource userAssignedIdentities_EUS_FLLM_DEMO_APP_agent_factory_api_mi_name_agent_factory_api 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = {
  parent: userAssignedIdentities_EUS_FLLM_DEMO_APP_agent_factory_api_mi_name_resource
  name: 'agent-factory-api'
  properties: {
    issuer: 'https://eastus.oic.prod-aks.azure.com/22179471-b099-4504-bfdb-3f184cdae122/436fd75b-0b60-44da-a37a-38988d378a47/'
    subject: 'system:serviceaccount:default:agent-factory-api'
    audiences: [
      'api://AzureADTokenExchange'
    ]
  }
}

resource userAssignedIdentities_EUS_FLLM_DEMO_APP_agent_hub_api_mi_name_agent_hub_api 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = {
  parent: userAssignedIdentities_EUS_FLLM_DEMO_APP_agent_hub_api_mi_name_resource
  name: 'agent-hub-api'
  properties: {
    issuer: 'https://eastus.oic.prod-aks.azure.com/22179471-b099-4504-bfdb-3f184cdae122/436fd75b-0b60-44da-a37a-38988d378a47/'
    subject: 'system:serviceaccount:default:agent-hub-api'
    audiences: [
      'api://AzureADTokenExchange'
    ]
  }
}

resource userAssignedIdentities_EUS_FLLM_DEMO_APP_chat_ui_mi_name_chat_ui 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = {
  parent: userAssignedIdentities_EUS_FLLM_DEMO_APP_chat_ui_mi_name_resource
  name: 'chat-ui'
  properties: {
    issuer: 'https://eastus.oic.prod-aks.azure.com/22179471-b099-4504-bfdb-3f184cdae122/436fd75b-0b60-44da-a37a-38988d378a47/'
    subject: 'system:serviceaccount:default:chat-ui'
    audiences: [
      'api://AzureADTokenExchange'
    ]
  }
}

resource userAssignedIdentities_EUS_FLLM_DEMO_APP_core_api_mi_name_core_api 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = {
  parent: userAssignedIdentities_EUS_FLLM_DEMO_APP_core_api_mi_name_resource
  name: 'core-api'
  properties: {
    issuer: 'https://eastus.oic.prod-aks.azure.com/22179471-b099-4504-bfdb-3f184cdae122/436fd75b-0b60-44da-a37a-38988d378a47/'
    subject: 'system:serviceaccount:default:core-api'
    audiences: [
      'api://AzureADTokenExchange'
    ]
  }
}

resource userAssignedIdentities_EUS_FLLM_DEMO_APP_data_source_hub_api_mi_name_data_source_hub_api 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = {
  parent: userAssignedIdentities_EUS_FLLM_DEMO_APP_data_source_hub_api_mi_name_resource
  name: 'data-source-hub-api'
  properties: {
    issuer: 'https://eastus.oic.prod-aks.azure.com/22179471-b099-4504-bfdb-3f184cdae122/436fd75b-0b60-44da-a37a-38988d378a47/'
    subject: 'system:serviceaccount:default:data-source-hub-api'
    audiences: [
      'api://AzureADTokenExchange'
    ]
  }
}

resource userAssignedIdentities_EUS_FLLM_DEMO_APP_gatekeeper_api_mi_name_gatekeeper_api 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = {
  parent: userAssignedIdentities_EUS_FLLM_DEMO_APP_gatekeeper_api_mi_name_resource
  name: 'gatekeeper-api'
  properties: {
    issuer: 'https://eastus.oic.prod-aks.azure.com/22179471-b099-4504-bfdb-3f184cdae122/436fd75b-0b60-44da-a37a-38988d378a47/'
    subject: 'system:serviceaccount:default:gatekeeper-api'
    audiences: [
      'api://AzureADTokenExchange'
    ]
  }
}

resource userAssignedIdentities_EUS_FLLM_DEMO_APP_langchain_api_mi_name_langchain_api 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = {
  parent: userAssignedIdentities_EUS_FLLM_DEMO_APP_langchain_api_mi_name_resource
  name: 'langchain-api'
  properties: {
    issuer: 'https://eastus.oic.prod-aks.azure.com/22179471-b099-4504-bfdb-3f184cdae122/436fd75b-0b60-44da-a37a-38988d378a47/'
    subject: 'system:serviceaccount:default:langchain-api'
    audiences: [
      'api://AzureADTokenExchange'
    ]
  }
}

resource userAssignedIdentities_EUS_FLLM_DEMO_APP_prompt_hub_api_mi_name_prompt_hub_api 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = {
  parent: userAssignedIdentities_EUS_FLLM_DEMO_APP_prompt_hub_api_mi_name_resource
  name: 'prompt-hub-api'
  properties: {
    issuer: 'https://eastus.oic.prod-aks.azure.com/22179471-b099-4504-bfdb-3f184cdae122/436fd75b-0b60-44da-a37a-38988d378a47/'
    subject: 'system:serviceaccount:default:prompt-hub-api'
    audiences: [
      'api://AzureADTokenExchange'
    ]
  }
}

resource userAssignedIdentities_EUS_FLLM_DEMO_APP_semantic_kernel_api_mi_name_semantic_kernel_api 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = {
  parent: userAssignedIdentities_EUS_FLLM_DEMO_APP_semantic_kernel_api_mi_name_resource
  name: 'semantic-kernel-api'
  properties: {
    issuer: 'https://eastus.oic.prod-aks.azure.com/22179471-b099-4504-bfdb-3f184cdae122/436fd75b-0b60-44da-a37a-38988d378a47/'
    subject: 'system:serviceaccount:default:semantic-kernel-api'
    audiences: [
      'api://AzureADTokenExchange'
    ]
  }
}

resource privateEndpoints_EUS_FLLM_DEMO_APP_BACKEND_aks_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_APP_BACKEND_aks_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Application'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'EUS-FLLM-DEMO-APP-BACKEND-aks-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_APP_BACKEND_aks_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-APP-BACKEND-aks-connection'
        properties: {
          privateLinkServiceId: managedClusters_EUS_FLLM_DEMO_APP_BACKEND_aks_name_resource.id
          groupIds: [
            'management'
          ]
          privateLinkServiceConnectionState: {
            status: 'Approved'
            description: 'Auto Approved'
            actionsRequired: 'None'
          }
        }
      }
    ]
    manualPrivateLinkServiceConnections: []
    subnet: {
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/FLLMServices'
    }
    ipConfigurations: []
    customDnsConfigs: []
  }
}

resource privateEndpoints_EUS_FLLM_DEMO_APP_FRONTEND_aks_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_APP_FRONTEND_aks_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Application'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'EUS-FLLM-DEMO-APP-FRONTEND-aks-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_APP_FRONTEND_aks_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-APP-FRONTEND-aks-connection'
        properties: {
          privateLinkServiceId: managedClusters_EUS_FLLM_DEMO_APP_FRONTEND_aks_name_resource.id
          groupIds: [
            'management'
          ]
          privateLinkServiceConnectionState: {
            status: 'Approved'
            description: 'Auto Approved'
            actionsRequired: 'None'
          }
        }
      }
    ]
    manualPrivateLinkServiceConnections: []
    subnet: {
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/FLLMServices'
    }
    ipConfigurations: []
    customDnsConfigs: []
  }
}

resource privateEndpoints_EUS_FLLM_DEMO_APP_BACKEND_aks_pe_name_aks 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_EUS_FLLM_DEMO_APP_BACKEND_aks_pe_name}/aks'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink.eastus.azmk8s.io'
        properties: {
          privateDnsZoneId: privateDnsZones_privatelink_eastus_azmk8s_io_externalid
        }
      }
    ]
  }
  dependsOn: [
    privateEndpoints_EUS_FLLM_DEMO_APP_BACKEND_aks_pe_name_resource
  ]
}

resource privateEndpoints_EUS_FLLM_DEMO_APP_FRONTEND_aks_pe_name_aks 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_EUS_FLLM_DEMO_APP_FRONTEND_aks_pe_name}/aks'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink.eastus.azmk8s.io'
        properties: {
          privateDnsZoneId: privateDnsZones_privatelink_eastus_azmk8s_io_externalid
        }
      }
    ]
  }
  dependsOn: [
    privateEndpoints_EUS_FLLM_DEMO_APP_FRONTEND_aks_pe_name_resource
  ]
}
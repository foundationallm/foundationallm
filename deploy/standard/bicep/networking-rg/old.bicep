param bastionHosts_EUS_FLLM_DEMO_NET_bh_name string = 'EUS-FLLM-DEMO-NET-bh'
param virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name string = 'EUS-FLLM-DEMO-NET-vnet'
param publicIPAddresses_EUS_FLLM_DEMO_NET_pip_name string = 'EUS-FLLM-DEMO-NET-pip'
param networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name string = 'EUS-FLLM-DEMO-NET-ado-nsg'
param networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name string = 'EUS-FLLM-DEMO-NET-ops-nsg'
param networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name string = 'EUS-FLLM-DEMO-NET-tfc-nsg'
param networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name string = 'EUS-FLLM-DEMO-NET-jumpbox-nsg'
param networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name string = 'EUS-FLLM-DEMO-NET-AppGateway-nsg'
param networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name string = 'EUS-FLLM-DEMO-NET-FLLMOpenAI-nsg'
param networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name string = 'EUS-FLLM-DEMO-NET-Datasources-nsg'
param networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMBackend_nsg_name string = 'EUS-FLLM-DEMO-NET-FLLMBackend-nsg'
param networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name string = 'EUS-FLLM-DEMO-NET-FLLMStorage-nsg'
param networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMFrontEnd_nsg_name string = 'EUS-FLLM-DEMO-NET-FLLMFrontEnd-nsg'
param networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name string = 'EUS-FLLM-DEMO-NET-FLLMServices-nsg'
param networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name string = 'EUS-FLLM-DEMO-NET-Vectorization-nsg'
param networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name string = 'EUS-FLLM-DEMO-NET-AzureBastionSubnet-nsg'
param applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-AGW-rg/providers/Microsoft.Network/applicationGateways/EUS-FLLM-DEMO-AGW-www-agw'
param applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-AGW-rg/providers/Microsoft.Network/applicationGateways/EUS-FLLM-DEMO-AGW-api-agw'




resource networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_allow_ado_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name}/allow-ado-agents-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.160/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4094
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_allow_ado_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name}/allow-ado-agents-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.160/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4094
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_ado_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name}/allow-ado-agents-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.160/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4094
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_allow_ado_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name}/allow-ado-agents-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.160/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4094
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMBackend_nsg_name_allow_ado_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMBackend_nsg_name}/allow-ado-agents-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.160/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4094
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMBackend_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMFrontEnd_nsg_name_allow_ado_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMFrontEnd_nsg_name}/allow-ado-agents-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.160/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4094
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMFrontEnd_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_ado_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name}/allow-ado-agents-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.160/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4094
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name_allow_ado_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name}/allow-ado-agents-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.160/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4094
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_allow_ado_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name}/allow-ado-agents-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.160/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4094
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_allow_ado_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name}/allow-ado-agents-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.160/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4094
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_allow_ado_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name}/allow-ado-agents-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.160/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4094
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_ado_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name}/allow-ado-agents-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.160/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4094
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_allow_ado_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name}/allow-ado-agents-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.160/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4094
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_allow_ado_services 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name}/allow-ado-services'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: 'Internet'
    access: 'Allow'
    priority: 256
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_allow_aks_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name}/allow-aks-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 256
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: [
      '10.0.16.0/22'
    ]
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_aks_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name}/allow-aks-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 256
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: [
      '10.0.16.0/22'
    ]
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_allow_aks_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name}/allow-aks-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 256
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: [
      '10.0.16.0/22'
    ]
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_allow_aks_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name}/allow-aks-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 264
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: [
      '10.0.16.0/22'
      '10.0.12.0/22'
    ]
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_allow_aks_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name}/allow-aks-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 256
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: [
      '10.0.16.0/22'
    ]
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_apim 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name}/allow-apim'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '3443'
    sourceAddressPrefix: 'ApiManagement'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 128
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_apim_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name}/allow-apim-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '443'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 320
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: [
      '10.0.5.0/24'
    ]
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_azure_cloud_communication_outbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name}/allow-azure-cloud-communication-outbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '443'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: 'AzureCloud'
    access: 'Allow'
    priority: 136
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_bastion_host_communicaiton_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name}/allow-bastion-host-communicaiton-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    sourceAddressPrefix: 'VirtualNetwork'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 152
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: [
      '5701'
      '8080'
    ]
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_bastion_host_communication_outbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name}/allow-bastion-host-communication-outbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    sourceAddressPrefix: 'VirtualNetwork'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 144
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: [
      '5701'
      '8080'
    ]
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_dns 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name}/allow-dns'
  properties: {
    protocol: 'Udp'
    sourcePortRange: '*'
    destinationPortRange: '53'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: 'Internet'
    access: 'Allow'
    priority: 288
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_allow_gatewaymanager_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name}/allow-gatewaymanager-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '65200-65535'
    sourceAddressPrefix: 'GatewayManager'
    destinationAddressPrefix: '*'
    access: 'Allow'
    priority: 148
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_gateway_manager_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name}/allow-gateway-manager-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '443'
    sourceAddressPrefix: 'GatewayManager'
    destinationAddressPrefix: '*'
    access: 'Allow'
    priority: 136
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_get_session_information_outbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name}/allow-get-session-information-outbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: 'Internet'
    access: 'Allow'
    priority: 152
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: [
      '80'
      '443'
    ]
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_https_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name}/allow-https-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '443'
    sourceAddressPrefix: 'Internet'
    destinationAddressPrefix: '*'
    access: 'Allow'
    priority: 128
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_allow_internet_http_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name}/allow-internet-http-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '80'
    sourceAddressPrefix: 'Internet'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 128
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_allow_internet_https_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name}/allow-internet-https-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '443'
    sourceAddressPrefix: 'Internet'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 132
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_allow_internet_outbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name}/allow-internet-outbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: 'Internet'
    access: 'Allow'
    priority: 256
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_allow_jumpbox_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name}/allow-jumpbox-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.32/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4093
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_allow_jumpbox_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name}/allow-jumpbox-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.32/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4093
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_jumpbox_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name}/allow-jumpbox-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.32/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4093
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_allow_jumpbox_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name}/allow-jumpbox-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.32/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4093
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMBackend_nsg_name_allow_jumpbox_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMBackend_nsg_name}/allow-jumpbox-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.32/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4093
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMBackend_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMFrontEnd_nsg_name_allow_jumpbox_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMFrontEnd_nsg_name}/allow-jumpbox-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.32/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4093
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMFrontEnd_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_jumpbox_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name}/allow-jumpbox-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.32/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4093
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name_allow_jumpbox_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name}/allow-jumpbox-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.32/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4093
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_allow_jumpbox_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name}/allow-jumpbox-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.32/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4093
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_allow_jumpbox_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name}/allow-jumpbox-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.32/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4093
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_allow_jumpbox_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name}/allow-jumpbox-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.32/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4093
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_jumpbox_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name}/allow-jumpbox-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.32/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4093
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_allow_jumpbox_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name}/allow-jumpbox-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.32/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4093
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_kv 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name}/allow-kv'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '443'
    sourceAddressPrefix: 'VirtualNetwork'
    destinationAddressPrefix: 'AzureKeyVault'
    access: 'Allow'
    priority: 224
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_lb 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name}/allow-lb'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '6390'
    sourceAddressPrefix: 'AzureLoadBalancer'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 192
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_allow_loadbalancer_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name}/allow-loadbalancer-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: 'AzureLoadBalancer'
    destinationAddressPrefix: '*'
    access: 'Allow'
    priority: 164
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_load_balancer_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name}/allow-load-balancer-inbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '443'
    sourceAddressPrefix: 'AzureLoadBalancer'
    destinationAddressPrefix: '*'
    access: 'Allow'
    priority: 144
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_allow_ntp 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name}/allow-ntp'
  properties: {
    protocol: 'Udp'
    sourcePortRange: '*'
    destinationPortRange: '123'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: 'Internet'
    access: 'Allow'
    priority: 257
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_allow_rdp 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name}/allow-rdp'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '3389'
    sourceAddressPrefix: 'VirtualNetwork'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 128
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_allow_rdp_services 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name}/allow-rdp-services'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '3389'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 256
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_allow_rdp_services 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name}/allow-rdp-services'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '*'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 256
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: [
      '10.0.0.0/24'
    ]
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_sql 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name}/allow-sql'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '1443'
    sourceAddressPrefix: 'VirtualNetwork'
    destinationAddressPrefix: 'SQL'
    access: 'Allow'
    priority: 192
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_ssh_rdp_outbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name}/allow-ssh-rdp-outbound'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 128
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: [
      '22'
      '3389'
    ]
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_storage 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name}/allow-storage'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '443'
    sourceAddressPrefix: 'VirtualNetwork'
    destinationAddressPrefix: 'Storage'
    access: 'Allow'
    priority: 128
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_allow_tfc_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name}/allow-tfc-agents-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.224/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4095
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_allow_tfc_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name}/allow-tfc-agents-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.224/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4095
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_tfc_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name}/allow-tfc-agents-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.224/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4095
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_allow_tfc_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name}/allow-tfc-agents-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.224/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4095
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMBackend_nsg_name_allow_tfc_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMBackend_nsg_name}/allow-tfc-agents-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.224/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4095
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMBackend_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMFrontEnd_nsg_name_allow_tfc_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMFrontEnd_nsg_name}/allow-tfc-agents-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.224/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4095
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMFrontEnd_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_tfc_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name}/allow-tfc-agents-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.224/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4095
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name_allow_tfc_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name}/allow-tfc-agents-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.224/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4095
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_allow_tfc_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name}/allow-tfc-agents-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.224/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4095
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_allow_tfc_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name}/allow-tfc-agents-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.224/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4095
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_allow_tfc_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name}/allow-tfc-agents-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.224/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4095
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_tfc_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name}/allow-tfc-agents-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.224/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4095
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_allow_tfc_agents_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name}/allow-tfc-agents-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '10.0.255.224/27'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4095
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_tfc_api 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name}/allow-tfc-api'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '443'
    sourceAddressPrefix: '*'
    access: 'Allow'
    priority: 128
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: [
      '75.2.98.97/32'
      '99.83.150.238/32'
    ]
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_tfc_notifications 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name}/allow-tfc-notifications'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '443'
    sourceAddressPrefix: '*'
    access: 'Allow'
    priority: 160
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: [
      '54.185.161.84/32'
      '44.236.246.186/32'
      '52.86.200.106/32'
      '52.70.186.109/32'
      '44.238.78.236/32'
      '52.86.201.227/32'
    ]
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_tfc_sentinel 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name}/allow-tfc-sentinel'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '443'
    sourceAddressPrefix: '*'
    access: 'Allow'
    priority: 192
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: [
      '54.185.161.84/32'
      '44.236.246.186/32'
      '52.86.200.106/32'
      '52.70.186.109/32'
      '44.238.78.236/32'
      '52.86.201.227/32'
    ]
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_tfc_services 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name}/allow-tfc-services'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '443'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: 'Internet'
    access: 'Allow'
    priority: 256
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_tfc_vcs 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name}/allow-tfc-vcs'
  properties: {
    protocol: 'Tcp'
    sourcePortRange: '*'
    destinationPortRange: '443'
    sourceAddressPrefix: '*'
    access: 'Allow'
    priority: 224
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: [
      '54.185.161.84/32'
      '44.236.246.186/32'
      '52.86.200.106/32'
      '52.70.186.109/32'
      '44.238.78.236/32'
      '52.86.201.227/32'
    ]
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_allow_vnet 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name}/allow-vnet'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: 'VirtualNetwork'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4068
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_vnet 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name}/allow-vnet'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: 'VirtualNetwork'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4068
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_vnet 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name}/allow-vnet'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: 'VirtualNetwork'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 4068
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_allow_vnet_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name}/allow-vnet-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: 'VirtualNetwork'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 192
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_allow_vnet_outbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name}/allow-vnet-outbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: 'VirtualNetwork'
    destinationAddressPrefix: 'VirtualNetwork'
    access: 'Allow'
    priority: 128
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_deny_all_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name}/deny-all-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: '*'
    access: 'Deny'
    priority: 4096
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_deny_all_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name}/deny-all-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: '*'
    access: 'Deny'
    priority: 4096
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_deny_all_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name}/deny-all-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: '*'
    access: 'Deny'
    priority: 4096
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_deny_all_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name}/deny-all-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: '*'
    access: 'Deny'
    priority: 4096
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_deny_all_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name}/deny-all-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: '*'
    access: 'Deny'
    priority: 4096
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name_deny_all_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name}/deny-all-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: '*'
    access: 'Deny'
    priority: 4096
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_deny_all_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name}/deny-all-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: '*'
    access: 'Deny'
    priority: 4096
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_deny_all_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name}/deny-all-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: '*'
    access: 'Deny'
    priority: 4096
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_deny_all_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name}/deny-all-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: '*'
    access: 'Deny'
    priority: 4096
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_deny_all_inbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name}/deny-all-inbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: '*'
    access: 'Deny'
    priority: 4096
    direction: 'Inbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_deny_all_outbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name}/deny-all-outbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: '*'
    access: 'Deny'
    priority: 4096
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_deny_all_outbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name}/deny-all-outbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: '*'
    access: 'Deny'
    priority: 4096
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_deny_all_outbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name}/deny-all-outbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: '*'
    access: 'Deny'
    priority: 4096
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_deny_all_outbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name}/deny-all-outbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: '*'
    access: 'Deny'
    priority: 4096
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_deny_all_outbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name}/deny-all-outbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: '*'
    access: 'Deny'
    priority: 4096
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_resource
  ]
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_deny_all_outbound 'Microsoft.Network/networkSecurityGroups/securityRules@2023-05-01' = {
  name: '${networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name}/deny-all-outbound'
  properties: {
    protocol: '*'
    sourcePortRange: '*'
    destinationPortRange: '*'
    sourceAddressPrefix: '*'
    destinationAddressPrefix: '*'
    access: 'Deny'
    priority: 4096
    direction: 'Outbound'
    sourcePortRanges: []
    destinationPortRanges: []
    sourceAddressPrefixes: []
    destinationAddressPrefixes: []
  }
  dependsOn: [
    networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_resource
  ]
}


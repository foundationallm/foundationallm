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

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
  name: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    securityRules: [
      {
        name: 'allow-ntp'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_allow_ntp.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-ado-services'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_allow_ado_services.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-ado-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_allow_ado_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-rdp-services'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_allow_rdp_services.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-tfc-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_allow_tfc_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'deny-all-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_deny_all_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-vnet'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_allow_vnet.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'deny-all-outbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_deny_all_outbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-jumpbox-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_allow_jumpbox_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
    ]
  }
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
  name: networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    securityRules: [
      {
        name: 'allow-internet-http-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_allow_internet_http_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-tfc-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_allow_tfc_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'deny-all-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_deny_all_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-ado-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_allow_ado_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-internet-https-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_allow_internet_https_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-gatewaymanager-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_allow_gatewaymanager_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-loadbalancer-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_allow_loadbalancer_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-jumpbox-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_allow_jumpbox_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
    ]
  }
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
  name: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    securityRules: [
      {
        name: 'deny-all-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_deny_all_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-load-balancer-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_load_balancer_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-ssh-rdp-outbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_ssh_rdp_outbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-tfc-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_tfc_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-ado-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_ado_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-bastion-host-communicaiton-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_bastion_host_communicaiton_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-get-session-information-outbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_get_session_information_outbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-https-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_https_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'deny-all-outbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_deny_all_outbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-gateway-manager-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_gateway_manager_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-azure-cloud-communication-outbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_azure_cloud_communication_outbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-bastion-host-communication-outbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_bastion_host_communication_outbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-jumpbox-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_jumpbox_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
    ]
  }
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
  name: networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    securityRules: [
      {
        name: 'allow-tfc-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_allow_tfc_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'deny-all-outbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_deny_all_outbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-ado-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_allow_ado_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'deny-all-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_deny_all_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-jumpbox-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_allow_jumpbox_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-aks-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_allow_aks_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
    ]
  }
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMBackend_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
  name: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMBackend_nsg_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    securityRules: [
      {
        name: 'allow-jumpbox-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMBackend_nsg_name_allow_jumpbox_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-tfc-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMBackend_nsg_name_allow_tfc_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-ado-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMBackend_nsg_name_allow_ado_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
    ]
  }
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMFrontEnd_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
  name: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMFrontEnd_nsg_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    securityRules: [
      {
        name: 'allow-ado-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMFrontEnd_nsg_name_allow_ado_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-jumpbox-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMFrontEnd_nsg_name_allow_jumpbox_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-tfc-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMFrontEnd_nsg_name_allow_tfc_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
    ]
  }
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
  name: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    securityRules: [
      {
        name: 'allow-storage'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_storage.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-apim'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_apim.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-vnet'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_vnet.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-lb'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_lb.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-sql'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_sql.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-kv'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_kv.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-tfc-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_tfc_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-jumpbox-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_jumpbox_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'deny-all-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_deny_all_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-ado-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_ado_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-aks-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_aks_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-apim-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_apim_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
    ]
  }
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
  name: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    securityRules: [
      {
        name: 'allow-ado-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name_allow_ado_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-jumpbox-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name_allow_jumpbox_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'deny-all-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name_deny_all_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-tfc-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name_allow_tfc_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
    ]
  }
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
  name: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    securityRules: [
      {
        name: 'allow-ado-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_allow_ado_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'deny-all-outbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_deny_all_outbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'deny-all-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_deny_all_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-tfc-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_allow_tfc_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-jumpbox-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_allow_jumpbox_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-aks-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_allow_aks_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
    ]
  }
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
  name: networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    securityRules: [
      {
        name: 'allow-vnet-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_allow_vnet_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-rdp'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_allow_rdp.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-vnet-outbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_allow_vnet_outbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-jumpbox-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_allow_jumpbox_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-ado-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_allow_ado_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-tfc-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_allow_tfc_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-internet-outbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_allow_internet_outbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
    ]
  }
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
  name: networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    securityRules: [
      {
        name: 'allow-tfc-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_allow_tfc_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-ado-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_allow_ado_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-rdp-services'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_allow_rdp_services.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-jumpbox-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_allow_jumpbox_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-aks-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_allow_aks_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'deny-all-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_deny_all_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
    ]
  }
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
  name: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    securityRules: [
      {
        name: 'allow-tfc-sentinel'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_tfc_sentinel.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-tfc-vcs'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_tfc_vcs.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-ado-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_ado_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-vnet'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_vnet.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-tfc-services'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_tfc_services.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'deny-all-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_deny_all_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'deny-all-outbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_deny_all_outbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-tfc-api'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_tfc_api.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-tfc-notifications'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_tfc_notifications.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-tfc-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_tfc_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-dns'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_dns.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-jumpbox-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_jumpbox_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
    ]
  }
}

resource networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
  name: networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    securityRules: [
      {
        name: 'allow-ado-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_allow_ado_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'deny-all-outbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_deny_all_outbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-tfc-agents-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_allow_tfc_agents_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'deny-all-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_deny_all_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-jumpbox-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_allow_jumpbox_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
      {
        name: 'allow-aks-inbound'
        id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_allow_aks_inbound.id
        type: 'Microsoft.Network/networkSecurityGroups/securityRules'
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
      }
    ]
  }
}

resource publicIPAddresses_EUS_FLLM_DEMO_NET_pip_name_resource 'Microsoft.Network/publicIPAddresses@2023-05-01' = {
  name: publicIPAddresses_EUS_FLLM_DEMO_NET_pip_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  sku: {
    name: 'Standard'
    tier: 'Regional'
  }
  properties: {
    ipAddress: '172.191.239.98'
    publicIPAddressVersion: 'IPv4'
    publicIPAllocationMethod: 'Static'
    idleTimeoutInMinutes: 4
    ipTags: []
    ddosSettings: {
      protectionMode: 'VirtualNetworkInherited'
    }
  }
}

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

resource bastionHosts_EUS_FLLM_DEMO_NET_bh_name_resource 'Microsoft.Network/bastionHosts@2023-05-01' = {
  name: bastionHosts_EUS_FLLM_DEMO_NET_bh_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  sku: {
    name: 'Standard'
  }
  properties: {
    dnsName: 'bst-90d9a46c-aa59-4bae-9bbb-2ed5f6fa681b.bastion.azure.com'
    scaleUnits: 2
    enableTunneling: false
    enableIpConnect: false
    enableFileCopy: true
    disableCopyPaste: false
    enableShareableLink: false
    ipConfigurations: [
      {
        name: 'default'
        id: '${bastionHosts_EUS_FLLM_DEMO_NET_bh_name_resource.id}/bastionHostIpConfigurations/default'
        properties: {
          privateIPAllocationMethod: 'Dynamic'
          publicIPAddress: {
            id: publicIPAddresses_EUS_FLLM_DEMO_NET_pip_name_resource.id
          }
          subnet: {
            id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_AzureBastionSubnet.id
          }
        }
      }
    ]
  }
}

resource virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_ado 'Microsoft.Network/virtualNetworks/subnets@2023-05-01' = {
  name: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name}/ado'
  properties: {
    addressPrefix: '10.0.255.160/27'
    networkSecurityGroup: {
      id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_resource.id
    }
    serviceEndpoints: []
    delegations: []
    privateEndpointNetworkPolicies: 'Enabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
  }
  dependsOn: [
    virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_resource

  ]
}

resource virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_AppGateway 'Microsoft.Network/virtualNetworks/subnets@2023-05-01' = {
  name: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name}/AppGateway'
  properties: {
    addressPrefix: '10.0.0.0/24'
    networkSecurityGroup: {
      id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_resource.id
    }
    applicationGatewayIPConfigurations: [
      {
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_externalid}/gatewayIPConfigurations/default'
      }
      {
        id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_externalid}/gatewayIPConfigurations/default'
      }
    ]
    serviceEndpoints: []
    delegations: []
    privateEndpointNetworkPolicies: 'Enabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
  }
  dependsOn: [
    virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_resource

  ]
}

resource virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_AzureBastionSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-05-01' = {
  name: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name}/AzureBastionSubnet'
  properties: {
    addressPrefix: '10.0.254.128/26'
    networkSecurityGroup: {
      id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_resource.id
    }
    serviceEndpoints: []
    delegations: []
    privateEndpointNetworkPolicies: 'Enabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
  }
  dependsOn: [
    virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_resource

  ]
}

resource virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_Datasources 'Microsoft.Network/virtualNetworks/subnets@2023-05-01' = {
  name: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name}/Datasources'
  properties: {
    addressPrefix: '10.0.2.0/24'
    networkSecurityGroup: {
      id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_resource.id
    }
    serviceEndpoints: []
    delegations: []
    privateEndpointNetworkPolicies: 'Enabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
  }
  dependsOn: [
    virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_resource

  ]
}

resource virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_FLLMBackend 'Microsoft.Network/virtualNetworks/subnets@2023-05-01' = {
  name: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name}/FLLMBackend'
  properties: {
    addressPrefix: '10.0.16.0/22'
    networkSecurityGroup: {
      id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMBackend_nsg_name_resource.id
    }
    serviceEndpoints: []
    delegations: []
    privateEndpointNetworkPolicies: 'Enabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
  }
  dependsOn: [
    virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_resource

  ]
}

resource virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_FLLMFrontEnd 'Microsoft.Network/virtualNetworks/subnets@2023-05-01' = {
  name: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name}/FLLMFrontEnd'
  properties: {
    addressPrefix: '10.0.12.0/22'
    networkSecurityGroup: {
      id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMFrontEnd_nsg_name_resource.id
    }
    serviceEndpoints: []
    delegations: []
    privateEndpointNetworkPolicies: 'Enabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
  }
  dependsOn: [
    virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_resource

  ]
}

resource virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_FLLMOpenAI 'Microsoft.Network/virtualNetworks/subnets@2023-05-01' = {
  name: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name}/FLLMOpenAI'
  properties: {
    addressPrefix: '10.0.5.0/24'
    networkSecurityGroup: {
      id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_resource.id
    }
    serviceEndpoints: [
      {
        service: 'Microsoft.CognitiveServices'
        locations: [
          '*'
        ]
      }
    ]
    delegations: []
    privateEndpointNetworkPolicies: 'Enabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
  }
  dependsOn: [
    virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_resource

  ]
}

resource virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_FLLMServices 'Microsoft.Network/virtualNetworks/subnets@2023-05-01' = {
  name: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name}/FLLMServices'
  properties: {
    addressPrefix: '10.0.3.0/24'
    networkSecurityGroup: {
      id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name_resource.id
    }
    serviceEndpoints: []
    delegations: []
    privateEndpointNetworkPolicies: 'Disabled'
    privateLinkServiceNetworkPolicies: 'Disabled'
  }
  dependsOn: [
    virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_resource

  ]
}

resource virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_FLLMStorage 'Microsoft.Network/virtualNetworks/subnets@2023-05-01' = {
  name: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name}/FLLMStorage'
  properties: {
    addressPrefix: '10.0.4.0/24'
    networkSecurityGroup: {
      id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_resource.id
    }
    serviceEndpoints: []
    delegations: []
    privateEndpointNetworkPolicies: 'Enabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
  }
  dependsOn: [
    virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_resource

  ]
}

resource virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_jumpbox 'Microsoft.Network/virtualNetworks/subnets@2023-05-01' = {
  name: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name}/jumpbox'
  properties: {
    addressPrefix: '10.0.255.32/27'
    networkSecurityGroup: {
      id: networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_resource.id
    }
    serviceEndpoints: []
    delegations: []
    privateEndpointNetworkPolicies: 'Enabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
  }
  dependsOn: [
    virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_resource

  ]
}

resource virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_ops 'Microsoft.Network/virtualNetworks/subnets@2023-05-01' = {
  name: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name}/ops'
  properties: {
    addressPrefix: '10.0.255.96/27'
    networkSecurityGroup: {
      id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_resource.id
    }
    serviceEndpoints: []
    delegations: []
    privateEndpointNetworkPolicies: 'Enabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
  }
  dependsOn: [
    virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_resource

  ]
}

resource virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_tfc 'Microsoft.Network/virtualNetworks/subnets@2023-05-01' = {
  name: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name}/tfc'
  properties: {
    addressPrefix: '10.0.255.224/27'
    networkSecurityGroup: {
      id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_resource.id
    }
    serviceEndpoints: []
    delegations: [
      {
        name: 'Microsoft.ContainerInstance/containerGroups-delegation'
        id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_tfc.id}/delegations/Microsoft.ContainerInstance/containerGroups-delegation'
        properties: {
          serviceName: 'Microsoft.ContainerInstance/containerGroups'
        }
        type: 'Microsoft.Network/virtualNetworks/subnets/delegations'
      }
    ]
    privateEndpointNetworkPolicies: 'Enabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
  }
  dependsOn: [
    virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_resource

  ]
}

resource virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_Vectorization 'Microsoft.Network/virtualNetworks/subnets@2023-05-01' = {
  name: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name}/Vectorization'
  properties: {
    addressPrefix: '10.0.6.0/24'
    networkSecurityGroup: {
      id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_resource.id
    }
    serviceEndpoints: []
    delegations: []
    privateEndpointNetworkPolicies: 'Enabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
  }
  dependsOn: [
    virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_resource

  ]
}

resource virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_resource 'Microsoft.Network/virtualNetworks@2023-05-01' = {
  name: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'Networking'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
    dhcpOptions: {
      dnsServers: []
    }
    subnets: [
      {
        name: 'ado'
        id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_ado.id
        properties: {
          addressPrefix: '10.0.255.160/27'
          networkSecurityGroup: {
            id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_resource.id
          }
          serviceEndpoints: []
          delegations: []
          privateEndpointNetworkPolicies: 'Enabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
        type: 'Microsoft.Network/virtualNetworks/subnets'
      }
      {
        name: 'tfc'
        id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_tfc.id
        properties: {
          addressPrefix: '10.0.255.224/27'
          networkSecurityGroup: {
            id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_resource.id
          }
          serviceEndpoints: []
          delegations: [
            {
              name: 'Microsoft.ContainerInstance/containerGroups-delegation'
              id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_tfc.id}/delegations/Microsoft.ContainerInstance/containerGroups-delegation'
              properties: {
                serviceName: 'Microsoft.ContainerInstance/containerGroups'
              }
              type: 'Microsoft.Network/virtualNetworks/subnets/delegations'
            }
          ]
          privateEndpointNetworkPolicies: 'Enabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
        type: 'Microsoft.Network/virtualNetworks/subnets'
      }
      {
        name: 'ops'
        id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_ops.id
        properties: {
          addressPrefix: '10.0.255.96/27'
          networkSecurityGroup: {
            id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_resource.id
          }
          serviceEndpoints: []
          delegations: []
          privateEndpointNetworkPolicies: 'Enabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
        type: 'Microsoft.Network/virtualNetworks/subnets'
      }
      {
        name: 'jumpbox'
        id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_jumpbox.id
        properties: {
          addressPrefix: '10.0.255.32/27'
          networkSecurityGroup: {
            id: networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_resource.id
          }
          serviceEndpoints: []
          delegations: []
          privateEndpointNetworkPolicies: 'Enabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
        type: 'Microsoft.Network/virtualNetworks/subnets'
      }
      {
        name: 'FLLMOpenAI'
        id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_FLLMOpenAI.id
        properties: {
          addressPrefix: '10.0.5.0/24'
          networkSecurityGroup: {
            id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_resource.id
          }
          serviceEndpoints: [
            {
              service: 'Microsoft.CognitiveServices'
              locations: [
                '*'
              ]
            }
          ]
          delegations: []
          privateEndpointNetworkPolicies: 'Enabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
        type: 'Microsoft.Network/virtualNetworks/subnets'
      }
      {
        name: 'Datasources'
        id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_Datasources.id
        properties: {
          addressPrefix: '10.0.2.0/24'
          networkSecurityGroup: {
            id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_resource.id
          }
          serviceEndpoints: []
          delegations: []
          privateEndpointNetworkPolicies: 'Enabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
        type: 'Microsoft.Network/virtualNetworks/subnets'
      }
      {
        name: 'Vectorization'
        id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_Vectorization.id
        properties: {
          addressPrefix: '10.0.6.0/24'
          networkSecurityGroup: {
            id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_resource.id
          }
          serviceEndpoints: []
          delegations: []
          privateEndpointNetworkPolicies: 'Enabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
        type: 'Microsoft.Network/virtualNetworks/subnets'
      }
      {
        name: 'FLLMStorage'
        id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_FLLMStorage.id
        properties: {
          addressPrefix: '10.0.4.0/24'
          networkSecurityGroup: {
            id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_resource.id
          }
          serviceEndpoints: []
          delegations: []
          privateEndpointNetworkPolicies: 'Enabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
        type: 'Microsoft.Network/virtualNetworks/subnets'
      }
      {
        name: 'AppGateway'
        id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_AppGateway.id
        properties: {
          addressPrefix: '10.0.0.0/24'
          networkSecurityGroup: {
            id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_resource.id
          }
          applicationGatewayIPConfigurations: [
            {
              id: '${applicationGateways_EUS_FLLM_DEMO_AGW_www_agw_externalid}/gatewayIPConfigurations/default'
            }
            {
              id: '${applicationGateways_EUS_FLLM_DEMO_AGW_api_agw_externalid}/gatewayIPConfigurations/default'
            }
          ]
          serviceEndpoints: []
          delegations: []
          privateEndpointNetworkPolicies: 'Enabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
        type: 'Microsoft.Network/virtualNetworks/subnets'
      }
      {
        name: 'AzureBastionSubnet'
        id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_AzureBastionSubnet.id
        properties: {
          addressPrefix: '10.0.254.128/26'
          networkSecurityGroup: {
            id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_resource.id
          }
          serviceEndpoints: []
          delegations: []
          privateEndpointNetworkPolicies: 'Enabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
        type: 'Microsoft.Network/virtualNetworks/subnets'
      }
      {
        name: 'FLLMServices'
        id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_FLLMServices.id
        properties: {
          addressPrefix: '10.0.3.0/24'
          networkSecurityGroup: {
            id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name_resource.id
          }
          serviceEndpoints: []
          delegations: []
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Disabled'
        }
        type: 'Microsoft.Network/virtualNetworks/subnets'
      }
      {
        name: 'FLLMBackend'
        id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_FLLMBackend.id
        properties: {
          addressPrefix: '10.0.16.0/22'
          networkSecurityGroup: {
            id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMBackend_nsg_name_resource.id
          }
          serviceEndpoints: []
          delegations: []
          privateEndpointNetworkPolicies: 'Enabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
        type: 'Microsoft.Network/virtualNetworks/subnets'
      }
      {
        name: 'FLLMFrontEnd'
        id: virtualNetworks_EUS_FLLM_DEMO_NET_vnet_name_FLLMFrontEnd.id
        properties: {
          addressPrefix: '10.0.12.0/22'
          networkSecurityGroup: {
            id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMFrontEnd_nsg_name_resource.id
          }
          serviceEndpoints: []
          delegations: []
          privateEndpointNetworkPolicies: 'Enabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
        type: 'Microsoft.Network/virtualNetworks/subnets'
      }
    ]
    virtualNetworkPeerings: []
    enableDdosProtection: false
  }
}
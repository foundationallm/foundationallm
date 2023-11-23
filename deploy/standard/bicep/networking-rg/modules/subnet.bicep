param addressPrefix string
param location string
param name string
param serviceEndpoints array = []
param tags object 
param vnetName string

resource main 'Microsoft.Network/virtualNetworks/subnets@2023-05-01' =  {
  name: '${vnetName}/${name}'

  properties: {
    addressPrefix: addressPrefix
    privateEndpointNetworkPolicies: 'Enabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
    serviceEndpoints: serviceEndpoints 

    networkSecurityGroup: {
      id: nsg.id
    }
  }
}

resource nsg 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
  location: location
  name: 'nsg-${vnetName}-${name}'
  tags: tags
}

// resource networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
//   name: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name
//   location: 'eastus'
//   tags: {
//     Environment: 'DEMO'
//     Project: 'FLLM'
//     Purpose: 'Networking'
//     Workspace: 'foundationallm-ops'
//   }
//   properties: {
//     securityRules: [
//       {
//         name: 'allow-ntp'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_allow_ntp.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Udp'
//           sourcePortRange: '*'
//           destinationPortRange: '123'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: 'Internet'
//           access: 'Allow'
//           priority: 257
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-ado-services'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_allow_ado_services.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: 'Internet'
//           access: 'Allow'
//           priority: 256
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-ado-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_allow_ado_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.160/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4094
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-rdp-services'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_allow_rdp_services.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '3389'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 256
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-tfc-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_allow_tfc_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.224/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4095
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'deny-all-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_deny_all_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: '*'
//           access: 'Deny'
//           priority: 4096
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-vnet'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_allow_vnet.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: 'VirtualNetwork'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4068
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'deny-all-outbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_deny_all_outbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: '*'
//           access: 'Deny'
//           priority: 4096
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-jumpbox-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_allow_jumpbox_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.32/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4093
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//     ]
//   }
// }

// resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
//   name: networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name
//   location: 'eastus'
//   tags: {
//     Environment: 'DEMO'
//     Project: 'FLLM'
//     Purpose: 'Networking'
//     Workspace: 'foundationallm-ops'
//   }
//   properties: {
//     securityRules: [
//       {
//         name: 'allow-internet-http-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_allow_internet_http_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '80'
//           sourceAddressPrefix: 'Internet'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 128
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-tfc-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_allow_tfc_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.224/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4095
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'deny-all-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_deny_all_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: '*'
//           access: 'Deny'
//           priority: 4096
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-ado-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_allow_ado_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.160/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4094
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-internet-https-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_allow_internet_https_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '443'
//           sourceAddressPrefix: 'Internet'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 132
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-gatewaymanager-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_allow_gatewaymanager_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '65200-65535'
//           sourceAddressPrefix: 'GatewayManager'
//           destinationAddressPrefix: '*'
//           access: 'Allow'
//           priority: 148
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-loadbalancer-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_allow_loadbalancer_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: 'AzureLoadBalancer'
//           destinationAddressPrefix: '*'
//           access: 'Allow'
//           priority: 164
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-jumpbox-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AppGateway_nsg_name_allow_jumpbox_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.32/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4093
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//     ]
//   }
// }

// resource networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
//   name: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name
//   location: 'eastus'
//   tags: {
//     Environment: 'DEMO'
//     Project: 'FLLM'
//     Purpose: 'Networking'
//     Workspace: 'foundationallm-ops'
//   }
//   properties: {
//     securityRules: [
//       {
//         name: 'deny-all-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_deny_all_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: '*'
//           access: 'Deny'
//           priority: 4096
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-load-balancer-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_load_balancer_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '443'
//           sourceAddressPrefix: 'AzureLoadBalancer'
//           destinationAddressPrefix: '*'
//           access: 'Allow'
//           priority: 144
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-ssh-rdp-outbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_ssh_rdp_outbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 128
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: [
//             '22'
//             '3389'
//           ]
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-tfc-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_tfc_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.224/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4095
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-ado-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_ado_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.160/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4094
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-bastion-host-communicaiton-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_bastion_host_communicaiton_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           sourceAddressPrefix: 'VirtualNetwork'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 152
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: [
//             '5701'
//             '8080'
//           ]
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-get-session-information-outbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_get_session_information_outbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: 'Internet'
//           access: 'Allow'
//           priority: 152
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: [
//             '80'
//             '443'
//           ]
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-https-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_https_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '443'
//           sourceAddressPrefix: 'Internet'
//           destinationAddressPrefix: '*'
//           access: 'Allow'
//           priority: 128
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'deny-all-outbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_deny_all_outbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: '*'
//           access: 'Deny'
//           priority: 4096
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-gateway-manager-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_gateway_manager_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '443'
//           sourceAddressPrefix: 'GatewayManager'
//           destinationAddressPrefix: '*'
//           access: 'Allow'
//           priority: 136
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-azure-cloud-communication-outbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_azure_cloud_communication_outbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '443'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: 'AzureCloud'
//           access: 'Allow'
//           priority: 136
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-bastion-host-communication-outbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_bastion_host_communication_outbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           sourceAddressPrefix: 'VirtualNetwork'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 144
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: [
//             '5701'
//             '8080'
//           ]
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-jumpbox-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_AzureBastionSubnet_nsg_name_allow_jumpbox_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.32/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4093
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//     ]
//   }
// }

// resource networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
//   name: networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name
//   location: 'eastus'
//   tags: {
//     Environment: 'DEMO'
//     Project: 'FLLM'
//     Purpose: 'Networking'
//     Workspace: 'foundationallm-ops'
//   }
//   properties: {
//     securityRules: [
//       {
//         name: 'allow-tfc-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_allow_tfc_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.224/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4095
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'deny-all-outbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_deny_all_outbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: '*'
//           access: 'Deny'
//           priority: 4096
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-ado-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_allow_ado_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.160/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4094
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'deny-all-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_deny_all_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: '*'
//           access: 'Deny'
//           priority: 4096
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-jumpbox-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_allow_jumpbox_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.32/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4093
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-aks-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Datasources_nsg_name_allow_aks_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 256
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: [
//             '10.0.16.0/22'
//           ]
//           destinationAddressPrefixes: []
//         }
//       }
//     ]
//   }
// }

// resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMBackend_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
//   name: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMBackend_nsg_name
//   location: 'eastus'
//   tags: {
//     Environment: 'DEMO'
//     Project: 'FLLM'
//     Purpose: 'Networking'
//     Workspace: 'foundationallm-ops'
//   }
//   properties: {
//     securityRules: [
//       {
//         name: 'allow-jumpbox-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMBackend_nsg_name_allow_jumpbox_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.32/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4093
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-tfc-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMBackend_nsg_name_allow_tfc_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.224/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4095
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-ado-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMBackend_nsg_name_allow_ado_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.160/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4094
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//     ]
//   }
// }

// resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMFrontEnd_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
//   name: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMFrontEnd_nsg_name
//   location: 'eastus'
//   tags: {
//     Environment: 'DEMO'
//     Project: 'FLLM'
//     Purpose: 'Networking'
//     Workspace: 'foundationallm-ops'
//   }
//   properties: {
//     securityRules: [
//       {
//         name: 'allow-ado-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMFrontEnd_nsg_name_allow_ado_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.160/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4094
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-jumpbox-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMFrontEnd_nsg_name_allow_jumpbox_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.32/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4093
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-tfc-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMFrontEnd_nsg_name_allow_tfc_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.224/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4095
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//     ]
//   }
// }

// resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
//   name: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name
//   location: 'eastus'
//   tags: {
//     Environment: 'DEMO'
//     Project: 'FLLM'
//     Purpose: 'Networking'
//     Workspace: 'foundationallm-ops'
//   }
//   properties: {
//     securityRules: [
//       {
//         name: 'allow-storage'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_storage.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '443'
//           sourceAddressPrefix: 'VirtualNetwork'
//           destinationAddressPrefix: 'Storage'
//           access: 'Allow'
//           priority: 128
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-apim'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_apim.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '3443'
//           sourceAddressPrefix: 'ApiManagement'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 128
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-vnet'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_vnet.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: 'VirtualNetwork'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4068
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-lb'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_lb.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '6390'
//           sourceAddressPrefix: 'AzureLoadBalancer'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 192
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-sql'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_sql.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '1443'
//           sourceAddressPrefix: 'VirtualNetwork'
//           destinationAddressPrefix: 'SQL'
//           access: 'Allow'
//           priority: 192
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-kv'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_kv.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '443'
//           sourceAddressPrefix: 'VirtualNetwork'
//           destinationAddressPrefix: 'AzureKeyVault'
//           access: 'Allow'
//           priority: 224
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-tfc-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_tfc_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.224/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4095
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-jumpbox-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_jumpbox_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.32/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4093
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'deny-all-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_deny_all_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: '*'
//           access: 'Deny'
//           priority: 4096
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-ado-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_ado_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.160/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4094
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-aks-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_aks_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 256
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: [
//             '10.0.16.0/22'
//           ]
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-apim-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMOpenAI_nsg_name_allow_apim_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '443'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 320
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: [
//             '10.0.5.0/24'
//           ]
//           destinationAddressPrefixes: []
//         }
//       }
//     ]
//   }
// }

// resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
//   name: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name
//   location: 'eastus'
//   tags: {
//     Environment: 'DEMO'
//     Project: 'FLLM'
//     Purpose: 'Networking'
//     Workspace: 'foundationallm-ops'
//   }
//   properties: {
//     securityRules: [
//       {
//         name: 'allow-ado-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name_allow_ado_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.160/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4094
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-jumpbox-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name_allow_jumpbox_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.32/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4093
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'deny-all-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name_deny_all_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: '*'
//           access: 'Deny'
//           priority: 4096
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-tfc-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMServices_nsg_name_allow_tfc_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.224/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4095
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//     ]
//   }
// }

// resource networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
//   name: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name
//   location: 'eastus'
//   tags: {
//     Environment: 'DEMO'
//     Project: 'FLLM'
//     Purpose: 'Networking'
//     Workspace: 'foundationallm-ops'
//   }
//   properties: {
//     securityRules: [
//       {
//         name: 'allow-ado-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_allow_ado_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.160/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4094
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'deny-all-outbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_deny_all_outbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: '*'
//           access: 'Deny'
//           priority: 4096
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'deny-all-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_deny_all_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: '*'
//           access: 'Deny'
//           priority: 4096
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-tfc-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_allow_tfc_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.224/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4095
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-jumpbox-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_allow_jumpbox_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.32/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4093
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-aks-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_FLLMStorage_nsg_name_allow_aks_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 256
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: [
//             '10.0.16.0/22'
//           ]
//           destinationAddressPrefixes: []
//         }
//       }
//     ]
//   }
// }

// resource networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
//   name: networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name
//   location: 'eastus'
//   tags: {
//     Environment: 'DEMO'
//     Project: 'FLLM'
//     Purpose: 'Networking'
//     Workspace: 'foundationallm-ops'
//   }
//   properties: {
//     securityRules: [
//       {
//         name: 'allow-vnet-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_allow_vnet_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: 'VirtualNetwork'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 192
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-rdp'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_allow_rdp.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '3389'
//           sourceAddressPrefix: 'VirtualNetwork'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 128
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-vnet-outbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_allow_vnet_outbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: 'VirtualNetwork'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 128
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-jumpbox-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_allow_jumpbox_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.32/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4093
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-ado-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_allow_ado_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.160/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4094
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-tfc-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_allow_tfc_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.224/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4095
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-internet-outbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_jumpbox_nsg_name_allow_internet_outbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: 'Internet'
//           access: 'Allow'
//           priority: 256
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//     ]
//   }
// }

// resource networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
//   name: networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name
//   location: 'eastus'
//   tags: {
//     Environment: 'DEMO'
//     Project: 'FLLM'
//     Purpose: 'Networking'
//     Workspace: 'foundationallm-ops'
//   }
//   properties: {
//     securityRules: [
//       {
//         name: 'allow-tfc-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_allow_tfc_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.224/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4095
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-ado-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_allow_ado_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.160/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4094
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-rdp-services'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_allow_rdp_services.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 256
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: [
//             '10.0.0.0/24'
//           ]
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-jumpbox-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_allow_jumpbox_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.32/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4093
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-aks-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_allow_aks_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 264
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: [
//             '10.0.16.0/22'
//             '10.0.12.0/22'
//           ]
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'deny-all-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ops_nsg_name_deny_all_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: '*'
//           access: 'Deny'
//           priority: 4096
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//     ]
//   }
// }

// resource networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
//   name: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name
//   location: 'eastus'
//   tags: {
//     Environment: 'DEMO'
//     Project: 'FLLM'
//     Purpose: 'Networking'
//     Workspace: 'foundationallm-ops'
//   }
//   properties: {
//     securityRules: [
//       {
//         name: 'allow-tfc-sentinel'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_tfc_sentinel.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '443'
//           sourceAddressPrefix: '*'
//           access: 'Allow'
//           priority: 192
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: [
//             '54.185.161.84/32'
//             '44.236.246.186/32'
//             '52.86.200.106/32'
//             '52.70.186.109/32'
//             '44.238.78.236/32'
//             '52.86.201.227/32'
//           ]
//         }
//       }
//       {
//         name: 'allow-tfc-vcs'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_tfc_vcs.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '443'
//           sourceAddressPrefix: '*'
//           access: 'Allow'
//           priority: 224
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: [
//             '54.185.161.84/32'
//             '44.236.246.186/32'
//             '52.86.200.106/32'
//             '52.70.186.109/32'
//             '44.238.78.236/32'
//             '52.86.201.227/32'
//           ]
//         }
//       }
//       {
//         name: 'allow-ado-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_ado_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.160/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4094
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-vnet'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_vnet.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: 'VirtualNetwork'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4068
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-tfc-services'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_tfc_services.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '443'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: 'Internet'
//           access: 'Allow'
//           priority: 256
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'deny-all-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_deny_all_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: '*'
//           access: 'Deny'
//           priority: 4096
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'deny-all-outbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_deny_all_outbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: '*'
//           access: 'Deny'
//           priority: 4096
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-tfc-api'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_tfc_api.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '443'
//           sourceAddressPrefix: '*'
//           access: 'Allow'
//           priority: 128
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: [
//             '75.2.98.97/32'
//             '99.83.150.238/32'
//           ]
//         }
//       }
//       {
//         name: 'allow-tfc-notifications'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_tfc_notifications.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '443'
//           sourceAddressPrefix: '*'
//           access: 'Allow'
//           priority: 160
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: [
//             '54.185.161.84/32'
//             '44.236.246.186/32'
//             '52.86.200.106/32'
//             '52.70.186.109/32'
//             '44.238.78.236/32'
//             '52.86.201.227/32'
//           ]
//         }
//       }
//       {
//         name: 'allow-tfc-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_tfc_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.224/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4095
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-dns'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_dns.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Udp'
//           sourcePortRange: '*'
//           destinationPortRange: '53'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: 'Internet'
//           access: 'Allow'
//           priority: 288
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-jumpbox-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_tfc_nsg_name_allow_jumpbox_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.32/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4093
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//     ]
//   }
// }

// resource networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_resource 'Microsoft.Network/networkSecurityGroups@2023-05-01' = {
//   name: networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name
//   location: 'eastus'
//   tags: {
//     Environment: 'DEMO'
//     Project: 'FLLM'
//     Purpose: 'Networking'
//     Workspace: 'foundationallm-ops'
//   }
//   properties: {
//     securityRules: [
//       {
//         name: 'allow-ado-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_allow_ado_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.160/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4094
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'deny-all-outbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_deny_all_outbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: '*'
//           access: 'Deny'
//           priority: 4096
//           direction: 'Outbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-tfc-agents-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_allow_tfc_agents_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.224/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4095
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'deny-all-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_deny_all_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '*'
//           destinationAddressPrefix: '*'
//           access: 'Deny'
//           priority: 4096
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-jumpbox-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_allow_jumpbox_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: 'Tcp'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           sourceAddressPrefix: '10.0.255.32/27'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 4093
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: []
//           destinationAddressPrefixes: []
//         }
//       }
//       {
//         name: 'allow-aks-inbound'
//         id: networkSecurityGroups_EUS_FLLM_DEMO_NET_Vectorization_nsg_name_allow_aks_inbound.id
//         type: 'Microsoft.Network/networkSecurityGroups/securityRules'
//         properties: {
//           protocol: '*'
//           sourcePortRange: '*'
//           destinationPortRange: '*'
//           destinationAddressPrefix: 'VirtualNetwork'
//           access: 'Allow'
//           priority: 256
//           direction: 'Inbound'
//           sourcePortRanges: []
//           destinationPortRanges: []
//           sourceAddressPrefixes: [
//             '10.0.16.0/22'
//           ]
//           destinationAddressPrefixes: []
//         }
//       }
//     ]
//   }
// }

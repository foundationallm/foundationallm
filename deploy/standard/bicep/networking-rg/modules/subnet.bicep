param addressPrefix string
param name string
param serviceEndpoints array = []
param vnetName string

resource subnet1 'Microsoft.Network/virtualNetworks/subnets@2023-05-01' =  {
  name: '${vnetName}/${name}'

  properties: {
    addressPrefix: addressPrefix
    privateEndpointNetworkPolicies: 'Enabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
    serviceEndpoints: serviceEndpoints 

    // networkSecurityGroup: {
    //   id: networkSecurityGroups_EUS_FLLM_DEMO_NET_ado_nsg_name_resource.id
    // }
  }
}

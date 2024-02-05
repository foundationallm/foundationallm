/** Inputs **/
@description('Action Group Id for alerts')
param actionGroupId string

@description('The Managed Identity for the AKS Cluster')
param admnistratorObjectIds array

@description('Application Gateway Details')
param agw object

@description('Application Gateway resource group name')
param agwResourceGroupName string

@description('DNS resource group name')
param dnsResourceGroupName string

@description('The Kubernetes Version')
param kubernetesVersion string = '1.26.6'

@description('Location for all resources')
param location string

@description('Log Analytic Workspace Id to use for diagnostics')
param logAnalyticWorkspaceId string

@description('Log Analytic Workspace Resource Id to use for diagnostics')
param logAnalyticWorkspaceResourceId string

param networkingResourceGroupName string

@description('Private DNS Zones for private endpoint')
param privateDnsZones array

@description('Resource suffix for all resources')
param resourceSuffix string

@description('Subnet Id for private endpoint')
param subnetId string

@description('Subnet Id for private endpoint')
param subnetIdPrivateEndpoint string

@description('Tags for all resources')
param tags object

@description('Timestamp for nested deployments')
param timestamp string = utcNow()

/** Locals **/
@description('Metric alerts for the resource.')
var alerts = [
  {
    description: 'Node CPU utilization greater than 95% for 1 hour'
    evaluationFrequency: 'PT5M'
    metricName: 'node_cpu_usage_percentage'
    name: 'node-cpu'
    operator: 'GreaterThan'
    severity: 3
    threshold: 95
    timeAggregation: 'Average'
    windowSize: 'PT5M'
  }
  {
    description: 'Node memory utilization greater than 95% for 1 hour'
    evaluationFrequency: 'PT5M'
    metricName: 'node_memory_working_set_percentage'
    name: 'node-memory'
    operator: 'GreaterThan'
    severity: 3
    threshold: 100
    timeAggregation: 'Average'
    windowSize: 'PT5M'
  }
]

@description('The Resource logs to enable')
var logs = [
  'cloud-controller-manager'
  'cluster-autoscaler'
  'csi-azuredisk-controller'
  'csi-azurefile-controller'
  'csi-snapshot-controller'
  'guard'
  'kube-apiserver'
  'kube-audit'
  'kube-audit-admin'
  'kube-controller-manager'
  'kube-scheduler'
]

@description('The Resource Name')
var name = '${serviceType}-${resourceSuffix}'

@description('The Resource Service Type token')
var serviceType = 'aks'

/** Outputs **/
@description('AKS OIDC Issuer URL')
output oidcIssuerUrl string = main.properties.oidcIssuerProfile.issuerURL

/** Resources **/
@description('The AKS Cluster')
resource main 'Microsoft.ContainerService/managedClusters@2023-01-02-preview' = {
  name: name
  location: location
  tags: tags
  dependsOn: [
    dnsRoleAssignment
  ]

  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${uai.id}': {}
    }
  }

  sku: {
    name: 'Basic'
    tier: 'Paid'
  }

  properties: {
    enableRBAC: true
    fqdnSubdomain: name
    kubernetesVersion: kubernetesVersion
    nodeResourceGroup: 'mrg-${name}'
    disableLocalAccounts: true
    workloadAutoScalerProfile: {}

    aadProfile: {
      managed: true
      adminGroupObjectIDs: admnistratorObjectIds
      enableAzureRBAC: true
      tenantID: subscription().tenantId
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
        config: { version: 'v2' }
        enabled: true
      }

      ingressApplicationGateway: {
        enabled: true
        config: {
          applicationGatewayId: agw.id
        }
      }

      omsagent: {
        enabled: true
        config: {
          logAnalyticsWorkspaceResourceID: logAnalyticWorkspaceId
          useAADAuth: 'true'
        }
      }
    }
    agentPoolProfiles: [
      {
        count: 1
        enableAutoScaling: true
        maxCount: 3
        minCount: 1
        mode: 'System'
        name: 'system'
        osDiskSizeGB: 1024
        tags: tags
        type: 'VirtualMachineScaleSets'
        vmSize: 'Standard_DS2_v2'
        vnetSubnetID: subnetId

        nodeTaints: [
          'CriticalAddonsOnly=true:NoSchedule'
        ]

        upgradeSettings: {
          maxSurge: '200'
        }
      }
      {
        count: 1
        enableAutoScaling: true
        maxCount: 3
        minCount: 1
        mode: 'User'
        name: 'user'
        osDiskSizeGB: 1024
        tags: tags
        type: 'VirtualMachineScaleSets'
        vmSize: 'Standard_DS2_v2'
        vnetSubnetID: subnetId

        upgradeSettings: {
          maxSurge: '200'
        }
      }
    ]

    apiServerAccessProfile: {
      enablePrivateCluster: true
      enablePrivateClusterPublicFQDN: false
      privateDNSZone: filter(privateDnsZones, (privateDnsZone) => privateDnsZone.key == 'aks')[0].id
    }

    autoUpgradeProfile: { upgradeChannel: 'stable' }

    azureMonitorProfile: {
      metrics: {
        enabled: true
      }
    }

    networkProfile: {
      dnsServiceIP: '10.100.254.1'
      ipFamilies: [ 'IPv4' ]
      loadBalancerSku: 'Standard'
      networkPlugin: 'azure'
      networkPolicy: 'azure'
      outboundType: 'loadBalancer'
      serviceCidr: '10.100.0.0/16'
      serviceCidrs: [ '10.100.0.0/16' ]

      loadBalancerProfile: {
        backendPoolType: 'nodeIPConfiguration'
        managedOutboundIPs: { count: 1 }
      }
    }

    oidcIssuerProfile: { enabled: true }

    privateLinkResources: [
      {
        groupId: 'management'
        name: 'management'
        requiredMembers: [ 'management' ]
        type: 'Microsoft.ContainerService/managedClusters/privateLinkResources'
      }
    ]

    securityProfile: {
      defender: {
        logAnalyticsWorkspaceResourceId: logAnalyticWorkspaceResourceId

        securityMonitoring: {
          enabled: true
        }
      }

      imageCleaner: {
        enabled: false
        intervalHours: 48
      }

      workloadIdentity: { enabled: true }
    }

    servicePrincipalProfile: {
      clientId: 'msi'
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

  }
}

@description('Diagnostic settings for the resource')
resource diagnostics 'Microsoft.Insights/diagnosticSettings@2017-05-01-preview' = {
  scope: main
  name: 'diag-${serviceType}'
  properties: {
    workspaceId: logAnalyticWorkspaceId
    logs: [for log in logs: {
      category: log
      enabled: true
    }]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}

@description('The Managed Identity for the AKS Cluster')
resource uai 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  location: location
  name: 'uai-${name}'
  tags: tags
}

/** Nested Modules **/
module agwClusterRoleAssignment 'utility/roleAssignments.bicep' = {
  name: 'agwra-${resourceSuffix}-${timestamp}'
  scope: resourceGroup(agwResourceGroupName)
  params: {
    principalId: uai.properties.principalId
    roleDefinitionIds: {
      Contributor: 'b24988ac-6180-42a0-ab88-20f7382dd24c'
    }
  }
}

module dnsRoleAssignment 'utility/roleAssignments.bicep' = {
  name: 'dnsra-${resourceSuffix}-${timestamp}'
  scope: resourceGroup(dnsResourceGroupName)
  params: {
    principalId: uai.properties.principalId
    roleDefinitionIds: {
      'Private DNS Zone Contributor': 'b12aa53e-6015-4669-85d0-8515ebb3ae7f'
    }
  }
}

module netRoleAssignment 'utility/roleAssignments.bicep' = {
  name: 'netra-${resourceSuffix}-${timestamp}'
  scope: resourceGroup(networkingResourceGroupName)
  params: {
    principalId: uai.properties.principalId
    roleDefinitionIds: {
      'Network Contributor': '4d97b98b-1d4f-4787-a291-c67834d212e7'
    }
  }
}

@description('Resource for configuring the Key Vault metric alerts.')
module metricAlerts 'utility/metricAlerts.bicep' = {
  name: 'a-${main.name}-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    alerts: alerts
    metricNamespace: 'Microsoft.ContainerService/managedClusters'
    nameSuffix: name
    serviceId: main.id
    tags: tags
  }
}

@description('Private endpoint for App Configuration')
module privateEndpoint 'utility/privateEndpoint.bicep' = {
  name: 'pe-${main.name}-${timestamp}'
  params: {
    groupId: 'management'
    location: location
    privateDnsZones: privateDnsZones
    subnetId: subnetIdPrivateEndpoint
    tags: tags

    service: {
      id: main.id
      name: main.name
    }
  }
}

module agwAgicRoleAssignment 'utility/roleAssignments.bicep' = {
  name: 'agicra-${resourceSuffix}-${timestamp}'
  scope: resourceGroup(agwResourceGroupName)
  params: {
    principalId: main.properties.addonProfiles.ingressApplicationGateway.identity.objectId
    roleDefinitionIds: {
      Contributor: 'b24988ac-6180-42a0-ab88-20f7382dd24c'
    }
  }
}

module subnetRoleAssignment 'utility/roleAssignments.bicep' = {
  name: 'sra-${resourceSuffix}-${timestamp}'
  scope: resourceGroup(networkingResourceGroupName)
  params: {
    principalId: main.properties.addonProfiles.ingressApplicationGateway.identity.objectId
    roleDefinitionIds: {
      'Network Contributor': '4d97b98b-1d4f-4787-a291-c67834d212e7'
    }
  }
}


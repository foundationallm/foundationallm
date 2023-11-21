param virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name string = 'EUS-FLLM-DEMO-JBX-vmss'
param metricAlerts_EUS_FLLM_DEMO_JBX_vmss_cpu_alert_name string = 'EUS-FLLM-DEMO-JBX-vmss-cpu-alert'
param metricAlerts_EUS_FLLM_DEMO_JBX_vmss_disk_alert_name string = 'EUS-FLLM-DEMO-JBX-vmss-disk-alert'
param virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-NET-rg/providers/Microsoft.Network/virtualNetworks/EUS-FLLM-DEMO-NET-vnet'
param disks_EUS_FLLM_DEMO_JBX_vmEUS_FLLM_DEMO_JBX_vmsOS_1_90783b1585464fbeac8766369c3e9569_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-JBX-rg/providers/Microsoft.Compute/disks/EUS-FLLM-DEMO-JBX-vmEUS-FLLM-DEMO-JBX-vmsOS__1_90783b1585464fbeac8766369c3e9569'
param disks_EUS_FLLM_DEMO_JBX_vmEUS_FLLM_DEMO_JBX_vmsOS_1_8931c3b217314710be0aa6ef665a0076_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-JBX-rg/providers/Microsoft.Compute/disks/EUS-FLLM-DEMO-JBX-vmEUS-FLLM-DEMO-JBX-vmsOS__1_8931c3b217314710be0aa6ef665a0076'
param disks_EUS_FLLM_DEMO_JBX_vmEUS_FLLM_DEMO_JBX_vmsOS_1_b35b5c9e79dc4a5a94c110645119dcb4_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-JBX-rg/providers/Microsoft.Compute/disks/EUS-FLLM-DEMO-JBX-vmEUS-FLLM-DEMO-JBX-vmsOS__1_b35b5c9e79dc4a5a94c110645119dcb4'
param disks_EUS_FLLM_DEMO_JBX_vmEUS_FLLM_DEMO_JBX_vmsOS_1_b1777ab752494061bdb2d909cd6a018d_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-JBX-rg/providers/Microsoft.Compute/disks/EUS-FLLM-DEMO-JBX-vmEUS-FLLM-DEMO-JBX-vmsOS__1_b1777ab752494061bdb2d909cd6a018d'
param disks_EUS_FLLM_DEMO_JBX_vmEUS_FLLM_DEMO_JBX_vmsOS_1_ac6015517acd415282c4772fc5da11bf_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-JBX-rg/providers/Microsoft.Compute/disks/EUS-FLLM-DEMO-JBX-vmEUS-FLLM-DEMO-JBX-vmsOS__1_ac6015517acd415282c4772fc5da11bf'
param disks_EUS_FLLM_DEMO_JBX_vmEUS_FLLM_DEMO_JBX_vmsOS_1_4cf95b847c81423c9e704a243548bd51_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-JBX-rg/providers/Microsoft.Compute/disks/EUS-FLLM-DEMO-JBX-vmEUS-FLLM-DEMO-JBX-vmsOS__1_4cf95b847c81423c9e704a243548bd51'
param actionGroups_EUS_FLLM_DEMO_OPS_ag_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-OPS-rg/providers/Microsoft.Insights/actionGroups/EUS-FLLM-DEMO-OPS-ag'

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource 'Microsoft.Compute/virtualMachineScaleSets@2023-03-01' = {
  name: virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'DevOps'
    Workspace: 'foundationallm-ops'
  }
  sku: {
    name: 'Standard_DS3_v2'
    tier: 'Standard'
    capacity: 6
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    singlePlacementGroup: true
    orchestrationMode: 'Uniform'
    upgradePolicy: {
      mode: 'Manual'
    }
    scaleInPolicy: {
      rules: [
        'Default'
      ]
      forceDeletion: false
    }
    virtualMachineProfile: {
      osProfile: {
        computerNamePrefix: 'jumpbox'
        adminUsername: '88f17ed202b2421b'
        windowsConfiguration: {
          provisionVMAgent: true
          enableAutomaticUpdates: true
          winRM: {
            listeners: []
          }
          enableVMAgentPlatformUpdates: false
        }
        secrets: []
        allowExtensionOperations: true
        requireGuestProvisionSignal: true
      }
      storageProfile: {
        osDisk: {
          osType: 'Windows'
          createOption: 'FromImage'
          caching: 'ReadWrite'
          writeAcceleratorEnabled: false
          managedDisk: {
            storageAccountType: 'Standard_LRS'
          }
          diskSizeGB: 127
        }
        imageReference: {
          publisher: 'MicrosoftWindowsDesktop'
          offer: 'Windows-10'
          sku: 'win10-22h2-pro'
          version: 'latest'
        }
      }
      networkProfile: {
        networkInterfaceConfigurations: [
          {
            name: 'primary'
            properties: {
              primary: true
              enableAcceleratedNetworking: true
              disableTcpStateTracking: false
              dnsSettings: {
                dnsServers: []
              }
              enableIPForwarding: false
              ipConfigurations: [
                {
                  name: 'internal'
                  properties: {
                    primary: true
                    subnet: {
                      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/jumpbox'
                    }
                    privateIPAddressVersion: 'IPv4'
                  }
                }
              ]
            }
          }
        ]
      }
      diagnosticsProfile: {
        bootDiagnostics: {
          enabled: true
        }
      }
      extensionProfile: {
        extensions: [
          {
            name: 'DependencyAgentWindows'
            properties: {
              autoUpgradeMinorVersion: true
              provisionAfterExtensions: []
              enableAutomaticUpgrade: false
              suppressFailures: false
              publisher: 'Microsoft.Azure.Monitoring.DependencyAgent'
              type: 'DependencyAgentWindows'
              typeHandlerVersion: '9.10'
              settings: {
                enableAMA: true
              }
            }
          }
          {
            name: 'AzureMonitorWindowsAgent'
            properties: {
              autoUpgradeMinorVersion: true
              provisionAfterExtensions: []
              enableAutomaticUpgrade: false
              suppressFailures: false
              publisher: 'Microsoft.Azure.Monitor'
              type: 'AzureMonitorWindowsAgent'
              typeHandlerVersion: '1.0'
              settings: {}
            }
          }
          {
            name: 'MicrosoftMonitoringAgent'
            properties: {
              autoUpgradeMinorVersion: true
              publisher: 'Microsoft.EnterpriseCloud.Monitoring'
              type: 'MicrosoftMonitoringAgent'
              typeHandlerVersion: '1.0'
              settings: {
                workspaceId: '40f29020-518b-4ea7-b3e9-fe520fb67aaa'
                stopOnMultipleConnections: 'false'
              }
            }
          }
        ]
        extensionsTimeBudget: 'PT1H30M'
      }
      priority: 'Regular'
      securityProfile: {
        encryptionAtHost: true
      }
    }
    overprovision: false
    doNotRunExtensionsOnOverprovisionedVMs: false
  }
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_AzureMonitorWindowsAgent 'Microsoft.Compute/virtualMachineScaleSets/extensions@2023-03-01' = {
  parent: virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  name: 'AzureMonitorWindowsAgent'
  properties: {
    provisioningState: 'Succeeded'
    autoUpgradeMinorVersion: true
    provisionAfterExtensions: []
    enableAutomaticUpgrade: false
    suppressFailures: false
    publisher: 'Microsoft.Azure.Monitor'
    type: 'AzureMonitorWindowsAgent'
    typeHandlerVersion: '1.0'
    settings: {}
  }
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_DependencyAgentWindows 'Microsoft.Compute/virtualMachineScaleSets/extensions@2023-03-01' = {
  parent: virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  name: 'DependencyAgentWindows'
  properties: {
    provisioningState: 'Succeeded'
    autoUpgradeMinorVersion: true
    provisionAfterExtensions: []
    enableAutomaticUpgrade: false
    suppressFailures: false
    publisher: 'Microsoft.Azure.Monitoring.DependencyAgent'
    type: 'DependencyAgentWindows'
    typeHandlerVersion: '9.10'
    settings: {
      enableAMA: true
    }
  }
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_MicrosoftMonitoringAgent 'Microsoft.Compute/virtualMachineScaleSets/extensions@2023-03-01' = {
  parent: virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  name: 'MicrosoftMonitoringAgent'
  properties: {
    provisioningState: 'Succeeded'
    autoUpgradeMinorVersion: true
    publisher: 'Microsoft.EnterpriseCloud.Monitoring'
    type: 'MicrosoftMonitoringAgent'
    typeHandlerVersion: '1.0'
    settings: {
      workspaceId: '40f29020-518b-4ea7-b3e9-fe520fb67aaa'
      stopOnMultipleConnections: 'false'
    }
  }
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_0 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines@2023-03-01' = {
  parent: virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  name: '0'
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'DevOps'
    Workspace: 'foundationallm-ops'
  }
  sku: {
    name: 'Standard_DS3_v2'
    tier: 'Standard'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    networkProfileConfiguration: {
      networkInterfaceConfigurations: [
        {
          name: 'primary'
          properties: {
            primary: true
            enableAcceleratedNetworking: true
            disableTcpStateTracking: false
            dnsSettings: {
              dnsServers: []
            }
            enableIPForwarding: false
            ipConfigurations: [
              {
                name: 'internal'
                properties: {
                  primary: true
                  subnet: {
                    id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/jumpbox'
                  }
                  privateIPAddressVersion: 'IPv4'
                }
              }
            ]
          }
        }
      ]
    }
    hardwareProfile: {
      vmSize: 'Standard_DS3_v2'
    }
    storageProfile: {
      imageReference: {
        publisher: 'MicrosoftWindowsDesktop'
        offer: 'Windows-10'
        sku: 'win10-22h2-pro'
        version: 'latest'
      }
      osDisk: {
        osType: 'Windows'
        name: 'EUS-FLLM-DEMO-JBX-vmEUS-FLLM-DEMO-JBX-vmsOS__1_90783b1585464fbeac8766369c3e9569'
        createOption: 'FromImage'
        caching: 'ReadWrite'
        writeAcceleratorEnabled: false
        managedDisk: {
          storageAccountType: 'Standard_LRS'
          id: disks_EUS_FLLM_DEMO_JBX_vmEUS_FLLM_DEMO_JBX_vmsOS_1_90783b1585464fbeac8766369c3e9569_externalid
        }
        diskSizeGB: 127
      }
      dataDisks: []
    }
    osProfile: {
      computerName: 'jumpbox000000'
      adminUsername: '88f17ed202b2421b'
      windowsConfiguration: {
        provisionVMAgent: true
        enableAutomaticUpdates: true
        winRM: {
          listeners: []
        }
        enableVMAgentPlatformUpdates: false
      }
      secrets: []
      allowExtensionOperations: true
      requireGuestProvisionSignal: true
    }
    securityProfile: {
      encryptionAtHost: true
    }
    networkProfile: {
      networkInterfaces: [
        {
          id: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_0.id}/networkInterfaces/primary'
        }
      ]
    }
    diagnosticsProfile: {
      bootDiagnostics: {
        enabled: true
      }
    }
  }
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_1 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines@2023-03-01' = {
  parent: virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  name: '1'
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'DevOps'
    Workspace: 'foundationallm-ops'
  }
  sku: {
    name: 'Standard_DS3_v2'
    tier: 'Standard'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    networkProfileConfiguration: {
      networkInterfaceConfigurations: [
        {
          name: 'primary'
          properties: {
            primary: true
            enableAcceleratedNetworking: true
            disableTcpStateTracking: false
            dnsSettings: {
              dnsServers: []
            }
            enableIPForwarding: false
            ipConfigurations: [
              {
                name: 'internal'
                properties: {
                  primary: true
                  subnet: {
                    id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/jumpbox'
                  }
                  privateIPAddressVersion: 'IPv4'
                }
              }
            ]
          }
        }
      ]
    }
    hardwareProfile: {
      vmSize: 'Standard_DS3_v2'
    }
    storageProfile: {
      imageReference: {
        publisher: 'MicrosoftWindowsDesktop'
        offer: 'Windows-10'
        sku: 'win10-22h2-pro'
        version: 'latest'
      }
      osDisk: {
        osType: 'Windows'
        name: 'EUS-FLLM-DEMO-JBX-vmEUS-FLLM-DEMO-JBX-vmsOS__1_8931c3b217314710be0aa6ef665a0076'
        createOption: 'FromImage'
        caching: 'ReadWrite'
        writeAcceleratorEnabled: false
        managedDisk: {
          storageAccountType: 'Standard_LRS'
          id: disks_EUS_FLLM_DEMO_JBX_vmEUS_FLLM_DEMO_JBX_vmsOS_1_8931c3b217314710be0aa6ef665a0076_externalid
        }
        diskSizeGB: 127
      }
      dataDisks: []
    }
    osProfile: {
      computerName: 'jumpbox000001'
      adminUsername: '88f17ed202b2421b'
      windowsConfiguration: {
        provisionVMAgent: true
        enableAutomaticUpdates: true
        winRM: {
          listeners: []
        }
        enableVMAgentPlatformUpdates: false
      }
      secrets: []
      allowExtensionOperations: true
      requireGuestProvisionSignal: true
    }
    securityProfile: {
      encryptionAtHost: true
    }
    networkProfile: {
      networkInterfaces: [
        {
          id: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_1.id}/networkInterfaces/primary'
        }
      ]
    }
    diagnosticsProfile: {
      bootDiagnostics: {
        enabled: true
      }
    }
  }
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_2 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines@2023-03-01' = {
  parent: virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  name: '2'
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'DevOps'
    Workspace: 'foundationallm-ops'
  }
  sku: {
    name: 'Standard_DS3_v2'
    tier: 'Standard'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    networkProfileConfiguration: {
      networkInterfaceConfigurations: [
        {
          name: 'primary'
          properties: {
            primary: true
            enableAcceleratedNetworking: true
            disableTcpStateTracking: false
            dnsSettings: {
              dnsServers: []
            }
            enableIPForwarding: false
            ipConfigurations: [
              {
                name: 'internal'
                properties: {
                  primary: true
                  subnet: {
                    id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/jumpbox'
                  }
                  privateIPAddressVersion: 'IPv4'
                }
              }
            ]
          }
        }
      ]
    }
    hardwareProfile: {
      vmSize: 'Standard_DS3_v2'
    }
    storageProfile: {
      imageReference: {
        publisher: 'MicrosoftWindowsDesktop'
        offer: 'Windows-10'
        sku: 'win10-22h2-pro'
        version: 'latest'
      }
      osDisk: {
        osType: 'Windows'
        name: 'EUS-FLLM-DEMO-JBX-vmEUS-FLLM-DEMO-JBX-vmsOS__1_b35b5c9e79dc4a5a94c110645119dcb4'
        createOption: 'FromImage'
        caching: 'ReadWrite'
        writeAcceleratorEnabled: false
        managedDisk: {
          storageAccountType: 'Standard_LRS'
          id: disks_EUS_FLLM_DEMO_JBX_vmEUS_FLLM_DEMO_JBX_vmsOS_1_b35b5c9e79dc4a5a94c110645119dcb4_externalid
        }
        diskSizeGB: 127
      }
      dataDisks: []
    }
    osProfile: {
      computerName: 'jumpbox000002'
      adminUsername: '88f17ed202b2421b'
      windowsConfiguration: {
        provisionVMAgent: true
        enableAutomaticUpdates: true
        winRM: {
          listeners: []
        }
        enableVMAgentPlatformUpdates: false
      }
      secrets: []
      allowExtensionOperations: true
      requireGuestProvisionSignal: true
    }
    securityProfile: {
      encryptionAtHost: true
    }
    networkProfile: {
      networkInterfaces: [
        {
          id: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_2.id}/networkInterfaces/primary'
        }
      ]
    }
    diagnosticsProfile: {
      bootDiagnostics: {
        enabled: true
      }
    }
  }
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_3 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines@2023-03-01' = {
  parent: virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  name: '3'
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'DevOps'
    Workspace: 'foundationallm-ops'
  }
  sku: {
    name: 'Standard_DS3_v2'
    tier: 'Standard'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    networkProfileConfiguration: {
      networkInterfaceConfigurations: [
        {
          name: 'primary'
          properties: {
            primary: true
            enableAcceleratedNetworking: true
            disableTcpStateTracking: false
            dnsSettings: {
              dnsServers: []
            }
            enableIPForwarding: false
            ipConfigurations: [
              {
                name: 'internal'
                properties: {
                  primary: true
                  subnet: {
                    id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/jumpbox'
                  }
                  privateIPAddressVersion: 'IPv4'
                }
              }
            ]
          }
        }
      ]
    }
    hardwareProfile: {
      vmSize: 'Standard_DS3_v2'
    }
    storageProfile: {
      imageReference: {
        publisher: 'MicrosoftWindowsDesktop'
        offer: 'Windows-10'
        sku: 'win10-22h2-pro'
        version: 'latest'
      }
      osDisk: {
        osType: 'Windows'
        name: 'EUS-FLLM-DEMO-JBX-vmEUS-FLLM-DEMO-JBX-vmsOS__1_b1777ab752494061bdb2d909cd6a018d'
        createOption: 'FromImage'
        caching: 'ReadWrite'
        writeAcceleratorEnabled: false
        managedDisk: {
          storageAccountType: 'Standard_LRS'
          id: disks_EUS_FLLM_DEMO_JBX_vmEUS_FLLM_DEMO_JBX_vmsOS_1_b1777ab752494061bdb2d909cd6a018d_externalid
        }
        diskSizeGB: 127
      }
      dataDisks: []
    }
    osProfile: {
      computerName: 'jumpbox000003'
      adminUsername: '88f17ed202b2421b'
      windowsConfiguration: {
        provisionVMAgent: true
        enableAutomaticUpdates: true
        winRM: {
          listeners: []
        }
        enableVMAgentPlatformUpdates: false
      }
      secrets: []
      allowExtensionOperations: true
      requireGuestProvisionSignal: true
    }
    securityProfile: {
      encryptionAtHost: true
    }
    networkProfile: {
      networkInterfaces: [
        {
          id: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_3.id}/networkInterfaces/primary'
        }
      ]
    }
    diagnosticsProfile: {
      bootDiagnostics: {
        enabled: true
      }
    }
  }
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_4 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines@2023-03-01' = {
  parent: virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  name: '4'
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'DevOps'
    Workspace: 'foundationallm-ops'
  }
  sku: {
    name: 'Standard_DS3_v2'
    tier: 'Standard'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    networkProfileConfiguration: {
      networkInterfaceConfigurations: [
        {
          name: 'primary'
          properties: {
            primary: true
            enableAcceleratedNetworking: true
            disableTcpStateTracking: false
            dnsSettings: {
              dnsServers: []
            }
            enableIPForwarding: false
            ipConfigurations: [
              {
                name: 'internal'
                properties: {
                  primary: true
                  subnet: {
                    id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/jumpbox'
                  }
                  privateIPAddressVersion: 'IPv4'
                }
              }
            ]
          }
        }
      ]
    }
    hardwareProfile: {
      vmSize: 'Standard_DS3_v2'
    }
    storageProfile: {
      imageReference: {
        publisher: 'MicrosoftWindowsDesktop'
        offer: 'Windows-10'
        sku: 'win10-22h2-pro'
        version: 'latest'
      }
      osDisk: {
        osType: 'Windows'
        name: 'EUS-FLLM-DEMO-JBX-vmEUS-FLLM-DEMO-JBX-vmsOS__1_ac6015517acd415282c4772fc5da11bf'
        createOption: 'FromImage'
        caching: 'ReadWrite'
        writeAcceleratorEnabled: false
        managedDisk: {
          storageAccountType: 'Standard_LRS'
          id: disks_EUS_FLLM_DEMO_JBX_vmEUS_FLLM_DEMO_JBX_vmsOS_1_ac6015517acd415282c4772fc5da11bf_externalid
        }
        diskSizeGB: 127
      }
      dataDisks: []
    }
    osProfile: {
      computerName: 'jumpbox000004'
      adminUsername: '88f17ed202b2421b'
      windowsConfiguration: {
        provisionVMAgent: true
        enableAutomaticUpdates: true
        winRM: {
          listeners: []
        }
        enableVMAgentPlatformUpdates: false
      }
      secrets: []
      allowExtensionOperations: true
      requireGuestProvisionSignal: true
    }
    securityProfile: {
      encryptionAtHost: true
    }
    networkProfile: {
      networkInterfaces: [
        {
          id: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_4.id}/networkInterfaces/primary'
        }
      ]
    }
    diagnosticsProfile: {
      bootDiagnostics: {
        enabled: true
      }
    }
  }
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_5 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines@2023-03-01' = {
  parent: virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  name: '5'
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'DevOps'
    Workspace: 'foundationallm-ops'
  }
  sku: {
    name: 'Standard_DS3_v2'
    tier: 'Standard'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    networkProfileConfiguration: {
      networkInterfaceConfigurations: [
        {
          name: 'primary'
          properties: {
            primary: true
            enableAcceleratedNetworking: true
            disableTcpStateTracking: false
            dnsSettings: {
              dnsServers: []
            }
            enableIPForwarding: false
            ipConfigurations: [
              {
                name: 'internal'
                properties: {
                  primary: true
                  subnet: {
                    id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/jumpbox'
                  }
                  privateIPAddressVersion: 'IPv4'
                }
              }
            ]
          }
        }
      ]
    }
    hardwareProfile: {
      vmSize: 'Standard_DS3_v2'
    }
    storageProfile: {
      imageReference: {
        publisher: 'MicrosoftWindowsDesktop'
        offer: 'Windows-10'
        sku: 'win10-22h2-pro'
        version: 'latest'
      }
      osDisk: {
        osType: 'Windows'
        name: 'EUS-FLLM-DEMO-JBX-vmEUS-FLLM-DEMO-JBX-vmsOS__1_4cf95b847c81423c9e704a243548bd51'
        createOption: 'FromImage'
        caching: 'ReadWrite'
        writeAcceleratorEnabled: false
        managedDisk: {
          storageAccountType: 'Standard_LRS'
          id: disks_EUS_FLLM_DEMO_JBX_vmEUS_FLLM_DEMO_JBX_vmsOS_1_4cf95b847c81423c9e704a243548bd51_externalid
        }
        diskSizeGB: 127
      }
      dataDisks: []
    }
    osProfile: {
      computerName: 'jumpbox000005'
      adminUsername: '88f17ed202b2421b'
      windowsConfiguration: {
        provisionVMAgent: true
        enableAutomaticUpdates: true
        winRM: {
          listeners: []
        }
        enableVMAgentPlatformUpdates: false
      }
      secrets: []
      allowExtensionOperations: true
      requireGuestProvisionSignal: true
    }
    securityProfile: {
      encryptionAtHost: true
    }
    networkProfile: {
      networkInterfaces: [
        {
          id: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_5.id}/networkInterfaces/primary'
        }
      ]
    }
    diagnosticsProfile: {
      bootDiagnostics: {
        enabled: true
      }
    }
  }
}

resource metricAlerts_EUS_FLLM_DEMO_JBX_vmss_cpu_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_JBX_vmss_cpu_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'DevOps'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    description: 'Alert on VM Threshold - Average CPU greater than 75% for 5 minutes'
    severity: 2
    enabled: true
    scopes: [
      virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          threshold: 75
          name: 'Metric1'
          metricNamespace: 'Microsoft.Compute/virtualmachinescalesets'
          metricName: 'Percentage CPU'
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

resource metricAlerts_EUS_FLLM_DEMO_JBX_vmss_disk_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_JBX_vmss_disk_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'DevOps'
    Workspace: 'foundationallm-ops'
  }
  properties: {
    description: 'Alert on VM Threshold - Average Disk Queue greater than 8 for 5 minutes'
    severity: 2
    enabled: true
    scopes: [
      virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          threshold: 8
          name: 'Metric1'
          metricNamespace: 'Microsoft.Compute/virtualmachinescalesets'
          metricName: 'OS Disk Queue Depth'
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

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_0_AzureMonitorWindowsAgent 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines/extensions@2023-03-01' = {
  name: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name}/0/AzureMonitorWindowsAgent'
  location: 'eastus'
  properties: {
    autoUpgradeMinorVersion: true
    provisionAfterExtensions: []
    enableAutomaticUpgrade: false
    suppressFailures: false
    publisher: 'Microsoft.Azure.Monitor'
    type: 'AzureMonitorWindowsAgent'
    typeHandlerVersion: '1.0'
    settings: {}
  }
  dependsOn: [
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_0
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  ]
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_1_AzureMonitorWindowsAgent 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines/extensions@2023-03-01' = {
  name: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name}/1/AzureMonitorWindowsAgent'
  location: 'eastus'
  properties: {
    autoUpgradeMinorVersion: true
    provisionAfterExtensions: []
    enableAutomaticUpgrade: false
    suppressFailures: false
    publisher: 'Microsoft.Azure.Monitor'
    type: 'AzureMonitorWindowsAgent'
    typeHandlerVersion: '1.0'
    settings: {}
  }
  dependsOn: [
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_1
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  ]
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_2_AzureMonitorWindowsAgent 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines/extensions@2023-03-01' = {
  name: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name}/2/AzureMonitorWindowsAgent'
  location: 'eastus'
  properties: {
    autoUpgradeMinorVersion: true
    provisionAfterExtensions: []
    enableAutomaticUpgrade: false
    suppressFailures: false
    publisher: 'Microsoft.Azure.Monitor'
    type: 'AzureMonitorWindowsAgent'
    typeHandlerVersion: '1.0'
    settings: {}
  }
  dependsOn: [
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_2
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  ]
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_3_AzureMonitorWindowsAgent 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines/extensions@2023-03-01' = {
  name: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name}/3/AzureMonitorWindowsAgent'
  location: 'eastus'
  properties: {
    autoUpgradeMinorVersion: true
    provisionAfterExtensions: []
    enableAutomaticUpgrade: false
    suppressFailures: false
    publisher: 'Microsoft.Azure.Monitor'
    type: 'AzureMonitorWindowsAgent'
    typeHandlerVersion: '1.0'
    settings: {}
  }
  dependsOn: [
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_3
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  ]
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_4_AzureMonitorWindowsAgent 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines/extensions@2023-03-01' = {
  name: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name}/4/AzureMonitorWindowsAgent'
  location: 'eastus'
  properties: {
    autoUpgradeMinorVersion: true
    provisionAfterExtensions: []
    enableAutomaticUpgrade: false
    suppressFailures: false
    publisher: 'Microsoft.Azure.Monitor'
    type: 'AzureMonitorWindowsAgent'
    typeHandlerVersion: '1.0'
    settings: {}
  }
  dependsOn: [
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_4
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  ]
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_5_AzureMonitorWindowsAgent 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines/extensions@2023-03-01' = {
  name: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name}/5/AzureMonitorWindowsAgent'
  location: 'eastus'
  properties: {
    autoUpgradeMinorVersion: true
    provisionAfterExtensions: []
    enableAutomaticUpgrade: false
    suppressFailures: false
    publisher: 'Microsoft.Azure.Monitor'
    type: 'AzureMonitorWindowsAgent'
    typeHandlerVersion: '1.0'
    settings: {}
  }
  dependsOn: [
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_5
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  ]
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_0_DependencyAgentWindows 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines/extensions@2023-03-01' = {
  name: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name}/0/DependencyAgentWindows'
  location: 'eastus'
  properties: {
    autoUpgradeMinorVersion: true
    provisionAfterExtensions: []
    enableAutomaticUpgrade: false
    suppressFailures: false
    publisher: 'Microsoft.Azure.Monitoring.DependencyAgent'
    type: 'DependencyAgentWindows'
    typeHandlerVersion: '9.10'
    settings: {}
  }
  dependsOn: [
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_0
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  ]
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_1_DependencyAgentWindows 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines/extensions@2023-03-01' = {
  name: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name}/1/DependencyAgentWindows'
  location: 'eastus'
  properties: {
    autoUpgradeMinorVersion: true
    provisionAfterExtensions: []
    enableAutomaticUpgrade: false
    suppressFailures: false
    publisher: 'Microsoft.Azure.Monitoring.DependencyAgent'
    type: 'DependencyAgentWindows'
    typeHandlerVersion: '9.10'
    settings: {}
  }
  dependsOn: [
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_1
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  ]
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_2_DependencyAgentWindows 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines/extensions@2023-03-01' = {
  name: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name}/2/DependencyAgentWindows'
  location: 'eastus'
  properties: {
    autoUpgradeMinorVersion: true
    provisionAfterExtensions: []
    enableAutomaticUpgrade: false
    suppressFailures: false
    publisher: 'Microsoft.Azure.Monitoring.DependencyAgent'
    type: 'DependencyAgentWindows'
    typeHandlerVersion: '9.10'
    settings: {}
  }
  dependsOn: [
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_2
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  ]
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_3_DependencyAgentWindows 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines/extensions@2023-03-01' = {
  name: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name}/3/DependencyAgentWindows'
  location: 'eastus'
  properties: {
    autoUpgradeMinorVersion: true
    provisionAfterExtensions: []
    enableAutomaticUpgrade: false
    suppressFailures: false
    publisher: 'Microsoft.Azure.Monitoring.DependencyAgent'
    type: 'DependencyAgentWindows'
    typeHandlerVersion: '9.10'
    settings: {}
  }
  dependsOn: [
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_3
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  ]
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_4_DependencyAgentWindows 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines/extensions@2023-03-01' = {
  name: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name}/4/DependencyAgentWindows'
  location: 'eastus'
  properties: {
    autoUpgradeMinorVersion: true
    provisionAfterExtensions: []
    enableAutomaticUpgrade: false
    suppressFailures: false
    publisher: 'Microsoft.Azure.Monitoring.DependencyAgent'
    type: 'DependencyAgentWindows'
    typeHandlerVersion: '9.10'
    settings: {}
  }
  dependsOn: [
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_4
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  ]
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_5_DependencyAgentWindows 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines/extensions@2023-03-01' = {
  name: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name}/5/DependencyAgentWindows'
  location: 'eastus'
  properties: {
    autoUpgradeMinorVersion: true
    provisionAfterExtensions: []
    enableAutomaticUpgrade: false
    suppressFailures: false
    publisher: 'Microsoft.Azure.Monitoring.DependencyAgent'
    type: 'DependencyAgentWindows'
    typeHandlerVersion: '9.10'
    settings: {}
  }
  dependsOn: [
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_5
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  ]
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_0_MicrosoftMonitoringAgent 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines/extensions@2023-03-01' = {
  name: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name}/0/MicrosoftMonitoringAgent'
  location: 'eastus'
  properties: {
    autoUpgradeMinorVersion: true
    publisher: 'Microsoft.EnterpriseCloud.Monitoring'
    type: 'MicrosoftMonitoringAgent'
    typeHandlerVersion: '1.0'
    settings: {}
  }
  dependsOn: [
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_0
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  ]
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_1_MicrosoftMonitoringAgent 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines/extensions@2023-03-01' = {
  name: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name}/1/MicrosoftMonitoringAgent'
  location: 'eastus'
  properties: {
    autoUpgradeMinorVersion: true
    publisher: 'Microsoft.EnterpriseCloud.Monitoring'
    type: 'MicrosoftMonitoringAgent'
    typeHandlerVersion: '1.0'
    settings: {}
  }
  dependsOn: [
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_1
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  ]
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_2_MicrosoftMonitoringAgent 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines/extensions@2023-03-01' = {
  name: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name}/2/MicrosoftMonitoringAgent'
  location: 'eastus'
  properties: {
    autoUpgradeMinorVersion: true
    publisher: 'Microsoft.EnterpriseCloud.Monitoring'
    type: 'MicrosoftMonitoringAgent'
    typeHandlerVersion: '1.0'
    settings: {}
  }
  dependsOn: [
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_2
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  ]
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_3_MicrosoftMonitoringAgent 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines/extensions@2023-03-01' = {
  name: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name}/3/MicrosoftMonitoringAgent'
  location: 'eastus'
  properties: {
    autoUpgradeMinorVersion: true
    publisher: 'Microsoft.EnterpriseCloud.Monitoring'
    type: 'MicrosoftMonitoringAgent'
    typeHandlerVersion: '1.0'
    settings: {}
  }
  dependsOn: [
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_3
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  ]
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_4_MicrosoftMonitoringAgent 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines/extensions@2023-03-01' = {
  name: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name}/4/MicrosoftMonitoringAgent'
  location: 'eastus'
  properties: {
    autoUpgradeMinorVersion: true
    publisher: 'Microsoft.EnterpriseCloud.Monitoring'
    type: 'MicrosoftMonitoringAgent'
    typeHandlerVersion: '1.0'
    settings: {}
  }
  dependsOn: [
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_4
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  ]
}

resource virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_5_MicrosoftMonitoringAgent 'Microsoft.Compute/virtualMachineScaleSets/virtualMachines/extensions@2023-03-01' = {
  name: '${virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name}/5/MicrosoftMonitoringAgent'
  location: 'eastus'
  properties: {
    autoUpgradeMinorVersion: true
    publisher: 'Microsoft.EnterpriseCloud.Monitoring'
    type: 'MicrosoftMonitoringAgent'
    typeHandlerVersion: '1.0'
    settings: {}
  }
  dependsOn: [
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_5
    virtualMachineScaleSets_EUS_FLLM_DEMO_JBX_vmss_name_resource
  ]
}
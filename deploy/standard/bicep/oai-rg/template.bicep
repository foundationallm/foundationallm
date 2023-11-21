@secure()
param users_1_lastName string
param vaults_EUS_FLLM_DEMO_OAI_kv_name string = 'EUS-FLLM-DEMO-OAI-kv'
param service_EUS_FLLM_DEMO_OAI_apim_name string = 'EUS-FLLM-DEMO-OAI-apim'
param publicIPAddresses_EUS_FLLM_DEMO_OAI_pip_name string = 'EUS-FLLM-DEMO-OAI-pip'
param privateEndpoints_EUS_FLLM_DEMO_OAI_kv_pe_name string = 'EUS-FLLM-DEMO-OAI-kv-pe'
param accounts_EUS_FLLM_DEMO_OAI_0_openai_name string = 'EUS-FLLM-DEMO-OAI-0-openai'
param accounts_EUS_FLLM_DEMO_OAI_1_openai_name string = 'EUS-FLLM-DEMO-OAI-1-openai'
param privateEndpoints_EUS_FLLM_DEMO_OAI_0_openai_pe_name string = 'EUS-FLLM-DEMO-OAI-0-openai-pe'
param privateEndpoints_EUS_FLLM_DEMO_OAI_1_openai_pe_name string = 'EUS-FLLM-DEMO-OAI-1-openai-pe'
param metricAlerts_EUS_FLLM_DEMO_OAI_kv_latency_alert_name string = 'EUS-FLLM-DEMO-OAI-kv-latency-alert'
param accounts_EUS_FLLM_DEMO_OAI_content_safety_name string = 'EUS-FLLM-DEMO-OAI-content-safety'
param metricAlerts_EUS_FLLM_DEMO_OAI_apim_capacity_alert_name string = 'EUS-FLLM-DEMO-OAI-apim-capacity-alert'
param metricAlerts_EUS_FLLM_DEMO_OAI_kv_saturation_alert_name string = 'EUS-FLLM-DEMO-OAI-kv-saturation-alert'
param privateEndpoints_EUS_FLLM_DEMO_OAI_content_safety_pe_name string = 'EUS-FLLM-DEMO-OAI-content-safety-pe'
param metricAlerts_EUS_FLLM_DEMO_OAI_kv_availability_alert_name string = 'EUS-FLLM-DEMO-OAI-kv-availability-alert'
param metricAlerts_EUS_FLLM_DEMO_OAI_openai_openai0_latency_alert_name string = 'EUS-FLLM-DEMO-OAI-openai-openai0-latency-alert'
param metricAlerts_EUS_FLLM_DEMO_OAI_openai_openai1_latency_alert_name string = 'EUS-FLLM-DEMO-OAI-openai-openai1-latency-alert'
param metricAlerts_EUS_FLLM_DEMO_OAI_openai_openai0_availability_alert_name string = 'EUS-FLLM-DEMO-OAI-openai-openai0-availability-alert'
param metricAlerts_EUS_FLLM_DEMO_OAI_openai_openai1_availability_alert_name string = 'EUS-FLLM-DEMO-OAI-openai-openai1-availability-alert'
param virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-NET-rg/providers/Microsoft.Network/virtualNetworks/EUS-FLLM-DEMO-NET-vnet'
param actionGroups_EUS_FLLM_DEMO_OPS_ag_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-OPS-rg/providers/Microsoft.Insights/actionGroups/EUS-FLLM-DEMO-OPS-ag'
param privateDnsZones_privatelink_openai_azure_com_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DNS-rg/providers/Microsoft.Network/privateDnsZones/privatelink.openai.azure.com'
param privateDnsZones_privatelink_cognitiveservices_azure_com_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DNS-rg/providers/Microsoft.Network/privateDnsZones/privatelink.cognitiveservices.azure.com'
param privateDnsZones_privatelink_vaultcore_azure_net_externalid string = '/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DNS-rg/providers/Microsoft.Network/privateDnsZones/privatelink.vaultcore.azure.net'

resource accounts_EUS_FLLM_DEMO_OAI_0_openai_name_resource 'Microsoft.CognitiveServices/accounts@2023-10-01-preview' = {
  name: accounts_EUS_FLLM_DEMO_OAI_0_openai_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'OpenAI'
    Workspace: 'FoundationaLLM-Platform'
  }
  sku: {
    name: 'S0'
  }
  kind: 'OpenAI'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    customSubDomainName: 'eus-fllm-demo-oai-0'
    publicNetworkAccess: 'Disabled'
    restrictOutboundNetworkAccess: false
    allowedFqdnList: []
    disableLocalAuth: false
    dynamicThrottlingEnabled: false
  }
}

resource accounts_EUS_FLLM_DEMO_OAI_1_openai_name_resource 'Microsoft.CognitiveServices/accounts@2023-10-01-preview' = {
  name: accounts_EUS_FLLM_DEMO_OAI_1_openai_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'OpenAI'
    Workspace: 'FoundationaLLM-Platform'
  }
  sku: {
    name: 'S0'
  }
  kind: 'OpenAI'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    customSubDomainName: 'eus-fllm-demo-oai-1'
    publicNetworkAccess: 'Disabled'
    restrictOutboundNetworkAccess: false
    allowedFqdnList: []
    disableLocalAuth: false
    dynamicThrottlingEnabled: false
  }
}

resource accounts_EUS_FLLM_DEMO_OAI_content_safety_name_resource 'Microsoft.CognitiveServices/accounts@2023-10-01-preview' = {
  name: accounts_EUS_FLLM_DEMO_OAI_content_safety_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'OpenAI'
    Workspace: 'FoundationaLLM-Platform'
  }
  sku: {
    name: 'S0'
  }
  kind: 'ContentSafety'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    customSubDomainName: 'eus-fllm-demo-oai-content-safety'
    publicNetworkAccess: 'Disabled'
    restrictOutboundNetworkAccess: false
    allowedFqdnList: []
    disableLocalAuth: false
    dynamicThrottlingEnabled: false
  }
}

resource vaults_EUS_FLLM_DEMO_OAI_kv_name_resource 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: vaults_EUS_FLLM_DEMO_OAI_kv_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'OpenAI'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: '22179471-b099-4504-bfdb-3f184cdae122'
    accessPolicies: []
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    enableRbacAuthorization: true
    enablePurgeProtection: true
    vaultUri: 'https://eus-fllm-demo-oai-kv.vault.azure.net/'
    provisioningState: 'Succeeded'
    publicNetworkAccess: 'Disabled'
  }
}

resource publicIPAddresses_EUS_FLLM_DEMO_OAI_pip_name_resource 'Microsoft.Network/publicIPAddresses@2023-05-01' = {
  name: publicIPAddresses_EUS_FLLM_DEMO_OAI_pip_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'OpenAI'
    Workspace: 'FoundationaLLM-Platform'
  }
  sku: {
    name: 'Standard'
    tier: 'Regional'
  }
  properties: {
    ipAddress: '172.190.223.143'
    publicIPAddressVersion: 'IPv4'
    publicIPAllocationMethod: 'Static'
    idleTimeoutInMinutes: 4
    dnsSettings: {
      domainNameLabel: 'eus-fllm-demo-oai-apim'
      fqdn: 'eus-fllm-demo-oai-apim.eastus.cloudapp.azure.com'
    }
    ipTags: []
    ddosSettings: {
      protectionMode: 'VirtualNetworkInherited'
    }
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_resource 'Microsoft.ApiManagement/service@2023-03-01-preview' = {
  name: service_EUS_FLLM_DEMO_OAI_apim_name
  location: 'East US'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'OpenAI'
    Workspace: 'FoundationaLLM-Platform'
  }
  sku: {
    name: 'Developer'
    capacity: 1
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    publisherEmail: 'info@solliance.net'
    publisherName: 'FoundationaLLM'
    notificationSenderEmail: 'apimgmt-noreply@mail.windowsazure.com'
    hostnameConfigurations: [
      {
        type: 'Proxy'
        hostName: 'eus-fllm-demo-oai-apim.azure-api.net'
        negotiateClientCertificate: false
        defaultSslBinding: true
        certificateSource: 'BuiltIn'
      }
    ]
    virtualNetworkConfiguration: {
      subnetResourceId: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/FLLMOpenAI'
    }
    customProperties: {
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Ssl30': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Tls10': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Tls11': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Ciphers.TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Ciphers.TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Ciphers.TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Ciphers.TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Ciphers.TLS_RSA_WITH_AES_128_CBC_SHA': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Ciphers.TLS_RSA_WITH_AES_128_CBC_SHA256': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Ciphers.TLS_RSA_WITH_AES_128_GCM_SHA256': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Ciphers.TLS_RSA_WITH_AES_256_CBC_SHA': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Ciphers.TLS_RSA_WITH_AES_256_CBC_SHA256': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Ciphers.TLS_RSA_WITH_AES_256_GCM_SHA384': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Ciphers.TripleDes168': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Protocols.Ssl30': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Protocols.Tls10': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Protocols.Tls11': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Protocols.Server.Http2': 'False'
    }
    virtualNetworkType: 'Internal'
    certificates: []
    natGatewayState: 'Disabled'
    apiVersionConstraint: {}
    publicIpAddressId: publicIPAddresses_EUS_FLLM_DEMO_OAI_pip_name_resource.id
    publicNetworkAccess: 'Enabled'
    legacyPortalStatus: 'Enabled'
    developerPortalStatus: 'Enabled'
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_EUS_FLLM_DEMO_OAI_api 'Microsoft.ApiManagement/service/apis@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'EUS-FLLM-DEMO-OAI-api'
  properties: {
    displayName: 'HA OpenAI'
    apiRevision: '1'
    subscriptionRequired: false
    path: 'openai'
    protocols: [
      'https'
    ]
    authenticationSettings: {
      oAuth2AuthenticationSettings: []
      openidAuthenticationSettings: []
    }
    subscriptionKeyParameterNames: {
      header: 'api-key'
      query: 'subscription-key'
    }
    isCurrent: true
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_EUS_FLLM_DEMO_OAI_0_backend 'Microsoft.ApiManagement/service/backends@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'EUS-FLLM-DEMO-OAI-0-backend'
  properties: {
    url: 'https://eus-fllm-demo-oai-0.openai.azure.com/openai'
    protocol: 'http'
    credentials: {
      query: {}
      header: {
        'api-key': [
          '{{EUS-FLLM-DEMO-OAI-0-primarykey}}'
          '{{EUS-FLLM-DEMO-OAI-0-secondarykey}}'
        ]
      }
    }
    tls: {
      validateCertificateChain: true
      validateCertificateName: true
    }
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_EUS_FLLM_DEMO_OAI_1_backend 'Microsoft.ApiManagement/service/backends@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'EUS-FLLM-DEMO-OAI-1-backend'
  properties: {
    url: 'https://eus-fllm-demo-oai-1.openai.azure.com/openai'
    protocol: 'http'
    credentials: {
      query: {}
      header: {
        'api-key': [
          '{{EUS-FLLM-DEMO-OAI-1-primarykey}}'
          '{{EUS-FLLM-DEMO-OAI-1-secondarykey}}'
        ]
      }
    }
    tls: {
      validateCertificateChain: true
      validateCertificateName: true
    }
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_administrators 'Microsoft.ApiManagement/service/groups@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'administrators'
  properties: {
    displayName: 'Administrators'
    description: 'Administrators is a built-in group containing the admin email account provided at the time of service creation. Its membership is managed by the system.'
    type: 'system'
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_developers 'Microsoft.ApiManagement/service/groups@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'developers'
  properties: {
    displayName: 'Developers'
    description: 'Developers is a built-in group. Its membership is managed by the system. Signed-in users fall into this group.'
    type: 'system'
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_guests 'Microsoft.ApiManagement/service/groups@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'guests'
  properties: {
    displayName: 'Guests'
    description: 'Guests is a built-in group. Its membership is managed by the system. Unauthenticated users visiting the developer portal fall into this group.'
    type: 'system'
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_azuremonitor 'Microsoft.ApiManagement/service/loggers@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'azuremonitor'
  properties: {
    loggerType: 'azureMonitor'
    isBuffered: true
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_EUS_FLLM_DEMO_OAI_0_primarykey 'Microsoft.ApiManagement/service/namedValues@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'EUS-FLLM-DEMO-OAI-0-primarykey'
  properties: {
    displayName: 'EUS-FLLM-DEMO-OAI-0-primarykey'
    keyVault: {
      secretIdentifier: 'https://eus-fllm-demo-oai-kv.vault.azure.net/secrets/EUS-FLLM-DEMO-OAI-0-primarykey/92e9b94340534942b7cb64f1024a3fec'
    }
    secret: true
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_EUS_FLLM_DEMO_OAI_0_secondarykey 'Microsoft.ApiManagement/service/namedValues@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'EUS-FLLM-DEMO-OAI-0-secondarykey'
  properties: {
    displayName: 'EUS-FLLM-DEMO-OAI-0-secondarykey'
    keyVault: {
      secretIdentifier: 'https://eus-fllm-demo-oai-kv.vault.azure.net/secrets/EUS-FLLM-DEMO-OAI-0-secondarykey/698374f980b04ef38d078115ef24a97e'
    }
    secret: true
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_EUS_FLLM_DEMO_OAI_1_primarykey 'Microsoft.ApiManagement/service/namedValues@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'EUS-FLLM-DEMO-OAI-1-primarykey'
  properties: {
    displayName: 'EUS-FLLM-DEMO-OAI-1-primarykey'
    keyVault: {
      secretIdentifier: 'https://eus-fllm-demo-oai-kv.vault.azure.net/secrets/EUS-FLLM-DEMO-OAI-1-primarykey/68de9c6db33f4250a07b6a6d7a90193a'
    }
    secret: true
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_EUS_FLLM_DEMO_OAI_1_secondarykey 'Microsoft.ApiManagement/service/namedValues@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'EUS-FLLM-DEMO-OAI-1-secondarykey'
  properties: {
    displayName: 'EUS-FLLM-DEMO-OAI-1-secondarykey'
    keyVault: {
      secretIdentifier: 'https://eus-fllm-demo-oai-kv.vault.azure.net/secrets/EUS-FLLM-DEMO-OAI-1-secondarykey/8f8af9f1172a49e4986c739af51b37ff'
    }
    secret: true
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_AccountClosedPublisher 'Microsoft.ApiManagement/service/notifications@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'AccountClosedPublisher'
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_BCC 'Microsoft.ApiManagement/service/notifications@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'BCC'
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_NewApplicationNotificationMessage 'Microsoft.ApiManagement/service/notifications@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'NewApplicationNotificationMessage'
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_NewIssuePublisherNotificationMessage 'Microsoft.ApiManagement/service/notifications@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'NewIssuePublisherNotificationMessage'
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_PurchasePublisherNotificationMessage 'Microsoft.ApiManagement/service/notifications@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'PurchasePublisherNotificationMessage'
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_QuotaLimitApproachingPublisherNotificationMessage 'Microsoft.ApiManagement/service/notifications@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'QuotaLimitApproachingPublisherNotificationMessage'
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_RequestPublisherNotificationMessage 'Microsoft.ApiManagement/service/notifications@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'RequestPublisherNotificationMessage'
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_policy 'Microsoft.ApiManagement/service/policies@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'policy'
  properties: {
    value: '<!--\r\n    IMPORTANT:\r\n    - Policy elements can appear only within the <inbound>, <outbound>, <backend> section elements.\r\n    - Only the <forward-request> policy element can appear within the <backend> section element.\r\n    - To apply a policy to the incoming request (before it is forwarded to the backend service), place a corresponding policy element within the <inbound> section element.\r\n    - To apply a policy to the outgoing response (before it is sent back to the caller), place a corresponding policy element within the <outbound> section element.\r\n    - To add a policy position the cursor at the desired insertion point and click on the round button associated with the policy.\r\n    - To remove a policy, delete the corresponding policy statement from the policy document.\r\n    - Policies are applied in the order of their appearance, from the top down.\r\n-->\r\n<policies>\r\n  <inbound />\r\n  <backend>\r\n    <forward-request />\r\n  </backend>\r\n  <outbound />\r\n</policies>'
    format: 'xml'
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_default 'Microsoft.ApiManagement/service/portalconfigs@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'default'
  properties: {
    enableBasicAuth: false
    signin: {
      require: false
    }
    signup: {
      termsOfService: {
        requireConsent: false
      }
    }
    delegation: {
      delegateRegistration: false
      delegateSubscription: false
    }
    cors: {
      allowedOrigins: []
    }
    csp: {
      mode: 'disabled'
      reportUri: []
      allowedSources: []
    }
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_delegation 'Microsoft.ApiManagement/service/portalsettings@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'delegation'
  properties: {
    subscriptions: {
      enabled: false
    }
    userRegistration: {
      enabled: false
    }
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_signin 'Microsoft.ApiManagement/service/portalsettings@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'signin'
  properties: {
    enabled: false
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_signup 'Microsoft.ApiManagement/service/portalsettings@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'signup'
  properties: {
    enabled: false
    termsOfService: {
      enabled: false
      consentRequired: false
    }
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_master 'Microsoft.ApiManagement/service/subscriptions@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'master'
  properties: {
    scope: '${service_EUS_FLLM_DEMO_OAI_apim_name_resource.id}/'
    displayName: 'Built-in all-access subscription'
    state: 'active'
    allowTracing: false
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_AccountClosedDeveloper 'Microsoft.ApiManagement/service/templates@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'AccountClosedDeveloper'
  properties: {
    subject: 'Thank you for using the $OrganizationName API!'
    body: '<!DOCTYPE html >\r\n<html>\r\n  <head />\r\n  <body>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">Dear $DevFirstName $DevLastName,</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n          On behalf of $OrganizationName and our customers we thank you for giving us a try. Your $OrganizationName API account is now closed.\r\n        </p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">Thank you,</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">Your $OrganizationName Team</p>\r\n    <a href="$DevPortalUrl">$DevPortalUrl</a>\r\n    <p />\r\n  </body>\r\n</html>'
    title: 'Developer farewell letter'
    description: 'Developers receive this farewell email after they close their account.'
    parameters: [
      {
        name: 'DevFirstName'
        title: 'Developer first name'
      }
      {
        name: 'DevLastName'
        title: 'Developer last name'
      }
      {
        name: 'OrganizationName'
        title: 'Organization name'
      }
      {
        name: 'DevPortalUrl'
        title: 'Developer portal URL'
      }
    ]
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_ApplicationApprovedNotificationMessage 'Microsoft.ApiManagement/service/templates@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'ApplicationApprovedNotificationMessage'
  properties: {
    subject: 'Your application $AppName is published in the application gallery'
    body: '<!DOCTYPE html >\r\n<html>\r\n  <head />\r\n  <body>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">Dear $DevFirstName $DevLastName,</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n          We are happy to let you know that your request to publish the $AppName application in the application gallery has been approved. Your application has been published and can be viewed <a href="http://$DevPortalUrl/Applications/Details/$AppId">here</a>.\r\n        </p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">Best,</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">The $OrganizationName API Team</p>\r\n  </body>\r\n</html>'
    title: 'Application gallery submission approved (deprecated)'
    description: 'Developers who submitted their application for publication in the application gallery on the developer portal receive this email after their submission is approved.'
    parameters: [
      {
        name: 'AppId'
        title: 'Application id'
      }
      {
        name: 'AppName'
        title: 'Application name'
      }
      {
        name: 'DevFirstName'
        title: 'Developer first name'
      }
      {
        name: 'DevLastName'
        title: 'Developer last name'
      }
      {
        name: 'OrganizationName'
        title: 'Organization name'
      }
      {
        name: 'DevPortalUrl'
        title: 'Developer portal URL'
      }
    ]
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_ConfirmSignUpIdentityDefault 'Microsoft.ApiManagement/service/templates@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'ConfirmSignUpIdentityDefault'
  properties: {
    subject: 'Please confirm your new $OrganizationName API account'
    body: '<!DOCTYPE html >\r\n<html>\r\n  <head>\r\n    <meta charset="UTF-8" />\r\n    <title>Letter</title>\r\n  </head>\r\n  <body>\r\n    <table width="100%">\r\n      <tr>\r\n        <td>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">Dear $DevFirstName $DevLastName,</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'"></p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">Thank you for joining the $OrganizationName API program! We host a growing number of cool APIs and strive to provide an awesome experience for API developers.</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">First order of business is to activate your account and get you going. To that end, please click on the following link:</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n            <a id="confirmUrl" href="$ConfirmUrl" style="text-decoration:none">\r\n              <strong>$ConfirmUrl</strong>\r\n            </a>\r\n          </p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">If clicking the link does not work, please copy-and-paste or re-type it into your browser\'s address bar and hit "Enter".</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">Thank you,</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">$OrganizationName API Team</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n            <a href="$DevPortalUrl">$DevPortalUrl</a>\r\n          </p>\r\n        </td>\r\n      </tr>\r\n    </table>\r\n  </body>\r\n</html>'
    title: 'New developer account confirmation'
    description: 'Developers receive this email to confirm their e-mail address after they sign up for a new account.'
    parameters: [
      {
        name: 'DevFirstName'
        title: 'Developer first name'
      }
      {
        name: 'DevLastName'
        title: 'Developer last name'
      }
      {
        name: 'OrganizationName'
        title: 'Organization name'
      }
      {
        name: 'DevPortalUrl'
        title: 'Developer portal URL'
      }
      {
        name: 'ConfirmUrl'
        title: 'Developer activation URL'
      }
      {
        name: 'DevPortalHost'
        title: 'Developer portal hostname'
      }
      {
        name: 'ConfirmQuery'
        title: 'Query string part of the activation URL'
      }
    ]
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_EmailChangeIdentityDefault 'Microsoft.ApiManagement/service/templates@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'EmailChangeIdentityDefault'
  properties: {
    subject: 'Please confirm the new email associated with your $OrganizationName API account'
    body: '<!DOCTYPE html >\r\n<html>\r\n  <head>\r\n    <meta charset="UTF-8" />\r\n    <title>Letter</title>\r\n  </head>\r\n  <body>\r\n    <table width="100%">\r\n      <tr>\r\n        <td>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">Dear $DevFirstName $DevLastName,</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'"></p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">You are receiving this email because you made a change to the email address on your $OrganizationName API account.</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">Please click on the following link to confirm the change:</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n            <a id="confirmUrl" href="$ConfirmUrl" style="text-decoration:none">\r\n              <strong>$ConfirmUrl</strong>\r\n            </a>\r\n          </p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">If clicking the link does not work, please copy-and-paste or re-type it into your browser\'s address bar and hit "Enter".</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">Thank you,</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">$OrganizationName API Team</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n            <a href="$DevPortalUrl">$DevPortalUrl</a>\r\n          </p>\r\n        </td>\r\n      </tr>\r\n    </table>\r\n  </body>\r\n</html>'
    title: 'Email change confirmation'
    description: 'Developers receive this email to confirm a new e-mail address after they change their existing one associated with their account.'
    parameters: [
      {
        name: 'DevFirstName'
        title: 'Developer first name'
      }
      {
        name: 'DevLastName'
        title: 'Developer last name'
      }
      {
        name: 'OrganizationName'
        title: 'Organization name'
      }
      {
        name: 'DevPortalUrl'
        title: 'Developer portal URL'
      }
      {
        name: 'ConfirmUrl'
        title: 'Developer confirmation URL'
      }
      {
        name: 'DevPortalHost'
        title: 'Developer portal hostname'
      }
      {
        name: 'ConfirmQuery'
        title: 'Query string part of the confirmation URL'
      }
    ]
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_InviteUserNotificationMessage 'Microsoft.ApiManagement/service/templates@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'InviteUserNotificationMessage'
  properties: {
    subject: 'You are invited to join the $OrganizationName developer network'
    body: '<!DOCTYPE html >\r\n<html>\r\n  <head />\r\n  <body>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">Dear $DevFirstName $DevLastName,</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n          Your account has been created. Please follow the link below to visit the $OrganizationName developer portal and claim it:\r\n        </p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n      <a href="$ConfirmUrl">$ConfirmUrl</a>\r\n    </p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">Best,</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">The $OrganizationName API Team</p>\r\n  </body>\r\n</html>'
    title: 'Invite user'
    description: 'An e-mail invitation to create an account, sent on request by API publishers.'
    parameters: [
      {
        name: 'OrganizationName'
        title: 'Organization name'
      }
      {
        name: 'DevFirstName'
        title: 'Developer first name'
      }
      {
        name: 'DevLastName'
        title: 'Developer last name'
      }
      {
        name: 'ConfirmUrl'
        title: 'Confirmation link'
      }
      {
        name: 'DevPortalHost'
        title: 'Developer portal hostname'
      }
      {
        name: 'ConfirmQuery'
        title: 'Query string part of the confirmation link'
      }
    ]
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_NewCommentNotificationMessage 'Microsoft.ApiManagement/service/templates@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'NewCommentNotificationMessage'
  properties: {
    subject: '$IssueName issue has a new comment'
    body: '<!DOCTYPE html >\r\n<html>\r\n  <head />\r\n  <body>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">Dear $DevFirstName $DevLastName,</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">This is a brief note to let you know that $CommenterFirstName $CommenterLastName made the following comment on the issue $IssueName you created:</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">$CommentText</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n          To view the issue on the developer portal click <a href="http://$DevPortalUrl/issues/$IssueId">here</a>.\r\n        </p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">Best,</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">The $OrganizationName API Team</p>\r\n  </body>\r\n</html>'
    title: 'New comment added to an issue (deprecated)'
    description: 'Developers receive this email when someone comments on the issue they created on the Issues page of the developer portal.'
    parameters: [
      {
        name: 'DevFirstName'
        title: 'Developer first name'
      }
      {
        name: 'DevLastName'
        title: 'Developer last name'
      }
      {
        name: 'CommenterFirstName'
        title: 'Commenter first name'
      }
      {
        name: 'CommenterLastName'
        title: 'Commenter last name'
      }
      {
        name: 'IssueId'
        title: 'Issue id'
      }
      {
        name: 'IssueName'
        title: 'Issue name'
      }
      {
        name: 'CommentText'
        title: 'Comment text'
      }
      {
        name: 'OrganizationName'
        title: 'Organization name'
      }
      {
        name: 'DevPortalUrl'
        title: 'Developer portal URL'
      }
    ]
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_NewDeveloperNotificationMessage 'Microsoft.ApiManagement/service/templates@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'NewDeveloperNotificationMessage'
  properties: {
    subject: 'Welcome to the $OrganizationName API!'
    body: '<!DOCTYPE html >\r\n<html>\r\n  <head>\r\n    <meta charset="UTF-8" />\r\n    <title>Letter</title>\r\n  </head>\r\n  <body>\r\n    <h1 style="color:#000505;font-size:18pt;font-family:\'Segoe UI\'">\r\n          Welcome to <span style="color:#003363">$OrganizationName API!</span></h1>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">Dear $DevFirstName $DevLastName,</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">Your $OrganizationName API program registration is completed and we are thrilled to have you as a customer. Here are a few important bits of information for your reference:</p>\r\n    <table width="100%" style="margin:20px 0">\r\n      <tr>\r\n            #if ($IdentityProvider == "Basic")\r\n            <td width="50%" style="height:40px;vertical-align:top;font-family:\'Segoe UI\';font-size:12pt">\r\n              Please use the following <strong>username</strong> when signing into any of the \${OrganizationName}-hosted developer portals:\r\n            </td><td style="vertical-align:top;font-family:\'Segoe UI\';font-size:12pt"><strong>$DevUsername</strong></td>\r\n            #else\r\n            <td width="50%" style="height:40px;vertical-align:top;font-family:\'Segoe UI\';font-size:12pt">\r\n              Please use the following <strong>$IdentityProvider account</strong> when signing into any of the \${OrganizationName}-hosted developer portals:\r\n            </td><td style="vertical-align:top;font-family:\'Segoe UI\';font-size:12pt"><strong>$DevUsername</strong></td>            \r\n            #end\r\n          </tr>\r\n      <tr>\r\n        <td style="height:40px;vertical-align:top;font-family:\'Segoe UI\';font-size:12pt">\r\n              We will direct all communications to the following <strong>email address</strong>:\r\n            </td>\r\n        <td style="vertical-align:top;font-family:\'Segoe UI\';font-size:12pt">\r\n          <a href="mailto:$DevEmail" style="text-decoration:none">\r\n            <strong>$DevEmail</strong>\r\n          </a>\r\n        </td>\r\n      </tr>\r\n    </table>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">Best of luck in your API pursuits!</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">$OrganizationName API Team</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n      <a href="http://$DevPortalUrl">$DevPortalUrl</a>\r\n    </p>\r\n  </body>\r\n</html>'
    title: 'Developer welcome letter'
    description: 'Developers receive this “welcome” email after they confirm their new account.'
    parameters: [
      {
        name: 'DevFirstName'
        title: 'Developer first name'
      }
      {
        name: 'DevLastName'
        title: 'Developer last name'
      }
      {
        name: 'DevUsername'
        title: 'Developer user name'
      }
      {
        name: 'DevEmail'
        title: 'Developer email'
      }
      {
        name: 'OrganizationName'
        title: 'Organization name'
      }
      {
        name: 'DevPortalUrl'
        title: 'Developer portal URL'
      }
      {
        name: 'IdentityProvider'
        title: 'Identity Provider selected by Organization'
      }
    ]
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_NewIssueNotificationMessage 'Microsoft.ApiManagement/service/templates@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'NewIssueNotificationMessage'
  properties: {
    subject: 'Your request $IssueName was received'
    body: '<!DOCTYPE html >\r\n<html>\r\n  <head />\r\n  <body>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">Dear $DevFirstName $DevLastName,</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">Thank you for contacting us. Our API team will review your issue and get back to you soon.</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n          Click this <a href="http://$DevPortalUrl/issues/$IssueId">link</a> to view or edit your request.\r\n        </p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">Best,</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">The $OrganizationName API Team</p>\r\n  </body>\r\n</html>'
    title: 'New issue received (deprecated)'
    description: 'This email is sent to developers after they create a new topic on the Issues page of the developer portal.'
    parameters: [
      {
        name: 'DevFirstName'
        title: 'Developer first name'
      }
      {
        name: 'DevLastName'
        title: 'Developer last name'
      }
      {
        name: 'IssueId'
        title: 'Issue id'
      }
      {
        name: 'IssueName'
        title: 'Issue name'
      }
      {
        name: 'OrganizationName'
        title: 'Organization name'
      }
      {
        name: 'DevPortalUrl'
        title: 'Developer portal URL'
      }
    ]
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_PasswordResetByAdminNotificationMessage 'Microsoft.ApiManagement/service/templates@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'PasswordResetByAdminNotificationMessage'
  properties: {
    subject: 'Your password was reset'
    body: '<!DOCTYPE html >\r\n<html>\r\n  <head />\r\n  <body>\r\n    <table width="100%">\r\n      <tr>\r\n        <td>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">Dear $DevFirstName $DevLastName,</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'"></p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">The password of your $OrganizationName API account has been reset, per your request.</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n                Your new password is: <strong>$DevPassword</strong></p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">Please make sure to change it next time you sign in.</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">Thank you,</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">$OrganizationName API Team</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n            <a href="$DevPortalUrl">$DevPortalUrl</a>\r\n          </p>\r\n        </td>\r\n      </tr>\r\n    </table>\r\n  </body>\r\n</html>'
    title: 'Password reset by publisher notification (Password reset by admin)'
    description: 'Developers receive this email when the publisher resets their password.'
    parameters: [
      {
        name: 'DevFirstName'
        title: 'Developer first name'
      }
      {
        name: 'DevLastName'
        title: 'Developer last name'
      }
      {
        name: 'DevPassword'
        title: 'New Developer password'
      }
      {
        name: 'OrganizationName'
        title: 'Organization name'
      }
      {
        name: 'DevPortalUrl'
        title: 'Developer portal URL'
      }
    ]
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_PasswordResetIdentityDefault 'Microsoft.ApiManagement/service/templates@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'PasswordResetIdentityDefault'
  properties: {
    subject: 'Your password change request'
    body: '<!DOCTYPE html >\r\n<html>\r\n  <head>\r\n    <meta charset="UTF-8" />\r\n    <title>Letter</title>\r\n  </head>\r\n  <body>\r\n    <table width="100%">\r\n      <tr>\r\n        <td>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">Dear $DevFirstName $DevLastName,</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'"></p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">You are receiving this email because you requested to change the password on your $OrganizationName API account.</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">Please click on the link below and follow instructions to create your new password:</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n            <a id="resetUrl" href="$ConfirmUrl" style="text-decoration:none">\r\n              <strong>$ConfirmUrl</strong>\r\n            </a>\r\n          </p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">If clicking the link does not work, please copy-and-paste or re-type it into your browser\'s address bar and hit "Enter".</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">Thank you,</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">$OrganizationName API Team</p>\r\n          <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n            <a href="$DevPortalUrl">$DevPortalUrl</a>\r\n          </p>\r\n        </td>\r\n      </tr>\r\n    </table>\r\n  </body>\r\n</html>'
    title: 'Password change confirmation'
    description: 'Developers receive this email when they request a password change of their account. The purpose of the email is to verify that the account owner made the request and to provide a one-time perishable URL for changing the password.'
    parameters: [
      {
        name: 'DevFirstName'
        title: 'Developer first name'
      }
      {
        name: 'DevLastName'
        title: 'Developer last name'
      }
      {
        name: 'OrganizationName'
        title: 'Organization name'
      }
      {
        name: 'DevPortalUrl'
        title: 'Developer portal URL'
      }
      {
        name: 'ConfirmUrl'
        title: 'Developer new password instruction URL'
      }
      {
        name: 'DevPortalHost'
        title: 'Developer portal hostname'
      }
      {
        name: 'ConfirmQuery'
        title: 'Query string part of the instruction URL'
      }
    ]
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_PurchaseDeveloperNotificationMessage 'Microsoft.ApiManagement/service/templates@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'PurchaseDeveloperNotificationMessage'
  properties: {
    subject: 'Your subscription to the $ProdName'
    body: '<!DOCTYPE html >\r\n<html>\r\n  <head />\r\n  <body>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">Greetings $DevFirstName $DevLastName!</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n          Thank you for subscribing to the <a href="http://$DevPortalUrl/products/$ProdId"><strong>$ProdName</strong></a> and welcome to the $OrganizationName developer community. We are delighted to have you as part of the team and are looking forward to the amazing applications you will build using our API!\r\n        </p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">Below are a few subscription details for your reference:</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n      <ul>\r\n            #if ($SubStartDate != "")\r\n            <li style="font-size:12pt;font-family:\'Segoe UI\'">Start date: $SubStartDate</li>\r\n            #end\r\n            \r\n            #if ($SubTerm != "")\r\n            <li style="font-size:12pt;font-family:\'Segoe UI\'">Subscription term: $SubTerm</li>\r\n            #end\r\n          </ul>\r\n    </p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n            Visit the developer <a href="http://$DevPortalUrl/developer">profile area</a> to manage your subscription and subscription keys\r\n        </p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">A couple of pointers to help get you started:</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n      <strong>\r\n        <a href="http://$DevPortalUrl/docs/services?product=$ProdId">Learn about the API</a>\r\n      </strong>\r\n    </p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">The API documentation provides all information necessary to make a request and to process a response. Code samples are provided per API operation in a variety of languages. Moreover, an interactive console allows making API calls directly from the developer portal without writing any code.</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n      <strong>\r\n        <a href="http://$DevPortalUrl/applications">Feature your app in the app gallery</a>\r\n      </strong>\r\n    </p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">You can publish your application on our gallery for increased visibility to potential new users.</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n      <strong>\r\n        <a href="http://$DevPortalUrl/issues">Stay in touch</a>\r\n      </strong>\r\n    </p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n          If you have an issue, a question, a suggestion, a request, or if you just want to tell us something, go to the <a href="http://$DevPortalUrl/issues">Issues</a> page on the developer portal and create a new topic.\r\n        </p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">Happy hacking,</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">The $OrganizationName API Team</p>\r\n    <a style="font-size:12pt;font-family:\'Segoe UI\'" href="http://$DevPortalUrl">$DevPortalUrl</a>\r\n  </body>\r\n</html>'
    title: 'New subscription activated'
    description: 'Developers receive this acknowledgement email after subscribing to a product.'
    parameters: [
      {
        name: 'DevFirstName'
        title: 'Developer first name'
      }
      {
        name: 'DevLastName'
        title: 'Developer last name'
      }
      {
        name: 'ProdId'
        title: 'Product ID'
      }
      {
        name: 'ProdName'
        title: 'Product name'
      }
      {
        name: 'OrganizationName'
        title: 'Organization name'
      }
      {
        name: 'SubStartDate'
        title: 'Subscription start date'
      }
      {
        name: 'SubTerm'
        title: 'Subscription term'
      }
      {
        name: 'DevPortalUrl'
        title: 'Developer portal URL'
      }
    ]
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_QuotaLimitApproachingDeveloperNotificationMessage 'Microsoft.ApiManagement/service/templates@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'QuotaLimitApproachingDeveloperNotificationMessage'
  properties: {
    subject: 'You are approaching an API quota limit'
    body: '<!DOCTYPE html >\r\n<html>\r\n  <head>\r\n    <style>\r\n          body {font-size:12pt; font-family:"Segoe UI","Segoe WP","Tahoma","Arial","sans-serif";}\r\n          .alert { color: red; }\r\n          .child1 { padding-left: 20px; }\r\n          .child2 { padding-left: 40px; }\r\n          .number { text-align: right; }\r\n          .text { text-align: left; }\r\n          th, td { padding: 4px 10px; min-width: 100px; }\r\n          th { background-color: #DDDDDD;}\r\n        </style>\r\n  </head>\r\n  <body>\r\n    <p>Greetings $DevFirstName $DevLastName!</p>\r\n    <p>\r\n          You are approaching the quota limit on you subscription to the <strong>$ProdName</strong> product (primary key $SubPrimaryKey).\r\n          #if ($QuotaResetDate != "")\r\n          This quota will be renewed on $QuotaResetDate.\r\n          #else\r\n          This quota will not be renewed.\r\n          #end\r\n        </p>\r\n    <p>Below are details on quota usage for the subscription:</p>\r\n    <p>\r\n      <table>\r\n        <thead>\r\n          <th class="text">Quota Scope</th>\r\n          <th class="number">Calls</th>\r\n          <th class="number">Call Quota</th>\r\n          <th class="number">Bandwidth</th>\r\n          <th class="number">Bandwidth Quota</th>\r\n        </thead>\r\n        <tbody>\r\n          <tr>\r\n            <td class="text">Subscription</td>\r\n            <td class="number">\r\n                  #if ($CallsAlert == true)\r\n                  <span class="alert">$Calls</span>\r\n                  #else\r\n                  $Calls\r\n                  #end\r\n                </td>\r\n            <td class="number">$CallQuota</td>\r\n            <td class="number">\r\n                  #if ($BandwidthAlert == true)\r\n                  <span class="alert">$Bandwidth</span>\r\n                  #else\r\n                  $Bandwidth\r\n                  #end\r\n                </td>\r\n            <td class="number">$BandwidthQuota</td>\r\n          </tr>\r\n              #foreach ($api in $Apis)\r\n              <tr><td class="child1 text">API: $api.Name</td><td class="number">\r\n                  #if ($api.CallsAlert == true)\r\n                  <span class="alert">$api.Calls</span>\r\n                  #else\r\n                  $api.Calls\r\n                  #end\r\n                </td><td class="number">$api.CallQuota</td><td class="number">\r\n                  #if ($api.BandwidthAlert == true)\r\n                  <span class="alert">$api.Bandwidth</span>\r\n                  #else\r\n                  $api.Bandwidth\r\n                  #end\r\n                </td><td class="number">$api.BandwidthQuota</td></tr>\r\n              #foreach ($operation in $api.Operations)\r\n              <tr><td class="child2 text">Operation: $operation.Name</td><td class="number">\r\n                  #if ($operation.CallsAlert == true)\r\n                  <span class="alert">$operation.Calls</span>\r\n                  #else\r\n                  $operation.Calls\r\n                  #end\r\n                </td><td class="number">$operation.CallQuota</td><td class="number">\r\n                  #if ($operation.BandwidthAlert == true)\r\n                  <span class="alert">$operation.Bandwidth</span>\r\n                  #else\r\n                  $operation.Bandwidth\r\n                  #end\r\n                </td><td class="number">$operation.BandwidthQuota</td></tr>\r\n              #end\r\n              #end\r\n            </tbody>\r\n      </table>\r\n    </p>\r\n    <p>Thank you,</p>\r\n    <p>$OrganizationName API Team</p>\r\n    <a href="$DevPortalUrl">$DevPortalUrl</a>\r\n    <p />\r\n  </body>\r\n</html>'
    title: 'Developer quota limit approaching notification'
    description: 'Developers receive this email to alert them when they are approaching a quota limit.'
    parameters: [
      {
        name: 'DevFirstName'
        title: 'Developer first name'
      }
      {
        name: 'DevLastName'
        title: 'Developer last name'
      }
      {
        name: 'ProdName'
        title: 'Product name'
      }
      {
        name: 'OrganizationName'
        title: 'Organization name'
      }
      {
        name: 'SubPrimaryKey'
        title: 'Primary Subscription key'
      }
      {
        name: 'DevPortalUrl'
        title: 'Developer portal URL'
      }
      {
        name: 'QuotaResetDate'
        title: 'Quota reset date'
      }
    ]
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_RejectDeveloperNotificationMessage 'Microsoft.ApiManagement/service/templates@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'RejectDeveloperNotificationMessage'
  properties: {
    subject: 'Your subscription request for the $ProdName'
    body: '<!DOCTYPE html >\r\n<html>\r\n  <head />\r\n  <body>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">Dear $DevFirstName $DevLastName,</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n          We would like to inform you that we reviewed your subscription request for the <strong>$ProdName</strong>.\r\n        </p>\r\n        #if ($SubDeclineReason == "")\r\n        <p style="font-size:12pt;font-family:\'Segoe UI\'">Regretfully, we were unable to approve it, as subscriptions are temporarily suspended at this time.</p>\r\n        #else\r\n        <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n          Regretfully, we were unable to approve it at this time for the following reason:\r\n          <div style="margin-left: 1.5em;"> $SubDeclineReason </div></p>\r\n        #end\r\n        <p style="font-size:12pt;font-family:\'Segoe UI\'"> We truly appreciate your interest. </p><p style="font-size:12pt;font-family:\'Segoe UI\'">All the best,</p><p style="font-size:12pt;font-family:\'Segoe UI\'">The $OrganizationName API Team</p><a style="font-size:12pt;font-family:\'Segoe UI\'" href="http://$DevPortalUrl">$DevPortalUrl</a></body>\r\n</html>'
    title: 'Subscription request declined'
    description: 'This email is sent to developers when their subscription requests for products requiring publisher approval is declined.'
    parameters: [
      {
        name: 'DevFirstName'
        title: 'Developer first name'
      }
      {
        name: 'DevLastName'
        title: 'Developer last name'
      }
      {
        name: 'SubDeclineReason'
        title: 'Reason for declining subscription'
      }
      {
        name: 'ProdName'
        title: 'Product name'
      }
      {
        name: 'OrganizationName'
        title: 'Organization name'
      }
      {
        name: 'DevPortalUrl'
        title: 'Developer portal URL'
      }
    ]
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_RequestDeveloperNotificationMessage 'Microsoft.ApiManagement/service/templates@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'RequestDeveloperNotificationMessage'
  properties: {
    subject: 'Your subscription request for the $ProdName'
    body: '<!DOCTYPE html >\r\n<html>\r\n  <head />\r\n  <body>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">Dear $DevFirstName $DevLastName,</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n          Thank you for your interest in our <strong>$ProdName</strong> API product!\r\n        </p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">\r\n          We were delighted to receive your subscription request. We will promptly review it and get back to you at <strong>$DevEmail</strong>.\r\n        </p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">Thank you,</p>\r\n    <p style="font-size:12pt;font-family:\'Segoe UI\'">The $OrganizationName API Team</p>\r\n    <a style="font-size:12pt;font-family:\'Segoe UI\'" href="http://$DevPortalUrl">$DevPortalUrl</a>\r\n  </body>\r\n</html>'
    title: 'Subscription request received'
    description: 'This email is sent to developers to acknowledge receipt of their subscription requests for products requiring publisher approval.'
    parameters: [
      {
        name: 'DevFirstName'
        title: 'Developer first name'
      }
      {
        name: 'DevLastName'
        title: 'Developer last name'
      }
      {
        name: 'DevEmail'
        title: 'Developer email'
      }
      {
        name: 'ProdName'
        title: 'Product name'
      }
      {
        name: 'OrganizationName'
        title: 'Organization name'
      }
      {
        name: 'DevPortalUrl'
        title: 'Developer portal URL'
      }
    ]
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_1 'Microsoft.ApiManagement/service/users@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: '1'
  properties: {
    firstName: 'Administrator'
    email: 'info@solliance.net'
    state: 'active'
    identities: [
      {
        provider: 'Azure'
        id: 'info@solliance.net'
      }
    ]
    lastName: users_1_lastName
  }
}

resource accounts_EUS_FLLM_DEMO_OAI_0_openai_name_completions 'Microsoft.CognitiveServices/accounts/deployments@2023-10-01-preview' = {
  parent: accounts_EUS_FLLM_DEMO_OAI_0_openai_name_resource
  name: 'completions'
  sku: {
    name: 'Standard'
    capacity: 120
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-35-turbo'
      version: '0301'
    }
    versionUpgradeOption: 'OnceNewDefaultVersionAvailable'
    currentCapacity: 120
  }
}

resource accounts_EUS_FLLM_DEMO_OAI_1_openai_name_completions 'Microsoft.CognitiveServices/accounts/deployments@2023-10-01-preview' = {
  parent: accounts_EUS_FLLM_DEMO_OAI_1_openai_name_resource
  name: 'completions'
  sku: {
    name: 'Standard'
    capacity: 120
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-35-turbo'
      version: '0301'
    }
    versionUpgradeOption: 'OnceNewDefaultVersionAvailable'
    currentCapacity: 120
  }
}

resource accounts_EUS_FLLM_DEMO_OAI_0_openai_name_embeddings 'Microsoft.CognitiveServices/accounts/deployments@2023-10-01-preview' = {
  parent: accounts_EUS_FLLM_DEMO_OAI_0_openai_name_resource
  name: 'embeddings'
  sku: {
    name: 'Standard'
    capacity: 120
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'text-embedding-ada-002'
      version: '2'
    }
    versionUpgradeOption: 'OnceNewDefaultVersionAvailable'
    currentCapacity: 120
    raiPolicyName: 'Microsoft.Default'
  }
}

resource accounts_EUS_FLLM_DEMO_OAI_1_openai_name_embeddings 'Microsoft.CognitiveServices/accounts/deployments@2023-10-01-preview' = {
  parent: accounts_EUS_FLLM_DEMO_OAI_1_openai_name_resource
  name: 'embeddings'
  sku: {
    name: 'Standard'
    capacity: 120
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'text-embedding-ada-002'
      version: '2'
    }
    versionUpgradeOption: 'OnceNewDefaultVersionAvailable'
    currentCapacity: 120
    raiPolicyName: 'Microsoft.Default'
  }
}

resource accounts_EUS_FLLM_DEMO_OAI_0_openai_name_accounts_EUS_FLLM_DEMO_OAI_0_openai_name_pe_00dddfb6_4018_4472_834f_6186e9cccac2 'Microsoft.CognitiveServices/accounts/privateEndpointConnections@2023-10-01-preview' = {
  parent: accounts_EUS_FLLM_DEMO_OAI_0_openai_name_resource
  name: '${accounts_EUS_FLLM_DEMO_OAI_0_openai_name}-pe.00dddfb6-4018-4472-834f-6186e9cccac2'
  location: 'eastus'
  properties: {
    privateEndpoint: {}
    groupIds: [
      'account'
    ]
    privateLinkServiceConnectionState: {
      status: 'Approved'
      description: 'Approved'
      actionsRequired: 'None'
    }
    provisioningState: 'Succeeded'
  }
}

resource accounts_EUS_FLLM_DEMO_OAI_1_openai_name_accounts_EUS_FLLM_DEMO_OAI_1_openai_name_pe_a7d17ac1_3964_4b7f_b484_42236527d105 'Microsoft.CognitiveServices/accounts/privateEndpointConnections@2023-10-01-preview' = {
  parent: accounts_EUS_FLLM_DEMO_OAI_1_openai_name_resource
  name: '${accounts_EUS_FLLM_DEMO_OAI_1_openai_name}-pe.a7d17ac1-3964-4b7f-b484-42236527d105'
  location: 'eastus'
  properties: {
    privateEndpoint: {}
    groupIds: [
      'account'
    ]
    privateLinkServiceConnectionState: {
      status: 'Approved'
      description: 'Approved'
      actionsRequired: 'None'
    }
    provisioningState: 'Succeeded'
  }
}

resource accounts_EUS_FLLM_DEMO_OAI_content_safety_name_accounts_EUS_FLLM_DEMO_OAI_content_safety_name_pe_fcef19f3_241e_4911_855e_44f70af0f3c2 'Microsoft.CognitiveServices/accounts/privateEndpointConnections@2023-10-01-preview' = {
  parent: accounts_EUS_FLLM_DEMO_OAI_content_safety_name_resource
  name: '${accounts_EUS_FLLM_DEMO_OAI_content_safety_name}-pe.fcef19f3-241e-4911-855e-44f70af0f3c2'
  location: 'eastus'
  properties: {
    privateEndpoint: {}
    groupIds: [
      'account'
    ]
    privateLinkServiceConnectionState: {
      status: 'Approved'
      description: 'Approved'
      actionsRequired: 'None'
    }
    provisioningState: 'Succeeded'
  }
}

resource accounts_EUS_FLLM_DEMO_OAI_0_openai_name_Microsoft_Default 'Microsoft.CognitiveServices/accounts/raiPolicies@2023-10-01-preview' = {
  parent: accounts_EUS_FLLM_DEMO_OAI_0_openai_name_resource
  name: 'Microsoft.Default'
  properties: {
    mode: 'Blocking'
    contentFilters: [
      {
        allowedContentLevel: 'Medium'
        blocking: true
        enabled: true
        source: 'Prompt'
      }
      {
        allowedContentLevel: 'Medium'
        blocking: true
        enabled: true
        source: 'Completion'
      }
      {
        allowedContentLevel: 'Medium'
        blocking: true
        enabled: true
        source: 'Prompt'
      }
      {
        allowedContentLevel: 'Medium'
        blocking: true
        enabled: true
        source: 'Completion'
      }
      {
        allowedContentLevel: 'Medium'
        blocking: true
        enabled: true
        source: 'Prompt'
      }
      {
        allowedContentLevel: 'Medium'
        blocking: true
        enabled: true
        source: 'Completion'
      }
      {
        allowedContentLevel: 'Medium'
        blocking: true
        enabled: true
        source: 'Prompt'
      }
      {
        allowedContentLevel: 'Medium'
        blocking: true
        enabled: true
        source: 'Completion'
      }
    ]
  }
}

resource accounts_EUS_FLLM_DEMO_OAI_1_openai_name_Microsoft_Default 'Microsoft.CognitiveServices/accounts/raiPolicies@2023-10-01-preview' = {
  parent: accounts_EUS_FLLM_DEMO_OAI_1_openai_name_resource
  name: 'Microsoft.Default'
  properties: {
    mode: 'Blocking'
    contentFilters: [
      {
        allowedContentLevel: 'Medium'
        blocking: true
        enabled: true
        source: 'Prompt'
      }
      {
        allowedContentLevel: 'Medium'
        blocking: true
        enabled: true
        source: 'Completion'
      }
      {
        allowedContentLevel: 'Medium'
        blocking: true
        enabled: true
        source: 'Prompt'
      }
      {
        allowedContentLevel: 'Medium'
        blocking: true
        enabled: true
        source: 'Completion'
      }
      {
        allowedContentLevel: 'Medium'
        blocking: true
        enabled: true
        source: 'Prompt'
      }
      {
        allowedContentLevel: 'Medium'
        blocking: true
        enabled: true
        source: 'Completion'
      }
      {
        allowedContentLevel: 'Medium'
        blocking: true
        enabled: true
        source: 'Prompt'
      }
      {
        allowedContentLevel: 'Medium'
        blocking: true
        enabled: true
        source: 'Completion'
      }
    ]
  }
}

resource metricAlerts_EUS_FLLM_DEMO_OAI_apim_capacity_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_OAI_apim_capacity_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'OpenAI'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    description: 'Service capacity greater than 75% for 1 hour'
    severity: 0
    enabled: true
    scopes: [
      service_EUS_FLLM_DEMO_OAI_apim_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT1H'
    criteria: {
      allOf: [
        {
          threshold: 75
          name: 'Metric1'
          metricNamespace: 'Microsoft.ApiManagement/service'
          metricName: 'Capacity'
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

resource metricAlerts_EUS_FLLM_DEMO_OAI_kv_availability_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_OAI_kv_availability_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'OpenAI'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    description: 'Service availability less than 99% for 1 hour'
    severity: 0
    enabled: true
    scopes: [
      vaults_EUS_FLLM_DEMO_OAI_kv_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT1H'
    criteria: {
      allOf: [
        {
          threshold: 99
          name: 'Metric1'
          metricNamespace: 'Microsoft.KeyVault/vaults'
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

resource metricAlerts_EUS_FLLM_DEMO_OAI_kv_latency_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_OAI_kv_latency_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'OpenAI'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    description: 'Service latency more than 1000ms for 1 hour'
    severity: 0
    enabled: true
    scopes: [
      vaults_EUS_FLLM_DEMO_OAI_kv_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT1H'
    criteria: {
      allOf: [
        {
          threshold: 1000
          name: 'Metric1'
          metricNamespace: 'Microsoft.KeyVault/vaults'
          metricName: 'ServiceApiLatency'
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

resource metricAlerts_EUS_FLLM_DEMO_OAI_kv_saturation_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_OAI_kv_saturation_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'OpenAI'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    description: 'Service saturation more than 75% for 1 hour'
    severity: 0
    enabled: true
    scopes: [
      vaults_EUS_FLLM_DEMO_OAI_kv_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT1H'
    criteria: {
      allOf: [
        {
          threshold: 75
          name: 'Metric1'
          metricNamespace: 'Microsoft.KeyVault/vaults'
          metricName: 'SaturationShoebox'
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

resource metricAlerts_EUS_FLLM_DEMO_OAI_openai_openai0_availability_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_OAI_openai_openai0_availability_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'OpenAI'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    description: 'Service availability less than 99% for 1 hour'
    severity: 0
    enabled: true
    scopes: [
      accounts_EUS_FLLM_DEMO_OAI_0_openai_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT1H'
    criteria: {
      allOf: [
        {
          threshold: 99
          name: 'Metric1'
          metricNamespace: 'Microsoft.CognitiveServices/accounts'
          metricName: 'SuccessRate'
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

resource metricAlerts_EUS_FLLM_DEMO_OAI_openai_openai0_latency_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_OAI_openai_openai0_latency_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'OpenAI'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    description: 'Service latency greater than 1000ms for 1 hour'
    severity: 0
    enabled: true
    scopes: [
      accounts_EUS_FLLM_DEMO_OAI_0_openai_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT1H'
    criteria: {
      allOf: [
        {
          threshold: 1000
          name: 'Metric1'
          metricNamespace: 'Microsoft.CognitiveServices/accounts'
          metricName: 'Latency'
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

resource metricAlerts_EUS_FLLM_DEMO_OAI_openai_openai1_availability_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_OAI_openai_openai1_availability_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'OpenAI'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    description: 'Service availability less than 99% for 1 hour'
    severity: 0
    enabled: true
    scopes: [
      accounts_EUS_FLLM_DEMO_OAI_1_openai_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT1H'
    criteria: {
      allOf: [
        {
          threshold: 99
          name: 'Metric1'
          metricNamespace: 'Microsoft.CognitiveServices/accounts'
          metricName: 'SuccessRate'
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

resource metricAlerts_EUS_FLLM_DEMO_OAI_openai_openai1_latency_alert_name_resource 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlerts_EUS_FLLM_DEMO_OAI_openai_openai1_latency_alert_name
  location: 'global'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'OpenAI'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    description: 'Service latency greater than 1000ms for 1 hour'
    severity: 0
    enabled: true
    scopes: [
      accounts_EUS_FLLM_DEMO_OAI_1_openai_name_resource.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT1H'
    criteria: {
      allOf: [
        {
          threshold: 1000
          name: 'Metric1'
          metricNamespace: 'Microsoft.CognitiveServices/accounts'
          metricName: 'Latency'
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

resource vaults_EUS_FLLM_DEMO_OAI_kv_name_vaults_EUS_FLLM_DEMO_OAI_kv_name_connection 'Microsoft.KeyVault/vaults/privateEndpointConnections@2023-07-01' = {
  parent: vaults_EUS_FLLM_DEMO_OAI_kv_name_resource
  name: '${vaults_EUS_FLLM_DEMO_OAI_kv_name}-connection'
  location: 'eastus'
  properties: {
    provisioningState: 'Succeeded'
    privateEndpoint: {}
    privateLinkServiceConnectionState: {
      status: 'Approved'
      actionsRequired: 'None'
    }
  }
}

resource vaults_EUS_FLLM_DEMO_OAI_kv_name_EUS_FLLM_DEMO_OAI_0_primarykey 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: vaults_EUS_FLLM_DEMO_OAI_kv_name_resource
  name: 'EUS-FLLM-DEMO-OAI-0-primarykey'
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'OpenAI'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    attributes: {
      enabled: true
    }
  }
}

resource vaults_EUS_FLLM_DEMO_OAI_kv_name_EUS_FLLM_DEMO_OAI_0_secondarykey 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: vaults_EUS_FLLM_DEMO_OAI_kv_name_resource
  name: 'EUS-FLLM-DEMO-OAI-0-secondarykey'
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'OpenAI'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    attributes: {
      enabled: true
    }
  }
}

resource vaults_EUS_FLLM_DEMO_OAI_kv_name_EUS_FLLM_DEMO_OAI_1_primarykey 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: vaults_EUS_FLLM_DEMO_OAI_kv_name_resource
  name: 'EUS-FLLM-DEMO-OAI-1-primarykey'
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'OpenAI'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    attributes: {
      enabled: true
    }
  }
}

resource vaults_EUS_FLLM_DEMO_OAI_kv_name_EUS_FLLM_DEMO_OAI_1_secondarykey 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: vaults_EUS_FLLM_DEMO_OAI_kv_name_resource
  name: 'EUS-FLLM-DEMO-OAI-1-secondarykey'
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'OpenAI'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    attributes: {
      enabled: true
    }
  }
}

resource privateEndpoints_EUS_FLLM_DEMO_OAI_0_openai_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_OAI_0_openai_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'OpenAI'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'EUS-FLLM-DEMO-OAI-0-openai-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_OAI_0_openai_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-OAI-0-openai-connection'
        properties: {
          privateLinkServiceId: accounts_EUS_FLLM_DEMO_OAI_0_openai_name_resource.id
          groupIds: [
            'account'
          ]
          privateLinkServiceConnectionState: {
            status: 'Approved'
            description: 'Approved'
            actionsRequired: 'None'
          }
        }
      }
    ]
    manualPrivateLinkServiceConnections: []
    subnet: {
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/FLLMOpenAI'
    }
    ipConfigurations: []
    customDnsConfigs: []
  }
}

resource privateEndpoints_EUS_FLLM_DEMO_OAI_1_openai_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_OAI_1_openai_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'OpenAI'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'EUS-FLLM-DEMO-OAI-1-openai-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_OAI_1_openai_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-OAI-1-openai-connection'
        properties: {
          privateLinkServiceId: accounts_EUS_FLLM_DEMO_OAI_1_openai_name_resource.id
          groupIds: [
            'account'
          ]
          privateLinkServiceConnectionState: {
            status: 'Approved'
            description: 'Approved'
            actionsRequired: 'None'
          }
        }
      }
    ]
    manualPrivateLinkServiceConnections: []
    subnet: {
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/FLLMOpenAI'
    }
    ipConfigurations: []
    customDnsConfigs: []
  }
}

resource privateEndpoints_EUS_FLLM_DEMO_OAI_content_safety_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_OAI_content_safety_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'OpenAI'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'EUS-FLLM-DEMO-OAI-content-safety-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_OAI_content_safety_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-OAI-content-safety-connection'
        properties: {
          privateLinkServiceId: accounts_EUS_FLLM_DEMO_OAI_content_safety_name_resource.id
          groupIds: [
            'account'
          ]
          privateLinkServiceConnectionState: {
            status: 'Approved'
            description: 'Approved'
            actionsRequired: 'None'
          }
        }
      }
    ]
    manualPrivateLinkServiceConnections: []
    subnet: {
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/FLLMOpenAI'
    }
    ipConfigurations: []
    customDnsConfigs: []
  }
}

resource privateEndpoints_EUS_FLLM_DEMO_OAI_kv_pe_name_resource 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: privateEndpoints_EUS_FLLM_DEMO_OAI_kv_pe_name
  location: 'eastus'
  tags: {
    Environment: 'DEMO'
    Project: 'FLLM'
    Purpose: 'OpenAI'
    Workspace: 'FoundationaLLM-Platform'
  }
  properties: {
    privateLinkServiceConnections: [
      {
        name: 'EUS-FLLM-DEMO-OAI-kv-connection'
        id: '${privateEndpoints_EUS_FLLM_DEMO_OAI_kv_pe_name_resource.id}/privateLinkServiceConnections/EUS-FLLM-DEMO-OAI-kv-connection'
        properties: {
          privateLinkServiceId: vaults_EUS_FLLM_DEMO_OAI_kv_name_resource.id
          groupIds: [
            'vault'
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
      id: '${virtualNetworks_EUS_FLLM_DEMO_NET_vnet_externalid}/subnets/FLLMOpenAI'
    }
    ipConfigurations: []
    customDnsConfigs: []
  }
}

resource privateEndpoints_EUS_FLLM_DEMO_OAI_content_safety_pe_name_contentSafety 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_EUS_FLLM_DEMO_OAI_content_safety_pe_name}/contentSafety'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink.cognitiveservices.azure.com'
        properties: {
          privateDnsZoneId: privateDnsZones_privatelink_cognitiveservices_azure_com_externalid
        }
      }
    ]
  }
  dependsOn: [
    privateEndpoints_EUS_FLLM_DEMO_OAI_content_safety_pe_name_resource
  ]
}

resource privateEndpoints_EUS_FLLM_DEMO_OAI_0_openai_pe_name_openai 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_EUS_FLLM_DEMO_OAI_0_openai_pe_name}/openai'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink.openai.azure.com'
        properties: {
          privateDnsZoneId: privateDnsZones_privatelink_openai_azure_com_externalid
        }
      }
    ]
  }
  dependsOn: [
    privateEndpoints_EUS_FLLM_DEMO_OAI_0_openai_pe_name_resource
  ]
}

resource privateEndpoints_EUS_FLLM_DEMO_OAI_1_openai_pe_name_openai 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_EUS_FLLM_DEMO_OAI_1_openai_pe_name}/openai'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink.openai.azure.com'
        properties: {
          privateDnsZoneId: privateDnsZones_privatelink_openai_azure_com_externalid
        }
      }
    ]
  }
  dependsOn: [
    privateEndpoints_EUS_FLLM_DEMO_OAI_1_openai_pe_name_resource
  ]
}

resource privateEndpoints_EUS_FLLM_DEMO_OAI_kv_pe_name_vaultcore 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: '${privateEndpoints_EUS_FLLM_DEMO_OAI_kv_pe_name}/vaultcore'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink.vaultcore.azure.net'
        properties: {
          privateDnsZoneId: privateDnsZones_privatelink_vaultcore_azure_net_externalid
        }
      }
    ]
  }
  dependsOn: [
    privateEndpoints_EUS_FLLM_DEMO_OAI_kv_pe_name_resource
  ]
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_EUS_FLLM_DEMO_OAI_api_ChatCompletions_Create 'Microsoft.ApiManagement/service/apis/operations@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_EUS_FLLM_DEMO_OAI_api
  name: 'ChatCompletions_Create'
  properties: {
    displayName: 'Creates a completion for the chat message'
    method: 'POST'
    urlTemplate: '/deployments/{deployment-id}/chat/completions?api-version={api-version}'
    templateParameters: [
      {
        name: 'deployment-id'
        type: 'string'
        required: true
        values: []
        schemaId: '653968a84634610c38ddd05b'
        typeName: 'Deployments-deployment-id-ChatCompletionsPostRequest'
      }
      {
        name: 'api-version'
        type: 'string'
        required: true
        values: []
        schemaId: '653968a84634610c38ddd05b'
        typeName: 'Deployments-deployment-id-ChatCompletionsPostRequest-1'
      }
    ]
    description: 'Creates a completion for the chat message'
    request: {
      queryParameters: []
      headers: []
      representations: [
        {
          contentType: 'application/json'
          examples: {
            default: {
              value: {}
            }
          }
          schemaId: '653968a84634610c38ddd05b'
          typeName: 'Deployments-deployment-id-ChatCompletionsPostRequest-2'
        }
      ]
    }
    responses: [
      {
        statusCode: 200
        description: 'OK'
        representations: [
          {
            contentType: 'application/json'
            examples: {
              default: {
                value: {}
              }
            }
            schemaId: '653968a84634610c38ddd05b'
            typeName: 'Deployments-deployment-id-ChatCompletionsPost200ApplicationJsonResponse'
          }
        ]
        headers: []
      }
    ]
  }
  dependsOn: [

    service_EUS_FLLM_DEMO_OAI_apim_name_resource
  ]
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_EUS_FLLM_DEMO_OAI_api_Completions_Create 'Microsoft.ApiManagement/service/apis/operations@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_EUS_FLLM_DEMO_OAI_api
  name: 'Completions_Create'
  properties: {
    displayName: 'Creates a completion for the provided prompt, parameters and chosen model.'
    method: 'POST'
    urlTemplate: '/deployments/{deployment-id}/completions?api-version={api-version}'
    templateParameters: [
      {
        name: 'deployment-id'
        type: 'string'
        required: true
        values: []
        schemaId: '653968a84634610c38ddd05b'
        typeName: 'Deployments-deployment-id-CompletionsPostRequest'
      }
      {
        name: 'api-version'
        type: 'string'
        required: true
        values: []
        schemaId: '653968a84634610c38ddd05b'
        typeName: 'Deployments-deployment-id-CompletionsPostRequest-1'
      }
    ]
    description: 'Creates a completion for the provided prompt, parameters and chosen model.'
    request: {
      queryParameters: []
      headers: []
      representations: [
        {
          contentType: 'application/json'
          examples: {
            default: {
              value: {}
            }
          }
          schemaId: '653968a84634610c38ddd05b'
          typeName: 'Deployments-deployment-id-CompletionsPostRequest-2'
        }
      ]
    }
    responses: [
      {
        statusCode: 200
        description: 'OK'
        representations: [
          {
            contentType: 'application/json'
            examples: {
              default: {
                value: {}
              }
            }
            schemaId: '653968a84634610c38ddd05b'
            typeName: 'Deployments-deployment-id-CompletionsPost200ApplicationJsonResponse'
          }
        ]
        headers: [
          {
            name: 'apim-request-id'
            description: 'Request ID for troubleshooting purposes'
            type: 'string'
            values: []
            schemaId: '653968a84634610c38ddd05b'
            typeName: 'Deployments-deployment-id-CompletionsPost200apim-request-idResponseHeader'
          }
        ]
      }
      {
        statusCode: 400
        description: 'Service unavailable'
        representations: [
          {
            contentType: 'application/json'
            examples: {
              default: {
                value: {}
              }
            }
            schemaId: '653968a84634610c38ddd05b'
            typeName: 'errorResponse'
          }
        ]
        headers: [
          {
            name: 'apim-request-id'
            description: 'Request ID for troubleshooting purposes'
            type: 'string'
            values: []
            schemaId: '653968a84634610c38ddd05b'
            typeName: 'Deployments-deployment-id-CompletionsPostdefaultapim-request-idResponseHeader'
          }
        ]
      }
      {
        statusCode: 500
        description: 'Service unavailable'
        representations: [
          {
            contentType: 'application/json'
            examples: {
              default: {
                value: {}
              }
            }
            schemaId: '653968a84634610c38ddd05b'
            typeName: 'errorResponse'
          }
        ]
        headers: [
          {
            name: 'apim-request-id'
            description: 'Request ID for troubleshooting purposes'
            type: 'string'
            values: []
            schemaId: '653968a84634610c38ddd05b'
            typeName: 'Deployments-deployment-id-CompletionsPostdefaultapim-request-idResponseHeader'
          }
        ]
      }
    ]
  }
  dependsOn: [

    service_EUS_FLLM_DEMO_OAI_apim_name_resource
  ]
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_EUS_FLLM_DEMO_OAI_api_embeddings_create 'Microsoft.ApiManagement/service/apis/operations@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_EUS_FLLM_DEMO_OAI_api
  name: 'embeddings_create'
  properties: {
    displayName: 'Get a vector representation of a given input that can be easily consumed by machine learning models and algorithms.'
    method: 'POST'
    urlTemplate: '/deployments/{deployment-id}/embeddings?api-version={api-version}'
    templateParameters: [
      {
        name: 'deployment-id'
        description: 'The deployment id of the model which was deployed.'
        type: 'string'
        required: true
        values: []
        schemaId: '653968a84634610c38ddd05b'
        typeName: 'Deployments-deployment-id-EmbeddingsPostRequest'
      }
      {
        name: 'api-version'
        type: 'string'
        required: true
        values: []
        schemaId: '653968a84634610c38ddd05b'
        typeName: 'Deployments-deployment-id-EmbeddingsPostRequest-1'
      }
    ]
    description: 'Get a vector representation of a given input that can be easily consumed by machine learning models and algorithms.'
    request: {
      queryParameters: []
      headers: []
      representations: [
        {
          contentType: 'application/json'
          examples: {
            default: {
              value: {}
            }
          }
          schemaId: '653968a84634610c38ddd05b'
          typeName: 'Deployments-deployment-id-EmbeddingsPostRequest-2'
        }
      ]
    }
    responses: [
      {
        statusCode: 200
        description: 'OK'
        representations: [
          {
            contentType: 'application/json'
            examples: {
              default: {
                value: {}
              }
            }
            schemaId: '653968a84634610c38ddd05b'
            typeName: 'Deployments-deployment-id-EmbeddingsPost200ApplicationJsonResponse'
          }
        ]
        headers: []
      }
    ]
  }
  dependsOn: [

    service_EUS_FLLM_DEMO_OAI_apim_name_resource
  ]
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_EUS_FLLM_DEMO_OAI_api_policy 'Microsoft.ApiManagement/service/apis/policies@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_EUS_FLLM_DEMO_OAI_api
  name: 'policy'
  properties: {
    value: '<policies>\r\n  <inbound>\r\n    <base />\r\n    <set-variable name="backendId" value="@(new Random(context.RequestId.GetHashCode()).Next(1, 3))" />\r\n    <choose>\r\n      <when condition="@(context.Variables.GetValueOrDefault&lt;int&gt;(&quot;backendId&quot;) == 1)">\r\n        <set-backend-service backend-id="EUS-FLLM-DEMO-OAI-0-backend" />\r\n      </when>\r\n      <when condition="@(context.Variables.GetValueOrDefault&lt;int&gt;(&quot;backendId&quot;) == 2)">\r\n        <set-backend-service backend-id="EUS-FLLM-DEMO-OAI-1-backend" />\r\n      </when>\r\n      <otherwise>\r\n        <!-- Should never happen, but you never know ;) -->\r\n        <return-response>\r\n          <set-status code="500" reason="InternalServerError" />\r\n          <set-header name="Microsoft-Azure-Api-Management-Correlation-Id" exists-action="override">\r\n            <value>@{return Guid.NewGuid().ToString();}</value>\r\n          </set-header>\r\n          <set-body>A gateway-related error occurred while processing the request.</set-body>\r\n        </return-response>\r\n      </otherwise>\r\n    </choose>\r\n  </inbound>\r\n  <backend>\r\n    <base />\r\n  </backend>\r\n  <outbound>\r\n    <base />\r\n  </outbound>\r\n  <on-error>\r\n    <base />\r\n  </on-error>\r\n</policies>'
    format: 'xml'
  }
  dependsOn: [

    service_EUS_FLLM_DEMO_OAI_apim_name_resource
  ]
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_EUS_FLLM_DEMO_OAI_api_653968a84634610c38ddd05b 'Microsoft.ApiManagement/service/apis/schemas@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_EUS_FLLM_DEMO_OAI_api
  name: '653968a84634610c38ddd05b'
  properties: {
    contentType: 'application/vnd.oai.openapi.components+json'
    document: {}
  }
  dependsOn: [

    service_EUS_FLLM_DEMO_OAI_apim_name_resource
  ]
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_EUS_FLLM_DEMO_OAI_api_default 'Microsoft.ApiManagement/service/apis/wikis@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_EUS_FLLM_DEMO_OAI_api
  name: 'default'
  properties: {
    documents: []
  }
  dependsOn: [

    service_EUS_FLLM_DEMO_OAI_apim_name_resource
  ]
}

resource Microsoft_ApiManagement_service_diagnostics_service_EUS_FLLM_DEMO_OAI_apim_name_azuremonitor 'Microsoft.ApiManagement/service/diagnostics@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_resource
  name: 'azuremonitor'
  properties: {
    logClientIp: true
    loggerId: service_EUS_FLLM_DEMO_OAI_apim_name_azuremonitor.id
    sampling: {
      samplingType: 'fixed'
      percentage: 100
    }
    frontend: {
      request: {
        dataMasking: {
          queryParams: [
            {
              value: '*'
              mode: 'Hide'
            }
          ]
        }
      }
    }
    backend: {
      request: {
        dataMasking: {
          queryParams: [
            {
              value: '*'
              mode: 'Hide'
            }
          ]
        }
      }
    }
  }
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_azuremonitor_azuremonitor 'Microsoft.ApiManagement/service/diagnostics/loggers@2018-01-01' = {
  parent: Microsoft_ApiManagement_service_diagnostics_service_EUS_FLLM_DEMO_OAI_apim_name_azuremonitor
  name: 'azuremonitor'
  dependsOn: [

    service_EUS_FLLM_DEMO_OAI_apim_name_resource
  ]
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_administrators_1 'Microsoft.ApiManagement/service/groups/users@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_administrators
  name: '1'
  dependsOn: [

    service_EUS_FLLM_DEMO_OAI_apim_name_resource
  ]
}

resource service_EUS_FLLM_DEMO_OAI_apim_name_developers_1 'Microsoft.ApiManagement/service/groups/users@2023-03-01-preview' = {
  parent: service_EUS_FLLM_DEMO_OAI_apim_name_developers
  name: '1'
  dependsOn: [

    service_EUS_FLLM_DEMO_OAI_apim_name_resource
  ]
}
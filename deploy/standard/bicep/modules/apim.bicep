/** Inputs **/
@description('Action Group Id for alerts')
param actionGroupId string

@description('Cognitive Account Endpoints')
param cognitiveAccounts array

@description('DNS resource group name')
param dnsResourceGroupName string

@description('Location for all resources')
param location string

@description('Log Analytic Workspace Id to use for diagnostics')
param logAnalyticWorkspaceId string

@description('Private DNS Zones for private endpoint')
param privateDnsZones array

@description('The API publisher details.')
param publisher object = {
  publisherEmail: 'info@solliance.net' //TODO Param
  publisherName: 'FoundationaLLM' /// todo param
}

@description('Resource suffix for all resources')
param resourceSuffix string

@description('Subnet Id for private endpoint')
param subnetId string

@description('Tags for all resources')
param tags object

@description('Timestamp for nested deployments')
param timestamp string = utcNow()

/** Locals **/
@description('Metric alerts for the resource.')
var alerts = [
  {
    description: 'Service capacity greater than 75% for 1 hour'
    evaluationFrequency: 'PT1M'
    metricName: 'Capacity'
    name: 'availability'
    operator: 'GreaterThan'
    severity: 0
    threshold: 75
    timeAggregation: 'Average'
    windowSize: 'PT1H'
  }
]

@description('Enumerate backend names for the HA OpenAI API Policy routing.')
var backendNames = [for (account, i) in cognitiveAccounts: 'backend-${account.name}']

@description('Enumerate choices for the HA OpenAI API Policy routing.')
var backendChoices = [for (name, i) in backendNames: format(choiceTemplate, i + 1, name)]

@description('Backend choices for the HA OpenAI API Policy routing.')
var choices = join(backendChoices, '')

@description('Template for backend choices.')
var choiceTemplate = '''
<when condition="@(context.Variables.GetValueOrDefault<int>("backendId") == {0})">
  <set-backend-service backend-id="{1}"/>
</when>
'''

@description('The Resource logs to enable')
var logs = [ 'WebSocketConnectionLogs', 'GatewayLogs' ]

@description('Formatted untruncated resource name')
var formattedName = toLower('${serviceType}-${resourceSuffix}')

@description('The Resource Name')
var name = substring(formattedName,0,min([length(formattedName),50]))

@description('Rendered policy that randomly distributes requests across OpenAI backends.')
var policy = format(policyTemplate, length(cognitiveAccounts) + 1, choices)

@description('Template for OpenAI routing policy.')
var policyTemplate = '''
<policies>
  <inbound>
    <base />
    <set-variable name="backendId" value="@(new Random(context.RequestId.GetHashCode()).Next(1, {0}))" />
    <choose>
      {1}
      <otherwise>
        <!-- Should never happen, but you never know ;) -->
        <return-response>
          <set-status code="500" reason="InternalServerError" />
          <set-header name="Microsoft-Azure-Api-Management-Correlation-Id" exists-action="override">
            <value>@{{return Guid.NewGuid().ToString();}}</value>
          </set-header>
          <set-body>A gateway-related error occurred while processing the request.</set-body>
        </return-response>
      </otherwise>
    </choose>
  </inbound>
  <backend>
    <base />
  </backend>
  <outbound>
    <base />
  </outbound>
  <on-error>
    <base />
  </on-error>
</policies>
'''

@description('The Resource Service Type token')
var serviceType = 'apim'

/** Outputs **/

/** Resources **/
@description('API Management')
resource main 'Microsoft.ApiManagement/service@2023-03-01-preview' = {
  location: location
  name: name
  tags: tags

  identity: {
    type: 'SystemAssigned'
  }

  sku: {
    capacity: 1
    name: 'Developer'
  }

  properties: {
    apiVersionConstraint: {
      minApiVersion: '2021-08-01'
    }
    certificates: []
    developerPortalStatus: 'Enabled'
    legacyPortalStatus: 'Enabled'
    natGatewayState: 'Disabled'
    notificationSenderEmail: 'apimgmt-noreply@mail.windowsazure.com'
    publicIpAddressId: pip.id
    publicNetworkAccess: 'Enabled'
    publisherEmail: publisher.publisherEmail
    publisherName: publisher.publisherName
    virtualNetworkConfiguration: { subnetResourceId: subnetId }
    virtualNetworkType: 'Internal'

    customProperties: {
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Protocols.Server.Http2': 'False'
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
    }

    hostnameConfigurations: [
      {
        certificateSource: 'BuiltIn'
        defaultSslBinding: true
        hostName: '${name}.azure-api.net'
        negotiateClientCertificate: false
        type: 'Proxy'
      }
    ]
  }
}

@description('HA OpenAI API')
resource api 'Microsoft.ApiManagement/service/apis@2023-03-01-preview' = {
  parent: main
  name: 'api-${name}'
  properties: {
    apiRevision: '1'
    displayName: 'HA OpenAI'
    format: 'openapi+json-link'
    isCurrent: true
    path: 'openai'
    protocols: [ 'https' ]
    subscriptionRequired: false
    value: 'https://raw.githubusercontent.com/Azure/azure-rest-api-specs/main/specification/cognitiveservices/data-plane/AzureOpenAI/inference/stable/2023-05-15/inference.json'

    authenticationSettings: {
      oAuth2AuthenticationSettings: []
      openidAuthenticationSettings: []
    }

    subscriptionKeyParameterNames: {
      header: 'api-key'
      query: 'subscription-key'
    }
  }
}

@description('HA OpenAI API Policy')
resource apiPolicy 'Microsoft.ApiManagement/service/apis/policies@2023-03-01-preview' = {
  dependsOn: [ backend ]
  name: 'policy'
  parent: api

  properties: {
    format: 'rawxml'
    value: policy
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

@description('Loggers')
resource loggerAzureMonitor 'Microsoft.ApiManagement/service/loggers@2023-03-01-preview' = {
  name: 'azuremonitor'
  parent: main

  properties: {
    isBuffered: true
    loggerType: 'azureMonitor'
  }
}

@description('Portal Configs')
resource portalConfigs 'Microsoft.ApiManagement/service/portalconfigs@2023-03-01-preview' = {
  name: 'default'
  parent: main

  properties: {
    enableBasicAuth: false
    signin: { require: false }
    signup: { termsOfService: { requireConsent: false } }
    cors: { allowedOrigins: [] }

    csp: {
      mode: 'disabled'
      reportUri: []
      allowedSources: []
    }

    delegation: {
      delegateRegistration: false
      delegateSubscription: false
    }
  }
}

@description('Public IP Address')
resource pip 'Microsoft.Network/publicIPAddresses@2023-05-01' = {
  name: 'pip-${name}'
  location: location
  tags: tags

  sku: {
    name: 'Standard'
    tier: 'Regional'
  }

  properties: {
    publicIPAddressVersion: 'IPv4'
    publicIPAllocationMethod: 'Static'
    idleTimeoutInMinutes: 4
    ipTags: []

    ddosSettings: {
      protectionMode: 'VirtualNetworkInherited'
    }

    dnsSettings: {
      domainNameLabel: name
      fqdn: '${name}.${location}.cloudapp.azure.com'
    }
  }
}

@description('Service Diagnostics')
resource serviceDiagnostics 'Microsoft.ApiManagement/service/diagnostics@2023-03-01-preview' = {
  name: 'azuremonitor'
  parent: main

  properties: {
    logClientIp: true
    loggerId: loggerAzureMonitor.id

    backend: {
      request: {
        dataMasking: {
          queryParams: [
            {
              mode: 'Hide'
              value: '*'
            }
          ]
        }
      }
    }

    frontend: {
      request: {
        dataMasking: {
          queryParams: [
            {
              mode: 'Hide'
              value: '*'
            }
          ]
        }
      }
    }

    sampling: {
      percentage: 100
      samplingType: 'fixed'
    }
  }
}

//** Nested Modules **//
module roleAssignment 'utility/roleAssignments.bicep' = {
  name: 'ra-${resourceSuffix}-${timestamp}'
  params: {
    principalId: main.identity.principalId
    roleDefinitionIds: {
      'Key Vault Secrets User': '4633458b-17de-408a-b874-0445c86b69e6'
    }
  }
}

module backend 'apimBackend.bicep' = [for (account, i) in cognitiveAccounts: {
  dependsOn: [ roleAssignment, dnsRecord, metricAlerts, serviceDiagnostics, pip, portalConfigs, loggerAzureMonitor, diagnostics, api ] // Introduce delay to allow role assignment to propogate
  name: 'backend-${account.name}-${timestamp}'
  params: {
    account: account
    apimName: main.name
    name: backendNames[i]
  }
}]

@description('Private DNS A-records.')
module dnsRecord 'utility/privateDnsARecord.bicep' = [for zone in privateDnsZones: {
  name: 'a-record-${zone.name}-${timestamp}'
  scope: resourceGroup(dnsResourceGroupName)
  params: {
    ipAddresses: main.properties.privateIPAddresses
    name: main.name
    privateDnsZones: privateDnsZones
  }
}]

@description('Resource for configuring the Key Vault metric alerts.')
module metricAlerts 'utility/metricAlerts.bicep' = {
  name: 'alert-${main.name}-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    alerts: alerts
    metricNamespace: 'Microsoft.ApiManagement/service'
    nameSuffix: name
    serviceId: main.id
    tags: tags
  }
}

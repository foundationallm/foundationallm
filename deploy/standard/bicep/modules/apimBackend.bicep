@description('OpenAI Cognitive Account')
param account object

@description('API Management Instance Name')
param apimName string

@description('Backend Name')
param name string

@description('OpenAI Cognitive Account Backends')
resource backend 'Microsoft.ApiManagement/service/backends@2023-03-01-preview' = {
  dependsOn: [ namedValues ]
  name: '${apimName}/${name}'

  properties: {
    protocol: 'http'
    url: '${account.endpoint}openai'

    credentials: {
      header: {
        'api-key': ['{{${account.keys[0].name}}}']
      }
    }

    tls: {
      validateCertificateChain: true
      validateCertificateName: true
    }
  }
}

@description('Reference the OpenAI account keys stored in Key Vault.')
resource namedValues 'Microsoft.ApiManagement/service/namedValues@2023-03-01-preview' = [for key in account.keys: {
  name: '${apimName}/${key.name}'

  properties: {
    displayName: key.name
    secret: true

    keyVault: {
      secretIdentifier: key.secretIdentifier
    }
  }
}]

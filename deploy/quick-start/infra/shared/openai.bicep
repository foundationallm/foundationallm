/*
  This Bicep file deploys an Azure Cognitive Services OpenAI resource and its associated deployments.
  It takes the following inputs:
    - deployments: An array of deployment objects containing information about the OpenAI deployments.
    - keyvaultName: The name of the Azure Key Vault where secrets will be stored.
    - location: The Azure region where the resource will be deployed. Defaults to the resource group's location.
    - name: The name of the OpenAI resource.
    - sku: The SKU (pricing tier) of the OpenAI resource. Defaults to 'S0'.
    - tags: Optional tags to be applied to the resource.

  The file defines the following resources:
    - openAi: The Azure Cognitive Services OpenAI resource.
    - openAiDeployments: The deployments associated with the OpenAI resource.

  The file also defines the following outputs:
    - endpoint: The endpoint URL of the OpenAI resource.
    - name: The name of the OpenAI resource.
*/

/********** Inputs **********/
param deployments array
param location string = resourceGroup().location
param name string
param sku string = 'S0'
param tags object = {}

/********** Resources **********/
resource openAi 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: name
  location: location
  sku: {
    name: sku
  }
  kind: 'OpenAI'
  properties: {
    customSubDomainName: name
    publicNetworkAccess: 'Enabled'
  }
  tags: tags
}

@batchSize(1)
resource openAiDeployments 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = [
  for deployment in deployments: {
    parent: openAi
    name: deployment.name
    sku: {
      capacity: deployment.sku.capacity
      name: deployment.sku.name
    }
    properties: {
      model: {
        format: 'OpenAI'
        name: deployment.model.name
        version: deployment.model.version
      }
    }
  }
]

/********** Nested Deployments **********/

/********** Outputs **********/
output endpoint string = openAi.properties.endpoint
output id string = openAi.id
output name string = openAi.name

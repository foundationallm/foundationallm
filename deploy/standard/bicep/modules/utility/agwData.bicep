/** Inputs **/
@description('The environment name token used in naming resources.')
param environmentName string

@description('Location used for all resources.')
param location string

@description('Project Name, used in naming resources.')
param project string

/** Locals **/
@description('The Application Gateway IDs')
var applicationGateways = [ 'www', 'api' ]

@description('Resource Suffix used in naming resources.')
var resourceSuffix = '${project}-${environmentName}-${location}-${workload}'

@description('Workload Token used in naming resources.')
var workload = 'agw'

/** Outputs **/
@description('Application Gateway Details to use in other modules.')
output applicationGateways array = [for (gateway, i) in applicationGateways: {
  id: main[i].id
  key: gateway
}]

/** Resources **/
resource main 'Microsoft.Network/applicationGateways@2023-05-01' existing = [for i in applicationGateways: {
  name: 'agw-${i}-${resourceSuffix}'
}]

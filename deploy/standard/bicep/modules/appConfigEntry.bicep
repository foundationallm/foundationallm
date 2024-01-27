/** Inputs **/
@description('App Configuration service name.')
param appConfigName string

@description('Key Value entry name.')
param name string

@description('Key Value content type.')
param contentType string = ''

@description('Tags for all resources')
param tags object

@description('Key Value value.')
param value string
/** Locals **/

/** Outputs **/

/** Resources **/
resource appConfig 'Microsoft.AppConfiguration/configurationStores@2023-03-01' existing = {
  name: appConfigName
}

resource main 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  name: name
  parent: appConfig
  properties: {
    contentType: contentType
    tags: tags
    value: value
  }
}

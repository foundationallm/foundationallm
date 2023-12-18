@description('The IP addresses.')
param ipAddresses array

@description('Private DNS Zones for private endpoint')
param privateDnsZones array

@description('The record host name without the domain.')
param name string

@description('Private DNS A-records.')
resource dnsRecord 'Microsoft.Network/privateDnsZones/A@2020-06-01' = [for zone in privateDnsZones: {
  name: '${zone.name}/${name}'
  properties: {
    ttl: 0
    aRecords: map(ipAddresses, (ip) => {
        ipv4Address: ip
      })
  }
}]

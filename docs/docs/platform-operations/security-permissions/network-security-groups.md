# Network Security Groups

FoundationaLLM Standard deployment uses Network Security Groups (NSGs) to control network traffic in Azure Virtual Networks.

## Overview

NSGs provide stateful packet filtering based on:
- Source/destination IP addresses
- Source/destination ports
- Protocol (TCP, UDP, etc.)
- Direction (inbound/outbound)

## Application Gateway NSG

The Application Gateway subnet requires specific NSG rules for Azure services.

### Inbound Rules

| Rule Name | Access | Source | Destination | Port | Protocol | Priority | Notes |
|-----------|--------|--------|-------------|------|----------|----------|-------|
| `allow-internet-http-inbound` | Allow | Internet | VirtualNetwork | 80 | TCP | 128 | Customizable |
| `allow-internet-https-inbound` | Allow | Internet | VirtualNetwork | 443 | TCP | 132 | Customizable |
| `allow-gatewaymanager-inbound` | Allow | GatewayManager | * | 65200-65535 | TCP | 148 | **Required** |
| `allow-loadbalancer-inbound` | Allow | AzureLoadBalancer | * | * | * | 164 | **Required** |
| `deny-all-inbound` | Deny | * | * | * | * | 4096 | Default deny |

### Important Notes

1. **GatewayManager Rule** - Required by Azure. Do not modify or delete.
2. **LoadBalancer Rule** - Required by Azure. Do not modify or delete.
3. **Internet Rules** - Can be restricted to specific IP ranges.

For more information, see [Azure Application Gateway NSG requirements](https://learn.microsoft.com/azure/application-gateway/configuration-infrastructure#network-security-groups).

## AKS Subnet NSGs

### Backend Cluster

| Direction | Source | Destination | Ports | Purpose |
|-----------|--------|-------------|-------|---------|
| Inbound | Application Gateway | AKS Subnet | 443 | API traffic |
| Inbound | AzureLoadBalancer | AKS Subnet | * | Health probes |
| Outbound | AKS Subnet | Internet | 443 | Azure services |
| Outbound | AKS Subnet | AzureCloud | 443 | Azure APIs |

### Frontend Cluster

| Direction | Source | Destination | Ports | Purpose |
|-----------|--------|-------------|-------|---------|
| Inbound | Application Gateway | AKS Subnet | 443 | Portal traffic |
| Inbound | AzureLoadBalancer | AKS Subnet | * | Health probes |
| Outbound | AKS Subnet | Backend | 443 | API calls |
| Outbound | AKS Subnet | Internet | 443 | Azure services |

## Private Endpoint Subnet

| Direction | Source | Destination | Ports | Purpose |
|-----------|--------|-------------|-------|---------|
| Inbound | AKS Subnets | Private Endpoints | 443 | Service access |
| Inbound | Jumpbox | Private Endpoints | 443 | Admin access |
| Outbound | * | * | * | Deny all |

## Customization Guidelines

### Restricting Internet Access

Replace Internet source with specific CIDRs:

```json
{
  "name": "allow-corporate-https-inbound",
  "properties": {
    "access": "Allow",
    "direction": "Inbound",
    "protocol": "Tcp",
    "sourceAddressPrefix": "203.0.113.0/24",
    "sourcePortRange": "*",
    "destinationAddressPrefix": "VirtualNetwork",
    "destinationPortRange": "443",
    "priority": 132
  }
}
```

### Adding Custom Rules

Example: Allow specific IP range:

```bash
az network nsg rule create \
  --resource-group <resource-group> \
  --nsg-name <nsg-name> \
  --name allow-partner-https \
  --priority 140 \
  --access Allow \
  --direction Inbound \
  --protocol Tcp \
  --source-address-prefixes 198.51.100.0/24 \
  --source-port-ranges '*' \
  --destination-address-prefixes VirtualNetwork \
  --destination-port-ranges 443
```

## Service Tags

Use Azure service tags for easier rule management:

| Tag | Description |
|-----|-------------|
| `VirtualNetwork` | All VNet addresses |
| `AzureLoadBalancer` | Azure Load Balancer |
| `Internet` | Public internet |
| `GatewayManager` | Azure Gateway Manager |
| `AzureCloud` | All Azure public IPs |
| `AzureCosmosDB` | Cosmos DB service |
| `Storage` | Azure Storage |
| `AzureKeyVault` | Key Vault service |

## Monitoring NSG Traffic

### Enable Flow Logs

```bash
az network watcher flow-log create \
  --resource-group <resource-group> \
  --nsg <nsg-name> \
  --storage-account <storage-account> \
  --enabled true
```

### View NSG Effective Rules

```bash
az network nic list-effective-nsg \
  --resource-group <resource-group> \
  --name <nic-name>
```

## Troubleshooting

### Common Issues

| Issue | Solution |
|-------|----------|
| Connection timeout | Check NSG rules allow traffic |
| Service unavailable | Verify Application Gateway rules |
| Inter-service failures | Check AKS subnet rules |

### NSG Diagnostic Commands

```bash
# List NSG rules
az network nsg rule list \
  --resource-group <resource-group> \
  --nsg-name <nsg-name> \
  --output table

# Check flow logs
az network watcher flow-log show \
  --resource-group <resource-group> \
  --nsg <nsg-name>
```

## Best Practices

| Practice | Description |
|----------|-------------|
| **Least Privilege** | Only allow necessary traffic |
| **Use Service Tags** | Easier maintenance than IP ranges |
| **Enable Flow Logs** | Audit and troubleshooting |
| **Document Changes** | Track rule modifications |
| **Test Changes** | Verify in staging first |

## Related Topics

- [Platform Security](platform-security.md)
- [Standard Deployment](../deployment/deployment-standard.md)
- [Troubleshooting](../monitoring-troubleshooting/troubleshooting.md)

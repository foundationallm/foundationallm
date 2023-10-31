output "id" {
  description = "The AKS cluster ID."
  value       = azurerm_kubernetes_cluster.main.id
}

output "oidcIssuerUrl" {
  description = "The URL of the OpenID Connect issuer."
  value       = azurerm_kubernetes_cluster.main.oidc_issuer_url
}
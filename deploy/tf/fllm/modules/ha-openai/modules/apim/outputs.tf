output "endpoint" {
  description = "The endpoint for the API Management instance."
  value       = azurerm_api_management.main.gateway_url
}

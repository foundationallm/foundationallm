output "endpoint" {
  description = "The endpoint for the cognitive services account."
  value       = local.main.properties.endpoint
}

output "key" {
  description = "The primary access key for the cognitive services account."
  value       = local.main.properties.primaryAccessKey
}

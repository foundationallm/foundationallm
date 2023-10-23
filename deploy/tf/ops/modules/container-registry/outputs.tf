output "id" {
  description = "The Container Registry Resource ID."
  value       = jsondecode(azapi_resource.main.output).id
}

output "redis_connection_string" {
  //to get it, use 'terraform output redis_connection_string'
  value       = azurerm_redis_cache.redis.primary_connection_string
  description = "To connect with Another Redis Desktop Manager, enable SSL Mode"
  sensitive   = true
}

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.85.0"
    }
  }
}

provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }
}

locals {
  project_name       = "ticketing"
  #run the commands below to get it (for macOS / linux):
  #dotnet publish ./Ticketing/Ticketing.csproj -c Release
  #zip -j ticketing.zip ./Ticketing/bin/Release/net8.0/publish/*
  ticketing_zip_path = "../ticketing.zip"

  #throw an error if file doesn't exist
  ticketing_zip_deploy_file = fileexists(local.ticketing_zip_path) ? local.ticketing_zip_path : [][0]
}

resource "azurerm_resource_group" "rg" {
  name     = "${local.project_name}-rg"
  location = "North Europe"
}

resource "azurerm_service_plan" "sp" {
  name                = "${local.project_name}-sp"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  os_type             = "Linux"
  sku_name            = "P1v3"
}

resource "azurerm_linux_web_app" "app" {
  depends_on          = [azurerm_postgresql_flexible_server_database.database]
  name                = "${local.project_name}-app"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_service_plan.sp.location
  service_plan_id     = azurerm_service_plan.sp.id
  https_only          = true

  webdeploy_publish_basic_authentication_enabled = false
  zip_deploy_file                                = local.ticketing_zip_deploy_file

  app_settings = {
    WEBSITE_RUN_FROM_PACKAGE       = 1
    SCM_DO_BUILD_DURING_DEPLOYMENT = true
    CacheExpirationInMinutes       = 5
  }

  site_config {
    application_stack {
      dotnet_version = "8.0"
    }
  }

  connection_string {
    name  = "DatabaseConnection"
    type  = "PostgreSQL"
    value = "Host=${azurerm_postgresql_flexible_server.database_server.fqdn};Port=5432;Database=${azurerm_postgresql_flexible_server_database.database.name};Username=${azurerm_postgresql_flexible_server.database_server.administrator_login};Password=${azurerm_postgresql_flexible_server.database_server.administrator_password};SslMode=Require;"
  }

  connection_string {
    name  = "RedisConnection"
    type  = "RedisCache"
    value = azurerm_redis_cache.redis.primary_connection_string
  }
}

resource "azurerm_postgresql_flexible_server" "database_server" {
  //to connect with PdAdmin, set SSL Mode to "Require"
  name                   = "${local.project_name}-psqlflexibleserver"
  resource_group_name    = azurerm_resource_group.rg.name
  location               = azurerm_resource_group.rg.location
  version                = "16"
  administrator_login    = var.login
  administrator_password = var.password
  storage_mb             = 32768
  sku_name               = "B_Standard_B2ms"
  zone                   = "1"
}

resource "azurerm_postgresql_flexible_server_firewall_rule" "allow_all_azure_ips" {
  server_id        = azurerm_postgresql_flexible_server.database_server.id
  name             = "AllowAllWindowsAzureIps"
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

data "http" "myip" {
  url = "https://ipv4.icanhazip.com"
}

resource "azurerm_postgresql_flexible_server_firewall_rule" "allow_my_ip" {
  server_id        = azurerm_postgresql_flexible_server.database_server.id
  name             = "AllowMyIp"
  start_ip_address = chomp(data.http.myip.response_body)
  end_ip_address   = chomp(data.http.myip.response_body)
}

resource "azurerm_postgresql_flexible_server_database" "database" {
  name      = "${local.project_name}-db"
  server_id = azurerm_postgresql_flexible_server.database_server.id
}

resource "azurerm_redis_cache" "redis" {
  name                = "${local.project_name}-redis-unique-name"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  capacity            = 1
  family              = "C"
  sku_name            = "Standard"
  enable_non_ssl_port = false
  minimum_tls_version = "1.2"
}

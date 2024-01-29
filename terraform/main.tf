terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.85.0"
    }
    azapi = {
      source  = "azure/azapi"
      version = "1.11.0"
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
  project_name                       = "ticketing"
  azurerm_communication_service_name = "${local.project_name}-az-communication-service"
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

  identity {
    type = "SystemAssigned"
  }

  app_settings = {
    WEBSITE_RUN_FROM_PACKAGE                  = 1
    SCM_DO_BUILD_DURING_DEPLOYMENT            = true
    CacheExpirationInMinutes                  = 5
    ServiceBusSettings__QueueName             = azurerm_servicebus_queue.queue.name
    ServiceBusSettings__MaxRetries            = 3
    ServiceBusSettings__TryTimeout            = 5,
    AppSettings__CartItemsExpirationInMinutes = 15
  }

  site_config {
    application_stack {
      dotnet_version = "8.0"
    }
  }

  connection_string {
    name  = "DatabaseConnection"
    type  = "PostgreSQL"
    value = "Host=${azurerm_postgresql_flexible_server.database_server.fqdn};Port=5432;Database=${azurerm_postgresql_flexible_server_database.database.name};Username=${var.login};Password=${var.password};SslMode=Require;"
  }

  connection_string {
    name  = "RedisConnection"
    type  = "RedisCache"
    value = azurerm_redis_cache.redis.primary_connection_string
  }

  connection_string {
    name  = "ServiceBusConnection"
    type  = "ServiceBus"
    value = "${azurerm_servicebus_namespace.namespace.name}.servicebus.windows.net"
  }
}

resource "azurerm_role_assignment" "allow_web_app_to_read_service_bus" {
  scope                = azurerm_servicebus_namespace.namespace.id
  role_definition_name = "Azure Service Bus Data Owner"
  principal_id         = azurerm_linux_web_app.app.identity[0].principal_id
}

locals {
  publish_web_app = <<-EOT
    dotnet publish ./Ticketing/Ticketing.csproj -c Release
    zip -j ticketing.zip ./Ticketing/bin/Release/net8.0/publish/*
    az webapp deployment source config-zip --resource-group ${azurerm_resource_group.rg.name} --name ${azurerm_linux_web_app.app.name} --src ./ticketing.zip
EOT
}

resource "null_resource" "azurerm_linux_web_app_publish" {
  provisioner "local-exec" {
    working_dir = "../"
    command     = local.publish_web_app
  }

  depends_on = [local.publish_web_app, azurerm_role_assignment.allow_web_app_to_read_service_bus]

  triggers = {
    publish_web_app = local.publish_web_app
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

resource "azurerm_email_communication_service" "email_communication_service" {
  name                = "${local.project_name}-emailcommunicationservice"
  resource_group_name = azurerm_resource_group.rg.name
  data_location       = "Europe"
}

resource "azapi_resource" "email_communication_service_domain" {
  //this is not yet supported by Terraform, so we use azapi_resource
  // https://learn.microsoft.com/en-us/azure/templates/microsoft.communication/emailservices/domains?pivots=deployment-language-terraform#domainproperties-2
  type      = "Microsoft.Communication/emailServices/domains@2023-04-01-preview"
  name      = "AzureManagedDomain"
  location  = "global"
  parent_id = azurerm_email_communication_service.email_communication_service.id

  body = jsonencode({
    properties = {
      domainManagement       = "AzureManaged"
      userEngagementTracking = "Disabled"
    }
  })

  response_export_values = ["properties.fromSenderDomain"]
}

resource "azapi_resource" "email_communication_service_domain_sender_username" {
  //this is also not yet supported by Terraform
  // https://github.com/hashicorp/terraform-provider-azurerm/issues/22549
  type      = "Microsoft.Communication/emailServices/domains/senderUsernames@2023-04-01-preview"
  name      = local.project_name
  parent_id = azapi_resource.email_communication_service_domain.id
  body      = jsonencode({
    properties = {
      displayName = local.project_name
      username    = local.project_name
    }
  })
}

data "azurerm_subscription" "current" {
}

resource "azapi_resource" "azurerm_communication_service" {
  //guess what? not yet supported by Terraform
  // https://github.com/hashicorp/terraform-provider-azurerm/issues/22995
  // https://learn.microsoft.com/en-us/azure/templates/microsoft.communication/communicationservices?pivots=deployment-language-terraform
  type      = "Microsoft.Communication/communicationServices@2023-04-01-preview"
  name      = local.azurerm_communication_service_name
  location  = "global"
  parent_id = azurerm_resource_group.rg.id
  body      = jsonencode({
    properties = {
      dataLocation  = "Europe"
      linkedDomains = [
        "/subscriptions/${data.azurerm_subscription.current.subscription_id}/resourceGroups/${azurerm_resource_group.rg.name}/providers/Microsoft.Communication/emailServices/${azurerm_email_communication_service.email_communication_service.name}/domains/${azapi_resource.email_communication_service_domain.name}"
      ]
    }
  })
}

resource "azurerm_servicebus_namespace" "namespace" {
  name                = "${local.project_name}-namespace"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  sku                 = "Standard"
}

resource "azurerm_servicebus_queue" "queue" {
  //should be the same as in NotificationHandler/ServiceBusQueueTrigger.cs, in the attribute [ServiceBusTrigger]
  name         = "${local.project_name}_servicebus_queue"
  namespace_id = azurerm_servicebus_namespace.namespace.id
}

resource "azurerm_storage_account" "storage" {
  name                     = "${local.project_name}storage"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_linux_function_app" "function" {
  name                = "${local.project_name}-linux-function-app"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location

  storage_account_name       = azurerm_storage_account.storage.name
  storage_account_access_key = azurerm_storage_account.storage.primary_access_key
  service_plan_id            = azurerm_service_plan.sp.id

  identity {
    type = "SystemAssigned"
  }

  app_settings = {
    WEBSITE_RUN_FROM_PACKAGE                      = 1
    SCM_DO_BUILD_DURING_DEPLOYMENT                = true
    ENABLE_ORYX_BUILD                             = true
    AzureWebJobsStorage                           = azurerm_storage_account.storage.primary_connection_string
    FUNCTIONS_WORKER_RUNTIME                      = "dotnet-isolated"
    ServiceBusConnection__fullyQualifiedNamespace = "${azurerm_servicebus_namespace.namespace.name}.servicebus.windows.net"
    ResourceEndpoint                              = "https://${local.azurerm_communication_service_name}.${azurerm_email_communication_service.email_communication_service.data_location}.communication.azure.com/"
    Sender                                        = "${local.project_name}@${jsondecode(azapi_resource.email_communication_service_domain.output).properties.fromSenderDomain}"
  }

  site_config {
    application_stack {
      dotnet_version              = "8.0"
      use_dotnet_isolated_runtime = true
    }
  }
}

resource "azurerm_role_assignment" "allow_function_to_read_service_bus" {
  scope                = azurerm_servicebus_namespace.namespace.id
  role_definition_name = "Azure Service Bus Data Receiver"
  principal_id         = azurerm_linux_function_app.function.identity[0].principal_id
}

data "azurerm_subscription" "primary" {}

resource "azurerm_role_definition" "allow_function_to_send_emails" {
  name  = "allow-function-to-send-emails"
  scope = data.azurerm_subscription.primary.id

  permissions {
    actions = [
      "Microsoft.Communication/CommunicationServices/Write", "Microsoft.Communication/CommunicationServices/Read"
    ]
    not_actions = []
  }

  assignable_scopes = [
    data.azurerm_subscription.primary.id,
  ]
}

resource "azurerm_role_assignment" "assign_custom_role_to_function" {
  scope              = data.azurerm_subscription.primary.id
  role_definition_id = azurerm_role_definition.allow_function_to_send_emails.role_definition_resource_id
  principal_id       = azurerm_linux_function_app.function.identity[0].principal_id
}

locals {
  publish_function_app = <<-EOT
      func azure functionapp publish ${azurerm_linux_function_app.function.name}
      az functionapp config set --name ${azurerm_linux_function_app.function.name} --resource-group ${azurerm_resource_group.rg.name} --linux-fx-version "DOTNET-ISOLATED|8.0"
EOT
}

resource "null_resource" "function_app_publish" {
  provisioner "local-exec" {
    working_dir = "../NotificationHandler"
    command     = local.publish_function_app
  }

  depends_on = [local.publish_function_app, azurerm_role_assignment.assign_custom_role_to_function]

  triggers = {
    publish_function_app = local.publish_function_app
  }
}

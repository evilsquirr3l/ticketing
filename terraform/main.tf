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
  location = "West Europe"
}

resource "azurerm_service_plan" "sp" {
  name                = "${local.project_name}-sp"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  os_type             = "Linux"
  sku_name            = "P1v3"
}

resource "azurerm_linux_web_app" "app" {
  name                = "${local.project_name}-app"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_service_plan.sp.location
  service_plan_id     = azurerm_service_plan.sp.id
  https_only          = true

  site_config {
    application_stack {
      dotnet_version = "8.0"
    }
  }

  webdeploy_publish_basic_authentication_enabled = false
  zip_deploy_file                                = local.ticketing_zip_deploy_file

  app_settings = {
    WEBSITE_RUN_FROM_PACKAGE       = 1
    SCM_DO_BUILD_DURING_DEPLOYMENT = true
  }
}

# About

The repository contains the code for .NET Mentoring Program D2+ Practice (Ticketing) program. This program was designed
to get practical experience in building scalable and performant REST API solutions. The course
covers technical diagrams creation, data access, and REST API layers implementation and testing. It also tackle
practical cases of asynchronous and multithreaded development topics.

> [!NOTE]
> All tasks and diagrams for this project are in the [docs](./docs) folder.

# What is used

- [ASP NET Core](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-8.0)
- [PostgreSQL](https://www.postgresql.org/)
- [Redis](https://redis.io/)
- [Docker](https://www.docker.com/) for running things locally and for unit/integration tests with [TestContainers](https://www.testcontainers.org/)
- [K6](https://k6.io/) for load testing
- [Terraform](https://www.terraform.io/) for infrastructure as code
- [Azure](https://azure.microsoft.com/) resources:
    - [Service bus](https://azure.microsoft.com/en-us/services/service-bus/)
    - [Functions](https://azure.microsoft.com/en-us/services/functions/) to read messages from the service bus and send emails
    - [Email Communication Service](https://learn.microsoft.com/en-us/azure/communication-services/concepts/email/email-overview)
    - [Logic Apps](https://learn.microsoft.com/en-us/azure/logic-apps/logic-apps-overview) for periodic tasks

# How to deploy

1. Install necessary tools:
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- [Terraform](https://learn.hashicorp.com/tutorials/terraform/install-cli?in=terraform/azure-get-started)
2. Run `az login` to authenticate with Azure

> [!WARNING]
> (Windows only) Change [this line](https://github.com/evilsquirr3l/ticketing/blob/main/terraform/main.tf#L97) that puts deployment files to a zip archive
> to something like
> ```Compress-Archive -Path .\Ticketing\bin\Release\net8.0\publish\* -DestinationPath ticketing.zip -Force```
> (I never tested it on Windows)

3. Run `terraform init` to initialize the Terraform configuration
4. Run `terraform plan` to see what resources will be created

5. Run `terraform apply -auto-approve` to deploy the infrastructure with default PostgreSQL username and password
> [!TIP]
> Default username and password for PostgreSQL are `Adm1n157r470r` and `4-v3ry-53cr37-p455w0rd` respectively.
> If you want to use your own username and password, use the command
> ```terraform apply -auto-approve -var="login=your_username" -var="password=your_password"```

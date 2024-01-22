# Hosting and Deployment

The purpose of the module is to consolidate in practice the materials on ‘Hosting and Deployment’ topic. This module
provides an in-depth understanding of deploying and hosting .NET applications on various platforms. It covers the key
concepts and techniques involved in deploying, configuring, and managing .NET applications on different hosting
platforms.

### What should be done

- Task 1
  Choose the cloud-based platform (AWS, Azure, GCP) where you will host your application created in the previous
  modules.
  Prepare the environment for deployment – create database instance, message queue. Deploy the application and make sure
  that all features work correctly.
- Task 2 (Optional)
  Add health check endpoint to your application that will be used to verify that an application is up and running.
  Health
  check endpoint should include:
  - Response Status
  - Application Version
  - Dependency Status. Information about the status of any external dependencies that the application relies on (database,
    service bus etc.)
    Deploy a new version of your application to a prepared environment and make sure that application is “healthy”.

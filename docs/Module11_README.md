# Background Workers in ASP.NET Core

This module aims to provide learners with a comprehensive understanding of background workers in ASP.NET Core and how to
use them effectively. Upon completion of this module, learners will be able to:

- Understand the benefits of using background workers in ASP.NET Core applications.
- Create and configure background workers to perform long-running tasks in the background.
- Implement best practices for working with background workers in ASP.NET Core.

### What should be done

- Task 1
  Implement background task in your application using Worker Service. This task should be responsible for releasing
  carted seats - if the seat was booked more than 15 minutes ago, the service have to remove the seat from the cart and
  make it available for purchase.

- Task 2 (Optional)
  Discuss with the mentor whether it was possible to implement the processing of messages from the queue (Module 7:
  Service Bus) using a Background Worker Service.


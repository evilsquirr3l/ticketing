using Azure;
using Azure.Communication.Email;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SharedModels;

namespace NotificationHandler;

public class ServiceBusQueueTrigger(ILogger<ServiceBusQueueTrigger> logger)
{
    [Function(nameof(ServiceBusReceivedMessageWithStringProperties))]
    public async Task ServiceBusReceivedMessageWithStringProperties(
        [ServiceBusTrigger("ticketing_servicebus_queue", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage serviceBusMessage)
    {
        var message = serviceBusMessage.Body.ToObjectFromJson<Message>();

        var resourceEndpoint =
            Environment.GetEnvironmentVariable("ResourceEndpoint", EnvironmentVariableTarget.Process) ??
            throw new InvalidOperationException("ResourceEndpoint is not set");
        var emailClient = new EmailClient(new Uri(resourceEndpoint), new DefaultAzureCredential());
        var subject = "Your very important notification about your payment!";
        try
        {
            await emailClient.SendAsync(
                WaitUntil.Completed,
                Environment.GetEnvironmentVariable("Sender", EnvironmentVariableTarget.Process),
                message.CustomerEmail,
                subject,
                null,
                message.ToString());
        }
        catch (RequestFailedException ex)
        {
            logger.LogError("Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}", ex.ErrorCode, ex.Message);
        }
    }
}

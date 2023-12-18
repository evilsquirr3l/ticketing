using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;

namespace NotificationHandler;

public class ServiceBusQueueTrigger
{
    private readonly ILogger<ServiceBusQueueTrigger> _logger;

    public ServiceBusQueueTrigger(ILogger<ServiceBusQueueTrigger> logger)
    {
        _logger = logger;
    }

    [Function(nameof(ServiceBusReceivedMessageWithStringProperties))]
    public void ServiceBusReceivedMessageWithStringProperties(
        [ServiceBusTrigger("mentoring-queue", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message body: {body}", message.Body.ToString());

        // Similarly the DeliveryCount property and the deliveryCount parameter are the same.
        _logger.LogInformation("Delivery Count: {count}", message.DeliveryCount);
    }
}

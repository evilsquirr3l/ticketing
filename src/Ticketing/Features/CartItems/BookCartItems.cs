using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SharedModels;
using Ticketing.Constants;
using Ticketing.Data;
using Ticketing.Data.Entities;
using Ticketing.Models;
using Ticketing.Settings;

namespace Ticketing.Features.CartItems;

[ApiController]
[ApiExplorerSettings(GroupName = "CartItems")]
public class BookCartItems(IMediator mediator) : ControllerBase
{
    [HttpPut]
    [Route("orders/carts/{cartId:guid}/book")]
    public async Task<Results<Created<PaymentViewModel>, NotFound>> Book(Guid cartId)
    {
        var payment = await mediator.Send(new BookCartItemsCommand(cartId));

        return payment is null
            ? TypedResults.NotFound()
            : TypedResults.Created($"payments/{payment.Id}/complete", payment);
    }

    public record BookCartItemsCommand(Guid CartId) : IRequest<PaymentViewModel?>;

    public class BookCartItemsCommandHandler(
        TicketingDbContext dbContext,
        IOutputCacheStore store,
        IAzureClientFactory<ServiceBusSender> serviceBusSenderFactory,
        IOptions<ServiceBusSettings> settings)
        : IRequestHandler<BookCartItemsCommand, PaymentViewModel?>
    {
        private readonly ServiceBusSender _sender = serviceBusSenderFactory.CreateClient(settings.Value.QueueName);

        public async Task<PaymentViewModel?> Handle(BookCartItemsCommand request, CancellationToken cancellationToken)
        {
            var cartItems = await dbContext.CartItems
                .Include(x => x.Offer)
                .ThenInclude(offer => offer.Price)
                .Include(x => x.Cart)
                .ThenInclude(x => x.Customer)
                .Where(x => x.CartId == request.CartId)
                .ToListAsync(cancellationToken: cancellationToken);

            if (await CartItemsAreNotFoundAsync(request, cartItems))
            {
                return null;
            }

            await store.EvictByTagAsync(Tags.Events, cancellationToken);

            var payment = await CreatePaymentAsync(cartItems);

            await BookSeatsAsync(cartItems);
            await dbContext.SaveChangesAsync(cancellationToken);

            var customer = cartItems[0].Cart.Customer;
            await SendMessage(payment.Id, "Book", payment.PaymentDate!.Value,
                customer.Email, customer.Name, payment.Amount);

            return new PaymentViewModel(payment.Id, payment.Amount, payment.PaymentDate);
        }

        private async Task<bool> CartItemsAreNotFoundAsync(BookCartItemsCommand request, List<CartItem> cartItems)
        {
            return cartItems.Count == 0 || await dbContext.Carts.FindAsync(request.CartId) is null;
        }

        private async Task<Payment> CreatePaymentAsync(IEnumerable<CartItem> cartItems)
        {
            var payment = new Payment
            {
                Amount = cartItems.Sum(x => x.Offer.Price.Amount),
                PaymentDate = DateTime.UtcNow
            };

            await dbContext.Payments.AddAsync(payment);
            return payment;
        }

        private async Task BookSeatsAsync(List<CartItem> cartItems)
        {
            foreach (var cartItem in cartItems)
            {
                var seat = await dbContext.Seats.FindAsync(cartItem.Offer.SeatId);
                if (seat is not null)
                {
                    seat.IsReserved = true;
                }
            }
        }

        private Task SendMessage(Guid paymentId, string operationName, DateTime operationDate, string customerEmail,
            string customerName, decimal amount)
        {
            var message = new Message(paymentId, operationName, operationDate, customerEmail, customerName, amount);
            var messageJson = JsonConvert.SerializeObject(message);
            var serviceBusMessage = new ServiceBusMessage(messageJson);

            return _sender.SendMessageAsync(serviceBusMessage);
        }
    }
}

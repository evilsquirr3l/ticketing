using System.Text;
using System.Text.Json;
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

namespace Ticketing.Features.CartItems;


[ApiController]
[ApiExplorerSettings(GroupName = "CartItems")]
public class BookCartItems : ControllerBase
{
    private readonly IMediator _mediator;

    public BookCartItems(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPut]
    [Route("orders/carts/{cartId:guid}/book")]
    public async Task<Results<Created<PaymentViewModel>, NotFound>> Book(Guid cartId)
    {
        var payment = await _mediator.Send(new BookCartItemsCommand(cartId));

        return payment is null ? TypedResults.NotFound() : TypedResults.Created($"payments/{payment.Id}/complete", payment);
    }
    
    public record BookCartItemsCommand(Guid CartId) : IRequest<PaymentViewModel?>;
    
    public class BookCartItemsCommandHandler : IRequestHandler<BookCartItemsCommand, PaymentViewModel?>
    {
        private readonly TicketingDbContext _dbContext;
        private readonly IOutputCacheStore _store;
        private readonly ServiceBusSender _sender;

        public BookCartItemsCommandHandler(TicketingDbContext dbContext, IOutputCacheStore store, IAzureClientFactory<ServiceBusSender> serviceBusSenderFactory, IOptions<ServiceBusSettings> settings)
        {
            _dbContext = dbContext;
            _store = store;
            _sender = serviceBusSenderFactory.CreateClient(settings.Value.QueueName);
        }
    
        public async Task<PaymentViewModel?> Handle(BookCartItemsCommand request, CancellationToken cancellationToken)
        {
            var cartItems = await _dbContext.CartItems
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

            await _store.EvictByTagAsync(Tags.Events, cancellationToken);

            var payment = CreatePaymentAsync(cartItems);

            await BookSeatsAsync(cartItems);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var customer = cartItems[0].Cart.Customer;
            await SendMessage(payment.Id, "Book", payment.PaymentDate!.Value,
                customer.Email, customer.Name, payment.Amount);

            return new PaymentViewModel(payment.Id, payment.Amount, payment.PaymentDate);
        }

        private async Task<bool> CartItemsAreNotFoundAsync(BookCartItemsCommand request, List<CartItem> cartItems)
        {
            return cartItems.Count == 0 || await _dbContext.Carts.FindAsync(request.CartId) is null;
        }

        private Payment CreatePaymentAsync(IEnumerable<CartItem> cartItems)
        {
            var payment = new Payment
            {
                Amount = cartItems.Sum(x => x.Offer.Price.Amount),
                PaymentDate = DateTime.UtcNow
            };

            _dbContext.Payments.Add(payment);
            return payment;
        }

        private async Task BookSeatsAsync(List<CartItem> cartItems)
        {
            foreach (var cartItem in cartItems)
            {
                var seat = await _dbContext.Seats.FindAsync(cartItem.Offer.SeatId);
                if (seat is not null)
                {
                    seat.IsReserved = true;
                }
            }
        }

        private Task SendMessage(Guid paymentId, string operationName, DateTime operationDate, string customerEmail, string customerName, decimal amount)
        {
            var message = new Message(paymentId, operationName, operationDate, customerEmail, customerName, amount);
            var messageJson = JsonConvert.SerializeObject(message);
            var serviceBusMessage = new ServiceBusMessage(messageJson);

            return _sender.SendMessageAsync(serviceBusMessage);
        }
    }
}

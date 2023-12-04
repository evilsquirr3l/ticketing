using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public BookCartItemsCommandHandler(TicketingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    
        public async Task<PaymentViewModel?> Handle(BookCartItemsCommand request, CancellationToken cancellationToken)
        {
            var cartItems = await _dbContext.CartItems
                .Include(x => x.Offer)
                .ThenInclude(offer => offer.Price)
                .Where(x => x.CartId == request.CartId)
                .ToListAsync(cancellationToken: cancellationToken);

            if (await CartItemsAreNotFoundAsync(request, cartItems))
            {
                return null;
            }

            var payment = CreatePaymentAsync(cartItems);

            await BookSeatsAsync(cartItems);

            await _dbContext.SaveChangesAsync(cancellationToken);

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
    }
}

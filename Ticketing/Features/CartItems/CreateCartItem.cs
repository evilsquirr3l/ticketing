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
public class CreateCartItem : ControllerBase
{
    private readonly IMediator _mediator;

    public CreateCartItem(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Route("orders/carts/{cartId:guid}")]
    public async Task<Results<BadRequest<string>, Created<CartItemViewModel>>> Create(Guid cartId,
        CreateCartItemCommand command)
    {
        try
        {
            var cartItem = await _mediator.Send(command with { CartId = cartId });

            return cartItem is null
                ? TypedResults.BadRequest("Cart, Offer or Event not found.")
                : TypedResults.Created($"orders/carts/{cartId}", cartItem);
        }
        catch (InvalidOperationException e)
        {
            return TypedResults.BadRequest(e.Message);
        }
    }

    public record CreateCartItemCommand(Guid CartId, Guid OfferId, Guid EventId) : IRequest<CartItemViewModel?>;

    public class CreateCartItemCommandHandler : IRequestHandler<CreateCartItemCommand, CartItemViewModel?>
    {
        private readonly TicketingDbContext _dbContext;
        private readonly TimeProvider _timeProvider;

        public CreateCartItemCommandHandler(TicketingDbContext dbContext, TimeProvider timeProvider)
        {
            _dbContext = dbContext;
            _timeProvider = timeProvider;
        }

        public async Task<CartItemViewModel?> Handle(CreateCartItemCommand request, CancellationToken cancellationToken)
        {
            if (await CartItemsAreNotFoundAsync(request))
            {
                return null;
            }

            await ThrowIfSeatIsReservedAsync(request, cancellationToken);

            var cartItem = new CartItem
            {
                CartId = request.CartId,
                OfferId = request.OfferId,
                CreatedAt = _timeProvider.GetUtcNow()
            };

            await TrySaveCartItemsAndReserveSeatsAsync(request, cartItem, cancellationToken);

            return new CartItemViewModel(cartItem.Id, cartItem.CartId, cartItem.OfferId, cartItem.CreatedAt);
        }

        private async Task TrySaveCartItemsAndReserveSeatsAsync(CreateCartItemCommand request,
            CartItem cartItem, CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.CartItems.AddAsync(cartItem, cancellationToken);
                await ReserveSeatAsync(request, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException e)
            {
                Console.WriteLine(e.Message);
                throw new InvalidOperationException("This offer is already in the cart.", e.InnerException);
            }
        }

        private async Task<bool> CartItemsAreNotFoundAsync(CreateCartItemCommand request)
        {
            return await _dbContext.Carts.FindAsync(request.CartId) is null ||
                   await _dbContext.Offers.FindAsync(request.OfferId) is null ||
                   await _dbContext.Events.FindAsync(request.EventId) is null;
        }

        private async Task ThrowIfSeatIsReservedAsync(CreateCartItemCommand request,
            CancellationToken cancellationToken)
        {
            var offer = await _dbContext.Offers
                .Include(x => x.Seat)
                .FirstOrDefaultAsync(x => x.Id == request.OfferId, cancellationToken: cancellationToken);

            if (offer!.Seat.IsReserved)
            {
                throw new InvalidOperationException("Seat is already reserved.");
            }
        }

        private async Task ReserveSeatAsync(CreateCartItemCommand request, CancellationToken cancellationToken)
        {
            var offer = await _dbContext.Offers
                .Include(x => x.Seat)
                .FirstOrDefaultAsync(x => x.Id == request.OfferId, cancellationToken: cancellationToken);

            offer!.Seat.IsReserved = true;
        }
    }
}

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

        public CreateCartItemCommandHandler(TicketingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CartItemViewModel?> Handle(CreateCartItemCommand request, CancellationToken cancellationToken)
        {
            if (await CartItemsAreNotFoundAsync(request))
            {
                return null;
            }

            await ThrowIfSeatIsReservedAsync(request, cancellationToken);
            await ThrowIfOfferIsAlreadyInTheCartAsync(request, cancellationToken);

            var cartItem = new CartItem
            {
                CartId = request.CartId,
                OfferId = request.OfferId
            };

            _dbContext.CartItems.Add(cartItem);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new CartItemViewModel(cartItem.Id, cartItem.CartId, cartItem.OfferId);
        }

        private async Task<bool> CartItemsAreNotFoundAsync(CreateCartItemCommand request)
        {
            return await _dbContext.Carts.FindAsync(request.CartId) is null ||
                   await _dbContext.Offers.FindAsync(request.OfferId) is null ||
                   await _dbContext.Events.FindAsync(request.EventId) is null;
        }

        private async Task ThrowIfSeatIsReservedAsync(CreateCartItemCommand request, CancellationToken cancellationToken)
        {
            var offer = await _dbContext.Offers
                .Include(x => x.Seat)
                .FirstOrDefaultAsync(x => x.Id == request.OfferId, cancellationToken: cancellationToken);

            if (offer!.Seat.IsReserved)
            {
                throw new InvalidOperationException("Seat is already reserved.");
            }
        }
        
        private async Task ThrowIfOfferIsAlreadyInTheCartAsync(CreateCartItemCommand request, CancellationToken cancellationToken)
        {
            var cart = await _dbContext.Carts
                .Include(x => x.CartItems)
                .FirstOrDefaultAsync(x => x.Id == request.CartId, cancellationToken: cancellationToken);

            if (cart!.CartItems.Any(x => x.OfferId == request.OfferId))
            {
                throw new InvalidOperationException("Offer is already in the cart.");
            }
        }
    }
}

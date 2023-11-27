using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<Results<BadRequest, Created<CartItemViewModel>>> Create(Guid cartId, CreateCartItemCommand command)
    {
        var cartItem = await _mediator.Send(command with { CartId = cartId });

        return cartItem is null ? TypedResults.BadRequest() : TypedResults.Created($"orders/carts/{cartId}", cartItem);
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
    }
}

using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticketing.Data;
using Ticketing.Models;

namespace Ticketing.Features.CartItems;

[ApiController]
[ApiExplorerSettings(GroupName = "CartItems")]
public class GetCartItems : ControllerBase
{
    private readonly IMediator _mediator;

    public GetCartItems(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    [Route("orders/carts/{cartId:guid}")]
    public async Task<Results<NotFound, Ok<IEnumerable<CartItemViewModel>>>> GetAllCartItems(Guid cartId)
    {
        var cartItems = await _mediator.Send(new GetAllCartItemsQuery(cartId));

        return cartItems is null ? TypedResults.NotFound() : TypedResults.Ok(cartItems);
    }
    
    public record GetAllCartItemsQuery(Guid CartId) : IRequest<IEnumerable<CartItemViewModel>?>;
    
    public class GetAllCartItemsQueryHandler : IRequestHandler<GetAllCartItemsQuery, IEnumerable<CartItemViewModel>?>
    {
        private readonly TicketingDbContext _dbContext;

        public GetAllCartItemsQueryHandler(TicketingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    
        public async Task<IEnumerable<CartItemViewModel>?> Handle(GetAllCartItemsQuery request, CancellationToken cancellationToken)
        {
            var cartItems = await _dbContext.CartItems
                .Where(x => x.CartId == request.CartId)
                .ToListAsync(cancellationToken: cancellationToken);

            var cartItemsViewModel = cartItems.Select(cartItem =>
                new CartItemViewModel(cartItem.Id, cartItem.CartId, cartItem.OfferId, cartItem.CreatedAt));

            return cartItemsViewModel;
        }
    }
}

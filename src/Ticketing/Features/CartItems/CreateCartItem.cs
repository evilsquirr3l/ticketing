using ErrorOr;
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
public class CreateCartItem(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Route("orders/carts/{cartId:guid}")]
    public async Task<Results<BadRequest<string>, Created<CartItemViewModel>>> Create(Guid cartId, CreateCartItemCommand command, CancellationToken token)
    {
        var result = await mediator.Send(command with { CartId = cartId }, token);

        return !result.IsError
            ? TypedResults.Created($"orders/carts/{cartId}", result.Value)
            : TypedResults.BadRequest(result.FirstError.Description);
    }

    public record CreateCartItemCommand(Guid CartId, Guid OfferId, Guid EventId) : IRequest<ErrorOr<CartItemViewModel>>;

    public class CreateCartItemCommandHandler(TicketingDbContext dbContext, TimeProvider timeProvider)
        : IRequestHandler<CreateCartItemCommand, ErrorOr<CartItemViewModel>>
    {
        public async Task<ErrorOr<CartItemViewModel>> Handle(CreateCartItemCommand request, CancellationToken cancellationToken)
        {
            if (await CartItemsAreNotFoundAsync(request))
            {
                return Error.NotFound(description: "Cart, offer or event not found.");
            }

            var seatReserved = await CheckIfSeatIsReservedAsync(request, cancellationToken);
            if (seatReserved.IsError)
            {
                return seatReserved.FirstError;
            }

            var cartItem = new CartItem
            {
                CartId = request.CartId,
                OfferId = request.OfferId,
                CreatedAt = timeProvider.GetUtcNow()
            };

            var result = await TrySaveCartItemsAndReserveSeatsAsync(request, cartItem, cancellationToken);
            
            return result.IsError ? result.FirstError : new CartItemViewModel(cartItem.Id, cartItem.CartId, cartItem.OfferId, cartItem.CreatedAt);
        }

        private async Task<ErrorOr<Success>> TrySaveCartItemsAndReserveSeatsAsync(CreateCartItemCommand request,
            CartItem cartItem, CancellationToken cancellationToken)
        {
            try
            {
                await dbContext.CartItems.AddAsync(cartItem, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Error.Conflict(description: "This offer is already in the cart.");
            }

            return Result.Success;
        }

        private async Task<bool> CartItemsAreNotFoundAsync(CreateCartItemCommand request)
        {
            return await dbContext.Carts.FindAsync(request.CartId) is null ||
                   await dbContext.Offers.FindAsync(request.OfferId) is null ||
                   await dbContext.Events.FindAsync(request.EventId) is null;
        }

        private async Task<ErrorOr<Success>> CheckIfSeatIsReservedAsync(CreateCartItemCommand request, CancellationToken cancellationToken)
        {
            var offer = await dbContext.Offers
                .Include(x => x.Payment)
                .FirstOrDefaultAsync(x => x.Id == request.OfferId, cancellationToken: cancellationToken);

            if (offer!.Payment is not null)
            {
                return Error.Conflict(description: "This seat is already reserved.");
            }

            return Result.Success;
        }
    }
}

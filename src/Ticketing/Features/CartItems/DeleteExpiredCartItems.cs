using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ticketing.Data;
using Ticketing.Settings;

namespace Ticketing.Features.CartItems;

[ApiController]
[ApiExplorerSettings(GroupName = "CartItems")]
public class DeleteExpiredCartItems(IMediator mediator) : ControllerBase
{
    [HttpDelete]
    [Route("orders/carts/expired")]
    public async Task<IResult> DeleteExpired()
    {
        await mediator.Send(new DeleteExpiredCartItemsCommand());

        return Results.Ok();
    }

    public record DeleteExpiredCartItemsCommand : IRequest;

    public class DeleteExpiredCartItemsCommandHandler(
        TicketingDbContext dbContext,
        TimeProvider timeProvider,
        IOptions<CartItemsExpiration> options)
        : IRequestHandler<DeleteExpiredCartItemsCommand>
    {
        private readonly int _cartItemsExpiration = options.Value.CartItemsExpirationInMinutes;

        public async Task Handle(DeleteExpiredCartItemsCommand request, CancellationToken cancellationToken)
        {
            var expiredAt = timeProvider.GetUtcNow().AddMinutes(-_cartItemsExpiration);
            var expiredCarts = await dbContext.CartItems
                .Where(x => x.CreatedAt <= expiredAt)
                .ToListAsync(cancellationToken: cancellationToken);

            dbContext.CartItems.RemoveRange(expiredCarts);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

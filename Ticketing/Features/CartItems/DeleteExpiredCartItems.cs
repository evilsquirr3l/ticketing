using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ticketing.Data;
using Ticketing.Settings;

namespace Ticketing.Features.CartItems;

[ApiController]
[ApiExplorerSettings(GroupName = "CartItems")]
public class DeleteExpiredCartItems : ControllerBase
{
    private readonly IMediator _mediator;

    public DeleteExpiredCartItems(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpDelete]
    [Route("orders/carts")]
    public async Task<IResult> DeleteExpired()
    {
        await _mediator.Send(new DeleteExpiredCartItemsCommand());

        return Results.Ok();
    }

    public record DeleteExpiredCartItemsCommand : IRequest;

    public class DeleteExpiredCartItemsCommandHandler : IRequestHandler<DeleteExpiredCartItemsCommand>
    {
        private readonly TicketingDbContext _dbContext;
        private readonly TimeProvider _timeProvider;
        private readonly int _cartItemsExpiration;

        public DeleteExpiredCartItemsCommandHandler(TicketingDbContext dbContext, TimeProvider timeProvider,
            IOptions<CartItemsExpiration> options)
        {
            _dbContext = dbContext;
            _timeProvider = timeProvider;
            _cartItemsExpiration = options.Value.CartItemsExpirationInMinutes;
        }

        public async Task Handle(DeleteExpiredCartItemsCommand request, CancellationToken cancellationToken)
        {
            var expiredAt = _timeProvider.GetUtcNow().AddMinutes(-_cartItemsExpiration);
            var expiredCarts = await _dbContext.CartItems
                .Where(x => x.CreatedAt <= expiredAt)
                .ToListAsync(cancellationToken: cancellationToken);

            _dbContext.CartItems.RemoveRange(expiredCarts);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

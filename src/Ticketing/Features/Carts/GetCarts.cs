using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticketing.Data;
using Ticketing.Models;

namespace Ticketing.Features.Carts;

[ApiController]
[ApiExplorerSettings(GroupName = "Carts")]
public class GetCarts(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Route("carts")]
    public async Task<IResult> GetAllCarts(int skip, int take = 50, Guid? customerId = null)
    {
        var carts = await mediator.Send(new GetAllCartsQuery(skip, take, customerId));

        return Results.Ok(carts);
    }

    public record GetAllCartsQuery(int Skip, int Take, Guid? CustomerId) : IRequest<PaginatedResult<CartViewModel>>;

    public class GetAllCartsQueryHandler(TicketingDbContext dbContext)
        : IRequestHandler<GetAllCartsQuery, PaginatedResult<CartViewModel>>
    {
        public async Task<PaginatedResult<CartViewModel>> Handle(GetAllCartsQuery request,
            CancellationToken cancellationToken)
        {
            var cartsEntities = await dbContext.Carts
                .Where(x => !request.CustomerId.HasValue || x.CustomerId == request.CustomerId)
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync(cancellationToken: cancellationToken);

            var carts = cartsEntities.Select(x => new CartViewModel(x.Id, x.CustomerId));

            var page = new Page
            {
                Count = cartsEntities.Count,
                Skip = request.Skip,
                Take = request.Take,
                Total = await dbContext.Carts.CountAsync(cancellationToken)
            };

            return new PaginatedResult<CartViewModel>(carts, page);
        }
    }
}

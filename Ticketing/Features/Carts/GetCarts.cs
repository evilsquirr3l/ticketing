using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticketing.Data;
using Ticketing.Models;

namespace Ticketing.Features.Carts;

[ApiController]
[ApiExplorerSettings(GroupName = "Carts")]
public class GetCarts : ControllerBase
{
    private readonly IMediator _mediator;

    public GetCarts(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Route("carts")]
    public async Task<IResult> GetAllCarts(int skip, int take = 50, Guid? customerId = null)
    {
        var carts = await _mediator.Send(new GetAllCartsQuery(skip, take, customerId));

        return Results.Ok(carts);
    }

    public record GetAllCartsQuery(int Skip, int Take, Guid? CustomerId) : IRequest<PaginatedResult<CartViewModel>>;

    public class GetAllCartsQueryHandler : IRequestHandler<GetAllCartsQuery, PaginatedResult<CartViewModel>>
    {
        private readonly TicketingDbContext _dbContext;

        public GetAllCartsQueryHandler(TicketingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PaginatedResult<CartViewModel>> Handle(GetAllCartsQuery request,
            CancellationToken cancellationToken)
        {
            var cartsEntities = await _dbContext.Carts
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
                Total = await _dbContext.Carts.CountAsync(cancellationToken)
            };

            return new PaginatedResult<CartViewModel>(carts, page);
        }
    }
}

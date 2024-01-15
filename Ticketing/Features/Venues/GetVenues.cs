using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticketing.Data;
using Ticketing.Models;

namespace Ticketing.Features.Venues;

[ApiController]
[ApiExplorerSettings(GroupName = "Venues")]
[ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any)]
public class GetVenues : ControllerBase
{
    private readonly IMediator _mediator;

    public GetVenues(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("venues")]
    public async Task<IResult> GetAllVenues(int skip, int take = 50)
    {
        var result = await _mediator.Send(new GetVenuesQuery(skip, take));

        return Results.Ok(result);
    }

    public record GetVenuesQuery(int Skip, int Take) : IRequest<PaginatedResult<VenueViewModel>>;

    public class GetAllVenuesQueryHandler : IRequestHandler<GetVenuesQuery, PaginatedResult<VenueViewModel>>
    {
        private readonly TicketingDbContext _dbContext;

        public GetAllVenuesQueryHandler(TicketingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PaginatedResult<VenueViewModel>> Handle(GetVenuesQuery request,
            CancellationToken cancellationToken)
        {
            var venuesEntities = await _dbContext.Venues
                .Include(x => x.Event)
                .Include(x => x.Manifest)
                .OrderBy(x => x.Location)
                .Skip(request.Skip)
                .Take(request.Take)
                .ToListAsync(cancellationToken: cancellationToken);

            var venues = venuesEntities.Select(x => new VenueViewModel(x.Id, x.Location,
                new EventViewModel(x.Event.Id, x.Event.Name, x.Event.Date, x.Event.Description),
                new ManifestViewModel(x.Manifest.Id, x.Manifest.Map)));

            var page = new Page
            {
                Count = venuesEntities.Count,
                Skip = request.Skip,
                Take = request.Take,
                Total = await _dbContext.Venues.CountAsync(cancellationToken)
            };

            return new PaginatedResult<VenueViewModel>(venues, page);
        }
    }
}

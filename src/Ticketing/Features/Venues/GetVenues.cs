using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticketing.Data;
using Ticketing.Models;

namespace Ticketing.Features.Venues;

[ApiController]
[ApiExplorerSettings(GroupName = "Venues")]
[ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any)]
public class GetVenues(IMediator mediator) : ControllerBase
{
    [HttpGet("venues")]
    public async Task<IResult> GetAllVenues(int skip, int take = 50)
    {
        var result = await mediator.Send(new GetVenuesQuery(skip, take));

        return Results.Ok(result);
    }

    public record GetVenuesQuery(int Skip, int Take) : IRequest<PaginatedResult<VenueViewModel>>;

    public class GetAllVenuesQueryHandler(TicketingDbContext dbContext)
        : IRequestHandler<GetVenuesQuery, PaginatedResult<VenueViewModel>>
    {
        public async Task<PaginatedResult<VenueViewModel>> Handle(GetVenuesQuery request,
            CancellationToken cancellationToken)
        {
            var venuesEntities = await dbContext.Venues
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
                Total = await dbContext.Venues.CountAsync(cancellationToken)
            };

            return new PaginatedResult<VenueViewModel>(venues, page);
        }
    }
}

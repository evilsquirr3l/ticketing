using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticketing.Data;
using Ticketing.Models;

namespace Ticketing.Features.Sections;

[ApiController]
[ApiExplorerSettings(GroupName = "Sections")]
public class GetSections : ControllerBase
{
    private readonly IMediator _mediator;

    public GetSections(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("api/{venueId:guid}/sections")]
    public async Task<IResult> GetAllSections(Guid venueId)
    {
        var result = await _mediator.Send(new GetAllSectionsQuery(venueId));

        return Results.Ok(result);
    }

    public record GetAllSectionsQuery(Guid VenueId) : IRequest<IEnumerable<SectionViewModel>>;

    public class GetAllSectionsQueryHandler : IRequestHandler<GetAllSectionsQuery, IEnumerable<SectionViewModel>>
    {
        private readonly TicketingDbContext _dbContext;

        public GetAllSectionsQueryHandler(TicketingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<SectionViewModel>> Handle(GetAllSectionsQuery request,
            CancellationToken cancellationToken)
        {
            var sections = await _dbContext.Manifests.Include(x => x.Venue).Where(x => x.Venue.Id == request.VenueId)
                .SelectMany(x => x.Sections)
                .Include(x => x.Rows)
                .ThenInclude(x => x.Seats)
                .ToListAsync(cancellationToken: cancellationToken);

            var sectionsViewModel = sections.Select(section =>
                new SectionViewModel(section.Id, section.Name,
                    section.Rows.Select(row => new RowViewModel(row.Id, row.Number,
                        row.Seats.Select(seat => new SeatViewModel(seat.Id, seat.SeatNumber))))));

            return sectionsViewModel;
        }
    }
}

using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticketing.Data;
using Ticketing.Models;

namespace Ticketing.Features.Sections;

[ApiController]
[ApiExplorerSettings(GroupName = "Sections")]
public class GetSectionsInVenue : ControllerBase
{
    private readonly IMediator _mediator;

    public GetSectionsInVenue(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{venueId:guid}/sections")]
    public async Task<Results<Ok<IEnumerable<SectionViewModel>>, NotFound>> GetAllSections(Guid venueId)
    {
        var result = await _mediator.Send(new GetAllSectionsQuery(venueId));

        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public record GetAllSectionsQuery(Guid VenueId) : IRequest<IEnumerable<SectionViewModel>?>;

    public class GetAllSectionsQueryHandler : IRequestHandler<GetAllSectionsQuery, IEnumerable<SectionViewModel>?>
    {
        private readonly TicketingDbContext _dbContext;

        public GetAllSectionsQueryHandler(TicketingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<SectionViewModel>?> Handle(GetAllSectionsQuery request,
            CancellationToken cancellationToken)
        {
            var venue = await _dbContext.Venues.FindAsync(request?.VenueId);

            if (venue is null)
            {
                return null;
            }
            
            var sections = await _dbContext.Manifests
                .Include(x => x.Venue)
                .Where(x => x.Venue.Id == request!.VenueId)
                .SelectMany(x => x.Sections)
                .Include(x => x.Rows)
                .ThenInclude(x => x.Seats)
                .ToListAsync(cancellationToken: cancellationToken);

            var sectionsViewModel = sections.Select(section =>
                new SectionViewModel(section.Id, section.Name,
                    section.Rows.Select(row => new RowViewModel(row.Id, row.Number,
                        row.Seats.Select(seat => new SeatViewModel(seat.Id, seat.SeatNumber, seat.IsReserved, seat.RowId))))));

            return sectionsViewModel;
        }
    }
}

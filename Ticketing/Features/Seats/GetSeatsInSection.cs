using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticketing.Data;
using Ticketing.Models;

namespace Ticketing.Features.Seats;

[ApiController]
[ApiExplorerSettings(GroupName = "Seats")]
public class GetSeatsInSection : ControllerBase
{
    private readonly IMediator _mediator;

    public GetSeatsInSection(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Route("events/{eventId:guid}/sections/{sectionId:guid}/seats")]
    public async Task<Results<NotFound, Ok<IEnumerable<SeatViewModel>>>> GetAllSeats(Guid eventId, Guid sectionId)
    {
        var result = await _mediator.Send(new GetAllSeatsQuery(eventId, sectionId));

        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public record GetAllSeatsQuery(Guid EventId, Guid SectionId) : IRequest<IEnumerable<SeatViewModel>?>;

    public class GetAllSeatsQueryHandler : IRequestHandler<GetAllSeatsQuery, IEnumerable<SeatViewModel>?>
    {
        private readonly TicketingDbContext _dbContext;

        public GetAllSeatsQueryHandler(TicketingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<SeatViewModel>?> Handle(GetAllSeatsQuery request,
            CancellationToken cancellationToken)
        {
            if (await EventAndSectionAreNotFoundAsync(request))
            {
                return null;
            }

            var seats = await _dbContext.Manifests
                .Include(x => x.Venue)
                .Where(x => x.Venue.EventId == request.EventId)
                .SelectMany(x => x.Sections)
                .Include(x => x.Rows)
                .ThenInclude(x => x.Seats)
                .SelectMany(x => x.Rows)
                .SelectMany(x => x.Seats)
                .ToListAsync(cancellationToken: cancellationToken);

            var seatsViewModel = seats.Select(seat =>
                new SeatViewModel(seat.Id, seat.SeatNumber));

            return seatsViewModel;
        }

        private async Task<bool> EventAndSectionAreNotFoundAsync(GetAllSeatsQuery request)
        {
            return await _dbContext.Events.FindAsync(request.EventId) is null ||
                   await _dbContext.Sections.FindAsync(request.SectionId) is null;
        }
    }
}

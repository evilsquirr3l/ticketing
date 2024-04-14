using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Ticketing.Constants;
using Ticketing.Data;
using Ticketing.Models;

namespace Ticketing.Features.Seats;

[ApiController]
[ApiExplorerSettings(GroupName = "Seats")]
[OutputCache(Tags = [Tags.Seats])]
public class GetSeatsInSection(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Route("events/{eventId:guid}/sections/{sectionId:guid}/seats")]
    public async Task<Results<NotFound, Ok<IEnumerable<SeatViewModel>>>> GetAllSeats(Guid eventId, Guid sectionId)
    {
        var result = await mediator.Send(new GetAllSeatsQuery(eventId, sectionId));

        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public record GetAllSeatsQuery(Guid EventId, Guid SectionId) : IRequest<IEnumerable<SeatViewModel>?>;

    public class GetAllSeatsQueryHandler(TicketingDbContext dbContext)
        : IRequestHandler<GetAllSeatsQuery, IEnumerable<SeatViewModel>?>
    {
        public async Task<IEnumerable<SeatViewModel>?> Handle(GetAllSeatsQuery request,
            CancellationToken cancellationToken)
        {
            if (await EventAndSectionAreNotFoundAsync(request))
            {
                return null;
            }

            var seats = await dbContext.Manifests
                .Include(x => x.Venue)
                .Where(x => x.Venue.EventId == request.EventId)
                .SelectMany(x => x.Sections)
                .Include(x => x.Rows)
                .ThenInclude(x => x.Seats)
                .SelectMany(x => x.Rows)
                .SelectMany(x => x.Seats)
                .ToListAsync(cancellationToken: cancellationToken);

            var seatsViewModel = seats.Select(seat =>
                new SeatViewModel(seat.Id, seat.SeatNumber, seat.RowId));

            return seatsViewModel;
        }

        private async Task<bool> EventAndSectionAreNotFoundAsync(GetAllSeatsQuery request)
        {
            return await dbContext.Events.FindAsync(request.EventId) is null ||
                   await dbContext.Sections.FindAsync(request.SectionId) is null;
        }
    }
}

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Ticketing.Constants;
using Ticketing.Data;
using Ticketing.Models;

namespace Ticketing.Features.Events;

[ApiController]
[ApiExplorerSettings(GroupName = "Events")]
[OutputCache(Tags = [Tags.Events])]
public class GetEvents(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Route("events")]
    public async Task<IResult> GetAll(int skip, int take = 50)
    {
        var events = await mediator.Send(new GetEventsQuery(skip, take));

        return Results.Ok(events);
    }
    
    public record GetEventsQuery(int Skip, int Take) : IRequest<PaginatedResult<EventViewModel>>;
    
    public class GetEventsQueryHandler(TicketingDbContext dbContext)
        : IRequestHandler<GetEventsQuery, PaginatedResult<EventViewModel>>
    {
        public async Task<PaginatedResult<EventViewModel>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
        {
            var events = await dbContext.Events
                .OrderBy(x => x.Name)
                .Skip(request.Skip)
                .Take(request.Take)
                .Select(x => new EventViewModel(x.Id, x.Name, x.Date, x.Description))
                .ToListAsync(cancellationToken);

            var page = new Page
            {
                Count = events.Count,
                Skip = request.Skip,
                Take = request.Take,
                Total = await dbContext.Events.CountAsync(cancellationToken)
            };
            
            return new PaginatedResult<EventViewModel>(events, page);
        }
    }
}

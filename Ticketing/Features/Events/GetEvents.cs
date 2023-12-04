using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Ticketing.Data;
using Ticketing.Models;

namespace Ticketing.Features.Events;

[ApiController]
[ApiExplorerSettings(GroupName = "Events")]
[OutputCache]
public class GetEvents : ControllerBase
{
    private readonly IMediator _mediator;

    public GetEvents(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    [Route("events")]
    public async Task<IResult> GetAll(int skip, int take = 50)
    {
        var events = await _mediator.Send(new GetEventsQuery(skip, take));

        return Results.Ok(events);
    }
    
    public record GetEventsQuery(int Skip, int Take) : IRequest<PaginatedResult<EventViewModel>>;
    
    public class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, PaginatedResult<EventViewModel>>
    {
        private readonly TicketingDbContext _dbContext;

        public GetEventsQueryHandler(TicketingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    
        public async Task<PaginatedResult<EventViewModel>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
        {
            var events = await _dbContext.Events
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
                Total = await _dbContext.Events.CountAsync(cancellationToken)
            };
            
            return new PaginatedResult<EventViewModel>(events, page);
        }
    }
}

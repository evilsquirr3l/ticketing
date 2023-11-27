using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ticketing.Data;
using Ticketing.Models;

namespace Ticketing.Features.Payments;

[ApiController]
[ApiExplorerSettings(GroupName = "Payments")]
public class GetPaymentById : ControllerBase
{
    private readonly IMediator _mediator;

    public GetPaymentById(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Route("payments/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var payment = await _mediator.Send(new GetPaymentByIdQuery(id));

        return payment is null ? NotFound() : Ok(payment);
    }

    public record GetPaymentByIdQuery(Guid Id) : IRequest<PaymentViewModel?>;

    public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentViewModel?>
    {
        private readonly TicketingDbContext _dbContext;

        public GetPaymentByIdQueryHandler(TicketingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PaymentViewModel?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
        {
            var payment = await _dbContext.Payments.FindAsync(request.Id);

            return payment is null ? null : new PaymentViewModel(payment.Id, payment.Amount, payment.PaymentDate);
        }
    }
}

using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Ticketing.Data;
using Ticketing.Models;

namespace Ticketing.Features.Payments;

[ApiController]
[ApiExplorerSettings(GroupName = "Payments")]
public class GetPaymentById(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Route("payments/{id:guid}")]
    public async Task<Results<Ok<PaymentViewModel>, NotFound>> GetById(Guid id)
    {
        var payment = await mediator.Send(new GetPaymentByIdQuery(id));

        return payment is null ? TypedResults.NotFound() : TypedResults.Ok(payment);
    }

    public record GetPaymentByIdQuery(Guid Id) : IRequest<PaymentViewModel?>;

    public class GetPaymentByIdQueryHandler(TicketingDbContext dbContext)
        : IRequestHandler<GetPaymentByIdQuery, PaymentViewModel?>
    {
        public async Task<PaymentViewModel?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
        {
            var payment = await dbContext.Payments.FindAsync(request.Id);

            return payment is null ? null : new PaymentViewModel(payment.Id, payment.Amount, payment.PaymentDate);
        }
    }
}

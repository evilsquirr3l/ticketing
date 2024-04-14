using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticketing.Data;
using Ticketing.Models;

namespace Ticketing.Features.Payments;

[ApiController]
[ApiExplorerSettings(GroupName = "Payments")]
public class FailPayment(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Route("payments/{paymentId:guid}/failed")]
    public async Task<Results<Ok<PaymentViewModel>, NotFound>> Fail(Guid paymentId)
    {
        var payment = await mediator.Send(new FailPaymentCommand(paymentId));

        return payment is null ? TypedResults.NotFound() : TypedResults.Ok(payment);
    }

    public record FailPaymentCommand(Guid PaymentId) : IRequest<PaymentViewModel?>;

    public class FailPaymentCommandHandler(TicketingDbContext dbContext)
        : IRequestHandler<FailPaymentCommand, PaymentViewModel?>
    {
        public async Task<PaymentViewModel?> Handle(FailPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await dbContext.Payments.FindAsync(request.PaymentId);

            if (payment is null)
            {
                return null;
            }

            var offer = await dbContext.Offers.Include(x => x.Seat)
                .FirstOrDefaultAsync(x => x.PaymentId == request.PaymentId, cancellationToken: cancellationToken);

            if (offer is null)
            {
                return null;
            }

            offer.PaymentId = null;
            
            await dbContext.SaveChangesAsync(cancellationToken);
            
            return new PaymentViewModel(payment.Id, payment.Amount, payment.PaymentDate);
        }
    }
}

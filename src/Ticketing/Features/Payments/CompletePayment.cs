using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Ticketing.Data;
using Ticketing.Models;

namespace Ticketing.Features.Payments;

[ApiController]
[ApiExplorerSettings(GroupName = "Payments")]
public class CompletePayment(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Route("payments/{paymentId:guid}/complete")]
    public async Task<Results<Ok<PaymentViewModel>, NotFound>> Complete(Guid paymentId)
    {
        var payment = await mediator.Send(new CompletePaymentCommand(paymentId));

        return payment is null ? TypedResults.NotFound() : TypedResults.Ok(payment);
    }
    
    public record CompletePaymentCommand(Guid PaymentId) : IRequest<PaymentViewModel?>;
    
    public class CompletePaymentCommandHandler(TicketingDbContext dbContext)
        : IRequestHandler<CompletePaymentCommand, PaymentViewModel?>
    {
        public async Task<PaymentViewModel?> Handle(CompletePaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await dbContext.Payments.FindAsync(request.PaymentId);

            if (payment is null)
            {
                return null;
            }

            //TODO: update to .NET8 and use TimeProvider
            payment.PaymentDate = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            return new PaymentViewModel(payment.Id, payment.Amount, payment.PaymentDate);
        }
    }
}

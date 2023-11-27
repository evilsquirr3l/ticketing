using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ticketing.Data;
using Ticketing.Models;

namespace Ticketing.Features.Payments;

[ApiController]
[ApiExplorerSettings(GroupName = "Payments")]
public class CompletePayment : ControllerBase
{
    private readonly IMediator _mediator;

    public CompletePayment(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    [Route("payments/{paymentId:guid}/complete")]
    public async Task<IActionResult> Complete(Guid paymentId)
    {
        var payment = await _mediator.Send(new CompletePaymentCommand(paymentId));

        return payment is null ? NotFound() : Ok(payment);
    }
    
    public record CompletePaymentCommand(Guid PaymentId) : IRequest<PaymentViewModel?>;
    
    public class CompletePaymentCommandHandler : IRequestHandler<CompletePaymentCommand, PaymentViewModel?>
    {
        private readonly TicketingDbContext _dbContext;

        public CompletePaymentCommandHandler(TicketingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    
        public async Task<PaymentViewModel?> Handle(CompletePaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _dbContext.Payments.FindAsync(request.PaymentId);

            if (payment is null)
            {
                return null;
            }

            //TODO: update to .NET8 and use TimeProvider
            payment.PaymentDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new PaymentViewModel(payment.Id, payment.Amount, payment.PaymentDate);
        }
    }
}

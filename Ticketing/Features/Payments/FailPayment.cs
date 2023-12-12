using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticketing.Data;
using Ticketing.Models;

namespace Ticketing.Features.Payments;

[ApiController]
[ApiExplorerSettings(GroupName = "Payments")]
public class FailPayment : ControllerBase
{
    private readonly IMediator _mediator;

    public FailPayment(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Route("payments/{paymentId:guid}/failed")]
    public async Task<IActionResult> Fail(Guid paymentId)
    {
        var payment = await _mediator.Send(new FailPaymentCommand(paymentId));

        return payment is null ? NotFound() : Ok(payment);
    }

    public record FailPaymentCommand(Guid PaymentId) : IRequest<PaymentViewModel?>;

    public class FailPaymentCommandHandler : IRequestHandler<FailPaymentCommand, PaymentViewModel?>
    {
        private readonly TicketingDbContext _dbContext;

        public FailPaymentCommandHandler(TicketingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PaymentViewModel?> Handle(FailPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _dbContext.Payments.FindAsync(request.PaymentId);

            if (payment is null)
            {
                return null;
            }

            var offer = await _dbContext.Offers.Include(x => x.Seat)
                .FirstOrDefaultAsync(x => x.PaymentId == request.PaymentId, cancellationToken: cancellationToken);

            if (offer is null)
            {
                return null;
            }

            offer.Seat.IsReserved = false;
            offer.PaymentId = null;
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return new PaymentViewModel(payment.Id, payment.Amount, payment.PaymentDate);
        }
    }
}

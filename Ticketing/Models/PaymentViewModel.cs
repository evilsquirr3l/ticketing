namespace Ticketing.Models;

public record PaymentViewModel(Guid Id, decimal Amount, DateTime PaymentDate);

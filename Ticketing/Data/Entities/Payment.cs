namespace Ticketing.Data.Entities;

public class Payment
{
    public Guid Id { get; set; }
    
    public decimal Amount { get; set; }
    
    public DateTime PaymentDate { get; set; }

    public Guid CartId { get; set; }

    public virtual Cart Cart { get; set; } = null!;
}

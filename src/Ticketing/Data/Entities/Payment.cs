namespace Ticketing.Data.Entities;

public class Payment : BaseEntity
{
    public decimal Amount { get; set; }

    public DateTimeOffset? PaymentDate { get; set; }
    
    public virtual Offer Offer { get; set; } = null!;
}

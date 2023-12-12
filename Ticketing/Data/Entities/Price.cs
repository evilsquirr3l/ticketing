namespace Ticketing.Data.Entities;

public class Price : BaseEntity
{
    public decimal Amount { get; set; }

    public virtual ICollection<Offer> Offers { get; set; } = null!;
}
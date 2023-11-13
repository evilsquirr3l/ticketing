namespace Ticketing.Data.Entities;

public sealed class Price
{
    public Guid Id { get; set; }
    
    public decimal Amount { get; set; }
    
    public required ICollection<Offer> Offers { get; set; }
}

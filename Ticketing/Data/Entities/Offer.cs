namespace Ticketing.Data.Entities;

public class Offer
{
    public Guid Id { get; set; }
    
    public required string Description { get; set; }
    
    public Guid PriceId { get; set; }
    
    public virtual required Price Price { get; set; }
    
    public DateTime ValidFrom { get; set; }
    
    public DateTime ValidUntil { get; set; }
    
    public virtual required ICollection<Seat> Seats { get; set; }
    
    public Guid? CustomerId { get; set; }
    
    public virtual Customer? Customer { get; set; }
}

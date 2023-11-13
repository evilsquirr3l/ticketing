namespace Ticketing.Data.Entities;

public sealed class Offer
{
    public Guid Id { get; set; }
    
    public required string Description { get; set; }
    
    public Guid PriceId { get; set; }
    
    public required Price Price { get; set; }
    
    public DateTime ValidFrom { get; set; }
    
    public DateTime ValidUntil { get; set; }
    
    public required ICollection<Seat> Seats { get; set; }
}

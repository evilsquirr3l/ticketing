namespace Ticketing.Data.Entities;

public class Offer : BaseEntity
{
    public required string OfferType { get; set; }

    public Guid? SeatId { get; set; }
    
    public Guid? SectionId { get; set; }
    
    public Guid EventId { get; set; }
    
    public Guid? PaymentId { get; set; }
    
    public Guid PriceId { get; set; }

    public virtual Seat Seat { get; set; } = null!;
    
    public virtual Section Section { get; set; } = null!;
    
    public virtual Event Event { get; set; } = null!;
    
    public virtual Payment? Payment { get; set; }
    
    public virtual Price Price { get; set; } = null!;
    
    public virtual ICollection<CartItem> CartItems { get; set; } = null!;
}

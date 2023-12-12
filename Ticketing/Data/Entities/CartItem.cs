namespace Ticketing.Data.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    
    public Guid OfferId { get; set; }

    public virtual Cart Cart { get; set; } = null!;
    
    public virtual Offer Offer { get; set; } = null!;
}

namespace Ticketing.Data.Entities;

public class Cart : BaseEntity
{
    public Guid CustomerId { get; set; }
    
    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<CartItem>? CartItems { get; set; }
}

namespace Ticketing.Data.Entities;

public class Cart
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }
    
    public virtual Customer Customer { get; set; } = null!;
    
    public virtual ICollection<CartItem> CartItems { get; set; } = null!;
}

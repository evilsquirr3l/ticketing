namespace Ticketing.Data.Entities;

public class Cart
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public virtual required Customer Customer { get; set; }
    
    public virtual required ICollection<CartItem> CartItems { get; set; }
}
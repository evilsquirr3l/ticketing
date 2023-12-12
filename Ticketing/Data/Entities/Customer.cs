namespace Ticketing.Data.Entities;

public class Customer : BaseEntity
{
    public required string Name { get; set; }
    
    public required string Email { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = null!;
}
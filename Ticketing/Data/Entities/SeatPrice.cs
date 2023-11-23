namespace Ticketing.Data.Entities;

public class SeatPrice
{
    public Guid Id { get; set; }
    
    public required string PriceType { get; set; }
    
    public Guid SeatId { get; set; }
    
    public Guid PriceId { get; set; }

    public virtual Seat Seat { get; set; } = null!;
    
    public virtual Price Price { get; set; } = null!;
    
    public virtual ICollection<CartItem> CartItems { get; set; } = null!;
}

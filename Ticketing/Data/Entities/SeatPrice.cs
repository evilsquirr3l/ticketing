namespace Ticketing.Data.Entities;

public class SeatPrice
{
    public Guid Id { get; set; }
    
    public required string PriceType { get; set; }
    
    public Guid SeatId { get; set; }
    
    public Guid PriceId { get; set; }

    public virtual required Seat Seat { get; set; }
    
    public virtual required Price Price { get; set; }
    
    public virtual required ICollection<CartItem> CartItems { get; set; }
}

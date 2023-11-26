namespace Ticketing.Data.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    
    public Guid SeatId { get; set; }

    public virtual Cart Cart { get; set; } = null!;
    
    public virtual Seat Seat { get; set; } = null!;
}
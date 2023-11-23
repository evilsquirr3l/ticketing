namespace Ticketing.Data.Entities;

public class CartItem
{
    public Guid Id { get; set; }

    public Guid CartId { get; set; }
    
    public Guid SeatPriceId { get; set; }

    public virtual Cart Cart { get; set; } = null!;
    
    public virtual SeatPrice SeatPrice { get; set; } = null!;
}

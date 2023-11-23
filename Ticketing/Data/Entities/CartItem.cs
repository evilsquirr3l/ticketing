namespace Ticketing.Data.Entities;

public class CartItem
{
    public Guid Id { get; set; }

    public Guid CartId { get; set; }
    
    public Guid SeatPriceId { get; set; }

    public virtual required Cart Cart { get; set; }
    
    public virtual required SeatPrice SeatPrice { get; set; }
}

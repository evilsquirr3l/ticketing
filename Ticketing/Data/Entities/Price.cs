namespace Ticketing.Data.Entities;

public class Price
{
    public Guid Id { get; set; }
    
    public decimal Amount { get; set; }

    public virtual required ICollection<SeatPrice> SeatPrices { get; set; }
}
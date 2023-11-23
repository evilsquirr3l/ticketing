namespace Ticketing.Data.Entities;

public class Seat
{
    public Guid Id { get; set; }
    
    public required string SeatNumber { get; set; }

    public Guid RowId { get; set; }

    public virtual required Row Row { get; set; }
    
    public virtual required ICollection<SeatPrice> SeatPrices { get; set; }
}
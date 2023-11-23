namespace Ticketing.Data.Entities;

public class Seat
{
    public Guid Id { get; set; }
    
    public required string SeatNumber { get; set; }

    public Guid RowId { get; set; }

    public virtual Row Row { get; set; } = null!;
    
    public virtual ICollection<SeatPrice> SeatPrices { get; set; } = null!;
}

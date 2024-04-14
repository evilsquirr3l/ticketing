namespace Ticketing.Data.Entities;

public class Seat : BaseEntity
{
    public required string SeatNumber { get; set; }
    
    public Guid RowId { get; set; }
    
    public virtual Row Row { get; set; } = null!;
    
    public virtual ICollection<Offer> Offers { get; set; } = null!;
}

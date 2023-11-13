namespace Ticketing.Data.Entities;

public sealed class Seat
{
    public Guid Id { get; set; }
    
    public required string SeatNumber { get; set; }
    
    public Guid RowId { get; set; }
    
    public required Row Row { get; set; }
    
    public Guid OfferId { get; set; }
    
    public required Offer Offer { get; set; }
}

namespace Ticketing.Data.Entities;

public sealed class Row
{
    public Guid Id { get; set; }
    
    public required string Number { get; set; }
    
    public Guid SectionId { get; set; }
    
    public required Section Section { get; set; }
    
    public required ICollection<Seat> Seats { get; set; }
}

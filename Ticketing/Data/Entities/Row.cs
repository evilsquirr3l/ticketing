namespace Ticketing.Data.Entities;

public class Row
{
    public Guid Id { get; set; }
    
    public required string Number { get; set; }

    public Guid SectionId { get; set; }

    public virtual Section Section { get; set; } = null!;
    
    public virtual ICollection<Seat> Seats { get; set; } = null!;
}

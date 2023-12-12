namespace Ticketing.Data.Entities;

public class Row : BaseEntity
{
    public required string Number { get; set; }

    public Guid SectionId { get; set; }
    
    public virtual Section Section { get; set; } = null!;

    public virtual ICollection<Seat> Seats { get; set; } = null!;
}
namespace Ticketing.Data.Entities;

public class Row
{
    public Guid Id { get; set; }
    
    public required string Number { get; set; }

    public Guid SectionId { get; set; }

    public virtual required Section Section { get; set; }
    
    public virtual required ICollection<Seat> Seats { get; set; }
}

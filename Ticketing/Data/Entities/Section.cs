namespace Ticketing.Data.Entities;

public class Section
{
    public Guid Id { get; set; }
    
    public required string Name { get; set; }

    public Guid VenueId { get; set; }

    public virtual Venue Venue { get; set; } = null!;
    
    public virtual ICollection<Row> Rows { get; set; } = null!;
}

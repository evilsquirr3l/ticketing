namespace Ticketing.Data.Entities;

public class Section
{
    public Guid Id { get; set; }
    
    public required string Name { get; set; }

    public Guid VenueId { get; set; }

    public virtual required Venue Venue { get; set; }
    
    public virtual required ICollection<Row> Rows { get; set; }
}

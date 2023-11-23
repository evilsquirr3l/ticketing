namespace Ticketing.Data.Entities;

public class Venue
{
    public Guid Id { get; set; }
    
    public required string Location { get; set; }

    public Guid EventId { get; set; }
    
    public virtual required Event Event { get; set; }
    
    public virtual required ICollection<Section> Sections { get; set; }
}

namespace Ticketing.Data.Entities;

public class Venue
{
    public Guid Id { get; set; }
    
    public required string Location { get; set; }

    public Guid EventId { get; set; }
    
    public virtual Event Event { get; set; } = null!;
    
    public virtual ICollection<Section> Sections { get; set; } = null!;
}
